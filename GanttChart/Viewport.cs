using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Braincase.GanttChart
{
    /// <summary>
    /// A Viewport that is placed over a world coordinate system and provides methods to transform between world and view coordinates
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// Construct a Viewport
        /// </summary>
        /// <param name="view"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        public Viewport(Control view)
        {
            _mView = view;
            _mhScroll = new HScrollBar();
            _mvScroll = new VScrollBar();
            _mScrollHolePatch = new UserControl();
            WorldWidth = view.Width;
            WorldHeight = view.Height;

            _mView.Controls.Add(_mhScroll);
            _mView.Controls.Add(_mvScroll);
            _mView.Controls.Add(_mScrollHolePatch);

            _mhScroll.Scroll += (s, e) => X = e.NewValue;
            _mvScroll.Scroll += (s, e) => Y = e.NewValue;
            _mView.Resize += (s, e) => this.Resize();
            _mView.MouseWheel += (s, e) => Y -= e.Delta > 0 ? WheelDelta : -WheelDelta;
            WheelDelta = _mvScroll.LargeChange;

            _RecalculateMatrix();
            _RecalculateRectangle();
        }

        /// <summary>
        /// Identity Matrix
        /// </summary>
        public static readonly Matrix Identity = new Matrix(1, 0, 0, 1, 0, 0); 

        /// <summary>
        /// Get or set the number of pixels to scroll on each click of the mouse
        /// </summary>
        public int WheelDelta { get; set; }

        /// <summary>
        /// Get the Rectangle area in world coordinates where the Viewport is currently viewing over
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return _mRectangle;
            }
        }

        /// <summary>
        /// Get the projection transformation matrix required for drawing models in the world projected into view
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _mMatrix;
            }
        }

        /// <summary>
        /// Resize the Viewport according to the view control and world dimensions, which ever larger and add scrollbars where approperiate
        /// </summary>
        public void Resize()
        {
            if (WorldWidth <= _mView.Width)
            {
                _mhScroll.Hide();
            }
            else
            {
                _mhScroll.Show();
                _mhScroll.Maximum = WorldWidth - _mView.Width;
                _mhScroll.Dock = DockStyle.None;
                _mhScroll.Location = new Point(0, _mView.Height - _mhScroll.Height);
                _mhScroll.Width = _mView.Width - _mvScroll.Width;
            }

            if (WorldHeight <= _mView.Height)
            {
                _mvScroll.Hide();
            }
            else
            {
                _mvScroll.Show();
                _mvScroll.Maximum = WorldHeight - _mView.Height;
                _mvScroll.Dock = DockStyle.None;
                _mvScroll.Location = new Point(_mView.Width - _mvScroll.Width, 0);
                _mvScroll.Height = _mView.Height - _mhScroll.Height;
            }

            _mScrollHolePatch.Location = new Point(_mhScroll.Right, _mvScroll.Bottom);
            _mScrollHolePatch.Size = new Size(_mvScroll.Width, _mhScroll.Height);

            _RecalculateRectangle();
            _RecalculateMatrix();

            _mView.Invalidate();
        }

        /// <summary>
        /// Convert view coordinates to world coordinates
        /// </summary>
        /// <param name="screencoord"></param>
        /// <returns></returns>
        public Point ViewToWorldCoord(Point screencoord)
        {
            return new Point(screencoord.X + X, screencoord.Y + Y);
        }

        /// <summary>
        /// Convert view coordinates to world coordinates
        /// </summary>
        /// <param name="screencoord"></param>
        /// <returns></returns>
        public PointF ViewToWorldCoord(PointF screencoord)
        {
            return new PointF(screencoord.X + X, screencoord.Y + Y);
        }

        /// <summary>
        /// Convert world coordinates to view coordinates
        /// </summary>
        /// <param name="screencoord"></param>
        /// <returns></returns>
        public PointF WorldToViewCoord(PointF worldcoord)
        {
            return new PointF(worldcoord.X - X, worldcoord.Y - Y);
        }

        /// <summary>
        /// Get or set the world width
        /// </summary>
        public int WorldWidth
        {
            get { return _mWorldWidth; }
            set
            {
                if (!value.Equals(_mWorldWidth))
                {
                    if (value < _mView.Width) value = _mView.Width;
                    _mWorldWidth = value;
                    Resize();
                }
            }
        }

        /// <summary>
        /// Get or set the world height
        /// </summary>
        public int WorldHeight
        {
            get { return _mWorldHeight; }
            set
            {
                if (!value.Equals(_mWorldHeight))
                {
                    if (value < _mView.Height) value = _mView.Height;
                    _mWorldHeight = value;
                    Resize();
                }
            }
        }

        /// <summary>
        /// Get or set the world X coordinate of the Viewport location, represented by the top left corner of the Viewport Rectangle
        /// </summary>
        public int X
        {
            get { return _mhScroll.Value; }
            set
            {
                if (!value.Equals(_mhScroll.Value))
                {
                    if (value > _mhScroll.Maximum) value = _mhScroll.Maximum;
                    else if (value < 0) value = 0;
                    _mhScroll.Value = value;
                    _RecalculateRectangle();
                    _RecalculateMatrix();
                    _mView.Invalidate();
                }
            }
        }

        /// <summary>
        /// Get or set the wordl Y coordinate of the Viewport location, represented by the top left corner of the Viewport Rectangle
        /// </summary>
        public int Y
        {
            get { return _mvScroll.Value; }
            set
            {
                if (!value.Equals(_mvScroll.Value))
                {
                    if (value > _mvScroll.Maximum) value = _mvScroll.Maximum;
                    else if (value < 0) value = 0;
                    _mvScroll.Value = value;
                    _RecalculateRectangle();
                    _RecalculateMatrix();
                    _mView.Invalidate();
                }
            }
        }

        /// <summary>
        /// Get the X world coordinate of the right end of the Viewport
        /// </summary>
        public int Right
        {
            get { return _mRectangle.Right; }
        }

        /// <summary>
        /// Get the Y world coordinate of the bottom of the Viewport
        /// </summary>
        public int Bottom { get { return _mRectangle.Bottom; } }

        private void _RecalculateRectangle()
        {
            _mRectangle = new Rectangle(X, Y, _mView.Width, _mView.Height);
        }

        private void _RecalculateMatrix()
        {
            _mMatrix = new Matrix();
            _mMatrix.Translate(-X, -Y);
        }

        Control _mView;
        HScrollBar _mhScroll;
        VScrollBar _mvScroll;
        UserControl _mScrollHolePatch;
        Rectangle _mRectangle = Rectangle.Empty;
        Matrix _mMatrix = new Matrix();
        int _mWorldHeight, _mWorldWidth;
    }
}
