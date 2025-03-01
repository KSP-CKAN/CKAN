using System;

namespace CKAN.GUI
{
    partial class SelectionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(SelectionDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.OptionsList = new System.Windows.Forms.ListBox();
            this.CancelSelectionButton = new System.Windows.Forms.Button();
            this.DefaultButton = new System.Windows.Forms.Button();
            this.SelectButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // MessageLabel
            //
            this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MessageLabel.Location = new System.Drawing.Point(5, 5);
            this.MessageLabel.Size = new System.Drawing.Size(390, 40);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.TabStop = false;
            this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            resources.ApplyResources(this.MessageLabel, "MessageLabel");
            //
            // panel1
            //
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Controls.Add(this.OptionsList);
            this.panel1.Controls.Add(this.CancelSelectionButton);
            this.panel1.Controls.Add(this.DefaultButton);
            this.panel1.Controls.Add(this.SelectButton);
            this.panel1.Margin = new System.Windows.Forms.Padding(10);
            this.panel1.Padding = new System.Windows.Forms.Padding(10);
            this.panel1.Location = new System.Drawing.Point(10, 10);
            this.panel1.Size = new System.Drawing.Size(400, 300);
            this.panel1.Name = "panel1";
            this.panel1.TabStop = false;
            //
            // OptionsList
            //
            this.OptionsList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.OptionsList.Location = new System.Drawing.Point(5, 5);
            this.OptionsList.Size = new System.Drawing.Size(390, 265);
            this.OptionsList.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.OptionsList.MultiColumn = false;
            this.OptionsList.Name = "OptionsList";
            this.OptionsList.SelectedIndexChanged += new System.EventHandler(OptionsList_SelectedIndexChanged);
            this.OptionsList.DoubleClick += new System.EventHandler(this.OptionsList_DoubleClick);
            this.OptionsList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OptionsList_KeyDown);
            resources.ApplyResources(this.OptionsList, "OptionsList");
            //
            // SelectButton
            //
            this.SelectButton.AutoSize = true;
            this.SelectButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.SelectButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.SelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectButton.Location = new System.Drawing.Point(325, 275);
            this.SelectButton.Size = new System.Drawing.Size(60, 20);
            this.SelectButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.TabIndex = 1;
            this.SelectButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.SelectButton, "SelectButton");
            //
            // DefaultButton
            //
            this.DefaultButton.AutoSize = true;
            this.DefaultButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.DefaultButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DefaultButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DefaultButton.Location = new System.Drawing.Point(175, 275);
            this.DefaultButton.Size = new System.Drawing.Size(60, 20);
            this.DefaultButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.DefaultButton.Name = "SelectButton";
            this.DefaultButton.TabIndex = 0;
            this.DefaultButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.DefaultButton, "DefaultButton");
            //
            // CancelSelectionButton
            //
            this.CancelSelectionButton.AutoSize = true;
            this.CancelSelectionButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelSelectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.CancelSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelSelectionButton.Location = new System.Drawing.Point(5, 275);
            this.CancelSelectionButton.Size = new System.Drawing.Size(60, 20);
            this.CancelSelectionButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelSelectionButton.Name = "CancelSelectionButton";
            this.CancelSelectionButton.TabIndex = 2;
            this.CancelSelectionButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.CancelSelectionButton, "CancelSelectionButton");
            //
            // SelectionDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 420);
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.AcceptButton = this.SelectButton;
            this.CancelButton = this.CancelSelectionButton;
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MessageLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "SelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox OptionsList;
        private System.Windows.Forms.Button CancelSelectionButton;
        private System.Windows.Forms.Button DefaultButton;
        private System.Windows.Forms.Button SelectButton;
    }
}
