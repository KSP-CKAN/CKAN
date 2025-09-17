using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using log4net;

namespace CKAN.GUI
{
    [ExcludeFromCodeCoverage]
    public class PluginController
    {
        public PluginController(string path, bool activate = true)
        {
            m_PluginsPath = path;
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                LoadAssembly(dll, activate);
            }
        }

        private void LoadAssembly(string dll, bool activate)
        {
            try
            {
                if (Assembly.UnsafeLoadFrom(dll) is Assembly assembly)
                {
                    try
                    {
                        log.InfoFormat("Loaded assembly - \"{0}\"", dll);

                        var typeName = string.Format("{0}.{0}",
                                                     Path.GetFileNameWithoutExtension(dll));

                        if (assembly.GetType(typeName) is Type type
                            && Activator.CreateInstance(type) is IGUIPlugin pluginInstance)
                        {
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
                            if (activate)
                            {
                                ActivatePlugin(pluginInstance);
                            }
                            log.WarnFormat("Successfully instantiated type \"{0}\" from {1}.dll",
                                           assembly.FullName, assembly.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WarnFormat("Failed to instantiate type \"{0}\" from {1} - {2}.dll",
                                       assembly.FullName, assembly.FullName, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WarnFormat("Failed to load assembly \"{0}\" - {1}",
                               dll, ex.Message);
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
            LoadAssembly(targetPath, false);
        }

        public void ActivatePlugin(IGUIPlugin plugin)
        {
            if (m_DormantPlugins.Contains(plugin))
            {
                try
                {
                    log.Debug("Initializing " + plugin.GetName());
                    plugin.Initialize();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed to activate plugin \"{0} - {1}\" - {2}",
                                    plugin.GetName(),
                                    plugin.GetVersion(),
                                    ex.ToString());
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
                    log.Debug("Deinitialize " + plugin.GetName());
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
                log.Debug("Deactivate " + plugin.GetName());
                DeactivatePlugin(plugin);
            }
            m_DormantPlugins.Remove(plugin);
        }

        public IReadOnlyCollection<IGUIPlugin> ActivePlugins  => m_ActivePlugins;
        public IReadOnlyCollection<IGUIPlugin> DormantPlugins => m_DormantPlugins;

        private readonly string              m_PluginsPath    = "";
        private readonly HashSet<IGUIPlugin> m_ActivePlugins  = new HashSet<IGUIPlugin>();
        private readonly HashSet<IGUIPlugin> m_DormantPlugins = new HashSet<IGUIPlugin>();

        private static readonly ILog log = LogManager.GetLogger(typeof(PluginController));
    }
}
