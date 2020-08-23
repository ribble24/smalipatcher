using System.Drawing;
using System.Windows.Forms;

namespace SmaliPatcher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.frameworkDivider = new MaterialSkin.Controls.MaterialDivider();
            this.logBox = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            this.bottomBar = new MaterialSkin.Controls.MaterialDivider();
            this._optionsList = new System.Windows.Forms.ListView();
            this.statusLabel = new System.Windows.Forms.Label();
            this.authorLabel = new System.Windows.Forms.Label();
            this.patchOptionsLabel = new System.Windows.Forms.Label();
            this.customSourceLabel = new System.Windows.Forms.Label();
            this.frameworkBox = new MaterialSkin.Controls.MaterialTextBox();
            this.frameworkBrowseButton = new MaterialSkin.Controls.MaterialButton();
            this.patchButton = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // materialDivider1
            // 
            this.materialDivider1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(0, 263);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(454, 38);
            this.materialDivider1.TabIndex = 0;
            this.materialDivider1.Text = "patchDivider";
            // 
            // frameworkDivider
            // 
            this.frameworkDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frameworkDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.frameworkDivider.Depth = 0;
            this.frameworkDivider.Location = new System.Drawing.Point(0, 425);
            this.frameworkDivider.MouseState = MaterialSkin.MouseState.HOVER;
            this.frameworkDivider.Name = "frameworkDivider";
            this.frameworkDivider.Size = new System.Drawing.Size(454, 38);
            this.frameworkDivider.TabIndex = 2;
            // 
            // logBox
            // 
            this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.logBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logBox.Depth = 0;
            this.logBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.logBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.logBox.Hint = "";
            this.logBox.Location = new System.Drawing.Point(12, 75);
            this.logBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(430, 182);
            this.logBox.TabIndex = 3;
            this.logBox.Text = "fOmey @ XDA\nPatcher version: ";
            // 
            // bottomBar
            // 
            this.bottomBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.bottomBar.Depth = 0;
            this.bottomBar.Location = new System.Drawing.Point(0, 566);
            this.bottomBar.MouseState = MaterialSkin.MouseState.HOVER;
            this.bottomBar.Name = "bottomBar";
            this.bottomBar.Size = new System.Drawing.Size(454, 23);
            this.bottomBar.TabIndex = 4;
            // 
            // _optionsList
            // 
            this._optionsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._optionsList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._optionsList.CheckBoxes = true;
            this._optionsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this._optionsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this._optionsList.HideSelection = false;
            this._optionsList.Location = new System.Drawing.Point(12, 307);
            this._optionsList.Name = "_optionsList";
            this._optionsList.Size = new System.Drawing.Size(430, 112);
            this._optionsList.TabIndex = 16;
            this._optionsList.UseCompatibleStateImageBehavior = false;
            this._optionsList.View = System.Windows.Forms.View.Details;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(214)))), ((int)(((byte)(214)))));
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.statusLabel.Location = new System.Drawing.Point(3, 571);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(52, 13);
            this.statusLabel.TabIndex = 7;
            this.statusLabel.Text = "Initializing";
            // 
            // authorLabel
            // 
            this.authorLabel.AutoSize = true;
            this.authorLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.authorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.authorLabel.ForeColor = System.Drawing.Color.White;
            this.authorLabel.Location = new System.Drawing.Point(329, 34);
            this.authorLabel.Name = "authorLabel";
            this.authorLabel.Size = new System.Drawing.Size(113, 20);
            this.authorLabel.TabIndex = 17;
            this.authorLabel.Text = "fOmey @ XDA";
            // 
            // patchOptionsLabel
            // 
            this.patchOptionsLabel.AutoSize = true;
            this.patchOptionsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(214)))), ((int)(((byte)(214)))));
            this.patchOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patchOptionsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.patchOptionsLabel.Location = new System.Drawing.Point(12, 272);
            this.patchOptionsLabel.Name = "patchOptionsLabel";
            this.patchOptionsLabel.Size = new System.Drawing.Size(109, 20);
            this.patchOptionsLabel.TabIndex = 18;
            this.patchOptionsLabel.Text = "Patch Options";
            // 
            // customSourceLabel
            // 
            this.customSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.customSourceLabel.AutoSize = true;
            this.customSourceLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(214)))), ((int)(((byte)(214)))));
            this.customSourceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customSourceLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.customSourceLabel.Location = new System.Drawing.Point(12, 434);
            this.customSourceLabel.Name = "customSourceLabel";
            this.customSourceLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.customSourceLabel.Size = new System.Drawing.Size(265, 20);
            this.customSourceLabel.TabIndex = 19;
            this.customSourceLabel.Text = "Custom Source (/system/framework)";
            // 
            // frameworkBox
            // 
            this.frameworkBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frameworkBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.frameworkBox.Depth = 0;
            this.frameworkBox.Font = new System.Drawing.Font("Roboto", 12F);
            this.frameworkBox.Location = new System.Drawing.Point(12, 472);
            this.frameworkBox.MaxLength = 50;
            this.frameworkBox.MouseState = MaterialSkin.MouseState.OUT;
            this.frameworkBox.Multiline = false;
            this.frameworkBox.Name = "frameworkBox";
            this.frameworkBox.Size = new System.Drawing.Size(342, 36);
            this.frameworkBox.TabIndex = 20;
            this.frameworkBox.Text = "";
            this.frameworkBox.UseTallSize = false;
            // 
            // frameworkBrowseButton
            // 
            this.frameworkBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.frameworkBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.frameworkBrowseButton.Depth = 0;
            this.frameworkBrowseButton.DrawShadows = true;
            this.frameworkBrowseButton.HighEmphasis = false;
            this.frameworkBrowseButton.Icon = null;
            this.frameworkBrowseButton.Location = new System.Drawing.Point(361, 472);
            this.frameworkBrowseButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.frameworkBrowseButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.frameworkBrowseButton.Name = "frameworkBrowseButton";
            this.frameworkBrowseButton.Size = new System.Drawing.Size(80, 36);
            this.frameworkBrowseButton.TabIndex = 21;
            this.frameworkBrowseButton.Text = "Browse";
            this.frameworkBrowseButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.frameworkBrowseButton.UseAccentColor = false;
            this.frameworkBrowseButton.UseVisualStyleBackColor = true;
            this.frameworkBrowseButton.Click += new System.EventHandler(this.frameworkBrowseButton_Click);
            // 
            // patchButton
            // 
            this.patchButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.patchButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.patchButton.Depth = 0;
            this.patchButton.DrawShadows = true;
            this.patchButton.HighEmphasis = true;
            this.patchButton.Icon = null;
            this.patchButton.Location = new System.Drawing.Point(176, 517);
            this.patchButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.patchButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.patchButton.Name = "patchButton";
            this.patchButton.Size = new System.Drawing.Size(102, 36);
            this.patchButton.TabIndex = 22;
            this.patchButton.Text = "ADB Patch";
            this.patchButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.patchButton.UseAccentColor = false;
            this.patchButton.UseVisualStyleBackColor = true;
            this.patchButton.Click += new System.EventHandler(this.patchButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 589);
            this.Controls.Add(this.patchButton);
            this.Controls.Add(this.frameworkBrowseButton);
            this.Controls.Add(this.frameworkBox);
            this.Controls.Add(this.customSourceLabel);
            this.Controls.Add(this.patchOptionsLabel);
            this.Controls.Add(this.authorLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this._optionsList);
            this.Controls.Add(this.bottomBar);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.frameworkDivider);
            this.Controls.Add(this.materialDivider1);
            this.Name = "MainForm";
            this.Text = "Smali Patcher";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialDivider frameworkDivider;
        public MaterialSkin.Controls.MaterialMultiLineTextBox logBox;
        private MaterialSkin.Controls.MaterialDivider bottomBar;
        private System.Windows.Forms.ListView _optionsList;
        public System.Windows.Forms.Label statusLabel;
        private Label authorLabel;
        private Label patchOptionsLabel;
        private Label customSourceLabel;
        private MaterialSkin.Controls.MaterialTextBox frameworkBox;
        private MaterialSkin.Controls.MaterialButton frameworkBrowseButton;
        private MaterialSkin.Controls.MaterialButton patchButton;
    }
}

