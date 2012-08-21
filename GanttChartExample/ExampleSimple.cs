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
    public partial class ExampleSimple : Form
    {
        Project _mProject;

        public ExampleSimple()
        {
            InitializeComponent();

            _mProject = new Project();
            _mProject.CreateTask();
            _mChart.Init(_mProject);

            this.AutoScroll = true;
        }

        protected override void OnResize(EventArgs e)
        {
            _mChart.Invalidate();

            base.OnResize(e);
        }
    }
}
