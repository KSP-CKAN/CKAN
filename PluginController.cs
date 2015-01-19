using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace CKAN
{
    public class PluginController
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(PluginController));

        private string m_PluginsPath = "";

        public PluginController(string path, bool doActivate = true)
        {
            m_PluginsPath = path;

            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                LoadAssembly(dll);
            }

            if (doActivate)
            {
                foreach (var plugin in m_DormantPlugins.ToArray()) // use .ToArray() to avoid modifying the collection during the for-each
                {
                    ActivatePlugin(plugin);
                }
            }
        }

        private void LoadAssembly(string dll)
        {
            Assembly assembly = null;

            try
            {
                assembly = Assembly.UnsafeLoadFrom(dll);
            }
            catch (Exception ex)
            {
                log.WarnFormat("Failed to load assembly \"{0}\" - {1}", dll, ex.Message);
                return;
            }

            log.InfoFormat("Loaded assembly - \"{0}\"", dll);

            try
            {
                var typeName = Path.GetFileNameWithoutExtension(dll);
                typeName = String.Format("{0}.{1}", typeName, typeName);

                Type type = assembly.GetType(typeName);
                IGUIPlugin pluginInstance = (IGUIPlugin)Activator.CreateInstance(type);

                foreach (var loadedPlugin in m_ActivePlugins)
                {
                    if (loadedPlugin.GetName() == pluginInstance.GetName())
                    {
                        if (loadedPlugin.GetVersion().IsLessThan(pluginInstance.GetVersion()))
                        {
                            DeactivatePlugin(loadedPlugin);
                            m_DormantPlugins.Remove(loadedPlugin);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                foreach (var loadedPlugin in m_DormantPlugins)
                {
                    if (loadedPlugin.GetName() == pluginInstance.GetName())
                    {
                        if (loadedPlugin.GetVersion().IsLessThan(pluginInstance.GetVersion()))
                        {
                            m_DormantPlugins.Remove(loadedPlugin);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                m_DormantPlugins.Add(pluginInstance);
                log.WarnFormat("Successfully instantiated type \"{0}\" from {1}.dll", assembly.FullName, assembly.FullName);
            }
            catch (Exception ex)
            {
                log.WarnFormat("Failed to instantiate type \"{0}\" from {1} - {2}.dll", assembly.FullName, assembly.FullName, ex.Message);
            }
        }

        public void AddNewAssemblyToPluginsPath(string path)
        {
            if (Path.GetExtension(path) != ".dll")
            {
                log.ErrorFormat("Not a .dll, skipping..");
                return;
            }

            var targetPath = Path.Combine(m_PluginsPath, Path.GetFileName(path));
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (Exception)
                {
                    log.ErrorFormat("Cannot copy plugin to {0}, because it already exists and is open..", targetPath);
                    return;
                }
            }

            File.Copy(path, targetPath);
            LoadAssembly(targetPath);
        }

        public void ActivatePlugin(IGUIPlugin plugin)
        {
            if (m_DormantPlugins.Contains(plugin))
            {
                try
                {
                    plugin.Initialize();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed to activate plugin \"{0} - {1}\" - {2}", plugin.GetName(), plugin.GetVersion(), ex.Message);
                    return;
                }

                m_ActivePlugins.Add(plugin);
                m_DormantPlugins.Remove(plugin);
                log.InfoFormat("Activated plugin \"{0} - {1}\"", plugin.GetName(), plugin.GetVersion());
            }
        }

        public void DeactivatePlugin(IGUIPlugin plugin)
        {
            if (m_ActivePlugins.Contains(plugin))
            {
                try
                {
                    plugin.Deinitialize();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed to deactivate plugin \"{0} - {1}\" - {2}", plugin.GetName(), plugin.GetVersion(), ex.Message);
                    return;
                }

                m_DormantPlugins.Add(plugin);
                m_ActivePlugins.Remove(plugin);
                log.InfoFormat("Deactivated plugin \"{0} - {1}\"", plugin.GetName(), plugin.GetVersion());
            }
        }

        public void UnloadPlugin(IGUIPlugin plugin)
        {
            if (m_ActivePlugins.Contains(plugin))
            {
                DeactivatePlugin(plugin);
            }

            if (m_DormantPlugins.Contains(plugin))
            {
                m_DormantPlugins.Remove(plugin);
            }
        }

        public List<IGUIPlugin> ActivePlugins
        {
            get { return m_ActivePlugins.ToList(); }
        }

        public List<IGUIPlugin> DormantPlugins
        {
            get { return m_DormantPlugins.ToList(); }
        }

        private HashSet<IGUIPlugin> m_ActivePlugins = new HashSet<IGUIPlugin>();
        private HashSet<IGUIPlugin> m_DormantPlugins = new HashSet<IGUIPlugin>();

    }

}
