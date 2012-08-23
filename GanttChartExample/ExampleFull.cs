using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Braincase.GanttChart
{
    public partial class ExampleFull : Form
    {
        OverlayPainter _mPainter = new OverlayPainter();

        Project _mProject;

        public class Resource
        {
            public string Name { get; set; }
        }

        public ExampleFull()
        {
            InitializeComponent();

            // Create a Project and some Tasks
            _mProject = new Project();
            var work = _mProject.CreateTask();
            var wake = _mProject.CreateTask();
            var teeth = _mProject.CreateTask();
            var shower = _mProject.CreateTask();
            var clothes = _mProject.CreateTask();
            var hair = _mProject.CreateTask();
            var pack = _mProject.CreateTask();

            for (int i = 0; i < 100; i++)
            {
                _mProject.CreateTask();
            }

            // Give each Task a name
            work.Name = "Prepare For Work";
            wake.Name = "Wake Up";
            teeth.Name = "Brush Teeth";
            shower.Name = "Shower";
            clothes.Name = "Change Clothes";
            hair.Name = "Style Hair";
            pack.Name = "Pack Suitcase";
            // Set task durations
            wake.Duration = 3;
            teeth.Duration = 5;
            shower.Duration = 7;
            clothes.Duration = 4;
            hair.Duration = 3;
            pack.Duration = 5;
            // Set task complete status
            wake.Complete = 0.9f;
            teeth.Complete = 0.5f;
            shower.Complete = 0.4f;

            // Give the Tasks some organisation, setting group and precedents
            _mProject.GroupTask(work, wake);
            _mProject.GroupTask(work, teeth);
            _mProject.GroupTask(work, shower);
            _mProject.GroupTask(work, clothes);
            _mProject.GroupTask(work, hair);
            _mProject.GroupTask(work, pack);
            _mProject.Relationships.Add(teeth, wake);
            _mProject.Relationships.Add(shower, wake);
            _mProject.Relationships.Add(clothes, shower);
            _mProject.Relationships.Add(hair, shower);
            _mProject.Relationships.Add(pack, hair);
            _mProject.Relationships.Add(pack, clothes);

            // Create and assign Resources.
            // Resource is just custom user class. The API can accept any object as resource.
            var jake = new Resource() { Name = "Jake" };
            var peter = new Resource() { Name = "Peter" };
            var john = new Resource() { Name = "John" };
            var lucas = new Resource() { Name = "Lucas" };
            var james = new Resource() { Name = "James" };
            var mary = new Resource() { Name = "Mary" };
            // Add some resources
            _mProject.Resources.AssignResource(wake, jake);
            _mProject.Resources.AssignResource(wake, peter);
            _mProject.Resources.AssignResource(wake, john);
            _mProject.Resources.AssignResource(teeth, jake);
            _mProject.Resources.AssignResource(teeth, james);
            _mProject.Resources.AssignResource(pack, james);
            _mProject.Resources.AssignResource(pack, lucas);
            _mProject.Resources.AssignResource(shower, mary);
            _mProject.Resources.AssignResource(shower, lucas);
            _mProject.Resources.AssignResource(shower, john);

            // Initialize the Chart with our Project
            _mChart.Init(_mProject);
            // Attach event listeners for events we are interested in
            _mChart.TaskMouseOver += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOver);
            _mChart.TaskMouseOut += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOut);
            _mChart.TaskSelected += new EventHandler<TaskMouseEventArgs>(_mChart_TaskSelected);
            _mChart.PaintOverlay += _mPainter.ChartOverlayPainter;
            _mChart.AllowTaskDragDrop = true;

            // Set Time information
            _mProject.TimeScale = TimeScale.Day;
            var span = DateTime.Today - _mProject.Start;
            _mProject.Now = (int)Math.Round(span.TotalDays); // set the "Now" marker at the correct date
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfWeek; // Set the chart to display days of week in header

            // The parent container for Chart should have autoscroll and should invalidate chart during resize
            // this will allow the UI user to scroll through the chart.
            _mSplitter1.Panel2.AutoScroll = true;
            _mSplitter1.Panel2.Resize += (s, e) => _mChart.Invalidate();
        }

        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            _mTaskGrid.SelectedObjects = _mChart.SelectedTasks.ToArray();
            _mResourceGrid.Items.Clear();
            _mResourceGrid.Items.AddRange(_mProject.Resources.GetResources(e.Task).Select(x => new ListViewItem(((Resource)x).Name)).ToArray());
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = "";
            _mPainter.HideToolTip();
            _mChart.Invalidate();
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            _mPainter.ShowToolTip(e.Location, string.Join(", ", _mProject.Resources.GetResources(e.Task).Select(x => ((Resource)x).Name)));
            lblStatus.Text = string.Format("{0} to {1}", _mProject.GetDateTime(e.Task.Start).ToLongDateString(), _mProject.GetDateTime(e.Task.End).ToLongDateString());
            _mChart.Invalidate();
        }

        #region Main Menu

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuFilePrint_Click(object sender, EventArgs e)
        {
            using (var dialog = new PrintDialog())
            {
                dialog.Document = new System.Drawing.Printing.PrintDocument();
                dialog.Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(doc_PrintPage);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dialog.Document.Print();
                }
            }
        }

        private void mnuViewDaysDayOfWeek_Click(object sender, EventArgs e)
        {
            _mProject.TimeScale = TimeScale.Day;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfWeek;
            _mChart.Invalidate();
        }

        private void mnuViewDaysDayOfMonth_Click(object sender, EventArgs e)
        {
            _mProject.TimeScale = TimeScale.Day;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfMonth;
            _mChart.Invalidate();
        }

        private void mnuViewWeeksDayOfMonth_Click(object sender, EventArgs e)
        {
            _mProject.TimeScale = TimeScale.Week;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfMonth;
            _mChart.Invalidate();
        }

        private void mnuViewWeeksWeekOfYear_Click(object sender, EventArgs e)
        {
            _mProject.TimeScale = TimeScale.Week;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.WeekOfYear;
            _mChart.Invalidate();
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            // start a new Project and init the chart with the project
            _mProject = new Project();
            _mProject.CreateTask().Name = "New Task";
            _mChart.Init(_mProject);
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
            _mChart.ShowRelationships = mnuViewRelationships.Checked = !mnuViewRelationships.Checked;
            _mChart.Invalidate();
        }

        private void mnuViewSlack_Click(object sender, EventArgs e)
        {
            _mChart.ShowSlack = mnuViewSlack.Checked = !mnuViewSlack.Checked;
            _mChart.Invalidate();
        }

        void doc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            _mChart.Draw(e.Graphics);
        }

        #endregion Main Menu

        #region Sidebar

        private void _mDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            _mProject.Start = _mDateTimePicker.Value;
            var span = DateTime.Today - _mProject.Start;
            _mProject.Now = (int)Math.Round(span.TotalDays);
            if (_mProject.TimeScale == TimeScale.Week) _mProject.Now = (_mProject.Now % 7) * 7;
            _mChart.Invalidate();
        }

        private void _mPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            _mChart.Invalidate();
        }

        #endregion Sidebar

        
    }

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
            var g = e.Graphics;
            var chart = e.Chart;
            
            // draw tool tip
            var size = g.MeasureString(_mText, chart.Font).ToSize();
            if (_mMouse != Point.Empty && _mText != string.Empty)
            {
                var point = _mMouse;
                point.Y -= size.Height;
                var tooltip = new Rectangle(_mMouse, size);
                tooltip.Inflate(5, 5);
                g.FillRectangle(Brushes.LightYellow, tooltip);
                g.DrawString(_mText, chart.Font, Brushes.Black, _mMouse);
            }

            // draw mouse command instructions
            int row = 12;
            var color = chart.HeaderFormat.Color;
            g.DrawString("Left Click - Select task and display properties in PropertyGrid", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("Left Mouse Drag - Change task starting point", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("Right Mouse Drag - Change task duration", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("Middle Mouse Drag - Change task complete percentage", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("Left Doubleclick - Toggle collaspe on task group", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("Left Mouse Dragdrop onto another task - Group drag task under drop task", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Left Mouse Dragdrop onto another task - Make drop task precedent of drag task", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("ALT + Left Dragdrop onto another task - Ungroup drag task from drop task / Remove drop task from drag task precedent list", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Left Mouse Dragdrop - Order tasks", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Middle Click - Create new task", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
            g.DrawString("ALT + Middle Click - Delete task", chart.Font, color, new PointF(10, chart.Height - row-- * chart.BarSpacing / 2));
        }

        #region Painter Helpers

        public void Clear()
        {
            DraggedRect = Rectangle.Empty;
            Line = int.MinValue;
            HideToolTip();
        }

        public void ShowToolTip(Point mouse, string text)
        {
            _mMouse = mouse;
            _mText = text;
        }

        public void HideToolTip()
        {
            _mMouse = Point.Empty;
            _mText = string.Empty;
        }

        Point _mMouse = Point.Empty;

        string _mText = string.Empty;

        public Rectangle DraggedRect = Rectangle.Empty;

        public int Line = int.MinValue;

        #endregion Painter Helpers
    }
}
