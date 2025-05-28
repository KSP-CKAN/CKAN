using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Extensions;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class TagsLabelsLinkList : FlowLayoutPanel
    {
        [ForbidGUICalls]
        public void UpdateTagsAndLabels(IEnumerable<ModuleTag>?   tags,
                                        IEnumerable<ModuleLabel>? labels)
        {
            Util.Invoke(this, () =>
            {
                SuspendLayout();
                Controls.Clear();
                if (tags != null)
                {
                    foreach (ModuleTag tag in tags)
                    {
                        Controls.Add(TagLabelLink(
                            tag.Name, tag, tagToolTip,
                            new LinkLabelLinkClickedEventHandler(TagLinkLabel_LinkClicked)));
                    }
                }
                if (labels != null)
                {
                    foreach (ModuleLabel mlbl in labels)
                    {
                        Controls.Add(TagLabelLink(
                            mlbl.Name, mlbl, Properties.Resources.FilterLinkToolTip,
                            new LinkLabelLinkClickedEventHandler(LabelLinkLabel_LinkClicked)));
                    }
                }
                ResumeLayout();
            });
        }

        public string TagToolTipText
        {
            get => tagToolTip;
            set
            {
                tagToolTip = value;
                foreach (var lbl in Controls.OfType<LinkLabel>()
                                            .Where(lbl => lbl.Tag is ModuleTag))
                {
                    ToolTip.SetToolTip(lbl, tagToolTip);
                }
            }
        }

        public event Action<ModuleTag,   bool>? TagClicked;
        public event Action<ModuleLabel, bool>? LabelClicked;
        public event Action<ModuleTag>?         ShowHideTag;
        public event Action<ModuleLabel>?       AddRemoveModuleLabel;

        private string tagToolTip = Properties.Resources.FilterLinkToolTip;

        private static int LinkLabelBottom(LinkLabel? lbl)
            => lbl == null ? 0
                           : lbl.Bottom + lbl.Margin.Bottom + lbl.Padding.Bottom;

        public int TagsHeight
            => LinkLabelBottom(Controls.OfType<LinkLabel>()
                                       .LastOrDefault());

        private LinkLabel TagLabelLink(string name,
                                       object tag,
                                       string toolTip,
                                       LinkLabelLinkClickedEventHandler onClick)
        {
            var backColor = (tag is ModuleLabel mlbl ? mlbl.Color : null)
                            ?? Color.Transparent;
            var link = new LinkLabel()
            {
                AutoSize     = true,
                BackColor    = backColor,
                LinkColor    = backColor.ForeColorForBackColor()
                               ?? SystemColors.GrayText,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin       = new Padding(0, 2, 4, 2),
                Text         = name,
                Tag          = tag,
            };
            link.LinkClicked += onClick;
            ToolTip.SetToolTip(link, toolTip);
            return link;
        }

        private void TagLinkLabel_LinkClicked(object?                        sender,
                                              LinkLabelLinkClickedEventArgs? e)
        {
            if (sender is LinkLabel { Tag: ModuleTag t } llbl)
            {
                switch (e)
                {
                    case { Button: MouseButtons.Left }:
                        TagClicked?.Invoke(t, ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift));
                        break;

                    case { Button: MouseButtons.Right }:
                        var showHideLink = new ToolStripMenuItem(string.Format(Properties.Resources.UtilShowHideLink,
                                                                               t.Name));
                        showHideLink.Click += (sender, ev) => ShowHideTag?.Invoke(t);
                        OpenLinkLabelContextMenu(llbl, showHideLink);
                        break;
                }
            }
        }

        private void LabelLinkLabel_LinkClicked(object?                        sender,
                                                LinkLabelLinkClickedEventArgs? e)
        {
            if (sender is LinkLabel { Tag: ModuleLabel l } llbl)
            {
                switch (e)
                {
                    case { Button: MouseButtons.Left }:
                        LabelClicked?.Invoke(l, ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift));
                        break;
                    case { Button: MouseButtons.Right }:
                        var addRemoveLink = new ToolStripMenuItem(string.Format(Properties.Resources.UtilAddRemoveModuleLink,
                                                                                l.Name));
                        addRemoveLink.Click += (sender, ev) => AddRemoveModuleLabel?.Invoke(l);
                        OpenLinkLabelContextMenu(llbl, addRemoveLink);
                        break;
                }
            }
        }

        private static void OpenLinkLabelContextMenu(LinkLabel                  label,
                                                     params ToolStripMenuItem[] options)
        {
            var menu = new ContextMenuStrip();
            if (Platform.IsMono)
            {
                menu.Renderer = new FlatToolStripRenderer();
            }
            menu.Items.AddRange(options);
            menu.Show(label.PointToScreen(new Point(0, label.Height)));
        }

        private readonly ToolTip ToolTip = new ToolTip()
        {
            AutoPopDelay = 10000,
            InitialDelay = 250,
            ReshowDelay  = 250,
            ShowAlways   = true,
        };
    }
}
