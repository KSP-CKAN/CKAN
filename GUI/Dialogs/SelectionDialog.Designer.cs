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
            this.SelectButton = new System.Windows.Forms.Button();
            this.DefaultButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OptionsList = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Controls.Add(this.MessageLabel);
            this.panel1.Controls.Add(this.OptionsList);
            this.panel1.Controls.Add(this.CancelButton);
            this.panel1.Controls.Add(this.DefaultButton);
            this.panel1.Controls.Add(this.SelectButton);
            this.panel1.Location = new System.Drawing.Point(10, 10);
            this.panel1.Size = new System.Drawing.Size(400, 400);
            this.panel1.Name = "panel1";
            this.OptionsList.TabStop = false;
            this.DefaultButton.UseVisualStyleBackColor = true;
            //
            // MessageLabel
            //
            this.MessageLabel.Location = new System.Drawing.Point(5, 5);
            this.MessageLabel.Size = new System.Drawing.Size(390, 40);
            this.MessageLabel.Name = "MessageLabel";
            this.OptionsList.TabStop = false;
            this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DefaultButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.MessageLabel, "MessageLabel");
            //
            // OptionsList
            //
            this.OptionsList.Location = new System.Drawing.Point(5, 55);
            this.OptionsList.Size = new System.Drawing.Size(390, 315);
            this.OptionsList.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.OptionsList.MultiColumn = false;
            this.OptionsList.SelectedIndexChanged += new System.EventHandler(OptionsList_SelectedIndexChanged);
            this.OptionsList.Name = "OptionsList";
            this.DefaultButton.UseVisualStyleBackColor = true;
            //
            // SelectButton
            //
            this.SelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectButton.Location = new System.Drawing.Point(325, 375);
            this.SelectButton.Size = new System.Drawing.Size(60, 20);
            this.SelectButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.TabIndex = 1;
            this.SelectButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.SelectButton, "SelectButton");
            //
            // DefaultButton
            //
            this.DefaultButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DefaultButton.Location = new System.Drawing.Point(160, 375);
            this.DefaultButton.Size = new System.Drawing.Size(60, 20);
            this.DefaultButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.DefaultButton.Name = "SelectButton";
            this.DefaultButton.TabIndex = 0;
            this.DefaultButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.DefaultButton, "DefaultButton");
            //
            // CancelButton
            //
            this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelButton.Location = new System.Drawing.Point(5, 375);
            this.CancelButton.Size = new System.Drawing.Size(60, 20);
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.TabIndex = 2;
            this.CancelButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.CancelButton, "CancelButton");
            //
            // SelectionDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 420);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "SelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            resources.ApplyResources(this, "$this");
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.Button DefaultButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.ListBox OptionsList;
    }
}
