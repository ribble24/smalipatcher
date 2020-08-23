using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmaliPatcher.Properties;

namespace SmaliPatcher
{
    public class MainForm : MaterialForm
    {
        private readonly Adb _adb;
        private MaterialFlatButton _browseButton;
        private MaterialSingleLineTextField _browseTextbox;
        private readonly Check _check;
        private IContainer components;
        private MaterialDivider _debugDivider;
        private RichTextBox _debugInfo;
        private Label _devLabel;
        private readonly Download _download;
        private Label _infoLabel;
        private Thread _infoThread;
        private Label _label1;
        private MaterialDivider _optionsDivider;
        private ListView _optionsList;
        private readonly Patch _patch;
        private MaterialRaisedButton _patchButton;
        public List<Patches> Patches = new List<Patches>();
        private PictureBox _paypalButton;
        private MaterialDivider _servicejarDivider;
        private Label _servicejarLabel;
        public bool SimulateAdb;
        public bool SkipCleanUp;
        private MaterialDivider _statusDivider;
        private Label _statusText;
        public Thread WorkerThread;

        public MainForm()
        {
            InitializeComponent();
            _download = new Download();
            _download.Init(this);
            this._patch = new Patch();
            this._patch.Init(this);
            _adb = new Adb();
            _adb.Init(this);
            _check = new Check();
            _check.Init(this);
            _debugInfo.AppendText("fOmey @ XDA\n");
            _debugInfo.AppendText("Patcher version: " + Application.ProductVersion);
            Patches.Add(new Patches(true, "Mock locations", "Treat mock locations as genuine location updates.",
                "services.jar"));
            Patches.Add(new Patches(false, "Secure flag", "Allow screenshots/screensharing in secure apps.",
                "services.jar"));
            Patches.Add(new Patches(false, "Signature verification", "Disable apk signature verification.",
                "services.jar"));
            Patches.Add(new Patches(false, "Signature spoofing", "Allow app signature spoofing permission.",
                "services.jar"));
            Patches.Add(new Patches(false, "Recovery reboot", "Reboot directly back into recovery from powermenu.",
                "services.jar"));
            Patches.Add(new Patches(false, "Samsung Knox", "Bypass Samsung knox-trip protection (secure folder).",
                "services.jar"));
            Patches.Add(new Patches(true, "High volume warning", "Disable high volume popup dialog.", "services.jar"));
            _optionsList.Columns.Add("", -2);
            _optionsList.Columns.Add("", -2);
            _optionsList.Columns.Add("", -2);
            foreach (Patches patch in Patches)
            {
                _optionsList.Items.Add("").SubItems.AddRange(new string[2]
                {
                    patch.PatchTitle,
                    patch.PatchDescription
                });
                _optionsList.Items[_optionsList.Items.Count - 1].Checked = patch.Status;
            }
        }

        private void DisplayHint(string text, int timeOut)
        {
            Invoke((Action) (() =>
            {
                _infoLabel.Text = text;
                _infoLabel.Visible = true;
            }));
            Thread.Sleep(timeOut);
            Invoke((Action) (() => _infoLabel.Visible = false));
        }

        public void HintUpdate(string text)
        {
            try
            {
                Invoke((Action) (() =>
                {
                    _infoThread = new Thread(() => DisplayHint(text, 5000));
                    _infoThread.Start();
                }));
            }
            catch
            {
            }
        }

        public void DebugUpdate(string text)
        {
            try
            {
                Invoke((Action) (() =>
                {
                    _debugInfo.AppendText(text);
                    _debugInfo.ScrollToCaret();
                }));
            }
            catch
            {
            }
        }

        public void StatusUpdate(string text)
        {
            try
            {
                Invoke((Action) (() => _statusText.Text = text));
            }
            catch
            {
            }
        }

        public void DisableControls()
        {
            try
            {
                Invoke((Action) (() =>
                {
                    _patchButton.Enabled = false;
                    _browseTextbox.Enabled = false;
                    _browseButton.Enabled = false;
                }));
            }
            catch
            {
            }
        }

        public void EnableControls()
        {
            try
            {
                Invoke((Action) (() =>
                {
                    _patchButton.Enabled = true;
                    _browseTextbox.Enabled = true;
                    _browseButton.Enabled = true;
                }));
            }
            catch
            {
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;
            if (commonOpenFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;
            _browseTextbox.Text = commonOpenFileDialog.FileName;
            for (int index = 0; index < Patches.Count(); ++index)
                Patches[index] = new Patches(false, Patches[index].PatchTitle, Patches[index].PatchDescription,
                    Patches[index].TargetFile);
            for (int index1 = 0; index1 < _optionsList.Items.Count; ++index1)
            for (int index2 = 0; index2 < Patches.Count(); ++index2)
                if (Patches[index2].PatchTitle == _optionsList.Items[index1].SubItems[1].Text)
                    Patches[index2] = new Patches(_optionsList.Items[index1].Checked, Patches[index2].PatchTitle,
                        Patches[index2].PatchDescription, Patches[index2].TargetFile);
            _optionsList.Items.Clear();
            foreach (Patches patch in Patches)
            foreach (string file in Directory.GetFiles(commonOpenFileDialog.FileName, patch.TargetFile,
                SearchOption.AllDirectories))
                if (Path.GetFileName(file) == patch.TargetFile)
                {
                    _optionsList.Items.Add("").SubItems.AddRange(new string[2]
                    {
                        patch.PatchTitle,
                        patch.PatchDescription
                    });
                    _optionsList.Items[_optionsList.Items.Count - 1].Checked = patch.Status;
                }
        }

        private void browseTextbox_TextChanged(object sender, EventArgs e)
        {
            if (_browseTextbox.Text.Length > 0)
                _patchButton.Text = "PATCH";
            if (_browseTextbox.Text.Length != 0)
                return;
            _patchButton.Text = "ADB PATCH";
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            StatusUpdate("Executing prepatch checks..");
            if (!_check.CheckJava())
                return;
            string path = _browseTextbox.Text;
            for (int index = 0; index < Patches.Count(); ++index)
                Patches[index] = new Patches(false, Patches[index].PatchTitle, Patches[index].PatchDescription,
                    Patches[index].TargetFile);
            int num1 = 0;
            for (int index1 = 0; index1 < _optionsList.Items.Count; ++index1)
            {
                bool s = _optionsList.Items[index1].Checked;
                if (s)
                    ++num1;
                for (int index2 = 0; index2 < Patches.Count(); ++index2)
                    if (Patches[index2].PatchTitle == _optionsList.Items[index1].SubItems[1].Text)
                        Patches[index2] = new Patches(s, Patches[index2].PatchTitle, Patches[index2].PatchDescription,
                            Patches[index2].TargetFile);
            }
            if (num1 == 0)
            {
                DebugUpdate("\n!!! ERROR: No patches selected, you must select atleast one.");
                StatusUpdate("ERROR..");
            }
            else
            {
                int num2 = _check.CheckAdb();
                switch (num2)
                {
                    case -1:
                        DebugUpdate("\n!!! ERROR: Unauthorized ADB device found.");
                        HintUpdate("Have you accepted the \"Allow USB debugging\" popup on your phone?");
                        StatusUpdate("ERROR..");
                        break;
                    case 0:
                        DebugUpdate("\n!!! ERROR: No ADB devices found.");
                        HintUpdate("Is your phone plugged into your PC?");
                        StatusUpdate("ERROR..");
                        break;
                    default:
                        if (num2 > 1)
                        {
                            DebugUpdate("\n!!! ERROR: Multiple ADB devices found.");
                            StatusUpdate("ERROR..");
                        }
                        break;
                }
                if (path.Length == 0 && num2 == 1)
                {
                    WorkerThread = new Thread(() => _adb.PullFileset());
                    WorkerThread.Start();
                }
                if (path.Length <= 0 || num2 != 1)
                    return;
                WorkerThread = new Thread(() => _patch.ProcessFrameworkDirectory(path));
                WorkerThread.Start();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            _check.CheckAdministrator();
            WorkerThread = new Thread(() => _download.DownloadBinary());
            WorkerThread.Start();
        }

        private void paypalButton_Click(object sender, EventArgs e) => Process.Start(
            "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=pauly.galea@gmail.com&item_name=fOmey");

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MainForm));
            _debugDivider = new MaterialDivider();
            _debugInfo = new RichTextBox();
            _servicejarDivider = new MaterialDivider();
            _browseButton = new MaterialFlatButton();
            _patchButton = new MaterialRaisedButton();
            _browseTextbox = new MaterialSingleLineTextField();
            _devLabel = new Label();
            _servicejarLabel = new Label();
            _statusDivider = new MaterialDivider();
            _statusText = new Label();
            _optionsDivider = new MaterialDivider();
            _label1 = new Label();
            _optionsList = new ListView();
            _paypalButton = new PictureBox();
            _infoLabel = new Label();
            ((ISupportInitialize) _paypalButton).BeginInit();
            SuspendLayout();
            _debugDivider.BackColor = Color.FromArgb(224, 224, 224);
            _debugDivider.Depth = 0;
            _debugDivider.Location = new Point(8, 69);
            _debugDivider.MouseState = MouseState.OUT;
            _debugDivider.Name = "_debugDivider";
            _debugDivider.Size = new Size(402, 145);
            _debugDivider.TabIndex = 0;
            _debugDivider.Text = "logDivider";
            _debugInfo.BackColor = Color.FromArgb(224, 224, 224);
            _debugInfo.BorderStyle = BorderStyle.None;
            _debugInfo.Font = new Font("Microsoft Sans Serif", 7f);
            _debugInfo.Location = new Point(13, 74);
            _debugInfo.Name = "_debugInfo";
            _debugInfo.ReadOnly = true;
            _debugInfo.Size = new Size(392, 135);
            _debugInfo.TabIndex = 1;
            _debugInfo.Text = "";
            _servicejarDivider.BackColor = Color.FromArgb(55, 71, 79);
            _servicejarDivider.Depth = 0;
            _servicejarDivider.Location = new Point(0, 379);
            _servicejarDivider.MouseState = MouseState.OUT;
            _servicejarDivider.Name = "_servicejarDivider";
            _servicejarDivider.Size = new Size(419, 40);
            _servicejarDivider.TabIndex = 2;
            _servicejarDivider.Text = "servicesJarDivider";
            _browseButton.AutoSize = true;
            _browseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _browseButton.Depth = 0;
            _browseButton.Image = null;
            _browseButton.Location = new Point(326, 428);
            _browseButton.Margin = new Padding(4, 6, 4, 6);
            _browseButton.MouseState = MouseState.OUT;
            _browseButton.Name = "_browseButton";
            _browseButton.Primary = false;
            _browseButton.Size = new Size(83, 36);
            _browseButton.TabIndex = 4;
            _browseButton.Text = "Browse..";
            _browseButton.UseVisualStyleBackColor = true;
            _browseButton.Click += browseButton_Click;
            _patchButton.AutoSize = true;
            _patchButton.Padding = new Padding(5);
            _patchButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _patchButton.Depth = 0;
            _patchButton.Image = null;
            _patchButton.Location = new Point(162, 473);
            _patchButton.MouseState = MouseState.OUT;
            _patchButton.Name = "_patchButton";
            _patchButton.Primary = true;
            _patchButton.Size = new Size(96, 36);
            _patchButton.TabIndex = 6;
            _patchButton.Text = "ADB PATCH";
            _patchButton.UseVisualStyleBackColor = true;
            _patchButton.Click += patchButton_Click;
            _browseTextbox.Depth = 0;
            _browseTextbox.Hint = "";
            _browseTextbox.Location = new Point(9, 434);
            _browseTextbox.MouseState = MouseState.OUT;
            _browseTextbox.Name = "_browseTextbox";
            _browseTextbox.SelectedText = "";
            _browseTextbox.SelectionLength = 0;
            _browseTextbox.SelectionStart = 0;
            _browseTextbox.Size = new Size(310, 23);
            _browseTextbox.TabIndex = 7;
            _browseTextbox.TabStop = false;
            _browseTextbox.UseSystemPasswordChar = false;
            _browseTextbox.TextChanged += browseTextbox_TextChanged;
            _devLabel.AutoSize = true;
            _devLabel.BackColor = Color.FromArgb(55, 71, 79);
            _devLabel.Font = new Font("Microsoft Sans Serif", 11f);
            _devLabel.ForeColor = Color.White;
            _devLabel.Location = new Point(268, 34);
            _devLabel.Name = "_devLabel";
            _devLabel.Size = new Size(105, 18);
            _devLabel.TabIndex = 10;
            _devLabel.Text = "fOmey @ XDA";
            _servicejarLabel.AutoSize = true;
            _servicejarLabel.BackColor = Color.FromArgb(55, 71, 79);
            _servicejarLabel.Font = new Font("Microsoft Sans Serif", 11f);
            _servicejarLabel.ForeColor = Color.White;
            _servicejarLabel.Location = new Point(12, 390);
            _servicejarLabel.Name = "_servicejarLabel";
            _servicejarLabel.Size = new Size(135, 18);
            _servicejarLabel.TabIndex = 11;
            _servicejarLabel.Text = "/system/framework";
            _statusDivider.BackColor = Color.FromArgb(38, 50, 56);
            _statusDivider.Depth = 0;
            _statusDivider.Location = new Point(0, 522);
            _statusDivider.MouseState = MouseState.OUT;
            _statusDivider.Name = "_statusDivider";
            _statusDivider.Size = new Size(419, 20);
            _statusDivider.TabIndex = 12;
            _statusDivider.Text = "servicesJarDivider";
            _statusText.AutoSize = true;
            _statusText.BackColor = Color.FromArgb(38, 50, 56);
            _statusText.Font = new Font("Microsoft Sans Serif", 8f);
            _statusText.ForeColor = Color.White;
            _statusText.Location = new Point(5, 525);
            _statusText.Name = "_statusText";
            _statusText.Size = new Size(30, 13);
            _statusText.TabIndex = 13;
            _statusText.Text = "Idle..";
            _optionsDivider.BackColor = Color.FromArgb(55, 71, 79);
            _optionsDivider.Depth = 0;
            _optionsDivider.Location = new Point(0, 219);
            _optionsDivider.MouseState = MouseState.OUT;
            _optionsDivider.Name = "_optionsDivider";
            _optionsDivider.Size = new Size(419, 40);
            _optionsDivider.TabIndex = 14;
            _label1.AutoSize = true;
            _label1.BackColor = Color.FromArgb(55, 71, 79);
            _label1.Font = new Font("Microsoft Sans Serif", 11f);
            _label1.ForeColor = Color.White;
            _label1.Location = new Point(12, 230);
            _label1.Name = "_label1";
            _label1.Size = new Size(102, 18);
            _label1.TabIndex = 15;
            _label1.Text = "Patch Options";
            _optionsList.BorderStyle = BorderStyle.None;
            _optionsList.CheckBoxes = true;
            _optionsList.Font = new Font("Microsoft Sans Serif", 7f);
            _optionsList.HeaderStyle = ColumnHeaderStyle.None;
            _optionsList.Location = new Point(8, 263);
            _optionsList.Name = "_optionsList";
            _optionsList.Size = new Size(402, 112);
            _optionsList.TabIndex = 16;
            _optionsList.UseCompatibleStateImageBehavior = false;
            _optionsList.View = View.Details;
            _paypalButton.BackColor = Color.FromArgb(55, 71, 79);
            _paypalButton.Image = Resources.Paypal;
            _paypalButton.ImageLocation = "";
            _paypalButton.Location = new Point(380, 29);
            _paypalButton.Name = "_paypalButton";
            _paypalButton.Size = new Size(24, 28);
            _paypalButton.SizeMode = PictureBoxSizeMode.AutoSize;
            _paypalButton.TabIndex = 17;
            _paypalButton.TabStop = false;
            _paypalButton.Click += paypalButton_Click;
            _infoLabel.BackColor = Color.FromArgb(55, 71, 79);
            _infoLabel.Font = new Font("Microsoft Sans Serif", 7f);
            _infoLabel.ForeColor = Color.White;
            _infoLabel.Location = new Point(10, 193);
            _infoLabel.Name = "_infoLabel";
            _infoLabel.Size = new Size(398, 19);
            _infoLabel.TabIndex = 0;
            _infoLabel.Text = "Info label";
            _infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            _infoLabel.Visible = false;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(419, 542);
            Controls.Add(_infoLabel);
            Controls.Add(_paypalButton);
            Controls.Add(_optionsList);
            Controls.Add(_label1);
            Controls.Add(_optionsDivider);
            Controls.Add(_statusText);
            Controls.Add(_statusDivider);
            Controls.Add(_servicejarLabel);
            Controls.Add(_devLabel);
            Controls.Add(_browseTextbox);
            Controls.Add(_patchButton);
            Controls.Add(_browseButton);
            Controls.Add(_servicejarDivider);
            Controls.Add(_debugInfo);
            Controls.Add(_debugDivider);
            Font = new Font("Microsoft Sans Serif", 8.25f);
            Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = nameof(MainForm);
            Text = "Smali Patcher.JF";
            Shown += MainForm_Shown;
            ((ISupportInitialize) _paypalButton).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}