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

        ProjectManager<Task, object> _mProject;

        public class Resource
        {
            public string Name { get; set; }
        }

        public ExampleFull()
        {
            InitializeComponent();

            // Create a Project and some Tasks
            _mProject = new ProjectManager<Task, object>();
            var work = new Task();
            var wake = new Task();
            var teeth = new Task();
            var shower = new Task();
            var clothes = new Task();
            var hair = new Task();
            var pack = new Task();

            _mProject.Add(work);
            _mProject.Add(wake);
            _mProject.Add(teeth);
            _mProject.Add(shower);
            _mProject.Add(clothes);
            _mProject.Add(hair);
            _mProject.Add(pack);

            // Create 1000 tasks for stress testing
            for (int i = 0; i < 1000; i++)
            {
                _mProject.Add(new Task() { Name = string.Format("New Task {0}", i.ToString()) });
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
            _mProject.SetDuration(wake, 3);
            _mProject.SetDuration(teeth, 5);
            _mProject.SetDuration(shower, 7);
            _mProject.SetDuration(clothes, 4);
            _mProject.SetDuration(hair, 3);
            _mProject.SetDuration(pack, 5);
            // Set task complete status
            _mProject.SetComplete(wake, 0.9f);
            _mProject.SetComplete(teeth, 0.5f);
            _mProject.SetComplete(shower, 0.4f);

            // Give the Tasks some organisation, setting group and precedents
            _mProject.Group(work, wake);
            _mProject.Group(work, teeth);
            _mProject.Group(work, shower);
            _mProject.Group(work, clothes);
            _mProject.Group(work, hair);
            _mProject.Group(work, pack);
            _mProject.Relate(wake, teeth);
            _mProject.Relate(wake, shower);
            _mProject.Relate(shower, clothes);
            _mProject.Relate(shower, hair);
            _mProject.Relate(hair, pack);
            _mProject.Relate(clothes, pack);

            // Create and assign Resources.
            // Resource is just custom user class. The API can accept any object as resource.
            var jake = new Resource() { Name = "Jake" };
            var peter = new Resource() { Name = "Peter" };
            var john = new Resource() { Name = "John" };
            var lucas = new Resource() { Name = "Lucas" };
            var james = new Resource() { Name = "James" };
            var mary = new Resource() { Name = "Mary" };
            // Add some resources
            _mProject.Assign(wake, jake);
            _mProject.Assign(wake, peter);
            _mProject.Assign(wake, john);
            _mProject.Assign(teeth, jake);
            _mProject.Assign(teeth, james);
            _mProject.Assign(pack, james);
            _mProject.Assign(pack, lucas);
            _mProject.Assign(shower, mary);
            _mProject.Assign(shower, lucas);
            _mProject.Assign(shower, john);

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

            // The parent container for Chart should have autoscroll
            // this will allow the UI user to scroll through the chart.
            _mSplitter1.Panel2.AutoScroll = true;
        }

        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            _mTaskGrid.SelectedObjects = _mChart.SelectedTasks.ToArray();
            _mResourceGrid.Items.Clear();
            _mResourceGrid.Items.AddRange(_mProject.ResourcesOf(e.Task).Select(x => new ListViewItem(((Resource)x).Name)).ToArray());
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = "";
            _mPainter.HideToolTip();
            _mChart.Invalidate();
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            _mPainter.ShowToolTip(e.Location, string.Join(", ", _mProject.ResourcesOf(e.Task).Select(x => ((Resource)x).Name)));
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
                dialog.Document.BeginPrint += new System.Drawing.Printing.PrintEventHandler(Document_BeginPrint);
                dialog.Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(doc_PrintPage);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dialog.Document.Print();
                }
            }
        }

        void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            
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
            _mProject = new ProjectManager<Task, object>();
            _mProject.Add(new Task() { Name = "New Task" });
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
            _mChart.ShowRelations = mnuViewRelationships.Checked = !mnuViewRelationships.Checked;
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
