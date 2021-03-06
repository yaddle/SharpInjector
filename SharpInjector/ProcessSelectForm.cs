﻿using System;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using MetroFramework;

namespace SharpInjector
{
    public struct ProcessContainer
    {
        public string Name;
        public Process Process;
        public int ImageIndex;
        public bool HasWindow;
        public bool IsWow64;
    }

    public partial class ProcessSelectForm : Form
    {
        private enum Filter
        {
            Window,
            All
        }

        private List<ProcessContainer> process_ids = new List<ProcessContainer>();

        private ImageList image_list = new ImageList { ImageSize = new Size(24, 24) };

        private static Thread Form_Loading_Thread { get; set; }

        public ProcessSelectForm()
        {
            InitializeComponent();
        }

        #region Custom Design

        private void Header_Background_Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            Extra.Imports.ReleaseCapture();
            Extra.Imports.SendMessage(Handle, Extra.Drag.WM_NCLBUTTONDOWN, Extra.Drag.HT_CAPTION, 0);
        }

        private void Header_Line_Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            Extra.Imports.ReleaseCapture();
            Extra.Imports.SendMessage(Handle, Extra.Drag.WM_NCLBUTTONDOWN, Extra.Drag.HT_CAPTION, 0);
        }

        private void Header_Title_Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            Extra.Imports.ReleaseCapture();
            Extra.Imports.SendMessage(Handle, Extra.Drag.WM_NCLBUTTONDOWN, Extra.Drag.HT_CAPTION, 0);
        }

        private void Header_Close_Label_Click(object sender, EventArgs e) => Close();
        private void Header_Close_Label_MouseEnter(object sender, EventArgs e) => Header_Close_Label.ForeColor = Color.LightGray;
        private void Header_Close_Label_MouseLeave(object sender, EventArgs e) => Header_Close_Label.ForeColor = Color.White;
        private void Header_Minimize_Label_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;
        private void Header_Minimize_Label_MouseEnter(object sender, EventArgs e) => Header_Minimize_Label.ForeColor = Color.LightGray;
        private void Header_Minimize_Label_MouseLeave(object sender, EventArgs e) => Header_Minimize_Label.ForeColor = Color.White;

        #endregion

        private void ProcessSelectForm_Load(object sender, EventArgs e)
        {
            image_list.ColorDepth = ColorDepth.Depth32Bit;

            Process_ListView.Columns.Add("", Process_ListView.Width - 20, HorizontalAlignment.Left);
            Process_ListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
            Process_ListView.SmallImageList = image_list;

            Task.Factory.StartNew(() =>
            {
                Form_Loading_Thread = Thread.CurrentThread;

                foreach (Process process in Process.GetProcesses())
                {
                    if (process.Id <= 0) continue;

                    try
                    {
                        image_list.Images.Add(Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap());

                        // man sollte im 32b modus ja eh nur 32b programme finden?
                        //bool is_64b_application = Extra.NativeMethods.IsWin64Emulator(process);

                        process_ids.Add(new ProcessContainer
                        {
                            HasWindow = !string.IsNullOrEmpty(process.MainWindowTitle),
                            ImageIndex = image_list.Images.Count - 1,
                            //Name = $"{process.Id.ToString().PadLeft(6, '0')} - ({(is_64b_application ? "x64" : "x32")}) - {process.ProcessName.ToLower()}",
                            Name = $"{process.Id.ToString().PadLeft(6, '0')} - (x32) - {process.ProcessName.ToLower()}",
                            Process = process,
                            //IsWow64 = is_64b_application
                            IsWow64 = false
                        });
                    }
                    catch (Exception)
                    {
                        //ImgList.Images.Add(Resources._default);
                    }
                }

                Process_ListView.Invoke(new MethodInvoker(() => RefreshList(Filter.All, process_ids)));

                Invoke((MethodInvoker)(() =>
                {
                    Window_List_Button.Enabled = true;
                    Process_List_Button.Enabled = true;
                }));

                Form_Loading_Thread = null;
            });
        }

        private void SearchTextbox_TextChanged(object sender, EventArgs e)
        {
            RefreshList(Filter.All, process_ids.Where(x => x.Name.Contains(SearchTextbox.Text.ToLower())).ToList());
        }

        private void Process_List_Button_Click(object sender, EventArgs e)
        {
            if (process_ids.Count == 0)
            {
                MetroMessageBox.Show(this, "ProcessID List is empty", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error, 115);
                return;
            }
            RefreshList(Filter.All, process_ids);
        }

        private void Window_List_Button_Click(object sender, EventArgs e)
        {
            if (process_ids.Count(x => x.HasWindow) == 0)
            {
                MetroMessageBox.Show(this, "No Windows found", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error, 115);
                return;
            }
            RefreshList(Filter.Window, process_ids);
        }

        private void Select_Button_Click(object sender, EventArgs e)
        {
            if (Process_ListView.SelectedItems.Count < 1)
            {
                MetroMessageBox.Show(this, "Select something first", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error, 115);
                return;
            }

            ProcessContainer selected_process = process_ids.FirstOrDefault(x => x.Name == Process_ListView.SelectedItems[0].Name);
            if (selected_process.Process == null)
            {
                MetroMessageBox.Show(this, "Could not find your selected Process", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error, 115);
                return;
            }

            Globals.Selected_Process = selected_process.Process;

            Close();
        }

        private void Close_Button_Click(object sender, EventArgs e) => Close();

        private void ProcessSelectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Form_Loading_Thread != null && Form_Loading_Thread.IsAlive) Form_Loading_Thread.Abort();
        }

        private void RefreshList(Filter filter, List<ProcessContainer> list)
        {
            if (Process_ListView.Items.Count > 0) Process_ListView.Items.Clear();

            list.ForEach(x =>
            {
                if (filter == Filter.All)
                {
                    ListViewItem listViewItem = new ListViewItem
                    {
                        Name = x.Name,
                        ImageIndex = x.ImageIndex,
                        Text = x.Name
                    };
                    Process_ListView.Items.Add(listViewItem);
                }

                if (filter == Filter.Window && x.HasWindow)
                {
                    ListViewItem listViewItem = new ListViewItem
                    {
                        Name = x.Name,
                        ImageIndex = x.ImageIndex,
                        Text = x.Name
                    };
                    Process_ListView.Items.Add(listViewItem);
                }
            });
        }
    }
}
