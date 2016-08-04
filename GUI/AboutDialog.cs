﻿using System.Diagnostics;
using System.Windows.Forms;

namespace CKAN
{
    public partial class AboutDialog : MonoCompatibleForm
    {
        public AboutDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/CKAN/blob/master/LICENSE.md");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/CKAN/graphs/contributors");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/CKAN/");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://forum.kerbalspaceprogram.com/index.php?/topic/143140-*");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://ksp-ckan.org");
        }
    }
}