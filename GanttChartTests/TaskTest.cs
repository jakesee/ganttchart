using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Braincase.GanttChart;

namespace GanttChartTests
{
    [TestClass]
    public class TaskTest
    {
        [TestMethod]
        public void SetTaskParameters()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set some initial values
            manager.SetStart(one, 22);
            manager.SetEnd(one, 24);
            manager.SetComplete(one, 0.3f);
            manager.SetCollapse(one, false);
            Assert.IsTrue(one.Start == 22);
            Assert.IsTrue(one.End == 24);
            Assert.IsTrue(one.Complete == 0.3f);
            Assert.IsTrue(one.IsCollapsed == false);
            Assert.IsTrue(one.Slack == 0);
            Assert.IsTrue(one.Duration == 2);

            // test: change the values and check again
            manager.SetStart(one, 34);
            manager.SetDuration(one, 56);
            manager.SetComplete(one, 0.9f);
            manager.SetCollapse(one, true); // should have no effect unless on group
            Assert.IsTrue(one.Start == 34);
            Assert.IsTrue(one.End == 90);
            Assert.IsTrue(one.Complete == 0.9f);
            Assert.IsTrue(one.IsCollapsed == false);
            Assert.IsTrue(one.Slack == 0);
            Assert.IsTrue(one.Duration == 56);
        }

        [TestMethod]
        public void RelateBeforePrecedentEndsLate()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: set task parameters and set relation
            manager.Relate(one, two);
            manager.SetStart(one, 10);
            manager.SetDuration(one, 5);
            manager.SetStart(two, 18);
            manager.SetDuration(two, 12);
            Assert.IsTrue(one.Start == 10);
            Assert.IsTrue(one.End == 15);
            Assert.IsTrue(one.Slack == 2, string.Format("Expected {0} != {1}", 2, one.Slack));
            Assert.IsTrue(two.Start == 18);
            Assert.IsTrue(two.End == 30);
            Assert.IsTrue(two.Slack == 0);
        }

        [TestMethod]
        public void AdjustDependantScheduleOnRelate()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: make 2 independant tasks
            manager.SetStart(one, 10);
            manager.SetDuration(one, 6);
            manager.SetStart(two, 10);
            manager.SetDuration(two, 10);
            Assert.IsTrue(one.Start == 10);
            Assert.IsTrue(one.End == 16);
            Assert.IsTrue(two.Start == 10);
            Assert.IsTrue(two.End == 20);

            // test: relate one and two; one before two
            manager.Relate(one, two);
            Assert.IsTrue(one.Start == 10);
            Assert.IsTrue(one.End == 16);
            Assert.IsTrue(two.Start == 17);
            Assert.IsTrue(two.End == 27);
        }

        [TestMethod]
        public void PrecendentPushingDependantLater()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: set task parameters and set relation
            manager.Relate(one, two);
            manager.SetStart(one, 10);
            manager.SetDuration(one, 5);
            manager.SetStart(two, 18);
            manager.SetDuration(two, 12);
            Assert.IsTrue(one.Start == 10);
            Assert.IsTrue(one.End == 15);
            Assert.IsTrue(one.Slack == 2, string.Format("Expected {0} != {1}", 2, one.Slack));
            Assert.IsTrue(two.Start == 18);
            Assert.IsTrue(two.End == 30);
            Assert.IsTrue(two.Slack == 0);
            
            // test: set one end date further than two start date
            manager.SetStart(one, 15);
            Assert.IsTrue(one.Start == 15);
            Assert.IsTrue(one.End == 20);
            Assert.IsTrue(one.Slack == 0, string.Format("Expected {0} != {1}", 1, one.Slack));
            Assert.IsTrue(two.Start == 21);
            Assert.IsTrue(two.End == 33, string.Format("Expected {0} != {1}", 33, two.End));
            Assert.IsTrue(one.Slack == 0);
        }

        [TestMethod]
        public void AdjustGroupScheduleOnMemberScheduleChange()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task() { Name = "group" };
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(group);
            manager.Add(one);
            manager.Add(two);

            // setup: create a group with 2 members and some initial schedule
            manager.Group(group, one);
            manager.Group(group, two);
            manager.SetStart(one, 10);
            manager.SetDuration(one, 6);
            manager.SetStart(two, 12);
            manager.SetEnd(two, 20);
            Assert.IsTrue(one.Start == 10);
            Assert.IsTrue(one.End == 16);
            Assert.IsTrue(two.Start == 12);
            Assert.IsTrue(two.Duration == 8);
            Assert.IsTrue(group.Start == 10);
            Assert.IsTrue(group.End == 20);
            Assert.IsTrue(group.Duration == 10, string.Format("Expected {0} != {1}", 10, group.Duration));

            // test: change the member schedule and confirm if the group schedule updates approperiately
            manager.SetStart(one, 26);
            Assert.IsTrue(group.Start == 12);
            Assert.IsTrue(group.End == 32);

            // test: change the member schedule back
            manager.SetStart(one, 10);
            Assert.IsTrue(group.Start == 10);
            Assert.IsTrue(group.End == 20);
            Assert.IsTrue(group.Duration == 10);
        }

        [TestMethod]
        public void CannotSetCompleteOnGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task() { Name = "group" };
            var member = new Task() { Name = "member" };
            manager.Add(group);
            manager.Add(member);

            // setup: set an initial complete on group
            manager.SetComplete(group, 0.345f);
            Assert.IsTrue(group.Complete == 0.345f);
            Assert.IsTrue(member.Complete == 0);

            // test: make a group and reset its complete status
            manager.Group(group, member);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(group.Complete == 0);
            Assert.IsTrue(member.Complete == 0);

            // test: set complete on a group (no effect);
            manager.SetComplete(group, 0.123f);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(group.Complete == 0);
            Assert.IsTrue(member.Complete == 0);
        }

        [TestMethod]
        public void AdjustGroupCompleteOnMemberCompleteChange()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task() { Name = "group" };
            var member = new Task() { Name = "member" };
            manager.Add(group);
            manager.Add(member);

            // setup: create a group
            manager.Group(group, member);
            Assert.IsTrue(member.Complete == 0);
            Assert.IsTrue(group.Complete == 0, string.Format("Expected {0} != {1}", 0, group.Complete));

            // test: set the complete on the member and 
            manager.SetComplete(member, 0.5f);
            Assert.IsTrue(member.Complete == 0.5f);
            Assert.IsTrue(group.Complete == 0.5f, string.Format("Expected {0} != {1}", 0.5f, group.Complete));
        }

        [TestMethod]
        public void AdjustGroupScheduleOnGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var one = new Task();
            manager.Add(group);
            manager.Add(one);

            // setup: set initial task parameters
            manager.SetStart(group, 6);
            manager.SetDuration(group, 5);
            manager.SetStart(one, 9);
            manager.SetEnd(one, 15);
            Assert.IsTrue(group.Start == 6);
            Assert.IsTrue(group.End == 11);
            Assert.IsTrue(one.Start == 9);
            Assert.IsTrue(one.Duration == 6);

            // test: make a group and group make have same parameters as one
            manager.Group(group, one);
            Assert.IsTrue(group.Start == one.Start);
            Assert.IsTrue(group.End == one.End);
            Assert.IsTrue(group.Duration == one.Duration);
            Assert.IsTrue(group.Complete == one.Complete);
        }

        [TestMethod]
        public void DurationCannotLessThanOne()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set initial value
            manager.SetStart(one, 14);
            manager.SetEnd(one, 20);
            Assert.IsTrue(one.Duration == 6);

            // test: moving the end point
            manager.SetEnd(one, 10);
            Assert.IsTrue(one.Duration == 1);

            // test: moving the duration point
            manager.SetDuration(one, -5);
            Assert.IsTrue(one.Duration == 1);
        }

        [TestMethod]
        public void CompleteMustBetweenZeroAndOne()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set initial value
            manager.SetStart(one, 14);
            manager.SetEnd(one, 20);
            manager.SetComplete(one, 0.5f);
            Assert.IsTrue(one.Duration == 6);
            Assert.IsTrue(one.Complete == 0.5f);

            // test: complete > 1 become 1
            manager.SetComplete(one, 25.4f);
            Assert.IsTrue(one.Complete == 1);

            // test: complete < 0 become 0
            manager.SetComplete(one, -33.8f);
            Assert.IsTrue(one.Complete == 0);
        }

        [TestMethod]
        public void DependantRescheduledStartEarlierThanPrecedentEnd()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: one before two
            manager.Relate(one, two);
            manager.SetStart(one, 5);
            manager.SetEnd(one, 10);
            manager.SetStart(two, 15);
            manager.SetDuration(two, 25);
            Assert.IsTrue(one.Start == 5);
            Assert.IsTrue(one.Duration == 5);
            Assert.IsTrue(two.Start == 15);
            Assert.IsTrue(two.End == 40);

            // test: reschedule two to start before end of one
            manager.SetStart(two, 8);
            Assert.IsTrue(one.Start == 5);
            Assert.IsTrue(one.Duration == 5);
            Assert.IsTrue(two.Start == 11);
            Assert.IsTrue(two.Duration == 25);
            Assert.IsTrue(two.End == 36);
        }

        [TestMethod]
        public void CriticalPath()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            var four = new Task();
            var five = new Task();
            var six = new Task();
            var seven = new Task();
            var eight = new Task();
            var nine = new Task();
            var group1 = new Task() { Name = "group1" };
            var group2 = new Task() { Name = "group2" };
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);
            manager.Add(four);
            manager.Add(five);
            manager.Add(six);
            manager.Add(seven);
            manager.Add(eight);
            manager.Add(nine);
            manager.Add(group1);
            manager.Add(group2);

            // setup: create some schedules and grouping, and confirm we laid it out correctly
            manager.Relate(one, two);
            manager.Relate(one, three);
            manager.Relate(three, four);
            manager.Relate(five, six);
            manager.Relate(six, four);
            manager.Relate(six, seven);
            manager.Relate(six, eight);
            manager.Relate(eight, nine);
            manager.Group(group1, one);
            manager.Group(group1, two);
            manager.Group(group1, three);
            manager.Group(group1, four);
            manager.Group(group2, five);
            manager.Group(group2, six);
            manager.Group(group2, seven);
            manager.Group(group2, eight);
            manager.SetStart(one, 0); manager.SetEnd(one, 3);
            manager.SetStart(two, 4); manager.SetEnd(two, 7);
            manager.SetStart(three, 5); manager.SetEnd(three, 8);
            manager.SetStart(four, 10); manager.SetEnd(four, 15);
            manager.SetStart(five, 1); manager.SetEnd(five, 5);
            manager.SetStart(six, 6); manager.SetEnd(six, 9);
            manager.SetStart(seven, 12); manager.SetEnd(seven, 15);
            manager.SetStart(eight, 11); manager.SetEnd(eight, 16);
            manager.SetStart(nine, 17); manager.SetEnd(nine, 20);
            Assert.IsTrue(one.Start == 0 && one.Duration == 3, string.Format("Expected ({0}, {1}) != ({2}, {3})", 0, 3, one.Start, one.Duration));
            Assert.IsTrue(two.Start == 4 && two.Duration == 3);
            Assert.IsTrue(three.Start == 5 && three.Duration == 3);
            Assert.IsTrue(four.Start == 10 && four.Duration == 5);
            Assert.IsTrue(five.Start == 1 && five.Duration == 4);
            Assert.IsTrue(six.Start == 6 && six.Duration == 3);
            Assert.IsTrue(seven.Start == 12 && seven.Duration == 3);
            Assert.IsTrue(eight.Start == 11 && eight.Duration == 5);
            Assert.IsTrue(nine.Start == 17 && nine.Duration == 3, string.Format("Expected ({0}, {1}) != ({2}, {3})", 17, 3, nine.Start, nine.Duration));
            var iter_nine = manager.PrecedentsOf(nine).GetEnumerator(); // nine is critical in this setup
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(eight));
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(six));
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(five));
            Assert.IsTrue(iter_nine.MoveNext() == false);

            // test: change the critical path and see if it gets updated correctly
            manager.SetStart(four, 17); // four now ends later than 9;
            var critical_paths = manager.CriticalPaths; // should have 2 paths: 4 to 1; and 4 to 5;
            Assert.IsTrue(critical_paths.Count() == 2, string.Format("Expected {0} != {1}", 2, critical_paths.Count()));
            var critical_tasks = critical_paths.SelectMany(x => x);
            Assert.IsTrue(critical_tasks.Distinct().Count() == 6, string.Format("Expected {0} != {1}", 5, critical_tasks.Distinct().Count())); // 4, 3, 1, 6, 5, group1
            Assert.IsTrue(critical_tasks.Contains(four));
            Assert.IsTrue(critical_tasks.Contains(three));
            Assert.IsTrue(critical_tasks.Contains(one));
            Assert.IsTrue(critical_tasks.Contains(six));
            Assert.IsTrue(critical_tasks.Contains(five));
            Assert.IsTrue(critical_tasks.Contains(group1));
        }
    }
}
