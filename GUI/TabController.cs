using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class TabController
    {

        public TabController(TabControl control)
        {
            m_TabControl = control;
            m_TabControl.Deselecting += OnDeselect;
            m_TabControl.Selecting += OnDeselect;

            foreach (TabPage page in m_TabControl.TabPages)
            {
                m_TabPages.Add(page.Name, page);
            }

            m_TabControl.TabPages.Clear();
        }

        public void ShowTab(string name, int index = 0, bool setActive = true)
        {
            Util.Invoke(m_TabControl, () =>
            {
                if (!m_TabControl.TabPages.Contains(m_TabPages[name]))
                {
                    if (index > m_TabControl.TabPages.Count)
                    {
                        index = m_TabControl.TabPages.Count;
                    }

                    m_TabControl.TabPages.Insert(index, m_TabPages[name]);
                }

                if (setActive)
                {
                    SetActiveTab(name);
                }
            });
        }

        public void HideTab(string name)
        {
            Util.Invoke(m_TabControl, () =>
            {
                // Unsafe to hide the active tab as of Mono 5.14
                if (m_TabControl.SelectedTab?.Name == name)
                {
                    m_TabControl.DeselectTab(name);
                }
                m_TabControl.TabPages.Remove(m_TabPages[name]);
            });
        }

        public void RenameTab(string name, string newDisplayName)
        {
            Util.Invoke(m_TabControl, () =>
            {
                m_TabPages[name].Text = newDisplayName;
            });
        }

        public void SetTabLock(bool state)
        {
            Util.Invoke(m_TabControl, () =>
            {
                m_TabLock = state;
            });
        }

        public void SetActiveTab(string name)
        {
            Util.Invoke(m_TabControl, () =>
            {
                var tabLock = m_TabLock;
                m_TabLock = false;
                m_TabControl.SelectTab(m_TabPages[name]);
                m_TabLock = tabLock;
            });
        }

        private void OnDeselect(object sender, TabControlCancelEventArgs args)
        {
            if (m_TabLock)
            {
                args.Cancel = true;
            }
            else if (Platform.IsMac)
            {
                if (args.Action == TabControlAction.Deselecting && args.TabPage != null)
                {
                    // Have to set visibility to false on children controls on hidden tabs because they don't
                    // always heed parent visibility on Mac OS X https://bugzilla.xamarin.com/show_bug.cgi?id=3124
                    foreach (Control control in args.TabPage.Controls)
                    {
                        control.Visible = false;
                    }
                }
                else if (args.Action == TabControlAction.Selecting && args.TabPage != null)
                {
                    // Set children controls' visibility back to true
                    foreach (Control control in args.TabPage.Controls)
                    {
                        control.Visible = true;

                        // Have to specifically tell the mod list's panel to refresh
                        // after things settle out because otherwise it doesn't
                        // when coming back to the mods tab from updating the repo
                        if (control is SplitContainer splitter)
                        {
                            Task.Factory.StartNew(
                                () =>
                                {
                                    Thread.Sleep(500);
                                    splitter.Panel1.Refresh();
                                });
                        }
                    }
                }
            }
        }

        private readonly TabControl m_TabControl;

        private bool m_TabLock;

        public Dictionary<string, TabPage> m_TabPages = new Dictionary<string, TabPage>();
    }
}
