﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Braincase.GanttChart
{
    /// <summary>
    /// An elaborate example on how the chart control might be used. 
    /// Start by collapsing all regions and then read the constructor.
    /// Refer to IProjectManager interface for method description.
    /// </summary>
    public partial class ExampleFull : Form
    {
        OverlayPainter _mOverlay = new OverlayPainter();

        ProjectManager _mManager = null;
        TaskFormat TkFormat;
        TaskFormat CTFormat;
        HeaderFormat HFormat;

        /// <summary>
        /// Example starts here
        /// </summary>
        public ExampleFull()
        {
            InitializeComponent();

            // Create a Project and some Tasks
            _mManager = new ProjectManager();
            var work = new MyTask(_mManager) { Name = "Prepare for Work" };
            var wake = new MyTask(_mManager) { Name = "Wake Up" };
            var teeth = new MyTask(_mManager) { Name = "Brush Teeth" };
            var shower = new MyTask(_mManager) { Name = "Shower" };
            var clothes = new MyTask(_mManager) { Name = "Change into New Clothes" };
            var hair = new MyTask(_mManager) { Name = "Blow My Hair" };
            var pack = new MyTask(_mManager) { Name = "Pack the Suitcase" };

            _mManager.Add(work);
            _mManager.Add(wake);
            _mManager.Add(teeth);
            _mManager.Add(shower);
            _mManager.Add(clothes);
            _mManager.Add(hair);
            _mManager.Add(pack);

            // Create another 1000 tasks for stress testing
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var task = new MyTask(_mManager) { Name = string.Format("New Task {0}", i.ToString()) };
                _mManager.Add(task);
                _mManager.SetStart(task, TimeSpan.FromDays(rand.Next(300)));
                _mManager.SetDuration(task, TimeSpan.FromDays(rand.Next(50)));
            }

            // Set task durations, e.g. using ProjectManager methods 
            _mManager.SetDuration(wake, TimeSpan.FromDays(3));
            _mManager.SetDuration(teeth, TimeSpan.FromDays(5));
            _mManager.SetDuration(shower, TimeSpan.FromDays(7));
            _mManager.SetDuration(clothes, TimeSpan.FromDays(4));
            _mManager.SetDuration(hair, TimeSpan.FromDays(3));
            _mManager.SetDuration(pack, TimeSpan.FromDays(5));

            // demostrate splitting a task
            _mManager.Split(pack, new MyTask(_mManager), new MyTask(_mManager), TimeSpan.FromDays(2));

            // Set task complete status, e.g. using newly created properties
            wake.Complete = 0.9f;
            teeth.Complete = 0.5f;
            shower.Complete = 0.4f;

            // Give the Tasks some organisation, setting group and precedents
            _mManager.Group(work, wake);
            _mManager.Group(work, teeth);
            _mManager.Group(work, shower);
            _mManager.Group(work, clothes);
            _mManager.Group(work, hair);
            _mManager.Group(work, pack);
            _mManager.Relate(wake, teeth);
            _mManager.Relate(wake, shower);
            _mManager.Relate(shower, clothes);
            _mManager.Relate(shower, hair);
            _mManager.Relate(hair, pack);
            _mManager.Relate(clothes, pack);

            // Create and assign Resources.
            // MyResource is just custom user class. The API can accept any object as resource.
            var jake = new MyResource() { Name = "Jake" };
            var peter = new MyResource() { Name = "Peter" };
            var john = new MyResource() { Name = "John" };
            var lucas = new MyResource() { Name = "Lucas" };
            var james = new MyResource() { Name = "James" };
            var mary = new MyResource() { Name = "Mary" };
            // Add some resources
            _mManager.Assign(wake, jake);
            _mManager.Assign(wake, peter);
            _mManager.Assign(wake, john);
            _mManager.Assign(teeth, jake);
            _mManager.Assign(teeth, james);
            _mManager.Assign(pack, james);
            _mManager.Assign(pack, lucas);
            _mManager.Assign(shower, mary);
            _mManager.Assign(shower, lucas);
            _mManager.Assign(shower, john);

            // Initialize the Chart with our ProjectManager and CreateTaskDelegate
            _mChart.Init(_mManager);
            _mChart.CreateTaskDelegate = delegate() { return new MyTask(_mManager); };

            // Attach event listeners for events we are interested in
            _mChart.TaskMouseOver += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOver);
            _mChart.TaskMouseOut += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOut);
            _mChart.TaskSelected += new EventHandler<TaskMouseEventArgs>(_mChart_TaskSelected);
            _mChart.PaintOverlay += _mOverlay.ChartOverlayPainter;
            _mChart.AllowTaskDragDrop = true;

            // set some tooltips to show the resources in each task
            _mChart.SetToolTip(wake, string.Join(", ", _mManager.ResourcesOf(wake).Select(x => (x as MyResource).Name)));
            _mChart.SetToolTip(teeth, string.Join(", ", _mManager.ResourcesOf(teeth).Select(x => (x as MyResource).Name)));
            _mChart.SetToolTip(pack, string.Join(", ", _mManager.ResourcesOf(pack).Select(x => (x as MyResource).Name)));
            _mChart.SetToolTip(shower, string.Join(", ", _mManager.ResourcesOf(shower).Select(x => (x as MyResource).Name)));

            // Set Time information
            var span = DateTime.Today - _mManager.Start;
            _mManager.Now = span; // set the "Now" marker at the correct date
            _mChart.TimeResolution = TimeResolution.Day; // Set the chart to display in days in header

            // Init the rest of the UI
            _InitExampleUI();            
        }
        void DefaultColors()
        {
            
            btnTaskColor.BackColor = ((SolidBrush)_mChart.TaskFormat.Color).Color;
            btnTaskBorder.BackColor = _mChart.TaskFormat.Border.Color;
            btnTaskBackFill.BackColor = ((SolidBrush)_mChart.TaskFormat.BackFill).Color;
            btnTaskForeFill.BackColor = ((SolidBrush)_mChart.TaskFormat.ForeFill).Color;
           // btnTaskSlackFill.BackColor = ((SolidBrush)_mChart.TaskFormat.SlackFill).Color;

            btnCriticalTaskColor.BackColor = ((SolidBrush)_mChart.CriticalTaskFormat.Color).Color;
            btnCriticalTaskBorder.BackColor = _mChart.CriticalTaskFormat.Border.Color;
            btnCriticalTaskBackFill.BackColor = ((SolidBrush)_mChart.CriticalTaskFormat.BackFill).Color;
            btnCriticalTaskForeFill.BackColor = ((SolidBrush)_mChart.CriticalTaskFormat.ForeFill).Color;
            //btnCriticalTaskSlackFill.BackColor = ((SolidBrush)_mChart.CriticalTaskFormat.SlackFill).Color;

            btnHeaderColor.BackColor = ((SolidBrush)_mChart.HeaderFormat.Color).Color;
            btnHeaderBorder.BackColor = _mChart.HeaderFormat.Border.Color;
            btnHeaderGradientLight.BackColor = _mChart.HeaderFormat.GradientLight;
            btnHeaderGradientDark.BackColor = _mChart.HeaderFormat.GradientDark;
            

        }
        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            _mTaskGrid.SelectedObjects = _mChart.SelectedTasks.Select(x => _mManager.IsPart(x) ? _mManager.SplitTaskOf(x) : x).ToArray();
            _mResourceGrid.Items.Clear();
            _mResourceGrid.Items.AddRange(_mManager.ResourcesOf(e.Task).Select(x => new ListViewItem(((MyResource)x).Name)).ToArray());
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = "";
            _mChart.Invalidate();
            
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = string.Format("{0} to {1}", _mManager.GetDateTime(e.Task.Start).ToLongDateString(), _mManager.GetDateTime(e.Task.End).ToLongDateString());
            _mChart.Invalidate();
        }

        private void _InitExampleUI()
        {
            TaskGridView.DataSource = new BindingSource(_mManager.Tasks, null);
            mnuFilePrint200.Click += (s, e) => _PrintDocument(2.0f);
            mnuFilePrint150.Click += (s, e) => _PrintDocument(1.5f);
            mnuFilePrint100.Click += (s, e) => _PrintDocument(1.0f);
            mnuFilePrint80.Click += (s, e) => _PrintDocument(0.8f);
            mnuFilePrint50.Click += (s, e) => _PrintDocument(0.5f);
            mnuFilePrint25.Click += (s, e) => _PrintDocument(0.25f);
            mnuFilePrint10.Click += (s, e) => _PrintDocument(0.1f);

            mnuFileImgPrint100.Click += (s, e) => _PrintImage(1.0f);
            mnuFileImgPrint50.Click += (s, e) => _PrintImage(0.5f);
            mnuFileImgPrint10.Click += (s, e) => _PrintImage(0.1f);
            DefaultColors();
        }

        #region Main Menu

        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fs = System.IO.File.OpenWrite(dialog.FileName))
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        bf.Serialize(fs, _mManager);
                    }
                }
            }
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fs = System.IO.File.OpenRead(dialog.FileName))
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        _mManager = bf.Deserialize(fs) as ProjectManager;
                        if (_mManager == null)
                        {
                            MessageBox.Show("Unable to load ProjectManager. Data structure might have changed from previous verions", "Gantt Chart", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            _mChart.Init(_mManager);
                            _mChart.Invalidate();
                        }
                    }
                }
            }
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuViewDaysDayOfWeek_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Week;
            _mChart.Invalidate();
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            // start a new Project and init the chart with the project
            _mManager = new ProjectManager();
            _mManager.Add(new Task() { Name = "New Task" });
            _mChart.Init(_mManager);
            _mChart.Invalidate();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Please visit http://www.jakesee.com/net-c-winforms-gantt-chart-control/ for more help and details", "Braincase Solutions - Gantt Chart", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                System.Diagnostics.Process.Start("http://www.jakesee.com/net-c-winforms-gantt-chart-control/");
            }
        }

        private void mnuViewRelationships_Click(object sender, EventArgs e)
        {
            _mChart.ShowRelations = mnuViewRelationships.Checked = !mnuViewRelationships.Checked;
            _mChart.Invalidate();
        }

        private void mnuViewSlack_Click(object sender, EventArgs e)
        {
            _mChart.ShowSlack = mnuViewSlack.Checked = !mnuViewSlack.Checked;
            _mChart.Invalidate();
        }

        private void mnuViewIntructions_Click(object sender, EventArgs e)
        {
            _mOverlay.PrintMode = !(mnuViewIntructions.Checked = !mnuViewIntructions.Checked);
            _mChart.Invalidate();
        }

        #region Timescale Views
        private void mnuViewDays_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Day;
            _ClearTimeResolutionMenu();
            mnuViewDays.Checked = true;
            _mChart.Invalidate();
        }

        private void mnuViewWeeks_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Week;
            _ClearTimeResolutionMenu();
            mnuViewWeeks.Checked = true;
            _mChart.Invalidate();
        }

        private void mnuViewHours_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Hour;
            _ClearTimeResolutionMenu();
            mnuViewHours.Checked = true;
            _mChart.Invalidate();
        }

        private void _ClearTimeResolutionMenu()
        {
            mnuViewDays.Checked = false;
            mnuViewWeeks.Checked = false;
            mnuViewHours.Checked = false;
        }
        #endregion Timescale Views

        #endregion Main Menu

        #region Sidebar

        private void _mDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            _mManager.Start = _mStartDatePicker.Value;
            var span = DateTime.Today - _mManager.Start;
            _mManager.Now = span;
            
            _mChart.Invalidate();
        }

        private void _mPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            _mChart.Invalidate();
        }

        private void _mNowDatePicker_ValueChanged(object sender, EventArgs e)
        {
            TimeSpan span = _mNowDatePicker.Value - _mStartDatePicker.Value;
            _mManager.Now = span.Add(new TimeSpan(1, 0, 0, 0));
            _mChart.Invalidate();
        }

        private void _mScrollDatePicker_ValueChanged(object sender, EventArgs e)
        {
            _mChart.ScrollTo(_mScrollDatePicker.Value);
            _mChart.Invalidate();
        }

        private void _mTaskGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (TaskGridView.SelectedRows.Count > 0)
            {
                var task = TaskGridView.SelectedRows[0].DataBoundItem as Task;
                _mChart.ScrollTo(task);
            }
        }

        #endregion Sidebar

        #region Print

        private void _PrintDocument(float scale)
        {
            using (var dialog = new PrintDialog())
            {
                dialog.Document = new System.Drawing.Printing.PrintDocument();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // set the print mode for the custom overlay painter so that we skip printing instructions
                    dialog.Document.BeginPrint += (s, arg) => _mOverlay.PrintMode = true;
                    dialog.Document.EndPrint += (s, arg) => _mOverlay.PrintMode = false;

                    // tell chart to print to the document at the specified scale
                    _mChart.Print(dialog.Document, scale);
                }
            }
        }

        private void _PrintImage(float scale)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Bitmap (*.bmp) | *.bmp";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // set the print mode for the custom overlay painter so that we skip printing instructions
                    _mOverlay.PrintMode = true;
                    // tell chart to print to the document at the specified scale

                    var bitmap = _mChart.Print(scale);
                    _mOverlay.PrintMode = false; // restore printing overlays

                    bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }


        #endregion Print        


        #region ColorChangeBTN

        private void btnTaskColor_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;
           

            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnTaskColor.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                { 
                  Color = b,
                  Border = _mChart.TaskFormat.Border,
                  BackFill = _mChart.TaskFormat.BackFill,
                  ForeFill = _mChart.TaskFormat.ForeFill,
                  SlackFill = _mChart.TaskFormat.SlackFill
                };
                _mChart.TaskFormat = temp;
                    
            }    
        }

        private void btnTaskBorder_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnTaskBorder.BackColor = TaskDialog.Color;                
                Pen p = new Pen(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.TaskFormat.Color,
                    Border = p,
                    BackFill = _mChart.TaskFormat.BackFill,
                    ForeFill = _mChart.TaskFormat.ForeFill,
                    SlackFill = _mChart.TaskFormat.SlackFill

                };
                _mChart.TaskFormat = temp;

            }
        }

        private void btnTaskBackFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnTaskBackFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.TaskFormat.Color,
                    Border = _mChart.TaskFormat.Border,
                    BackFill = b,
                    ForeFill = _mChart.TaskFormat.ForeFill,
                    SlackFill = _mChart.TaskFormat.SlackFill

                };
                _mChart.TaskFormat = temp;

            }
        }

        private void btnTaskForeFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnTaskForeFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.TaskFormat.Color,
                    Border = _mChart.TaskFormat.Border,
                    BackFill = _mChart.TaskFormat.BackFill,
                    ForeFill = b,
                    SlackFill = _mChart.TaskFormat.SlackFill

                };
                _mChart.TaskFormat = temp;

            }
        }
        private void btnTaskSlackFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnTaskSlackFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.TaskFormat.Color,
                    Border = _mChart.TaskFormat.Border,
                    BackFill = _mChart.TaskFormat.BackFill,
                    ForeFill = _mChart.TaskFormat.ForeFill,
                    SlackFill = b

                };
                _mChart.TaskFormat = temp;

            }
        }
        private void btnCriticalTaskColor_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnCriticalTaskColor.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = b,
                    Border = _mChart.CriticalTaskFormat.Border,
                    BackFill = _mChart.CriticalTaskFormat.BackFill,
                    ForeFill = _mChart.CriticalTaskFormat.ForeFill,
                    SlackFill = _mChart.CriticalTaskFormat.SlackFill

                };
                _mChart.CriticalTaskFormat = temp;

            }
        }

        private void btnCriticalTaskBorder_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnCriticalTaskBorder.BackColor = TaskDialog.Color;                
                Pen p = new Pen(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.CriticalTaskFormat.Color,
                    Border = p,
                    BackFill = _mChart.CriticalTaskFormat.BackFill,
                    ForeFill = _mChart.CriticalTaskFormat.ForeFill,
                    SlackFill = _mChart.CriticalTaskFormat.SlackFill

                };
                _mChart.CriticalTaskFormat = temp;

            }
        }

        private void btnCriticalTaskBackFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnCriticalTaskBackFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.CriticalTaskFormat.Color,
                    Border = _mChart.CriticalTaskFormat.Border,
                    BackFill = b,
                    ForeFill = _mChart.CriticalTaskFormat.ForeFill,
                    SlackFill = _mChart.CriticalTaskFormat.SlackFill

                };
                _mChart.CriticalTaskFormat = temp;

            }
        }

        

        private void btnCriticalTaskForeFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnCriticalTaskForeFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.CriticalTaskFormat.Color,
                    Border = _mChart.CriticalTaskFormat.Border,
                    BackFill = _mChart.CriticalTaskFormat.BackFill,
                    ForeFill =b,
                    SlackFill = _mChart.CriticalTaskFormat.SlackFill

                };
                _mChart.CriticalTaskFormat = temp;

            }
        }

        private void btnCriticalTaskSlackFill_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnCriticalTaskSlackFill.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                TaskFormat temp = new TaskFormat()
                {
                    Color = _mChart.CriticalTaskFormat.Color,
                    Border = _mChart.CriticalTaskFormat.Border,
                    BackFill = _mChart.CriticalTaskFormat.BackFill,
                    ForeFill = _mChart.CriticalTaskFormat.ForeFill,
                    SlackFill = b

                };
                _mChart.CriticalTaskFormat = temp;

            }
        }

        private void btnHeaderColor_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnHeaderColor.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                HeaderFormat temp = new HeaderFormat()
                {
                    Color = b,
                    Border = _mChart.HeaderFormat.Border,
                    GradientLight = _mChart.HeaderFormat.GradientLight,
                    GradientDark = _mChart.HeaderFormat.GradientDark
                   

                };
                _mChart.HeaderFormat = temp;

            }
        }

        private void btnHeaderBorder_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnHeaderBorder.BackColor = TaskDialog.Color;                
                Pen p = new Pen(TaskDialog.Color);
                HeaderFormat temp = new HeaderFormat()
                {
                    Color = _mChart.HeaderFormat.Color,
                    Border = p,
                    GradientLight = _mChart.HeaderFormat.GradientLight,
                    GradientDark = _mChart.HeaderFormat.GradientDark


                };
                _mChart.HeaderFormat = temp;

            }
        }

        private void btnHeaderGradientLight_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnHeaderGradientLight.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                HeaderFormat temp = new HeaderFormat()
                {
                    Color = _mChart.HeaderFormat.Color,
                    Border = _mChart.HeaderFormat.Border,
                    GradientLight = TaskDialog.Color,
                    GradientDark = _mChart.HeaderFormat.GradientDark


                };
                _mChart.HeaderFormat = temp;

            }
        }

        private void btnHeaderGradientDark_Click(object sender, EventArgs e)
        {
            ColorDialog TaskDialog = new ColorDialog();
            TaskDialog.AllowFullOpen = false;
            TaskDialog.ShowHelp = true;
            TaskDialog.Color = this.BackColor;


            if (TaskDialog.ShowDialog() == DialogResult.OK)
            {
                btnHeaderGradientDark.BackColor = TaskDialog.Color;
                Brush b = new SolidBrush(TaskDialog.Color);
                HeaderFormat temp = new HeaderFormat()
                {
                    Color = _mChart.HeaderFormat.Color,
                    Border = _mChart.HeaderFormat.Border,
                    GradientLight = _mChart.HeaderFormat.GradientLight,
                    GradientDark = TaskDialog.Color


                };
                _mChart.HeaderFormat = temp;

            }
        }

        #endregion
    }

    #region overlay painter
    /// <summary>
    /// An example of how to encapsulate a helper painter for painter additional features on Chart
    /// </summary>
    public class OverlayPainter
    {
        /// <summary>
        /// Hook such a method to the chart paint event listeners
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartOverlayPainter(object sender, ChartPaintEventArgs e)
        {
            // Don't want to print instructions to file
            if (this.PrintMode) return;

            var g = e.Graphics;
            var chart = e.Chart;

            // Demo: Static billboards begin -----------------------------------
            // Demonstrate how to draw static billboards
            // "push matrix" -- save our transformation matrix
            e.Chart.BeginBillboardMode(e.Graphics);

            // draw mouse command instructions
            int margin = 300;
            int left = 20;
            var color = chart.HeaderFormat.Color;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("THIS IS DRAWN BY A CUSTOM OVERLAY PAINTER TO SHOW DEFAULT MOUSE COMMANDS.");
            builder.AppendLine("*******************************************************************************************************");
            builder.AppendLine("Left Click - Select task and display properties in PropertyGrid");
            builder.AppendLine("Left Mouse Drag - Change task starting point");
            builder.AppendLine("Right Mouse Drag - Change task duration");
            builder.AppendLine("Middle Mouse Drag - Change task complete percentage");
            builder.AppendLine("Left Doubleclick - Toggle collaspe on task group");
            builder.AppendLine("Right Doubleclick - Split task into task parts");
            builder.AppendLine("Left Mouse Dragdrop onto another task - Group drag task under drop task");
            builder.AppendLine("Right Mouse Dragdrop onto another task part - Join task parts");
            builder.AppendLine("SHIFT + Left Mouse Dragdrop onto another task - Make drop task precedent of drag task");
            builder.AppendLine("ALT + Left Dragdrop onto another task - Ungroup drag task from drop task / Remove drop task from drag task precedent list");
            builder.AppendLine("SHIFT + Left Mouse Dragdrop - Order tasks");
            builder.AppendLine("SHIFT + Middle Click - Create new task");
            builder.AppendLine("ALT + Middle Click - Delete task");
            builder.AppendLine("Left Doubleclick - Toggle collaspe on task group");
            var size = g.MeasureString(builder.ToString(), e.Chart.Font);
            var background = new Rectangle(left, chart.Height - margin, (int)size.Width, (int)size.Height);
            background.Inflate(10, 10);
            g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(background, Color.LightYellow, Color.Transparent, System.Drawing.Drawing2D.LinearGradientMode.Vertical), background);
            g.DrawRectangle(Pens.Brown, background);
            g.DrawString(builder.ToString(), chart.Font, color, new PointF(left, chart.Height - margin));

            
            // "pop matrix" -- restore the previous matrix
            e.Chart.EndBillboardMode(e.Graphics);
            // Demo: Static billboards end -----------------------------------
        }

        public bool PrintMode { get; set; }
    }
    #endregion overlay painter

    #region custom task and resource
    /// <summary>
    /// A custom resource of your own type (optional)
    /// </summary>
    [Serializable]
    public class MyResource
    {
        public string Name { get; set; }
    }
    /// <summary>
    /// A custom task of your own type deriving from the Task interface (optional)
    /// </summary>
    [Serializable]
    public class MyTask : Task
    {
        public MyTask(ProjectManager manager)
            : base()
        {
            Manager = manager;
        }

        private ProjectManager Manager { get; set; }

        public new TimeSpan Start { get { return base.Start; } set { Manager.SetStart(this, value); } }
        public new TimeSpan End { get { return base.End; } set { Manager.SetEnd(this, value); } }
        public new TimeSpan Duration { get { return base.Duration; } set { Manager.SetDuration(this, value); } }
        public new float Complete { get { return base.Complete; } set { Manager.SetComplete(this, value); } }
    }
    #endregion custom task and resource
}
