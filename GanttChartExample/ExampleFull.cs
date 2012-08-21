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
        OverlayPainter painter = new OverlayPainter();

        Project _mProject;

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

            // Initialize the Chart with our Project
            _mChart.Init(_mProject);
            // Attach event listeners for events we are interested in
            _mChart.TaskMouseOver += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOver);
            _mChart.TaskMouseOut += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOut);
            _mChart.TaskSelected += new EventHandler<TaskMouseEventArgs>(_mChart_TaskSelected);
            //_mChart.PaintOverlay += painter.ChartOverlayPainter;
            _mChart.AllowTaskDragDrop = true;

            // Set Time information
            _mProject.TimeScale = TimeScale.Day;
            _mProject.Now = 15; // set the "Now" marker at 15 days after the Project.Start date (default DateTime.Now)
            _mChart.TimeScaleDisplay = TimeScaleDisplay.DayOfWeek; // Set the chart to display days of week in header

            // The parent container for Chart should have autoscroll and should invalidate chart during resize
            // this will allow the UI user to scroll through the chart.
            _mSplitter1.Panel2.AutoScroll = true;
            _mSplitter1.Panel2.Resize += (s, e) => _mChart.Invalidate();
        }

        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            _mPropertyGrid.SelectedObject = _mChart.SelectedTask;
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = "";
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            lblStatus.Text = string.Format("{0} to {1}", _mProject.GetDateTime(e.Task.Start).ToLongDateString(), _mProject.GetDateTime(e.Task.End).ToLongDateString());
        }

        private void _mPropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            _mChart.Invalidate();
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

        void doc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            _mChart.Draw(e.Graphics);
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _mDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            _mProject.Start = _mDateTimePicker.Value;
            _mChart.Invalidate();
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
    }

    public class OverlayPainter
    {
        public void ChartOverlayPainter(object sender, ChartPaintEventArgs e)
        {
            var g = e.Graphics;
            var chart = e.Chart;
            /*if (DraggedRect != Rectangle.Empty)
                g.FillRectangle(Brushes.SlateGray, DraggedRect);

            if (Line != int.MinValue)
            {
                float y = Line * e.Chart.BarSpacing - (chart.BarSpacing - chart.BarHeight) / 2.0f;
                g.DrawLine(Pens.CornflowerBlue, new PointF(0, y), new PointF(chart.Width, y));
            }*/

            int line = 12;
            g.DrawString("Left Click - Select task and display properties in PropertyGrid", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("Left Mouse Drag - Change task starting point", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("Right Mouse Drag - Change task duration", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("Middle Mouse Drag - Change task complete percentage", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("Left Doubleclick - Toggle collaspe on task group", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("Left Mouse Dragdrop onto another task - Group drag task under drop task", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Left Mouse Dragdrop onto another task - Make drop task precedent of drag task", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("ALT + Left Dragdrop onto another task - Ungroup drag task from drop task / Remove drop task from drag task precedent list", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Left Mouse Dragdrop - Order tasks", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("SHIFT + Middle Click - Create new task", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
            g.DrawString("ALT + Middle Click - Delete task", chart.Font, chart.FontColor, new PointF(10, chart.Height - line-- * chart.BarSpacing / 2));
        }

        public void Clear()
        {
            DraggedRect = Rectangle.Empty;
            Line = int.MinValue;
        }

        public Rectangle DraggedRect = Rectangle.Empty;

        public int Line = int.MinValue;
    }
}
