using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Braincase.GanttChart
{
    public enum TimeScaleDisplay
    {
        DayOfWeek, DayOfMonth, WeekOfYear
    }

    public partial class Chart : UserControl
    {
        /// <summary>
        /// Create a GanttChart
        /// </summary>
        public Chart()
        {
            // Factory values
            BarSpacing = 26;
            BarHeight = 20;
            BarWidth = 20;
            FontColor = Brushes.Black;
            TaskBorderColor = Pens.Maroon;
            TaskBackColor = Brushes.MediumSlateBlue;
            TaskForeColor = Brushes.YellowGreen;
            GridLineColor = Pens.LightGray;
            TimeScaleDisplay = GanttChart.TimeScaleDisplay.DayOfWeek;
            AllowTaskDragDrop = true;

            // Designer values
            InitializeComponent();
        }

        /// <summary>
        /// Get the selected tasks
        /// </summary>
        public IEnumerable<Task> SelectedTasks
        {
            get
            {
                return _mSelectedTasks.ToArray();
            }
        }

        /// <summary>
        /// Get the latest selected task
        /// </summary>
        public Task SelectedTask
        {
            get
            {
                return _mSelectedTasks.LastOrDefault();
            }
        }
        
        /// <summary>
        /// Get visible tasks currently drawn on the chart
        /// </summary>
        public IEnumerable<Task> VisibleTasks
        {
            get
            {
                if(_mProject != null)
                    return _mProject.Tasks.Where(x => !x.Ancestors.Any(a => a.IsCollapsed));
                else 
                    return new Task[0];
            }
        }

        /// <summary>
        /// Get or set pixel distance from top of each Task to the next
        /// </summary>
        [DefaultValue(26)]
        public int BarSpacing { get; set; }

        /// <summary>
        /// Get or set pixel height of each Task
        /// </summary>
        [DefaultValue(20)]
        public int BarHeight { get; set; }

        /// <summary>
        /// Get or set pixel width of each unit of period
        /// </summary>
        [DefaultValue(20)]
        public int BarWidth { get; set; }

        /// <summary>
        /// Get or set Task outline color
        /// </summary>
        public Pen TaskBorderColor { get; set; }

        /// <summary>
        /// Get or set the grid line pen color
        /// </summary>
        public Pen GridLineColor { get; set; }

        /// <summary>
        /// Get or set Task background color
        /// </summary>
        public Brush TaskBackColor { get; set; }

        /// <summary>
        /// Get or set Task foreground color
        /// </summary>
        public Brush TaskForeColor { get; set; }

        /// <summary>
        /// Get or set Task font color
        /// </summary>
        public Brush FontColor { get; set; }

        /// <summary>
        /// Get or set whether dragging of Tasks is allowed. Set to false when not dragging to skip drag(drop) tracking.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowTaskDragDrop { get; set; }

        /// <summary>
        /// Get or set the time scale display format
        /// </summary>
        [DefaultValue(TimeScaleDisplay.DayOfWeek)]
        public TimeScaleDisplay TimeScaleDisplay { get; set; }

        /// <summary>
        /// Occurs when the mouse is moving over a Task
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskMouseOver = null;

        /// <summary>
        /// Occurs when the mouse leaves a Task
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskMouseOut = null;

        /// <summary>
        /// Occurs when a Task is clicked
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskMouseClick = null;

        /// <summary>
        /// Occurs when a Task is double clicked by the mouse
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskMouseDoubleClick = null;

        /// <summary>
        /// Occurs when a Task is being dragged by the mouse
        /// </summary>
        public event EventHandler<TaskDragDropEventArgs> TaskMouseDrag = null;

        /// <summary>
        /// Occurs when a dragged Task is being dropped by releasing any previously pressed mouse button.
        /// </summary>
        public event EventHandler<TaskDragDropEventArgs> TaskMouseDrop = null;

        /// <summary>
        /// Occurs when a task is selected.
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskSelected = null;

        /// <summary>
        /// Occurs before one or more tasks are being deselected. All Task in Chart.SelectedTasks will be deselected.
        /// </summary>
        public event EventHandler<TaskMouseEventArgs> TaskDeselecting = null;

        /// <summary>
        /// Occurs before a Task gets painted
        /// </summary>
        public event EventHandler<TaskPaintEventArgs> PaintTask = null;

        /// <summary>
        /// Occurs before overlays get painted
        /// </summary>
        public event EventHandler<ChartPaintEventArgs> PaintOverlay = null;

        /// <summary>
        /// Occurs before the header gets painted
        /// </summary>
        public event EventHandler<ChartPaintEventArgs> PaintHeader = null;

        /// <summary>
        /// Get the line number of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool TryGetLine(Task task, out int line)
        {
            line = 0;
            if (_mTaskRects.ContainsKey(task))
            {
                line = _mTaskRects[task].Top / this.BarSpacing;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the task at the specified line number
        /// </summary>
        /// <param name="line"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetTask(int line, out Task task)
        {
            task = null;
            if (line > 0 && line < _mProject.Tasks.Count())
            {
                task = _mProject.Tasks.ElementAtOrDefault(line - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialize this Chart with a Project
        /// </summary>
        /// <param name="project"></param>
        public void Init(Project project)
        {
            _mProject = project;
            this.DoubleBuffered = true;
            _Resize();
        }

        /// <summary>
        /// Draw the Chart using the specified graphics
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(Graphics graphics)
        {
            _mTaskRects.Clear(); // clear bar areas
            _Resize(); // resize drawing area
            int line = 0; // starting line;
            line = _DrawHeader(graphics, line);

            if (_mProject != null)
            {
                // draw bar charts
                this._DrawTasks(graphics, this.VisibleTasks, line);

                // draw predecessor arrows
                this._DrawPredecessorLines(graphics);
            }

            // draw "Now" line
            float xf = _mProject.Now * BarWidth;
            int ih = (int)(Math.Round(this.Height / 2.5f) / 2 + 1);
            for (int h = 0; h <= this.Height; h += 5)
                graphics.DrawLine(Pens.Gray, new PointF() { X = xf, Y = h }, new PointF() { X = xf, Y = h + 2.5f });

            // Paint overlays
            OnPaintOverlay(new ChartPaintEventArgs(this, line, graphics, this.ClientRectangle, this.Font, this.FontColor, this.TaskBorderColor, this.GridLineColor, this.TaskBackColor, this.TaskForeColor));

            // flush
            graphics.Flush();
        }

        #region UserControl Events
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.DesignMode)
                this.Draw(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Hot tracking
            var task = _GetTaskUnderMouse(e);
            if (_mMouseEntered && task == null)
            {
                _mMouseEntered = false;
                OnTaskMouseOut(new TaskMouseEventArgs(null, Rectangle.Empty, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
            else if(!_mMouseEntered && task != null)
            {
                _mMouseEntered = true;
                OnTaskMouseOver(new TaskMouseEventArgs(task, _mTaskRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }

            // Dragging
            if (AllowTaskDragDrop && _mDragSource != null)
            {
                Task target = task;
                if (target == _mDragSource) target = null;
                Rectangle targetRect = target == null ? Rectangle.Empty : _mTaskRects[target];
                int line = e.Location.Y / BarSpacing;
                if (line < 1) line = 0;
                OnTaskMouseDrag(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mTaskRects[_mDragSource], target, targetRect, line, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                _mDragLastLocation = e.Location;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            var task = _GetTaskUnderMouse(e);
            if (task != null)
            {
                OnTaskMouseClick(new TaskMouseEventArgs(task, _mTaskRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
            else
            {
                OnTaskDeselecting(new TaskMouseEventArgs(task, Rectangle.Empty, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Begin Drag
            if (AllowTaskDragDrop)
            {
                _mDragSource = _GetTaskUnderMouse(e);
                if (_mDragSource != null)
                {
                    _mDragStartLocation = e.Location;
                    _mDragLastLocation = e.Location;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Drop task
            if (AllowTaskDragDrop && _mDragSource != null)
            {
                var target = _GetTaskUnderMouse(e);
                if (target == _mDragSource) target = null;
                var targetRect = target == null ? Rectangle.Empty : _mTaskRects[target];
                int line = e.Location.Y / BarSpacing;
                if (line < 1) line = 0;
                OnTaskMouseDrop(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mTaskRects[_mDragSource], target, targetRect, line, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                _mDragSource = null;
                _mDragLastLocation = Point.Empty;
                _mDragStartLocation = Point.Empty;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var task = _GetTaskUnderMouse(e);
            if (task != null)
            {
                OnTaskMouseDoubleClick(new TaskMouseEventArgs(task, _mTaskRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }

            base.OnMouseDoubleClick(e);
        }
        #endregion UserControl Events

        #region Chart Events
        protected virtual void OnTaskMouseOver(TaskMouseEventArgs e)
        {
            if (TaskMouseOver != null)
                TaskMouseOver(this, e);

            this.Cursor = Cursors.Hand;
        }

        protected virtual void OnTaskMouseOut(TaskMouseEventArgs e)
        {
            if (TaskMouseOut != null)
                TaskMouseOut(this, e);

            this.Cursor = Cursors.Default;
        }

        protected virtual void OnTaskMouseDrag(TaskDragDropEventArgs e)
        {
            // fire listeners
            if (TaskMouseDrag != null)
                TaskMouseDrag(this, e);

            // Default drag behaviors **********************************
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                e.Source.Complete += (float)(e.X - e.PreviousLocation.X) / (e.Source.Duration * this.BarWidth);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var delta = (e.PreviousLocation.X - e.StartLocation.X);
                _mPainter.DraggedRect = e.SourceRect;
                _mPainter.DraggedRect.Width += delta;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _mPainter.Clear();

                if (e.Target == null)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        // insertion line
                        _mPainter.Line = e.Line;
                    }
                    else
                    {
                        // displacing horizontally
                        _mPainter.DraggedRect = e.SourceRect;
                        _mPainter.DraggedRect.Offset((e.X - e.StartLocation.X) / this.BarWidth * this.BarWidth, 0);
                    }
                }
                else // drop targetting (subtask / predecessor)
                {
                    _mPainter.DraggedRect = e.TargetRect;
                    _mPainter.Line = int.MinValue;
                }
            }
            this.Invalidate();
        }

        protected virtual void OnTaskMouseDrop(TaskDragDropEventArgs e)
        {
            // Fire event
            if (TaskMouseDrop != null)
                TaskMouseDrop(this, e);

            var delta = (e.PreviousLocation.X - e.StartLocation.X) / this.BarWidth;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (e.Target == null)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        // insert
                        int from;
                        if (this.TryGetLine(e.Source, out from))
                            _mProject.Move(e.Source, e.Line - from);
                    }
                    else
                    {
                        // displace horizontally
                        if (_mProject.Relationships[e.Source].Count() == 0) e.Source.Start += delta;
                        else e.Source.Delay += delta;
                    }
                }
                else // have drop target
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        _mProject.Relationships.Add(e.Source, e.Target);
                    }
                    else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    {
                        if (e.Source.Parent == e.Target)
                        {
                            _mProject.UngroupTask(e.Source);
                        }
                        else
                        {
                            _mProject.Relationships.Delete(e.Source, e.Target);
                        }
                    }
                    else
                    {
                        _mProject.GroupTask(e.Target, e.Source);
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                e.Source.Duration += delta;
            }

            _mPainter.Clear();
            this.Invalidate();
        }

        protected virtual void OnTaskMouseClick(TaskMouseEventArgs e)
        {
            if (TaskMouseClick != null)
                TaskMouseClick(this, e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    _mSelectedTasks.Add(e.Task);
                }
                else
                {
                    OnTaskDeselecting(e);
                    _mSelectedTasks.Add(e.Task);
                }
                OnTaskSelected(e);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                if (ModifierKeys.HasFlag(Keys.Shift))
                    _mProject.CreateTask();
                else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    _mProject.Remove(e.Task);
            }
            this.Invalidate();
        }

        protected virtual void OnTaskMouseDoubleClick(TaskMouseEventArgs e)
        {
            if (TaskMouseDoubleClick != null)
                TaskMouseDoubleClick(this, e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                e.Task.IsCollapsed = !e.Task.IsCollapsed;
                this.Invalidate();
            }
        }

        protected virtual void OnTaskSelected(TaskMouseEventArgs e)
        {
            if (TaskSelected != null)
                TaskSelected(this, e);
        }

        protected virtual void OnTaskDeselecting(TaskMouseEventArgs e)
        {
            if (TaskDeselecting != null)
                TaskDeselecting(this, e);

            // deselect all tasks
            _mSelectedTasks.Clear();
        }

        protected virtual void OnPaintOverlay(ChartPaintEventArgs e)
        {
            if (this.PaintOverlay != null)
                PaintOverlay(this, e);

            if(!e.Handled)
                _mPainter.ChartOverlayPainter(this, e);
        }
        #endregion Chart Events

        #region OverlayPainter
        private OverlayPainter _mPainter = new OverlayPainter();
        public class OverlayPainter
        {
            public void ChartOverlayPainter(object sender, ChartPaintEventArgs e)
            {
                var g = e.Graphics;
                var chart = e.Chart;
                if (DraggedRect != Rectangle.Empty)
                {
                    g.DrawRectangle(Pens.Red, DraggedRect);
                }

                if (Line != int.MinValue)
                {
                    float y = Line * e.Chart.BarSpacing - (chart.BarSpacing - chart.BarHeight) / 2.0f;
                    g.DrawLine(Pens.CornflowerBlue, new PointF(0, y), new PointF(chart.Width, y));
                }
            }

            public void Clear()
            {
                DraggedRect = Rectangle.Empty;
                Line = int.MinValue;
            }

            public Rectangle DraggedRect = Rectangle.Empty;

            public int Line = int.MinValue;
        }
        #endregion

        #region Private Helper Methods

        private void _DrawMarker(Graphics graphics, float offsetX, float offsetY)
        {
            var marker = _Marker.Select(p => new PointF(p.X + offsetX, p.Y + offsetY)).ToArray();
            graphics.FillPolygon(Brushes.LightGoldenrodYellow, marker);
            graphics.DrawPolygon(new Pen(SystemColors.ButtonShadow), marker);
        }

        private int _DrawHeaderWeek(Graphics graphics, ChartPaintEventArgs e)
        {
            var max = this.Width / this.BarWidth;
            var start = _mProject.Start.AddDays(-(int)_mProject.Start.DayOfWeek);
            DateTime current = start;
            System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
            int zerobasedweek = cal.GetWeekOfYear(current, System.Globalization.CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday) - 1;
            var month = current.Month;
            for (int i = 0; i < max; i++)
            {
                current = start.AddDays(i * 7);
                var h2rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, (e.Line + 1) * this.BarSpacing, this.BarWidth, this.BarHeight);
                var column = new Rectangle(h2rect.Left, h2rect.Bottom - 3, this.BarWidth, this.Height);
                if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth)
                {
                    var h2 = current.Day.ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.FontColor, h2pos);
                    if (month != current.Month)
                    {
                        var h1 = current.ToString("MMM yy");
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, 0, Enumerable.Range(1, DateTime.DaysInMonth(current.Year, current.Month)).Select(x => new DateTime(current.Year, current.Month, x)).Count(d => d.DayOfWeek == DayOfWeek.Sunday) * BarWidth, BarSpacing);
                        var h1pos = _TextAlignCenterMiddle(graphics, h1rect, h1, e.Font);
                        graphics.DrawString(h1, e.Font, e.FontColor, h1pos);
                        graphics.DrawLine(new Pen(SystemColors.ActiveBorder), new PointF(i * this.BarWidth - this.BarWidth / 2, 0), new PointF(i * this.BarWidth - this.BarWidth / 2, BarSpacing));
                    }
                    month = current.Month;
                }
                else if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.WeekOfYear)
                {
                    var h2 = (zerobasedweek + 1).ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.FontColor, h2pos);
                    if (i % 4 == 0)
                    {
                        var h1 = current.ToShortDateString();
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, 0, 4 * BarWidth, BarSpacing);
                        var h1pos = _TextAlignLeftMiddle(graphics, h1rect, current.ToShortDateString(), e.Font, h2pos.X - h1rect.Left);
                        graphics.DrawString(h1, e.Font, e.FontColor, h1pos);
                        _DrawMarker(graphics, (h2rect.Left + h2rect.Right) / 2f, BarSpacing - 5f);
                        
                    }
                    zerobasedweek = ++zerobasedweek % 52;
                }
                graphics.DrawLine(new Pen(SystemColors.ActiveBorder), new Point(column.Left, column.Top), new Point(column.Left, column.Bottom));
            }

            return e.Line + 2;
        }

        private int _DrawHeaderDay(Graphics graphics, ChartPaintEventArgs e)
        {
            var max = this.Width / this.BarWidth;
            DateTime current;
            for (int i = 0; i < max; i++)
            {
                current = _mProject.Start.AddDays(i);
                var h2rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, (e.Line + 1) * this.BarSpacing, this.BarWidth, this.BarHeight);
                var column = new Rectangle(h2rect.Left, h2rect.Bottom - 3, this.BarWidth, this.Height);
                if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfWeek)
                {
                    var h2 = Chart.ShortDays[current.DayOfWeek];    
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.FontColor, h2pos);
                    if (current.DayOfWeek == DayOfWeek.Sunday || current.DayOfWeek == DayOfWeek.Saturday)
                    {
                        var pattern = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent20, Color.Gray, Color.Transparent);
                        graphics.FillRectangle(pattern, h2rect);
                        graphics.FillRectangle(pattern, column);
                    }
                    if (current.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, 0, 7 * BarWidth, BarSpacing);
                        var h1pos = _TextAlignLeftMiddle(graphics, h1rect, current.ToShortDateString(), e.Font, h2pos.X - h1rect.Left);
                        graphics.DrawString(current.ToShortDateString(), e.Font, e.FontColor, h1pos);
                        _DrawMarker(graphics, (h2rect.Left + h2rect.Right) / 2f, BarSpacing - 5f);
                    }
                }
                else if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth)
                {
                    var h2 = current.Day.ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.FontColor, h2pos);
                    if (current.Day == 1)
                    {
                        var text = current.ToString("MMMM");
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, 0, DateTime.DaysInMonth(current.Year, current.Month) * BarWidth, BarSpacing);
                        var h1pos = _TextAlignCenterMiddle(graphics, h1rect, text, e.Font);
                        graphics.DrawString(current.ToString("MMMM"), e.Font, e.FontColor, h1pos);
                        graphics.DrawLine(new Pen(SystemColors.ActiveBorder), new Point(h1rect.Left, h1rect.Top), new Point(h1rect.Left, h1rect.Bottom));
                    }
                }
                graphics.DrawLine(new Pen(SystemColors.ActiveBorder), new Point(column.Left, column.Top), new Point(column.Left, column.Bottom));
            }

            return e.Line + 2;
        }

        private int _DrawHeader(Graphics graphics, int line)
        {
            var e = new ChartPaintEventArgs(this, line, graphics, this.ClientRectangle, this.Font, this.FontColor, this.TaskBorderColor, this.GridLineColor, this.TaskBackColor, this.TaskForeColor);
            if(PaintHeader != null)
            {
                PaintHeader(this, e);
            }

            line = e.Line;
            if (!e.Handled)
            {
                // Specify header elements
                var h1rect = new Rectangle(0, 0, this.Width, this.BarSpacing);
                var h2rect = new Rectangle(0, BarSpacing, this.Width, this.BarHeight);
                var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(h1rect, SystemColors.ButtonHighlight, SystemColors.ButtonFace, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                // Draw header 1 bar
                graphics.FillRectangle(gradient, h1rect);
                graphics.DrawRectangle(new Pen(SystemColors.ActiveBorder), h1rect);
                // Draw header 2 bar
                graphics.FillRectangle(gradient, h2rect);
                graphics.DrawRectangle(new Pen(SystemColors.ActiveBorder), h2rect);

                if (_mProject.TimeScale == GanttChart.TimeScale.Week)
                {
                    line = _DrawHeaderWeek(graphics, e);
                }
                else if (_mProject.TimeScale == GanttChart.TimeScale.Day)
                {
                    line = _DrawHeaderDay(graphics, e);
                }
            }

            return line;
        }

        private void _DrawPredecessorLines(Graphics graphics)
        {
            foreach (var task in _mTaskRects.Keys)
            {
                foreach(var predecessor in _mProject.Relationships[task])
                {
                    if (_mTaskRects.ContainsKey(predecessor))
                    {
                        var prect = _mTaskRects[predecessor];
                        var srect = _mTaskRects[task];

                        graphics.DrawLine(Pens.Black, new PointF(prect.Right, prect.Top + prect.Height / 2.0f), new PointF(prect.Right + BarWidth / 2, prect.Top + prect.Height / 2.0f));
                        graphics.DrawLine(Pens.Black, new PointF(prect.Right + BarWidth / 2, srect.Top + srect.Height / 2.0f), new PointF(prect.Right + BarWidth / 2, prect.Top + prect.Height / 2.0f));
                        graphics.DrawLine(Pens.Black, new PointF(prect.Right + BarWidth / 2, srect.Top + srect.Height / 2.0f), new PointF(srect.Left, srect.Top + srect.Height / 2.0f));
                    }
                }
            }
        }

        private int _DrawTasks(Graphics graphics, IEnumerable<Task> tasks, int line)
        {
            // draw bars
            foreach (var task in tasks)
            {
                var e = new TaskPaintEventArgs(task, this, line, graphics, this.ClientRectangle, this.Font, this.FontColor, this.TaskBorderColor, this.GridLineColor, this.TaskBackColor, this.TaskForeColor);
                if (PaintTask != null) PaintTask(this, e);
                _mTaskRects.Add(task, e.Rectangle); // collect hit areas
                if (!e.IsHandled) // not handled then we paint it ourselves
                {
                    if (_mProject.CriticalPaths.SelectMany<IEnumerable<Task>, IEnumerable<Task>>(x => x).Contains(task))
                    {
                        e.TaskBackColor = Brushes.Crimson;
                    }
                    var outline = e.Rectangle;
                    var fill = outline;
                    fill.Width = (int)(fill.Width * task.Complete);
                    graphics.FillRectangle(e.TaskBackColor, outline);
                    graphics.FillRectangle(e.TaskForeColor, fill);
                    graphics.DrawRectangle(e.TaskBorderColor, outline);
                    var size = graphics.MeasureString(task.Name, e.Font);
                    graphics.DrawString(task.Name, e.Font, e.FontColor, new PointF(task.End * BarWidth, outline.Top + (this.BarHeight - size.Height) / 2));
                    
                    // check if this is a parent task / group task, then draw the bracket
                    if (task.IsGroup)
                    {
                        var rod = new Rectangle(task.Start * BarWidth, outline.Top, task.Duration * BarWidth, BarHeight / 2);
                        graphics.FillRectangle(Brushes.Black, rod);
                        // left bracket
                        graphics.FillPolygon(Brushes.Black, new Point[] {
                        new Point() { X = task.Start * BarWidth, Y = outline.Top },
                        new Point() { X = task.Start * BarWidth, Y = outline.Top + BarHeight },
                        new Point() { X = task.Start * BarWidth + BarWidth, Y = outline.Top } });
                        // right bracket
                        graphics.FillPolygon(Brushes.Black, new Point[] {
                        new Point() { X = task.End * BarWidth, Y = outline.Top },
                        new Point() { X = task.End * BarWidth, Y = outline.Top + BarHeight },
                        new Point() { X = task.End * BarWidth - BarWidth, Y = outline.Top } });
                    }
                }

                line++;
            }

            return line;
        }

        private PointF _TextAlignCenterMiddle(Graphics graphics, Rectangle rect, string text, Font font)
        {
            var size = graphics.MeasureString(text, font);
            return new PointF(rect.Left + (rect.Width - size.Width) / 2, rect.Top + (rect.Height - size.Height) / 2);
        }

        private PointF _TextAlignLeftMiddle(Graphics graphics, Rectangle rect, string text, Font font, float leftMargin = 0.0f)
        {
            var size = graphics.MeasureString(text, font);
            return new PointF(rect.Left + leftMargin, rect.Top + (rect.Height - size.Height) / 2);
        }

        private Task _GetTaskUnderMouse(MouseEventArgs e)
        {
            foreach (var task in _mTaskRects.Keys)
            {
                if (_mTaskRects[task].Contains(e.Location))
                    return task;
            }

            return null;
        }

        private void _Resize()
        {
            this.Dock = DockStyle.None;
            int count = this.VisibleTasks.Count();
            if (count > 0)
            {
                this.Height = Math.Max(this.Parent.Height, this.VisibleTasks.Count() * this.BarSpacing + this.BarHeight);
                this.Width = Math.Max(this.Parent.Width, this.VisibleTasks.Max(x => x.End) * this.BarWidth + 200);
            }
            else
            {
                this.Height = this.Parent.Height;
                this.Width = this.Parent.Width;
            }
        }

        #endregion Private Helper Methods

        #region Private Helper Variables
        /// <summary>
        /// Printing labels for header
        /// </summary>
        private static readonly SortedDictionary<DayOfWeek, string> ShortDays = new SortedDictionary<DayOfWeek, string>
        {
            {DayOfWeek.Sunday, "S"},
            {DayOfWeek.Monday, "M"},
            {DayOfWeek.Tuesday, "T"},
            {DayOfWeek.Wednesday, "W"},
            {DayOfWeek.Thursday, "T"},
            {DayOfWeek.Friday, "F"},
            {DayOfWeek.Saturday, "S"}
        };

        /// <summary>
        /// Polygon points for Header markers
        /// </summary>
        private static readonly PointF[] _Marker = new PointF[] {
            new PointF(-4, 0),
            new PointF(4, 0),
            new PointF(4, 4),
            new PointF(0, 8),
            new PointF(-4f, 4)
        };

        Project _mProject = null; // The project to be visualised / rendered as a Gantt Chart
        Task _mDragSource = null; // The dragged source Task
        Point _mDragLastLocation = Point.Empty; // Record the dragging mouse offset
        Point _mDragStartLocation = Point.Empty;
        List<Task> _mSelectedTasks = new List<Task>(); // List of selected tasks
        Dictionary<Task, Rectangle> _mTaskRects = new Dictionary<Task, Rectangle>(); // list of hitareas for Task Rectangles
        bool _mMouseEntered = false; // flag whether the mouse has entered a Task rectangle or not
        #endregion Private Helper Variables
    }

    public class TaskMouseEventArgs : MouseEventArgs
    {
        public Task Task { get; private set; }
        public Rectangle Rectangle { get; private set; }

        public TaskMouseEventArgs(Task task, Rectangle rectangle, MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Task = task;
            this.Rectangle = rectangle;
        }
    }

    public class TaskDragDropEventArgs : MouseEventArgs
    {
        public Point PreviousLocation { get; private set; }

        public Point StartLocation { get; private set; }

        public Task Source { get; private set; }

        public Task Target { get; private set; }

        public Rectangle SourceRect { get; private set; }

        public Rectangle TargetRect { get; private set; }

        public int Line { get; private set; }

        public TaskDragDropEventArgs(Point startLocation, Point prevLocation, Task source, Rectangle sourceRect, Task target, Rectangle targetRect, int line, MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Source = source;
            this.SourceRect = sourceRect;
            this.Target = target;
            this.TargetRect = targetRect;
            this.PreviousLocation = prevLocation;
            this.StartLocation = startLocation;
            this.Line = line;
        }
    }

    public class ChartPaintEventArgs : PaintEventArgs
    {
        public Chart Chart { get; private set; }
        public int Line { get; private set; }
        public bool Handled { get; set; }
        public Font Font { get; set; }
        public Brush FontColor { get; set; }
        public Pen TaskBorderColor { get; set; }
        public Pen GridLineColor { get; set; }
        public Brush TaskBackColor { get; set; }
        public Brush TaskForeColor { get; set; }

        /// <summary>
        /// Get or set whether the task is already painted and can be skipped over by Chart
        /// </summary>
        public ChartPaintEventArgs(Chart chart, int line, Graphics graphics, Rectangle clipRect, Font font, Brush fontColor, Pen outline, Pen gridLineColor, Brush background, Brush foreground)
            : base(graphics, clipRect)
        {
            this.Chart = chart;
            this.Line = line;
            this.Handled = false;
            this.Font = font;
            this.FontColor = fontColor;
            this.TaskBorderColor = outline;
            this.GridLineColor = gridLineColor;
            this.TaskBackColor = background;
            this.TaskForeColor = foreground;
        }
    }

    public class TaskPaintEventArgs : ChartPaintEventArgs
    {
        public Task Task { get; private set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(this.Task.Start * this.Chart.BarWidth, this.Line * this.Chart.BarSpacing, this.Task.Duration * this.Chart.BarWidth, this.Chart.BarHeight);
            }    
        }

        /// <summary>
        /// Get or set whether the task is already painted and can be skipped over by Chart
        /// </summary>
        public bool IsHandled { get; set; }

        public TaskPaintEventArgs(Task task, Chart chart, int line, Graphics graphics, Rectangle clipRect, Font font, Brush fontColor, Pen outline, Pen gridLineColor, Brush background, Brush foreground) // need to create a paint event for each task for custom painting
            : base(chart, line, graphics, clipRect, font, fontColor, outline, gridLineColor, background, foreground)
        {
            Task = task;
        }
    }
}
