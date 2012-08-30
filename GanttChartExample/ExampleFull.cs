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

        ProjectManager _mManager = null;

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
                _mManager.SetStart(task, rand.Next(100));
                _mManager.SetDuration(task, rand.Next(10));
            }

            // Set task durations, e.g. using ProjectManager methods 
            _mManager.SetDuration(wake, 3);
            _mManager.SetDuration(teeth, 5);
            _mManager.SetDuration(shower, 7);
            _mManager.SetDuration(clothes, 4);
            _mManager.SetDuration(hair, 3);
            _mManager.SetDuration(pack, 5);

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

            // Initialize the Chart with our ProjectManager
            _mChart.Init(_mManager);

            // Attach event listeners for events we are interested in
            _mChart.TaskMouseOver += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOver);
            _mChart.TaskMouseOut += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOut);
            _mChart.TaskSelected += new EventHandler<TaskMouseEventArgs>(_mChart_TaskSelected);
            _mChart.MouseMove += (s, e) => this.Text = e.Location.ToString();
            _mChart.PaintOverlay += _mPainter.ChartOverlayPainter;
            _mChart.AllowTaskDragDrop = true;

            // Set Time information
            _mManager.TimeScale = TimeScale.Day;
            var span = DateTime.Today - _mManager.Start;
            _mManager.Now = (int)Math.Round(span.TotalDays); // set the "Now" marker at the correct date
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfWeek; // Set the chart to display days of week in header

            // The parent container for Chart should have autoscroll
            // this will allow the UI user to scroll through the chart.
            _mSplitter1.Panel2.AutoScroll = true;
        }

        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            _mTaskGrid.SelectedObjects = _mChart.SelectedTasks.ToArray();
            _mResourceGrid.Items.Clear();
            _mResourceGrid.Items.AddRange(_mManager.ResourcesOf(e.Task).Select(x => new ListViewItem(((MyResource)x).Name)).ToArray());
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = "";
            _mPainter.HideToolTip();
            _mChart.Invalidate();
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            _mPainter.ShowToolTip(e.Location, string.Join(", ", _mManager.ResourcesOf(e.Task).Select(x => (x as MyResource).Name)));
            lblStatus.Text = string.Format("{0} to {1}", _mManager.GetDateTime(e.Task.Start).ToLongDateString(), _mManager.GetDateTime(e.Task.End).ToLongDateString());
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
            _mManager.TimeScale = TimeScale.Day;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfWeek;
            _mChart.Invalidate();
        }

        private void mnuViewDaysDayOfMonth_Click(object sender, EventArgs e)
        {
            _mManager.TimeScale = TimeScale.Day;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfMonth;
            _mChart.Invalidate();
        }

        private void mnuViewWeeksDayOfMonth_Click(object sender, EventArgs e)
        {
            _mManager.TimeScale = TimeScale.Week;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfMonth;
            _mChart.Invalidate();
        }

        private void mnuViewWeeksWeekOfYear_Click(object sender, EventArgs e)
        {
            _mManager.TimeScale = TimeScale.Week;
            _mChart.TimeScaleDisplay = TimeScaleDisplay.WeekOfYear;
            _mChart.Invalidate();
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            // start a new Project and init the chart with the project
            _mManager = new ProjectManager();
            _mManager.Add(new Task() { Name = "New Task" });
            _mChart.Init(_mManager);
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
            _mManager.Start = _mDateTimePicker.Value;
            var span = DateTime.Today - _mManager.Start;
            _mManager.Now = (int)Math.Round(span.TotalDays);
            if (_mManager.TimeScale == TimeScale.Week) _mManager.Now = (_mManager.Now % 7) * 7;
            _mChart.Invalidate();
        }

        private void _mPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            _mChart.Invalidate();
        }

        #endregion Sidebar
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
    #endregion overlay painter

    #region custom task and resource
    public class MyResource
    {
        public string Name { get; set; }
    }

    public class MyTask : Task
    {

        public MyTask(ProjectManager manager)
            : base()
        {
            Manager = manager;
        }

        private ProjectManager Manager { get; set; }

        public new int Start { get { return base.Start; } set { Manager.SetStart(this, value); } }
        public new int End { get { return base.End; } set { Manager.SetEnd(this, value); } }
        public new int Duration { get { return base.Duration; } set { Manager.SetDuration(this, value); } }
        public new float Complete { get { return base.Complete; } set { Manager.SetComplete(this, value); } }
    }
    #endregion custom task and resource
}
