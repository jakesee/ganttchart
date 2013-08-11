using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace Braincase.GanttChart
{
    static class GDIExtention
    {
        public static void DrawRectangle(this Graphics graphics, Pen pen, RectangleF rectangle)
        {
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }

    /// <summary>
    /// Gantt Chart control
    /// </summary>
    public partial class Chart : UserControl
    {
        /// <summary>
        /// Construct a gantt chart
        /// </summary>
        public Chart()
        {
            // Designer values
            InitializeComponent();

            // Factory values
            HeaderOneHeight = 32;
            HeaderTwoHeight = 20;
            BarSpacing = 26;
            BarHeight = 20;
            BarWidth = 20;
            this.DoubleBuffered = true;
            var viewport = new ControlViewport(this);
            viewport.WheelDelta = BarSpacing;
            _mViewport = viewport;
            TimeScaleDisplay = GanttChart.TimeScaleDisplay.DayOfWeek;
            AllowTaskDragDrop = true;
            ShowRelations = true;
            ShowSlack = false;
            AccumulateRelationsOnGroup = false;
            ShowTaskLabels = true;
            this.Dock = DockStyle.Fill;
            this.Margin = new Padding(0, 0, 0, 0);
            this.Padding = new Padding(0, 0, 0, 0);
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
        }

        /// <summary>
        /// Delegate method for creating a new Task. Creates Task by default.
        /// </summary>
        public Func<Task> CreateTaskDelegate = delegate() { return new Task(); };

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
        /// Get or set header1 pixel height
        /// </summary>
        [DefaultValue(32)]
        public int HeaderOneHeight { get; set; }

        /// <summary>
        /// Get or set the DateTime string format. Default value is D/M/YYYY
        /// </summary>
        [DefaultValue("D/M/YYYY")]
        public string FullDateStringFormat { get; set; }

        /// <summary>
        /// Get or set header2 pixel height
        /// </summary>
        [DefaultValue(20)]
        public int HeaderTwoHeight { get; set; }

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
        public bool TryGetRow(Task task, out int row)
        {
            row = 0;
            if (_mChartTaskHitRects.ContainsKey(task))
            {
                // collection contains parts
                row = _ChartCoordToChartRow(_mChartTaskHitRects[task].Top);
                return true;
            }
            else if (_mChartTaskRects.ContainsKey(task))
            {
                // collectino contains splits
                row = _ChartCoordToChartRow(_mChartTaskRects[task].Top);
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
                task = _mChartTaskRects.ElementAtOrDefault(row - 1).Key;
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
            _GenerateModels();
        }

        /// <summary>
        /// Print the Chart to the specified PrintDocument.
        /// </summary>
        public void Print(PrintDocument document, float scale = 1.0f)
        {
            // save a copy of the current viewport and swap it with PrintViewport
            var viewport = _mViewport;

            float x = 0; // viewport world x, y coords
            float y = 0;
            int pageCount = 0;
     
            document.PrintPage += (s, e) =>
            {
                e.HasMorePages = false;
                pageCount++;
                
                // create a PrintViewport to navigate the world
                var printViewport = new PrintViewport(e.Graphics,
                    viewport.WorldWidth, viewport.WorldHeight,
                    e.MarginBounds.Width, e.MarginBounds.Height,
                    e.PageSettings.Margins.Left, e.PageSettings.Margins.Right);
                printViewport.Scale = scale;
                _mViewport = printViewport;

                // move the viewport
                printViewport.X = x;
                printViewport.Y = y;

                // set clip and draw
                e.Graphics.SetClip(e.MarginBounds);
                _Draw(e.Graphics, e.PageBounds);

                // check if reached end of printing
                if (printViewport.Rectangle.Right < printViewport.WorldWidth)
                {
                    // continue horizontally
                    x += printViewport.Rectangle.Width;
                    e.HasMorePages = true;
                }
                else
                {
                    // reached end of worldwidth so we go down vertically once
                    x = 0;
                    if (printViewport.Rectangle.Bottom < printViewport.WorldHeight)
                    {
                        y += printViewport.Rectangle.Height;
                        e.HasMorePages = true;
                    }
                }
            };
            document.Print();

            // restore the viewport 
            _mViewport = viewport;
        }

        /// <summary>
        /// Print the Chart to the specified Image
        /// </summary>
        /// <param name="scale">Scale to print the image at.</param>
        public Bitmap Print(float scale = 1.0f)
        {
            var viewport = _mViewport;
            _mViewport = new ImageViewport(scale, viewport.WorldWidth, viewport.WorldHeight);

            Bitmap image = new Bitmap((int)Math.Ceiling(viewport.WorldWidth * scale), (int)Math.Ceiling(viewport.WorldHeight * scale));
            var graphics = Graphics.FromImage(image);

            _Draw(graphics, Rectangle.Ceiling(viewport.Rectangle));

            _mViewport = viewport;

            return image;
        }

        /// <summary>
        /// Get information about the chart area at the mouse coordinate of the chart
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public ChartInfo GetChartInfo(Point mouse)
        {
            var row = _ChartCoordToChartRow(mouse.Y);
            var col = _GetDeviceColumnUnderMouse(mouse);
            var task = _GetTaskUnderMouse(mouse);
            return new ChartInfo(row, _mHeaderInfo.Dates[col], task);
        }

        /// <summary>
        /// Set tool tip for the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="text"></param>
        public void SetToolTip(Task task, string text)
        {
            if (task != null && text != string.Empty)
                _mTaskToolTip[task] = text;
        }

        /// <summary>
        /// Get tool tip currently set for the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string GetToolTip(Task task)
        {
            if (task != null)
                return _mTaskToolTip[task];
            else
                return string.Empty;
        }

        /// <summary>
        /// Clear tool tip for the specified task
        /// </summary>
        /// <param name="task"></param>
        public void ClearToolTip(Task task)
        {
            if(task != null)
                _mTaskToolTip.Remove(task);
        }

        /// <summary>
        /// Clear all tool tips
        /// </summary>
        public void ClearToolTips()
        {
            _mTaskToolTip.Clear();
        }

        /// <summary>
        /// Scroll to the specified DateTime
        /// </summary>
        /// <param name="datetime"></param>
        public void ScrollTo(DateTime datetime)
        {
            TimeSpan span = datetime - _mProject.Start;
            _mViewport.X = span.Days * this.BarWidth;
        }

        /// <summary>
        /// Scroll to the specified task
        /// </summary>
        /// <param name="task"></param>
        public void ScrollTo(Task task)
        {
            if (_mChartTaskRects.ContainsKey(task))
            {
                var rect = _mChartTaskRects[task];
                _mViewport.X = rect.Left - this.BarWidth;
                _mViewport.Y = rect.Top - this.HeaderOneHeight - this.HeaderTwoHeight;
            }
        }

        /// <summary>
        /// Begin billboard mode. Graphics must orginate from Chart and be same as that used in EndBillboardMode.
        /// </summary>
        /// <param name="graphics"></param>
        public void BeginBillboardMode(Graphics graphics)
        {
            graphics.Transform = ControlViewport.Identity;
        }

        /// <summary>
        /// End billboard mode. Graphics must orginate from Chart and be same as that used in BeginBillboardMode.
        /// </summary>
        /// <param name="graphics"></param>
        public void EndBillboardMode(Graphics graphics)
        {
            graphics.Transform = _mViewport.Projection;
        }

        #region UserControl Events

        /// <summary>
        /// Raises the System.Windows.Forms.Control.Paint event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!this.DesignMode)
                this._Draw(e.Graphics, e.ClipRectangle);
        }
        
        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseMove event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Hot tracking
            var task = _GetTaskUnderMouse(e.Location);
            if (_mMouseEntered != null && task == null)
            {
                OnTaskMouseOut(new TaskMouseEventArgs(_mMouseEntered, RectangleF.Empty, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                _mMouseEntered = null;

            }
            else if(_mMouseEntered == null && task != null)
            {
                _mMouseEntered = task;
                OnTaskMouseOver(new TaskMouseEventArgs(_mMouseEntered, _mChartTaskHitRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }

            // Dragging
            if (AllowTaskDragDrop && _mDragSource != null)
            {
                Task target = task;
                if (target == _mDragSource) target = null;
                RectangleF targetRect = target == null ? RectangleF.Empty : _mChartTaskHitRects[target];
                int row = _DeviceCoordToChartRow(e.Location.Y);
                OnTaskMouseDrag(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mChartTaskHitRects[_mDragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                _mDragLastLocation = e.Location;
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseClick event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            var task = _GetTaskUnderMouse(e.Location);
            if (task != null)
            {
                OnTaskMouseClick(new TaskMouseEventArgs(task, _mChartTaskHitRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
            else
            {
                OnTaskDeselecting(new TaskMouseEventArgs(task, RectangleF.Empty, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
            base.OnMouseClick(e);
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseDown event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Begin Drag
            if (AllowTaskDragDrop)
            {
                _mDragSource = _GetTaskUnderMouse(e.Location);
                if (_mDragSource != null)
                {
                    _mDragStartLocation = e.Location;
                    _mDragLastLocation = e.Location;
                }
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseUp event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Drop task
            if (AllowTaskDragDrop && _mDragSource != null)
            {
                var target = _GetTaskUnderMouse(e.Location);
                if (target == _mDragSource) target = null;
                var targetRect = target == null ? RectangleF.Empty : _mChartTaskHitRects[target];
                int row = _DeviceCoordToChartRow(e.Location.Y);
                OnTaskMouseDrop(new TaskDragDropEventArgs(_mDragStartLocation, _mDragLastLocation, _mDragSource, _mChartTaskHitRects[_mDragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                _mDragSource = null;
                _mDragLastLocation = Point.Empty;
                _mDragStartLocation = Point.Empty;
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseDoubleClick event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var task = _GetTaskUnderMouse(e.Location);
            if (task != null)
            {
                OnTaskMouseDoubleClick(new TaskMouseEventArgs(task, _mChartTaskHitRects[task], e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }

            base.OnMouseDoubleClick(e);
        }

        #endregion UserControl Events

        #region Chart Events
        /// <summary>
        /// Raises the TaskMouseOver event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTaskMouseOver(TaskMouseEventArgs e)
        {
            if (TaskMouseOver != null)
                TaskMouseOver(this, e);

            this.Cursor = Cursors.Hand;

            var task = e.Task;
            if (_mProject.IsPart(e.Task)) task = _mProject.SplitTaskOf(task);
            if (_mTaskToolTip.ContainsKey(task))
            {
                _mOverlay.ShowToolTip(_mViewport.DeviceToWorldCoord(e.Location), _mTaskToolTip[task]);
                this.Invalidate();
            }
        }
        /// <summary>
        /// Raises the TaskMouseOver event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTaskMouseOut(TaskMouseEventArgs e)
        {
            if (TaskMouseOut != null)
                TaskMouseOut(this, e);

            this.Cursor = Cursors.Default;

            _mOverlay.HideToolTip();
            this.Invalidate();
        }
        /// <summary>
        /// Raises the TaskMouseDrag( event
        /// </summary>
        /// <param name="e"></param>
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
                if (e.Target == null)
                {
                    var delta = (e.PreviousLocation.X - e.StartLocation.X);
                    _mOverlay.DraggedRect = e.SourceRect;
                    _mOverlay.DraggedRect.Width += delta;
                }
                else // drop targetting (join)
                {
                    _mOverlay.DraggedRect = e.TargetRect;
                    _mOverlay.Row = int.MinValue;
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _mOverlay.Clear();

                if (e.Target == null)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        // insertion line
                        _mOverlay.Row = e.Row;
                    }
                    else
                    {
                        // displacing horizontally
                        _mOverlay.DraggedRect = e.SourceRect;
                        _mOverlay.DraggedRect.Offset((e.X - e.StartLocation.X) / this.BarWidth * this.BarWidth, 0);
                    }
                }
                else // drop targetting (subtask / predecessor)
                {
                    _mOverlay.DraggedRect = e.TargetRect;
                    _mOverlay.Row = int.MinValue;
                }
            }
            this.Invalidate();
        }
        /// <summary>
        /// Raises the TaskMouseDrop event
        /// </summary>
        /// <param name="e"></param>
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
                        Task source = e.Source;
                        if(_mProject.IsPart(source)) source = _mProject.SplitTaskOf(source);
                        if (this.TryGetRow(source, out from))
                            _mProject.Move(source, e.Row - from);
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
                        var source = e.Source;
                        if (_mProject.IsPart(source)) source = _mProject.SplitTaskOf(source);
                        if (_mProject.ParentOf(source) == e.Target)
                        {
                            _mProject.Ungroup(e.Target, e.Source);
                        }
                        else
                        {
                            _mProject.Unrelate(e.Target, source);
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
                if (e.Target == null)
                {
                    var duration = e.Source.Duration + delta;
                    _mProject.SetDuration(e.Source, duration);
                }
                else // have target then we do a join
                {
                    _mProject.Join(e.Target, e.Source);
                }
            }

            _mOverlay.Clear();
            this.Invalidate();
        }
        /// <summary>
        /// Raises the TaskMouseClick event
        /// </summary>
        /// <param name="e"></param>
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
                    var newtask = CreateTaskDelegate();
                    _mProject.Add(newtask);
                    _mProject.SetStart(newtask, e.Task.Start);
                    _mProject.SetDuration(newtask, 5);
                    if (_mProject.IsPart(e.Task)) _mProject.Move(newtask, _mProject.IndexOf(_mProject.SplitTaskOf(e.Task)) + 1 - _mProject.IndexOf(newtask));
                    else _mProject.Move(newtask, _mProject.IndexOf(e.Task) + 1 - _mProject.IndexOf(newtask));
                }
                else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    _mProject.Delete(e.Task);
            }
            this.Invalidate();
        }
        /// <summary>
        /// Raises the TaskMouseDoubleClick event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTaskMouseDoubleClick(TaskMouseEventArgs e)
        {
            if (TaskMouseDoubleClick != null)
                TaskMouseDoubleClick(this, e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                e.Task.IsCollapsed = !e.Task.IsCollapsed;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int duration = (int)((_mViewport.DeviceToWorldCoord(e.Location).X - e.Rectangle.Left) / this.BarWidth);
                if (_mProject.IsPart(e.Task)) _mProject.Split(e.Task, new Task(), duration);
                else _mProject.Split(e.Task, CreateTaskDelegate(), CreateTaskDelegate(), duration);
            }

            this.Invalidate();
        }
        /// <summary>
        /// Raises the TaskSelected event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTaskSelected(TaskMouseEventArgs e)
        {
            if (TaskSelected != null)
                TaskSelected(this, e);
        }
        /// <summary>
        /// Raises the TaskDeselecting event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTaskDeselecting(TaskMouseEventArgs e)
        {
            if (TaskDeselecting != null)
                TaskDeselecting(this, e);

            // deselect all tasks
            _mSelectedTasks.Clear();
        }
        /// <summary>
        /// Raises the PaintOverlay event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPaintOverlay(ChartPaintEventArgs e)
        {
            if (this.PaintOverlay != null)
                PaintOverlay(this, e);
        }

        #endregion Chart Events

        #region OverlayPainter
        private ChartOverlay _mOverlay = new ChartOverlay();
        class ChartOverlay
        {
            public void Paint(ChartPaintEventArgs e)
            {
                var g = e.Graphics;
                var chart = e.Chart;

                // dragging outline / trail
                if (DraggedRect != RectangleF.Empty)
                    g.DrawRectangle(Pens.Red, DraggedRect);

                // insertion indicator line
                if (Row != int.MinValue)
                {
                    float y = e.Chart._ChartRowToChartCoord(Row) + e.Chart.BarHeight / 2.0f;
                    g.DrawLine(Pens.CornflowerBlue, new PointF(0, y), new PointF(e.Chart.Width, y));
                }

                // tool tip
                if (_mToolTipMouse != Point.Empty && _mToolTipText != string.Empty)
                {
                    var size = g.MeasureString(_mToolTipText, chart.Font).ToSize();
                    var tooltiprect = new RectangleF(_mToolTipMouse, size);
                    tooltiprect.Offset(0, -tooltiprect.Height);
                    var textstart = new PointF(tooltiprect.Left, tooltiprect.Top);
                    tooltiprect.Inflate(5, 5);
                    g.FillRectangle(Brushes.LightYellow, tooltiprect);
                    g.DrawString(_mToolTipText, chart.Font, Brushes.Black, textstart);
                }
            }

            public void ShowToolTip(PointF worldcoord, string text)
            {
                _mToolTipMouse = worldcoord;
                _mToolTipText = text;
            }

            public void HideToolTip()
            {
                _mToolTipMouse = Point.Empty;
                _mToolTipText = string.Empty;
            }

            public void Clear()
            {
                DraggedRect = RectangleF.Empty;
                Row = int.MinValue;
            }

            private PointF _mToolTipMouse = PointF.Empty;
            private string _mToolTipText = string.Empty;
            public RectangleF DraggedRect = RectangleF.Empty;
            public int Row = int.MinValue;
        }
        #endregion

        #region Private Helper Methods

        private Task _GetTaskUnderMouse(Point mouse)
        {
            var chartcoord = _mViewport.DeviceToWorldCoord(mouse);

            if (!_mHeaderInfo.H1Rect.Contains(chartcoord)
                && !_mHeaderInfo.H2Rect.Contains(chartcoord))
            {
                foreach (var task in _mChartTaskHitRects.Keys)
                {
                    if (_mChartTaskHitRects[task].Contains(chartcoord))
                        return task;
                }
            }

            return null;
        }

        private int _GetDeviceColumnUnderMouse(Point mouse)
        {
            var worldcoord = _mViewport.DeviceToWorldCoord(mouse);

            return _mHeaderInfo.Columns.Select((x, i) => new { x, i }).FirstOrDefault(x => x.x.Contains(worldcoord)).i;
        }

        /// <summary>
        /// Convert view Y coordinate to zero based row number
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int _DeviceCoordToChartRow(float y)
        {
           y = _mViewport.DeviceToWorldCoord(new PointF(0, y)).Y;
           var row = (int)((y - this.BarSpacing - this.HeaderOneHeight) / this.BarSpacing);
           return row < 0 ? 0 : row;
        }

        /// <summary>
        /// Convert world Y coordinate to zero-based row number
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int _ChartCoordToChartRow(float y)
        {
            var row = (int)((y - this.HeaderTwoHeight - this.HeaderOneHeight) / this.BarSpacing);
            return row < 0 ? 0 : row;
        }

        /// <summary>
        /// Convert zero based row number to client Y coordinates
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private float _ChartRowToChartCoord(int row)
        {
            return row * this.BarSpacing + this.HeaderTwoHeight + this.HeaderOneHeight;
        }

        /// <summary>
        /// Generate the task models and resize the world accordingly
        /// </summary>
        private void _GenerateModels()
        {
            // Clear Models
            _mChartTaskRects.Clear();
            _mChartTaskHitRects.Clear();
            _mChartSlackRects.Clear();
            _mChartTaskPartRects.Clear();

            var pHeight = this.Parent == null ? this.Width : this.Parent.Height;
            var pWidth = this.Parent == null ? this.Height : this.Parent.Width;

            // loop over the tasks and pick up items
            int end = int.MinValue;
            int row = 0;
            foreach (var task in _mProject.Tasks)
            {
                if (!_mProject.AncestorsOf(task).Any(x => x.IsCollapsed))
                {
                    int y_coord = row * this.BarSpacing + this.HeaderTwoHeight + this.HeaderOneHeight + (this.BarSpacing - this.BarHeight) / 2;
                    RectangleF taskRect;

                    // Compute task rectangle
                    taskRect = new RectangleF(task.Start * this.BarWidth + this.BarWidth / 2, y_coord, task.Duration * this.BarWidth, this.BarHeight);
                    _mChartTaskRects.Add(task, taskRect);
                    
                    if(!_mProject.IsSplit(task))
                    {
                        // Add normal Task Rectangles to hitRect collection for hit testing
                        _mChartTaskHitRects.Add(task, taskRect);
                    }
                    else // Compute task part rectangles if task is a split task
                    {
                        var parts = new List<KeyValuePair<Task, RectangleF>>();
                        _mChartTaskPartRects.Add(task, parts);
                        foreach (var part in _mProject.PartsOf(task))
                        {
                            taskRect = new RectangleF(part.Start * this.BarWidth + this.BarWidth / 2, y_coord, part.Duration * this.BarWidth, this.BarHeight);
                            parts.Add(new KeyValuePair<Task,RectangleF>(part, taskRect));

                            // Parts are mouse enabled, add to hitRect collection
                            _mChartTaskHitRects.Add(part, taskRect);
                        }
                    }

                    // Compute Slack Rectangles
                    if (this.ShowSlack)
                    {
                        var slackRect = new RectangleF(task.End * this.BarWidth + this.BarWidth / 2, y_coord, task.Slack * this.BarWidth, this.BarHeight);
                        _mChartSlackRects.Add(task, slackRect);
                    }

                    // Find maximum end time
                    if (task.End > end) end = task.End;

                    row++;
                }
            }
            row += 5;
            _mViewport.WorldHeight = Math.Max(pHeight, row * this.BarSpacing + this.BarHeight);
            _mViewport.WorldWidth = Math.Max(pWidth, end * this.BarWidth + 200);
        }
        
        /// <summary>
        /// Generate Header rectangles and dates
        /// </summary>
        private void _GenerateHeaders()
        {
            // only generate the necessary headers by determining of current viewport location
            var h1Rect = new RectangleF(_mViewport.X, _mViewport.Y, _mViewport.Rectangle.Width, this.HeaderOneHeight);
            var h2Rect = new RectangleF(h1Rect.Left, h1Rect.Bottom, _mViewport.Rectangle.Width, this.HeaderTwoHeight);
            var h1LabelRects = new List<RectangleF>();
            var h2LabelRects = new List<RectangleF>();
            var columns = new List<RectangleF>();
            var h1labels = new List<string>();
            var h2labels = new List<string>();
            var dates = new List<DateTime>();
            
            // generate columns across the viewport area
            var curDate = __CalculateViewportStart();
            var h2LabelRect_Y = _mViewport.Y + this.HeaderOneHeight;
            var columns_Y = h2LabelRect_Y + this.HeaderTwoHeight;
            for (int x = (int)(_mViewport.X - _mViewport.X % this.BarWidth); x < _mViewport.Rectangle.Right; x += this.BarWidth)
            {
                dates.Add(curDate);
                h2LabelRects.Add(new RectangleF(x, h2LabelRect_Y, this.BarWidth, this.HeaderTwoHeight));
                columns.Add(new RectangleF(x, columns_Y, this.BarWidth, _mViewport.Rectangle.Height));

                __NextColumn(ref curDate);
            }

            _mHeaderInfo.H1Rect = h1Rect;
            _mHeaderInfo.H2Rect = h2Rect;
            _mHeaderInfo.H1LabelRects = h1LabelRects;
            _mHeaderInfo.H2LabelRects = h2LabelRects;
            _mHeaderInfo.Columns = columns;
            _mHeaderInfo.Dates = dates;
        }

        /// <summary>
        /// Calculate the date in the first visible column in the viewport
        /// </summary>
        /// <returns></returns>
        private DateTime __CalculateViewportStart()
        {
            int vpTime = (int)(_mViewport.X / this.BarWidth);
            if (_mProject.TimeScale == TimeScale.Day)
            {
                return _mProject.Start.AddDays(vpTime);
            }
            else /* if (_mProject.TimeScale == TimeScale.Week) */
            {
                var startWeek = _mProject.Start.AddDays(-(int)_mProject.Start.DayOfWeek);
                return startWeek.AddDays(vpTime * 7);
            }
        }

        /// <summary>
        /// Integrates the current DateTime by one column of TimeScale
        /// </summary>
        /// <param name="current"></param>
        private void __NextColumn(ref DateTime current)
        {
            if (_mProject.TimeScale == TimeScale.Day)
                current = current.AddDays(1);
            else // if (_mProject.TimeScale == TimeScale.Week)
                current = current.AddDays(7);
        }

        /// <summary>
        /// Draw the Chart using the specified graphics
        /// </summary>
        private void _Draw(Graphics graphics, Rectangle clipRect)
        {
            graphics.Clear(Color.White);

            int row = 0;
            if (_mProject != null)
            {
                // generate rectangles
                _GenerateModels();
                _GenerateHeaders();

                // set model view matrix
                graphics.Transform = _mViewport.Projection;

                // draw columns in the background
                _DrawColumns(graphics);

                // draw bar charts
                row = this._DrawTasks(graphics, clipRect);

                // draw predecessor arrows
                if (this.ShowRelations)
                    this._DrawPredecessorLines(graphics);

                // draw the header
                _DrawHeader(graphics, clipRect);

                // Paint overlays
                ChartPaintEventArgs paintargs = new ChartPaintEventArgs(graphics, clipRect, this);
                OnPaintOverlay(paintargs);

                _mOverlay.Paint(paintargs);
            }
            else
            {
                // nothing to draw
            }

            // flush
            graphics.Flush();
        }

        private void _DrawHeader(Graphics graphics, Rectangle clipRect)
        {
            var info = _mHeaderInfo;
            var viewRect = _mViewport.Rectangle;
            var e = new HeaderPaintEventArgs(graphics, clipRect, this, this.Font, this.HeaderFormat);
            if (PaintHeader != null)
                PaintHeader(this, e);

            // Draw header backgrounds
            var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(info.H1Rect, e.Format.GradientLight, e.Format.GradientDark, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            graphics.FillRectangles(gradient, new RectangleF[] { info.H1Rect, info.H2Rect });
            graphics.DrawRectangles(e.Format.Border, new RectangleF[] { info.H1Rect, info.H2Rect });

            // draw h2 labels
            string label = string.Empty;
            System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
            for (int i = 0; i < info.H2LabelRects.Count; i++)
            {
                var h2labelrect = info.H2LabelRects[i];
                var date = info.Dates[i];
                if (this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfWeek)
                    label = Chart.ShortDays[date.DayOfWeek];
                else if (this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth)
                    label = date.Day.ToString();
                else if (this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.WeekOfYear)
                    label = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday).ToString();

                var h2textrect = _TextAlignCenterMiddle(graphics, h2labelrect, label, e.Font);
                graphics.DrawString(label, e.Font, e.Format.Color, h2textrect);

                // draw h1 label and rects
                __DrawHeaderOne(graphics, i, e, h2textrect);
            }

            // draw "Now" line
            float xf = (_mProject.Now + 0.5f) * BarWidth;
            var pen = new Pen(e.Format.Border.Color);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            graphics.DrawLine(pen, new PointF(xf, _mViewport.Y), new PointF(xf, _mViewport.Rectangle.Bottom));
        }

        private void _DrawColumns(Graphics graphics)
        {
            // draw column lines
            graphics.DrawRectangles(this.HeaderFormat.Border, _mHeaderInfo.Columns.ToArray());

            // fill weekend columns
            if(_mProject.TimeScale == TimeScale.Day)
            {
                for (int i = 0; i < _mHeaderInfo.Dates.Count; i++)
                {
                    var date = _mHeaderInfo.Dates[i];
                    // highlight weekends for day time scale
                    if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        var pattern = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent20, this.HeaderFormat.Border.Color, Color.Transparent);
                        graphics.FillRectangle(pattern, _mHeaderInfo.Columns[i]);
                    }
                }
            }
        }

        private void __DrawHeaderOne(Graphics graphics, int columnIndex, HeaderPaintEventArgs e, RectangleF h2textrect)
        {
            var info = _mHeaderInfo;
            var h2labelrect = info.H2LabelRects[columnIndex];
            var date = info.Dates[columnIndex];
            var h1labelrect = RectangleF.Empty;

            // draw h1 labels for day of week
            if (this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfWeek)
            {
                // put a date on every sunday
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    var columnsinlabel = _mProject.TimeScale == TimeScale.Day ? 7 : 1;
                    h1labelrect = new RectangleF(h2labelrect.X, _mViewport.Y, columnsinlabel * BarWidth, this.HeaderOneHeight);
                    var h1textrect = _TextAlignLeftMiddle(graphics, h1labelrect, date.ToShortDateString(), e.Font, h2textrect.X - h2labelrect.X);
                    graphics.DrawString(date.ToString(this.FullDateStringFormat), e.Font, e.Format.Color, h1textrect);
                    __DrawMarker(graphics, (h2labelrect.Left + h2labelrect.Right) / 2f, _mHeaderInfo.H1Rect.Bottom - 5f);
                }
            }
            else if (this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.DayOfMonth || this.TimeScaleDisplay == GanttChart.TimeScaleDisplay.WeekOfYear)
            {
                // put a month
                if (columnIndex == 0 || (date.Month != info.Dates[columnIndex - 1].Month))
                {
                    var h1label = _mProject.TimeScale == TimeScale.Day ? date.ToString("MMMM yyyy") : date.ToString("MMM yy");
                    var columnsinlabel = _mProject.TimeScale == TimeScale.Day
                        ? DateTime.DaysInMonth(date.Year, date.Month)
                        : Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month)).Select(day => new DateTime(date.Year, date.Month, day)).Count(d => d.DayOfWeek == DayOfWeek.Sunday);
                    if (columnIndex == 0)
                    {
                        var leftcolshiftcount = _mProject.TimeScale == TimeScale.Day ? (date.Day - 1) : date.Day / 7;
                        h1labelrect = new RectangleF(h2labelrect.X - leftcolshiftcount * BarWidth, _mViewport.Y, columnsinlabel * BarWidth, this.HeaderOneHeight);
                    }
                    else h1labelrect = new RectangleF(h2labelrect.X, _mViewport.Y, columnsinlabel * BarWidth, this.HeaderOneHeight);
                    var h1textrect = _TextAlignCenterMiddle(graphics, h1labelrect, h1label, e.Font);
                    graphics.DrawString(h1label, e.Font, e.Format.Color, h1textrect);
                    graphics.DrawRectangle(e.Format.Border, h1labelrect);
                }
            }

            // ****************************************
            // Add the H1 Label rect to our header info
            // *****************************************
            _mHeaderInfo.H1LabelRects.Add(h1labelrect);
        }

        private void __DrawMarker(Graphics graphics, float offsetX, float offsetY)
        {
            var marker = _Marker.Select(p => new PointF(p.X + offsetX, p.Y + offsetY)).ToArray();
            graphics.FillPolygon(Brushes.LightGoldenrodYellow, marker);
            graphics.DrawPolygon(new Pen(SystemColors.ButtonShadow), marker);
        }

        private int _DrawTasks(Graphics graphics, Rectangle clipRect)
        {
            var viewRect = _mViewport.Rectangle;
            int row = 0;
            var crit_task_set = new HashSet<Task>(_mProject.CriticalPaths.SelectMany(x => x));
            var pen = new Pen(Color.Gray);
            float labelMargin = this.BarWidth / 2.0f + 3.0f;
            pen.DashStyle = DashStyle.Dot;
            TaskPaintEventArgs e;
            foreach (var task in _mChartTaskRects.Keys)
            {
                // Get the taskrect
                var taskrect = _mChartTaskRects[task];

                // Only begin drawing when the taskrect is to the left of the clipRect's right edge
                if (taskrect.Left <= viewRect.Right)
                {
                    // Crtical Path
                    bool critical = crit_task_set.Contains(task);
                    if (critical) e = new TaskPaintEventArgs(graphics, clipRect, this, task, row, critical, this.Font, this.CriticalTaskFormat);
                    else e = new TaskPaintEventArgs(graphics, clipRect, this, task, row, critical, this.Font, this.TaskFormat);
                    if (PaintTask != null) PaintTask(this, e);

                    if (viewRect.IntersectsWith(taskrect))
                    {
                        if (_mProject.IsSplit(task))
                        {
                            __DrawTaskParts(graphics, e, task, pen);
                        }
                        else
                        {
                            __DrawRegularTaskAndGroup(graphics, e, task, taskrect);
                        }
                    }

                    // write text
                    if (this.ShowTaskLabels && task.Name != string.Empty)
                    {
                        var name = task.Name;
                        var txtrect = _TextAlignLeftMiddle(graphics, taskrect, name, e.Font, labelMargin);
                        txtrect.Offset(taskrect.Width, 0);
                        if (viewRect.IntersectsWith(txtrect))
                        {
                            graphics.DrawString(name, e.Font, e.Format.Color, txtrect);
                        }
                    }

                    // draw slack
                    if (this.ShowSlack && task.Complete < 1.0f)
                    {
                        var slackrect = _mChartSlackRects[task];
                        if (viewRect.IntersectsWith(slackrect))
                            graphics.FillRectangle(e.Format.SlackFill, slackrect);
                    }
                }

                row++;
            }

            return row;
        }

        private void _DrawPredecessorLines(Graphics graphics)
        {
            var viewRect = _mViewport.Rectangle;
            RectangleF clipRectF = new RectangleF(viewRect.X, viewRect.Y, viewRect.Width, viewRect.Height);
            foreach (var p in _mProject.Precedents)
            {
                var precedent = p;
                IEnumerable<Task> dependants = _mProject.DirectDependantsOf(precedent);

                // check with _mTaskRects that the precedent was drawn; this is needed to connect the lines
                if (_mChartTaskRects.ContainsKey(precedent))
                {
                    foreach (var d in dependants)
                    {
                        var dependant = d;

                        // check with _mTaskRects that the dependant was drawn; this is needed to connect the lines
                        if (_mChartTaskRects.ContainsKey(dependant))
                        {
                            var prect = _mChartTaskRects[precedent];
                            var srect = _mChartTaskRects[dependant];
                            if (precedent.End <= dependant.Start)
                            {
                                /*
                                var p1 = new PointF(prect.Right, prect.Top + prect.Height / 2.0f);
                                var p2 = new PointF(srect.Left - BarWidth / 2, prect.Top + prect.Height / 2.0f);
                                var p3 = new PointF(srect.Left - BarWidth / 2, srect.Top + srect.Height / 2.0f);
                                var p4 = new PointF(srect.Left, srect.Top + srect.Height / 2.0f);
                                var size = new SizeF(Math.Abs(p4.X - p1.X), Math.Abs(p4.Y - p1.Y));
                                var linerect = p1.Y < p4.Y ? new RectangleF(p1, size) : new RectangleF(new PointF(p1.X, p1.Y - size.Height), size); 
                                 */
                                var p1 = new PointF(prect.Right, prect.Top + prect.Height / 2.0f);
                                var p2 = new PointF(prect.Right + BarWidth / 2, prect.Top + prect.Height / 2.0f);
                                var p3 = new PointF(prect.Right + BarWidth / 2, srect.Top + srect.Height / 2.0f);
                                var p4 = new PointF(srect.Left, srect.Top + srect.Height / 2.0f);
                                var size = new SizeF(Math.Abs(p4.X - p1.X), Math.Abs(p4.Y - p1.Y));
                                var linerect = p1.Y < p4.Y ? new RectangleF(p1, size) : new RectangleF(new PointF(p1.X, p1.Y - size.Height), size);
                                if (clipRectF.IntersectsWith(linerect))
                                {
                                    graphics.DrawLines(Pens.Black, new PointF[] { p1, p2, p3, p4 });
                                    graphics.FillRectangle(Brushes.Black, p3.X - 1.5f, p3.Y - 1.5f, 3, 3);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void __DrawRegularTaskAndGroup(Graphics graphics, TaskPaintEventArgs e, Task task, RectangleF taskRect)
        {
            var fill = taskRect;
            fill.Width = (int)(fill.Width * task.Complete);
            graphics.FillRectangle(e.Format.BackFill, taskRect);
            graphics.FillRectangle(e.Format.ForeFill, fill);
            graphics.DrawRectangle(e.Format.Border, taskRect);

            // check if this is a parent task / group task, then draw the bracket
            if (_mProject.IsGroup(task))
            {
                var rod = new RectangleF(taskRect.Left, taskRect.Top, taskRect.Width, taskRect.Height / 2);
                graphics.FillRectangle(Brushes.Black, rod);

                if (!task.IsCollapsed)
                {
                    // left bracket
                    graphics.FillPolygon(Brushes.Black, new PointF[] {
                                new PointF() { X = taskRect.Left, Y = taskRect.Top },
                                new PointF() { X = taskRect.Left, Y = taskRect.Top + BarHeight },
                                new PointF() { X = taskRect.Left + BarWidth, Y = taskRect.Top } });
                    // right bracket
                    graphics.FillPolygon(Brushes.Black, new PointF[] {
                                new PointF() { X = taskRect.Right, Y = taskRect.Top },
                                new PointF() { X = taskRect.Right, Y = taskRect.Top + BarHeight },
                                new PointF() { X = taskRect.Right - BarWidth, Y = taskRect.Top } });
                }
            }
        }

        private void __DrawTaskParts(Graphics graphics, TaskPaintEventArgs e, Task task, Pen pen)
        {
            var parts = _mChartTaskPartRects[task];

            // Draw line indicator
            var firstRect = parts[0].Value;
            var lastRect = parts[parts.Count - 1].Value;
            var y_coord = (firstRect.Top + firstRect.Bottom) / 2.0f;
            var point1 = new PointF(firstRect.Right, y_coord);
            var point2 = new PointF(lastRect.Left, y_coord);
            graphics.DrawLine(pen, point1, point2);

            // Draw Part Rectangles
            var taskRects = parts.Select(x => x.Value).ToArray();
            graphics.FillRectangles(e.Format.BackFill, taskRects);

            // Draw % complete indicators
            graphics.FillRectangles(e.Format.ForeFill, parts.Select(x => new RectangleF(x.Value.X, x.Value.Y, x.Value.Width * x.Key.Complete, x.Value.Height)).ToArray());

            // Draw border
            graphics.DrawRectangles(e.Format.Border, taskRects);
        }

        private RectangleF _TextAlignCenterMiddle(Graphics graphics, RectangleF rect, string text, Font font)
        {
            var size = graphics.MeasureString(text, font);
            return new RectangleF(new PointF(rect.Left + (rect.Width - size.Width) / 2, rect.Top + (rect.Height - size.Height) / 2), size);
        }

        private RectangleF _TextAlignLeftMiddle(Graphics graphics, RectangleF rect, string text, Font font, float leftMargin = 0.0f)
        {
            var size = graphics.MeasureString(text, font);
            return new RectangleF(new PointF(rect.Left + leftMargin, rect.Top + (rect.Height - size.Height) / 2), size);
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

        class HeaderInfo
        {
            public RectangleF H1Rect;
            public RectangleF H2Rect;
            public List<RectangleF> H1LabelRects;
            public List<RectangleF> H2LabelRects;
            public List<RectangleF> Columns;
            public List<DateTime> Dates;
        }

        ProjectManager<Task, object> _mProject = null; // The project to be visualised / rendered as a Gantt Chart
        IViewport _mViewport = null;
        Task _mDragSource = null; // The dragged source Task
        Point _mDragLastLocation = Point.Empty; // Record the dragging mouse offset
        Point _mDragStartLocation = Point.Empty;
        List<Task> _mSelectedTasks = new List<Task>(); // List of selected tasks
        Dictionary<Task, RectangleF> _mChartTaskHitRects = new Dictionary<Task, RectangleF>(); // list of hitareas for Task Rectangles
        Dictionary<Task, RectangleF> _mChartTaskRects = new Dictionary<Task, RectangleF>();
        Dictionary<Task, List<KeyValuePair<Task, RectangleF>>> _mChartTaskPartRects = new Dictionary<Task, List<KeyValuePair<Task, RectangleF>>>();
        Dictionary<Task, RectangleF> _mChartSlackRects = new Dictionary<Task, RectangleF>();
        HeaderInfo _mHeaderInfo = new HeaderInfo();
        Task _mMouseEntered = null; // flag whether the mouse has entered a Task rectangle or not
        Dictionary<Task, string> _mTaskToolTip = new Dictionary<Task, string>();
        #endregion Private Helper Variables
    }

    #region Chart Formatting

    /// <summary>
    /// Time scale display format
    /// </summary>
    public enum TimeScaleDisplay
    {
        /// <summary>
        /// Day of the week: S, M, T, W, T, F, S
        /// </summary>
        DayOfWeek,
        /// <summary>
        /// Day of month; 1 to 31
        /// </summary>
        DayOfMonth,
        /// <summary>
        /// Week of the year: 1 to 52; or 53 in case of leap year
        /// </summary>
        WeekOfYear
    }

    /// <summary>
    /// Format for painting tasks
    /// </summary>
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

    /// <summary>
    /// Format for painting relations
    /// </summary>
    public struct RelationFormat
    {
        /// <summary>
        /// Get or set the line pen
        /// </summary>
        public Pen Line { get; set; }
    }

    /// <summary>
    /// Format for painting chart header
    /// </summary>
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
        /// <summary>
        /// Get or set the lighter color in the gradient
        /// </summary>
        public Color GradientLight { get; set; }
        /// <summary>
        /// Get or set the darker color in the gradient
        /// </summary>
        public Color GradientDark { get; set; }
    }
    #endregion Chart Formatting

    #region EventAgrs
    /// <summary>
    /// Provides data for TaskMouseEvent
    /// </summary>
    public class TaskMouseEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Subject Task of the event
        /// </summary>
        public Task Task { get; private set; }
        /// <summary>
        /// Rectangle bounds of the Task
        /// </summary>
        public RectangleF Rectangle { get; private set; }
        /// <summary>
        /// Initialize a new instance of TaskMouseEventArgs with the MouseEventArgs parameters and the Task involved.
        /// </summary>
        public TaskMouseEventArgs(Task task, RectangleF rectangle, MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Task = task;
            this.Rectangle = rectangle;
        }
    }
    /// <summary>
    /// Provides data for TaskDragDropEvent
    /// </summary>
    public class TaskDragDropEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Get the previous mouse location
        /// </summary>
        public Point PreviousLocation { get; private set; }
        /// <summary>
        /// Get the starting mouse location of this drag drop event
        /// </summary>
        public Point StartLocation { get; private set; }
        /// <summary>
        /// Get the source task that is being dragged
        /// </summary>
        public Task Source { get; private set; }
        /// <summary>
        /// Get the target task that is being dropped on
        /// </summary>
        public Task Target { get; private set; }
        /// <summary>
        /// Get the rectangle bounds of the source task in chart coordinates
        /// </summary>
        public RectangleF SourceRect { get; private set; }
        /// <summary>
        /// Get the rectangle bounds of the target task in chart coordinates
        /// </summary>
        public RectangleF TargetRect { get; private set; }
        /// <summary>
        /// Get the chart row number that the mouse is current at.
        /// </summary>
        public int Row { get; private set; }
        /// <summary>
        /// Initialize a new instance of TaskDragDropEventArgs with the MouseEventArgs parameters and the Task involved and the previous mouse location.
        /// </summary>
        public TaskDragDropEventArgs(Point startLocation, Point prevLocation, Task source, RectangleF sourceRect, Task target, RectangleF targetRect, int row, MouseButtons buttons, int clicks, int x, int y, int delta)
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

    /// <summary>
    /// Provides data for ChartPaintEvent
    /// </summary>
    public class ChartPaintEventArgs : PaintEventArgs
    {
        /// <summary>
        /// Get the chart that for this event
        /// </summary>
        public Chart Chart { get; private set; }

        /// <summary>
        /// Initialize a new instance of ChartPaintEventArgs with the PaintEventArgs graphics and clip rectangle, and the chart itself.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipRect"></param>
        /// <param name="chart"></param>
        public ChartPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart)
            : base(graphics, clipRect)
        {
            this.Chart = chart;
        }
    }

    /// <summary>
    /// Provides data for ChartPaintEvent
    /// </summary>
    public class HeaderPaintEventArgs : ChartPaintEventArgs
    {
        /// <summary>
        /// Get or set the font to use for drawing the text on the header
        /// </summary>
        public Font Font { get; set; }
        /// <summary>
        /// Get or set the header formatting
        /// </summary>
        public HeaderFormat Format { get; set; }

        /// <summary>
        /// Initialize a new instance of HeaderPaintEventArgs with the editable default font and header format
        /// </summary>
        public HeaderPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart, Font font, HeaderFormat format)
            : base(graphics, clipRect, chart)
        {
            this.Font = font;
            this.Format = format;  
        }
    }

    /// <summary>
    /// Provides data for TaskPaintEvent
    /// </summary>
    public class TaskPaintEventArgs : ChartPaintEventArgs
    {
        /// <summary>
        /// Get the task to be painted
        /// </summary>
        public Task Task { get; private set; }
        /// <summary>
        /// Get the rectangle bounds of the task
        /// </summary>
        public RectangleF Rectangle
        {
            get
            {
                return new RectangleF(this.Task.Start * this.Chart.BarWidth, this.Row * this.Chart.BarSpacing + this.Chart.BarSpacing + this.Chart.HeaderOneHeight, this.Task.Duration * this.Chart.BarWidth, this.Chart.BarHeight);
            }    
        }
        /// <summary>
        /// Get the row number of the task
        /// </summary>
        public int Row { get; private set; }
        /// <summary>
        /// Get or set the font to be used to draw the task label
        /// </summary>
        public Font Font { get; set; }
        /// <summary>
        /// Get or set the formatting of the task
        /// </summary>
        public TaskFormat Format { get; set; }
        /// <summary>
        /// Get whether the task is in a critical path
        /// </summary>
        public bool IsCritical { get; private set; }
        /// <summary>
        /// Initialize a new instance of TaskPaintEventArgs with the editable default font and task paint format
        /// </summary>
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

    /// <summary>
    /// Provides data for RelationPaintEvent
    /// </summary>
    public class RelationPaintEventArgs : ChartPaintEventArgs
    {
        /// <summary>
        /// Get the precedent task in the relation
        /// </summary>
        public Task Precedent { get; private set; }

        /// <summary>
        /// Get the dependant task in the relation
        /// </summary>
        public Task Dependant { get; private set; }

        /// <summary>
        /// Get or set the formatting to use for drawing the relation
        /// </summary>
        public RelationFormat Format { get; set; }

        /// <summary>
        /// Initialize a new instance of RelationPaintEventArgs with the editable default font and relation paint format
        /// </summary>
        public RelationPaintEventArgs(Graphics graphics, Rectangle clipRect, Chart chart, Task before, Task after, RelationFormat format)
            : base(graphics, clipRect, chart)
        {
            this.Precedent = before;
            this.Dependant = after;
            this.Format = format;
        }
    }

    #endregion EventArgs

    /// <summary>
    /// Provides information about the chart at a specific row and date/time.
    /// </summary>
    public struct ChartInfo
    {
        /// <summary>
        /// Get or set the chart row number
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Get or set the chart date/time
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Get or set the task
        /// </summary>
        public Task Task { get; set; }
        /// <summary>
        /// Construct a passive data structure to hold chart information
        /// </summary>
        /// <param name="row"></param>
        /// <param name="dateTime"></param>
        /// <param name="task"></param>
        public ChartInfo(int row, DateTime dateTime, Task task)
            : this()
        {
            Row = row;
            DateTime = dateTime;
            Task = task;
        }
    }

    public class Row
    {
        public int Index { get; set; }
        public float Height { get; set; }
    }

    public class Column
    {
        public int Index { get; set; }
        public DateTime DateTime { get; set; }
    }
}
