using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{

    class TabController
    {

        public TabController(TabControl control)
        {
            m_TabControl = control;
            m_TabControl.Deselecting += OnDeselect;

            foreach (TabPage page in m_TabControl.TabPages)
            {
                m_TabPages.Add(page.Name, page);
            }

            m_TabControl.TabPages.Clear();
        }

        public void ShowTab(string name, int index = 0, bool setActive = true)
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

        public void HideTab(string name)
        {
            m_TabControl.TabPages.Remove(m_TabPages[name]);
        }

        public void SetTabLock(bool state)
        {
            m_TabLock = state;
        }

        public void SetActiveTab(string name)
        {
            m_TabControl.SelectTab(m_TabPages[name]);
        }

        private void OnDeselect(object sender, TabControlCancelEventArgs args)
        {
            if (m_TabLock)
            {
                args.Cancel = true;
            }
        }

        private TabControl m_TabControl = null;

        private bool m_TabLock = false;

        private Dictionary<string, TabPage> m_TabPages = new Dictionary<string, TabPage>();

    }


}
