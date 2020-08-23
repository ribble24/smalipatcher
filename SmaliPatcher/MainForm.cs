using MaterialSkin.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmaliLib;
using SmaliLib.Patches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SmaliPatcher
{
    public partial class MainForm : MaterialForm
    {
        SmaliLibMain lib;
        IPlatform platform;
        Dictionary<IPatch, bool> patches = new Dictionary<IPatch, bool>();
        public bool setStatus = false;

        public MainForm()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            platform = new Platform(this);
            lib = new SmaliLibMain(platform);
            InitializeComponent();
            logBox.Text += lib.GetVersion().ToString();
            _optionsList.Columns.Add("", -2);
            _optionsList.Columns.Add("", -2);
            _optionsList.Columns.Add("", -2);
            foreach (IPatch patch in lib.GetPatches())
            {
                patches.Add(patch, patch is MockLocations || patch is HighVolumeWarning);
                _optionsList.Items.Add("").SubItems.AddRange(new string[2]
                {
                    patch.Title,
                    patch.Description
                });
                _optionsList.Items[_optionsList.Items.Count - 1].Checked = patches[patch];
            }

            _optionsList.ItemCheck += (sender, e) =>
            {
                patches[patches.Keys.ElementAt(e.Index)] = e.NewValue == CheckState.Checked;
            };

            new Thread(() =>
            {
                Invoke((Action)(() =>
                {
                    UpdateAvailable(false);
                }));
                if (!lib.CheckResources())
                    lib.DownloadDeps();
                int status = lib.CheckAdb();
                switch (status)
                {
                    case -1:
                        platform.ErrorCritical("The connected device is not authorized. Have you accepted the popup?");
                        break;
                    case 0:
                        platform.ErrorCritical("No device found! Make sure it is connected and has ADB enabled");
                        break;
                    default:
                        if (status > 1)
                            platform.ErrorCritical("Too many devices connected. Use one only!");
                        else
                        {
                            Invoke((Action)(() =>
                            {
                                statusLabel.Text = "Idle";
                                UpdateAvailable(true);
                            }));
                            platform.Log("Initialized!");
                        }
                        break;
                }
            }).Start();
        }

        public void UpdateAvailable(bool available)
        {
            patchButton.Enabled = available;
        }

        private void frameworkBrowseButton_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;
            if (commonOpenFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;
            frameworkBox.Text = commonOpenFileDialog.FileName;
            _optionsList.Items.Clear();
            foreach (IPatch patch in patches.Keys)
            {
                patches[patch] = false;
                _optionsList.Items.Add("").SubItems.AddRange(new string[2]
                {
                    patch.Title,
                    patch.Description
                });
                _optionsList.Items[_optionsList.Items.Count - 1].Checked = patches[patch];
            }
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            UpdateAvailable(false);
            statusLabel.Text = "Initializing";
            string framework = frameworkBox.Text;
            new Thread(() =>
            {
                setStatus = true;
                bool pullFramework = string.IsNullOrWhiteSpace(framework);
                framework = pullFramework ? lib.DumpFramework() : framework;
                if (!Directory.Exists(framework))
                {
                    platform.ErrorCritical("The specified path doesn't exist");
                }
                else
                {
                    IPatch[] selected = patches.Where(s => s.Value).Select(s => s.Key).ToArray();
                    lib.PatchFramework(framework, selected);
                    lib.PackModule(selected, false, pullFramework);
                }
                setStatus = false;
                Invoke((Action)(() =>
                {
                    statusLabel.Text = "Idle";
                    UpdateAvailable(true);
                }));
            }).Start();
        }
    }
}
