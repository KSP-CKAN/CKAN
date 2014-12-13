using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{

    class TabController
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
            if (m_TabControl.InvokeRequired)
            {
                m_TabControl.Invoke(new MethodInvoker(() => _ShowTab(name, index, setActive)));
            }
            else
            {
                _ShowTab(name, index, setActive);
            }
        }

        public void HideTab(string name)
        {
            if (m_TabControl.InvokeRequired)
            {
                m_TabControl.Invoke(new MethodInvoker(() => _HideTab(name)));
            }
            else
            {
                _HideTab(name);
            }
        }

        public void RenameTab(string name, string newDisplayName)
        {
            if (m_TabControl.InvokeRequired)
            {

                m_TabControl.Invoke(new MethodInvoker(() => _RenameTab(name, newDisplayName)));
            }
            else
            {
                _RenameTab(name, newDisplayName);
            }
        }

        public void SetTabLock(bool state)
        {
            if (m_TabControl.InvokeRequired)
            {
                m_TabControl.Invoke(new MethodInvoker(() => _SetTabLock(state)));
            }
            else
            {
                _SetTabLock(state);
            }
        }

        public void SetActiveTab(string name)
        {
            if(m_TabControl.InvokeRequired)
            {
                m_TabControl.Invoke(new MethodInvoker(() => _SetActiveTab(name)));
            }
            else
            {
                _SetActiveTab(name);
            }
        }

        private void _ShowTab(string name, int index = 0, bool setActive = true)
        {
            if (m_TabControl.TabPages.Contains(m_TabPages[name]))
            {
                if (setActive)
                {
                    SetActiveTab(name);
                }

                return;
            }

            if (index > m_TabControl.TabPages.Count)
            {
                index = m_TabControl.TabPages.Count;
            }

            m_TabControl.TabPages.Insert(index, m_TabPages[name]);

            if (setActive)
            {
                SetActiveTab(name);
            }
        }

        private void _HideTab(string name)
        {
            m_TabControl.TabPages.Remove(m_TabPages[name]);
        }

        public void _RenameTab(string name, string newDisplayName)
        {
            m_TabPages[name].Text = newDisplayName;
        }

        private void _SetTabLock(bool state)
        {
            m_TabLock = state;
        }

        private void _SetActiveTab(string name)
        {
            var tabLock = m_TabLock;
            m_TabLock = false;
            m_TabControl.SelectTab(m_TabPages[name]);
            m_TabLock = tabLock;
        }

        private void OnDeselect(object sender, TabControlCancelEventArgs args)
        {
            if (m_TabLock)
            {
                args.Cancel = true;
            }
        }

        private TabControl m_TabControl;

        private bool m_TabLock;

        private Dictionary<string, TabPage> m_TabPages = new Dictionary<string, TabPage>();

    }


}
