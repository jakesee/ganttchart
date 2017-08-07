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
        ProjectManager _mProject;

        public ExampleSimple()
        {
            InitializeComponent();

            _mProject = new ProjectManager();
            _mProject.Add(new Task() { Name = "New Task" });
            _mChart.Init(_mProject);
        }
    }
}
