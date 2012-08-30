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
    public partial class Chart : UserControl
    {
        /// <summary>
        /// Create a GanttChart
        /// </summary>
        public Chart()
        {
            // Factory values
            HeaderHeight = 32;
            BarSpacing = 26;
            BarHeight = 20;
            BarWidth = 20;
            TimeScaleDisplay = GanttChart.TimeScaleDisplay.DayOfWeek;
            AllowTaskDragDrop = true;
            ShowRelations = true;
            ShowSlack = false;
            AccumulateRelationsOnGroup = false;
            ShowTaskLabels = true;
            this.Dock = DockStyle.Fill;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            // Formatting
            TaskFormat = new GanttChart.TaskFormat() {
                Color = Brushes.Black,
                Border = Pens.Maroon,
                BackFill = Brushes.MediumSlateBlue,
                ForeFill = Brushes.YellowGreen,
                SlackFill = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.LightDownwardDiagonal, Color.Blue, Color.Transparent)
            };
            CriticalTaskFormat = new GanttChart.TaskFormat() {
                Color = Brushes.Black,
                Border = Pens.Maroon,
                BackFill = Brushes.Crimson,
                ForeFill = Brushes.YellowGreen,
                SlackFill = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.LightDownwardDiagonal, Color.Red, Color.Transparent)
            };
            HeaderFormat = new GanttChart.HeaderFormat() {
                Color = Brushes.Black,
                Border = new Pen(SystemColors.ActiveBorder),
                GradientLight = SystemColors.ButtonHighlight,
                GradientDark = SystemColors.ButtonFace
            };

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
        /// Get the Rectangle region on the chart that is currently visible
        /// </summary>
        public Rectangle Viewport
        {
            get
            {
                var pHeight = this.Parent == null ? this.Width : this.Parent.Height;
                var pWidth = this.Parent == null ? this.Height : this.Parent.Width;
                return new Rectangle(-this.Location.X, -this.Location.Y, pWidth, pHeight);
            }
        }

        /// <summary>
        /// Get or set header pixel height
        /// </summary>
        [DefaultValue(20)]
        public int HeaderHeight { get; set; }

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
        /// Get or set format for Tasks
        /// </summary>
        public TaskFormat TaskFormat { get; set;  }

        /// <summary>
        /// Get or set format for critical Tasks
        /// </summary>
        public TaskFormat CriticalTaskFormat { get; set; }

        /// <summary>
        /// Get or set format for headers
        /// </summary>
        public HeaderFormat HeaderFormat { get; set; }

        /// <summary>
        /// Get or set format for relations
        /// </summary>
        public RelationFormat RelationFormat { get; set; }

        /// <summary>
        /// Get or set whether dragging of Tasks is allowed. Set to false when not dragging to skip drag(drop) tracking.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowTaskDragDrop { get; set; }

        /// <summary>
        /// Get or set whether to show relations
        /// </summary>
        [DefaultValue(true)]
        public bool ShowRelations { get; set; }

        /// <summary>
        /// Get or set whether to show task labels
        /// </summary>
        [DefaultValue(true)]
        public bool ShowTaskLabels { get; set; }

        /// <summary>
        /// Get or set whether to accumulate relations on group tasks and show relations even when group is collapsed. (Not working well; still improving on it)
        /// </summary>
        [DefaultValue(false)]
        public bool AccumulateRelationsOnGroup { get; set; }

        /// <summary>
        /// Get or set whether to show slack
        /// </summary>
        [DefaultValue(false)]
        public bool ShowSlack { get; set; }

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
        public event EventHandler<HeaderPaintEventArgs> PaintHeader = null;

        /// <summary>
        /// Get the line number of the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool TryGetLine(Task task, out int row)
        {
            row = 0;
            if (_mTaskRects.ContainsKey(task))
            {
                row = _ClientCoordToRow(_mTaskRects[task].Top);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the task at the specified line number
        /// </summary>
        /// <param name="row"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetTask(int row, out Task task)
        {
            task = null;
            if (row > 0 && row < _mProject.Tasks.Count())
            {
                task = _mProject.Tasks.ElementAtOrDefault(row - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialize this Chart with a Project
        /// </summary>
        /// <param name="project"></param>
        public void Init(ProjectManager<Task, object> project)
        {
            _mProject = project;
            this.DoubleBuffered = true;
            _CalculateModels();
        }

        /// <summary>
        /// Draw the items in the Viewport
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(Graphics graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("Graphics cannot be null");
                
            this._Draw(graphics, this.Viewport);
        }

        /// <summary>
        /// Draw the Chart using the specified graphics
        /// </summary>
        /// <param name="graphics"></param>
        private void _Draw(Graphics graphics, Rectangle clipRect)
        {
            graphics.Clear(this.BackColor);
            
            int row = 0;
            if (_mProject != null)
            {
                _CalculateModels(); // resize drawing area

                // draw header -- hpoefully this can stay on top in the future
                _DrawHeader(graphics, clipRect);

                // draw bar charts
                row = this._DrawTasks(graphics, clipRect);

                // draw predecessor arrows
                if (this.ShowRelations)
                    this._DrawPredecessorLines(graphics, clipRect);

                // draw "Now" line
                float xf = _mProject.Now * BarWidth;
                var pen = new Pen(Color.Gray);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                graphics.DrawLine(pen, new PointF(xf, 0), new PointF(xf, this.Height));

                // Paint overlays
                OnPaintOverlay(new ChartPaintEventArgs(graphics, clipRect, this));
            }
            else
            {
                string msg = "No Project";
                var rect = _TextAlignCenterMiddle(graphics, this.Viewport, msg, this.Font);
                graphics.DrawString("No Projects Initialised", this.Font, Brushes.Black, new PointF(rect.Left, rect.Top));
            }

            // flush
            graphics.Flush();
        }

        #region UserControl Events

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.DesignMode)
                this._Draw(e.Graphics, e.ClipRectangle);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // base.OnPaintBackground(e); // Disallow
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
                int row = _ClientCoordToRow(e.Location.Y);
                OnTaskMouseDrag(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mTaskRects[_mDragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
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
                int row = _ClientCoordToRow(e.Location.Y);
                OnTaskMouseDrop(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mTaskRects[_mDragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
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
                var complete = e.Source.Complete + (float)(e.X - e.PreviousLocation.X) / (e.Source.Duration * this.BarWidth);
                _mProject.SetComplete(e.Source, complete);
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
                        _mPainter.Row = e.Row;
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
                    _mPainter.Row = int.MinValue;
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
                            _mProject.Move(e.Source, e.Row - from);
                    }
                    else
                    {
                        // displace horizontally
                        var start = e.Source.Start + delta;
                        _mProject.SetStart(e.Source, start);
                    }
                }
                else // have drop target
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        _mProject.Relate(e.Target, e.Source);
                    }
                    else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    {
                        if (_mProject.ParentOf(e.Source) == e.Target)
                        {
                            _mProject.Ungroup(e.Source);
                        }
                        else
                        {
                            _mProject.Unrelate(e.Target, e.Source);
                        }
                    }
                    else
                    {
                        _mProject.Group(e.Target, e.Source);
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var duration = e.Source.Duration + delta;
                _mProject.SetDuration(e.Source, duration);
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
                {
                    var newtask = new Task();
                    _mProject.Add(newtask);
                    _mProject.SetStart(newtask, e.Task.Start);
                    _mProject.SetDuration(newtask, 5);
                    _mProject.Move(newtask, _mProject.IndexOf(e.Task) + 1 - _mProject.IndexOf(newtask));
                }
                else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    _mProject.Delete(e.Task);
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

                if (Row != int.MinValue)
                {
                    // float y = e.Chart._RowToClientCoord(Row) - (e.Chart.BarSpacing - e.Chart.BarHeight) / 2.0f;
                    float y = e.Chart._RowToClientCoord(Row) + e.Chart.BarHeight / 2.0f;
                    g.DrawLine(Pens.CornflowerBlue, new PointF(0, y), new PointF(chart.Width, y));
                }
            }

            public void Clear()
            {
                DraggedRect = Rectangle.Empty;
                Row = int.MinValue;
            }

            public Rectangle DraggedRect = Rectangle.Empty;

            public int Row = int.MinValue;
        }
        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Convert client Y coordinate to zero based row number
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int _ClientCoordToRow(float y)
        {
           var row = (int)((y - this.BarSpacing - this.HeaderHeight) / this.BarSpacing);
           return row < 0 ? 0 : row;
        }
        /// <summary>
        /// Convert zero based row number to client Y coordinates
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private float _RowToClientCoord(int row)
        {
            return row * this.BarSpacing + this.BarSpacing + this.HeaderHeight;
        }

        private Rectangle _RowToTaskRect(int row, Task task)
        {
            return new Rectangle(task.Start * this.BarWidth, (int)_RowToClientCoord(row), task.Duration * this.BarWidth, this.BarHeight);
        }

        private Rectangle _RowToSlackRect(int row, Task task)
        {
            return new Rectangle(task.End * this.BarWidth, (int)_RowToClientCoord(row), task.Slack * this.BarWidth, this.BarHeight);
        }

        private void _DrawMarker(Graphics graphics, float offsetX, float offsetY)
        {
            var marker = _Marker.Select(p => new PointF(p.X + offsetX, p.Y + offsetY)).ToArray();
            graphics.FillPolygon(Brushes.LightGoldenrodYellow, marker);
            graphics.DrawPolygon(new Pen(SystemColors.ButtonShadow), marker);
        }

        private void _DrawHeaderWeek(Graphics graphics, HeaderPaintEventArgs e, Rectangle clipRect, Rectangle H1RECT, Rectangle H2RECT)
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
                var h2rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H2RECT.Top, this.BarWidth, this.BarHeight);
                var column = new Rectangle(h2rect.Left, h2rect.Bottom - 3, this.BarWidth, this.Height);
                if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth)
                {
                    var h2 = current.Day.ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.Format.Color, h2pos);
                    if (month != current.Month)
                    {
                        var h1 = current.ToString("MMM yy");
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H1RECT.Top, Enumerable.Range(1, DateTime.DaysInMonth(current.Year, current.Month)).Select(x => new DateTime(current.Year, current.Month, x)).Count(d => d.DayOfWeek == DayOfWeek.Sunday) * BarWidth, this.HeaderHeight);
                        var h1pos = _TextAlignCenterMiddle(graphics, h1rect, h1, e.Font);
                        graphics.DrawString(h1, e.Font, e.Format.Color, h1pos);
                        graphics.DrawLine(e.Format.Border, new PointF(h1rect.Left, h1rect.Top), new PointF(h1rect.Left, h1rect.Bottom));
                    }
                    month = current.Month;
                }
                else if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.WeekOfYear)
                {
                    var h2 = (zerobasedweek + 1).ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.Format.Color, h2pos);
                    if (i % 4 == 0)
                    {
                        var h1 = current.ToShortDateString();
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H1RECT.Top, 4 * BarWidth, this.HeaderHeight);
                        var h1pos = _TextAlignLeftMiddle(graphics, h1rect, current.ToShortDateString(), e.Font, h2pos.X - h1rect.Left);
                        graphics.DrawString(h1, e.Font, e.Format.Color, h1pos);
                        _DrawMarker(graphics, (h2rect.Left + h2rect.Right) / 2f, this.HeaderHeight - 5f);
                        
                    }
                    zerobasedweek = ++zerobasedweek % 52;
                }
                graphics.DrawLine(e.Format.Border, new Point(column.Left, column.Top), new Point(column.Left, column.Bottom));
            }
        }

        private void _DrawHeaderDay(Graphics graphics, HeaderPaintEventArgs e, Rectangle clipRect, Rectangle H1RECT, Rectangle H2RECT)
        {
            var max = this.Width / this.BarWidth;
            DateTime current;
            for (int i = 0; i < max; i++)
            {
                current = _mProject.Start.AddDays(i);
                var h2rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H2RECT.Top, this.BarWidth, this.BarHeight);
                var column = new Rectangle(h2rect.Left, h2rect.Bottom - 3, this.BarWidth, this.Height);
               
                if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfWeek)
                {
                    var h2 = Chart.ShortDays[current.DayOfWeek];
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.Format.Color, h2pos);
                    if (current.DayOfWeek == DayOfWeek.Sunday || current.DayOfWeek == DayOfWeek.Saturday)
                    {
                        var pattern = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent20, e.Format.Border.Color, Color.Transparent);
                        graphics.FillRectangle(pattern, h2rect);
                        graphics.FillRectangle(pattern, column);
                    }
                    if (current.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H1RECT.Top, 7 * BarWidth, this.HeaderHeight);
                        var h1pos = _TextAlignLeftMiddle(graphics, h1rect, current.ToShortDateString(), e.Font, h2pos.X - h1rect.Left);
                        graphics.DrawString(current.ToShortDateString(), e.Font, e.Format.Color, h1pos);
                        _DrawMarker(graphics, (h2rect.Left + h2rect.Right) / 2f, H1RECT.Bottom - 5f);
                    }
                }
                else if (TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth)
                {
                    var h2 = current.Day.ToString();
                    var h2pos = _TextAlignCenterMiddle(graphics, h2rect, h2, e.Font);
                    graphics.DrawString(h2, e.Font, e.Format.Color, h2pos);
                    if (current.Day == 1)
                    {
                        var text = current.ToString("MMMM");
                        var h1rect = new Rectangle(i * this.BarWidth - this.BarWidth / 2, H1RECT.Top, DateTime.DaysInMonth(current.Year, current.Month) * BarWidth, this.HeaderHeight);
                        var h1pos = _TextAlignCenterMiddle(graphics, h1rect, text, e.Font);
                        graphics.DrawString(current.ToString("MMMM"), e.Font, e.Format.Color, h1pos);
                        graphics.DrawLine(e.Format.Border, new Point(h1rect.Left, h1rect.Top), new Point(h1rect.Left, h1rect.Bottom));
                    }
                }
                graphics.DrawLine(e.Format.Border, new Point(column.Left, column.Top), new Point(column.Left, column.Bottom));
                
            }
        }

        private void _DrawHeader(Graphics graphics, Rectangle clipRect)
        {
            var e = new HeaderPaintEventArgs(graphics, clipRect, this, this.Font, this.HeaderFormat);
            if(PaintHeader != null)
                PaintHeader(this, e);

            // Specify header elements
            var h1rect = new Rectangle(0, 0, this.Width, this.HeaderHeight);
            var h2rect = new Rectangle(h1rect.Left, h1rect.Bottom, this.Width, this.BarHeight);
            var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(h1rect, e.Format.GradientLight, e.Format.GradientDark, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            // Draw header 1 bar
            graphics.FillRectangle(gradient, h1rect);
            graphics.DrawRectangle(e.Format.Border, h1rect);
            // Draw header 2 bar
            graphics.FillRectangle(gradient, h2rect);
            graphics.DrawRectangle(e.Format.Border, h2rect);

            if (_mProject.TimeScale == GanttChart.TimeScale.Week)
            {
                _DrawHeaderWeek(graphics, e, clipRect, h1rect, h2rect);
            }
            else if (_mProject.TimeScale == GanttChart.TimeScale.Day)
            {
                _DrawHeaderDay(graphics, e, clipRect, h1rect, h2rect);
            }
        }

        private void _DrawPredecessorLines(Graphics graphics, Rectangle clipRect)
        {
            RectangleF cliprectf = new RectangleF(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
            foreach (var p in _mProject.Precedents)
            {
                var precedent = p;
                IEnumerable<Task> dependants = _mProject.DirectDependantsOf(precedent);

                // check with _mTaskRects that the precedent was drawn; this is needed to connect the lines
                if (_mTaskRects.ContainsKey(precedent))
                {
                    foreach (var d in dependants)
                    {
                        var dependant = d;

                        // check with _mTaskRects that the dependant was drawn; this is needed to connect the lines
                        if (_mTaskRects.ContainsKey(dependant))
                        {
                            var prect = _mTaskRects[precedent];
                            var srect = _mTaskRects[dependant];
                            if (precedent.End <= dependant.Start)
                            {
                                var p1 = new PointF(prect.Right, prect.Top + prect.Height / 2.0f);
                                var p2 = new PointF(srect.Left - BarWidth / 2, prect.Top + prect.Height / 2.0f);
                                var p3 = new PointF(srect.Left - BarWidth / 2, srect.Top + srect.Height / 2.0f);
                                var p4 = new PointF(srect.Left, srect.Top + srect.Height / 2.0f);
                                var size = new SizeF(Math.Abs(p4.X - p1.X), Math.Abs(p4.Y - p1.Y));
                                var linerect = p1.Y < p4.Y ? new RectangleF(p1, size) : new RectangleF(new PointF(p1.X, p1.Y - size.Height), size);
                                if (cliprectf.IntersectsWith(linerect))
                                {
                                    graphics.DrawLines(Pens.Black, new PointF[] { p1, p2, p3, p4 });
                                }
                            }
                        }
                    }
                }
            }
        }

        private int _DrawTasks(Graphics graphics, Rectangle clipRect)
        {
            // draw bars
            var clipRectF = new RectangleF(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
            int row = 0;
            var crit_task_set = new HashSet<Task>(_mProject.CriticalPaths.SelectMany(x => x));
            // var crit_task_set = _mProject.CriticalPaths.SelectMany(x => x);
            TaskPaintEventArgs e;
            foreach (var task in _mTaskRects.Keys)
            {
                // get the taskrect
                var taskrect = _mTaskRects[task];

                // only begin drawing when the taskrect is to the left of the clipRect's right edge
                if (taskrect.Left <= clipRect.Right)
                {
                    // Crtical Path
                    bool critical = crit_task_set.Contains(task);
                    if (critical) e = new TaskPaintEventArgs(graphics, clipRect, this, task, row, critical, this.Font, this.CriticalTaskFormat);
                    else e = new TaskPaintEventArgs(graphics, clipRect, this, task, row, critical, this.Font, this.TaskFormat);
                    if (PaintTask != null) PaintTask(this, e);

                    // draw task bar
                    if (clipRect.IntersectsWith(taskrect))
                    {
                        var fill = taskrect;
                        fill.Width = (int)(fill.Width * task.Complete);
                        graphics.FillRectangle(e.Format.BackFill, taskrect);
                        graphics.FillRectangle(e.Format.ForeFill, fill);
                        graphics.DrawRectangle(e.Format.Border, taskrect);

                        // check if this is a parent task / group task, then draw the bracket
                        if (_mProject.IsGroup(task))
                        {
                            var rod = new Rectangle(task.Start * BarWidth, taskrect.Top, task.Duration * BarWidth, BarHeight / 2);
                            graphics.FillRectangle(Brushes.Black, rod);

                            if (!task.IsCollapsed)
                            {
                                // left bracket
                                graphics.FillPolygon(Brushes.Black, new Point[] {
                                new Point() { X = task.Start * BarWidth, Y = taskrect.Top },
                                new Point() { X = task.Start * BarWidth, Y = taskrect.Top + BarHeight },
                                new Point() { X = task.Start * BarWidth + BarWidth, Y = taskrect.Top } });
                                // right bracket
                                graphics.FillPolygon(Brushes.Black, new Point[] {
                                new Point() { X = task.End * BarWidth, Y = taskrect.Top },
                                new Point() { X = task.End * BarWidth, Y = taskrect.Top + BarHeight },
                                new Point() { X = task.End * BarWidth - BarWidth, Y = taskrect.Top } });
                            }
                        }
                    }

                    // write text
                    if (this.ShowTaskLabels && task.Name != string.Empty)
                    {
                        var txtrect = _TextAlignLeftMiddle(graphics, taskrect, task.Name, e.Font);
                        txtrect.Offset(taskrect.Width, 0);
                        if (clipRectF.IntersectsWith(txtrect))
                        {
                            graphics.DrawString(task.Name, e.Font, e.Format.Color, txtrect);
                        }
                    }

                    // draw slack
                    if (this.ShowSlack && task.Complete < 1.0f)
                    {
                        var slackrect = _RowToSlackRect(row, task);
                        if (clipRect.IntersectsWith(slackrect))
                            graphics.FillRectangle(e.Format.SlackFill, slackrect);
                    }
                }

                row++;
            }

            return row;
        }

        private RectangleF _TextAlignCenterMiddle(Graphics graphics, Rectangle rect, string text, Font font)
        {
            var size = graphics.MeasureString(text, font);
            return new RectangleF(new PointF(rect.Left + (rect.Width - size.Width) / 2, rect.Top + (rect.Height - size.Height) / 2), size);
        }

        private RectangleF _TextAlignLeftMiddle(Graphics graphics, Rectangle rect, string text, Font font, float leftMargin = 0.0f)
        {
            var size = graphics.MeasureString(text, font);
            return new RectangleF(new PointF(rect.Left + leftMargin, rect.Top + (rect.Height - size.Height) / 2), size);
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

        private void _CalculateModels()
        {
            // Clear Models
            _mTaskRects.Clear();

            this.Dock = DockStyle.None;
            var pHeight = this.Parent == null ? this.Width : this.Parent.Height;
            var pWidth = this.Parent == null ? this.Height : this.Parent.Width;

            // loop over the tasks and pick up items
            int end = int.MinValue;
            int count = 0;
            foreach (var task in _mProject.Tasks)
            {
                if (!_mProject.AncestorsOf(task).Any(x => x.IsCollapsed))
                {
                    var rect = new Rectangle(task.Start * this.BarWidth, count * this.BarSpacing + this.BarSpacing + this.HeaderHeight, task.Duration * this.BarWidth, this.BarHeight);
                    if (task.End > end) end = task.End;

                    // store models
                    _mTaskRects.Add(task, rect);

                    count++;
                }
            }
            count += 5;
            this.Height = Math.Max(pHeight, count * this.BarSpacing + this.BarHeight);
            this.Width = Math.Max(pWidth, end * this.BarWidth + 200);

            // see if we found any tasks with Task.End
            if(end == int.MinValue)
            {
                this.Height = pHeight;
                this.Width = pWidth;
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

        ProjectManager<Task, object> _mProject = null; // The project to be visualised / rendered as a Gantt Chart
        Task _mDragSource = null; // The dragged source Task
        Point _mDragLastLocation = Point.Empty; // Record the dragging mouse offset
        Point _mDragStartLocation = Point.Empty;
        List<Task> _mSelectedTasks = new List<Task>(); // List of selected tasks
        Dictionary<Task, Rectangle> _mTaskRects = new Dictionary<Task, Rectangle>(); // list of hitareas for Task Rectangles
        bool _mMouseEntered = false; // flag whether the mouse has entered a Task rectangle or not
        #endregion Private Helper Variables
    }

    public enum TimeScaleDisplay
    {
        DayOfWeek, DayOfMonth, WeekOfYear
    }

    public struct TaskFormat
    {
        /// <summary>
        /// Get or set Task outline color
        /// </summary>
        public Pen Border { get; set; }

        /// <summary>
        /// Get or set Task background color
        /// </summary>
        public Brush BackFill { get; set; }

        /// <summary>
        /// Get or set Task foreground color
        /// </summary>
        public Brush ForeFill { get; set; }

        /// <summary>
        /// Get or set Task font color
        /// </summary>
        public Brush Color { get; set; }

        /// <summary>
        /// Get or set the brush for slack bars
        /// </summary>
        public Brush SlackFill { get; set; }
    }

    public struct RelationFormat
    {
        public Pen Line { get; set; }
    }

    public struct HeaderFormat
    {
        /// <summary>
        /// Font color
        /// </summary>
        public Brush Color { get; set; }
        /// <summary>
        /// Border and line colors
        /// </summary>
        public Pen Border { get; set; }

        public Color GradientLight { get; set; }

        public Color GradientDark { get; set; }
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

        public int Row { get; private set; }

        public TaskDragDropEventArgs(Point startLocation, Point prevLocation, Task source, Rectangle sourceRect, Task target, Rectangle targetRect, int row, MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Source = source;
            this.SourceRect = sourceRect;
            this.Target = target;
            this.TargetRect = targetRect;
            this.PreviousLocation = prevLocation;
            this.StartLocation = startLocation;
            this.Row = row;
        }
    }

    public class ChartPaintEventArgs : PaintEventArgs
    {
        public Chart Chart { get; private set; }

        public ChartPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart)
            : base(graphics, clipRect)
        {
            this.Chart = chart;
        }
    }

    public class HeaderPaintEventArgs : ChartPaintEventArgs
    {
        public Font Font { get; set; }
        public HeaderFormat Format { get; set; }

        /// <summary>
        /// Get or set whether the task is already painted and can be skipped over by Chart
        /// </summary>
        public HeaderPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart, Font font, HeaderFormat format)
            : base(graphics, clipRect, chart)
        {
            this.Font = font;
            this.Format = format;  
        }
    }

    public class TaskPaintEventArgs : ChartPaintEventArgs
    {
        public Task Task { get; private set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(this.Task.Start * this.Chart.BarWidth, this.Row * this.Chart.BarSpacing + this.Chart.BarSpacing + this.Chart.HeaderHeight, this.Task.Duration * this.Chart.BarWidth, this.Chart.BarHeight);
            }    
        }
        public int Row { get; private set; }
        public Font Font { get; set; }
        public bool IsCritical { get; private set; }
        public TaskFormat Format { get; set; }

        public TaskPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart, Task task, int row, bool critical, Font font, TaskFormat format) // need to create a paint event for each task for custom painting
            : base(graphics, clipRect, chart)
        {
            this.Task = task;
            this.Row = row;
            this.Font = font;
            this.Format = format;
            this.IsCritical = critical;
        }
    }

    public class RelationPaintEventArgs : ChartPaintEventArgs
    {
        public Task Precedent { get; private set; }

        public Task Dependant { get; private set; }

        public RelationFormat Format { get; set; }

        public RelationPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart, Task before, Task after, RelationFormat format)
            : base(graphics, clipRect, chart)
        {
            this.Precedent = before;
            this.Dependant = after;
            this.Format = format;
        }
    }
}
