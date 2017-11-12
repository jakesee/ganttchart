using Braincase.GanttChart;
using NUnit.Framework;
using System.Windows.Forms;

namespace GanttChartNUnitTests
{
    /// <summary>
    ///This is a test class for ChartTest and is intended
    ///to contain all ChartTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class ChartTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [Test]
        public void AddChartToForm()
        {
            // add to form
            Form form = new Form();
            Chart chart = new Chart();
            form.Controls.Add(chart);

            // init chart
            var manager = new ProjectManager<Task, object>();
            chart.Init(manager);
        }

        /// <summary>
        ///A test for Init
        ///</summary>
        [Test]
        public void DeferredAddChartToForm()
        {
            Chart chart = new Chart();
            var manager = new ProjectManager<Task, object>();
            chart.Init(manager);

            // deferred add to form
            Form form = new Form();
            form.Controls.Add(chart);
        }

        /// <summary>
        /// Allow drawing without init
        /// </summary>
        [Test]
        public void DrawWithoutInit()
        {
            Chart chart = new Chart();
            Form form = new Form();

            // test: paint chart without initialization
            chart.Invalidate();
        }
    }
}
