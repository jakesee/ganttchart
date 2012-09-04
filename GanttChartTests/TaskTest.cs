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

        [TestMethod]
        public void TaskStartLessThanEnd()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var task = new Task();
            manager.Add(task);

            // test: ensure end and start must conserve precedence
            manager.SetStart(task, 10);
            manager.SetEnd(task, 5);
            Assert.IsTrue(task.Start < task.End);

            // test: ensure negative case holds
            manager.SetStart(task, -9);
            manager.SetEnd(task, -15);
            Assert.IsTrue(task.Start < task.End);
        }

        [TestMethod]
        public void SetSplitSchedule()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a regular task of 30 duration
            manager.SetStart(split, 12);
            manager.SetDuration(split, 30);
            Assert.IsTrue(split.Start == 12);
            Assert.IsTrue(split.Duration == 30);
            Assert.IsTrue(split.End == 42);

            // test: split task and get the correct 10 unit duration back for each part
            manager.Split(split, part1, part2, 10);
            manager.Split(part2, part3, 10);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 12);
            Assert.IsTrue(part1.End == 22);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 23);
            Assert.IsTrue(part2.End == 33);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 34);
            Assert.IsTrue(part3.End == 44);

            Assert.IsTrue(split.Start == 12);
            Assert.IsTrue(split.Duration == 32);
            Assert.IsTrue(split.End == 44);
        }

        [TestMethod]
        public void MovePartLaterStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split
            manager.SetStart(split, 0);
            manager.SetDuration(split, 30);
            manager.Split(split, part1, part2, 10);
            manager.Split(part2, part3, 10);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 11);
            Assert.IsTrue(part2.End == 21);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 22);
            Assert.IsTrue(part3.End == 32);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 32);
            Assert.IsTrue(split.End == 32);

            // test: move part3 to a later start
            manager.SetStart(part3, 40);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 11);
            Assert.IsTrue(part2.End == 21);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 40);
            Assert.IsTrue(part3.End == 50);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 50);
            Assert.IsTrue(split.End == 50);
        }

        [TestMethod]
        public void MovePartEarlierStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split with extra interrupt in between
            manager.SetStart(split, 0);
            manager.SetDuration(split, 30);
            manager.Split(split, part1, part2, 10);
            manager.Split(part2, part3, 10);
            manager.SetStart(part2, 16);
            manager.SetStart(part3, 32);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 16);
            Assert.IsTrue(part2.End == 26);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 32);
            Assert.IsTrue(part3.End == 42);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 42);
            Assert.IsTrue(split.End == 42);

            // test: start part3 slight earlier
            manager.SetStart(part3, 28);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 16);
            Assert.IsTrue(part2.End == 26);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 28);
            Assert.IsTrue(part3.End == 38);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 38);
            Assert.IsTrue(split.End == 38);
        }

        [TestMethod]
        public void MovePartEarlierOverlap()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split with extra interrupt in between
            manager.SetStart(split, 0);
            manager.SetDuration(split, 30);
            manager.Split(split, part1, part2, 10);
            manager.Split(part2, part3, 10);
            manager.SetStart(part2, 16);
            manager.SetStart(part3, 32);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 16);
            Assert.IsTrue(part2.End == 26);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 32);
            Assert.IsTrue(part3.End == 42);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 42);
            Assert.IsTrue(split.End == 42);

            // test: move part3 earlier, should pack parts to get earliest end date while start date remain
            manager.SetStart(part3, 26);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 15);
            Assert.IsTrue(part2.End == 25);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 26);
            Assert.IsTrue(part3.End == 36);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 36);
            Assert.IsTrue(split.End == 36);

            // test: move part3 even earlier, tight packing
            manager.SetStart(part3, 1);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 11);
            Assert.IsTrue(part2.End == 21);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 22);
            Assert.IsTrue(part3.End == 32);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 32);
            Assert.IsTrue(split.End == 32);
        }

        [TestMethod]
        public void MovePartLaterOverlap()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split
            manager.SetStart(split, 0);
            manager.SetDuration(split, 30);
            manager.Split(split, part1, part2, 10);
            manager.Split(part2, part3, 10);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part1.End == 10);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 11);
            Assert.IsTrue(part2.End == 21);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 22);
            Assert.IsTrue(part3.End == 32);

            Assert.IsTrue(split.Start == 0);
            Assert.IsTrue(split.Duration == 32);
            Assert.IsTrue(split.End == 32);

            // test: move part1 to overlap with part2, should pack parts to get earliest end date
            // with at least 1 time unit interrupt between each part
            manager.SetStart(part1, 12);
            Assert.IsTrue(part1.Duration == 10);
            Assert.IsTrue(part1.Start == 12);
            Assert.IsTrue(part1.End == 22);

            Assert.IsTrue(part2.Duration == 10);
            Assert.IsTrue(part2.Start == 23);
            Assert.IsTrue(part2.End == 33);

            Assert.IsTrue(part3.Duration == 10);
            Assert.IsTrue(part3.Start == 34);
            Assert.IsTrue(part3.End == 44);

            Assert.IsTrue(split.Start == 12);
            Assert.IsTrue(split.Duration == 32);
            Assert.IsTrue(split.End == 44);
        }

        [TestMethod]
        public void SetSplitTaskStartMaintainPartsRelativeSchedule()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a split task
            manager.Split(split, part1, part2, 1);
            manager.Split(part2, part3, 1);
            manager.SetStart(part1, 0);
            manager.SetStart(part2, 15);
            manager.SetStart(part3, 30);
            manager.SetDuration(part1, 3);
            manager.SetDuration(part2, 4);
            manager.SetDuration(part3, 5);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part2.Start == 15);
            Assert.IsTrue(part3.Start == 30);
            Assert.IsTrue(part1.Duration == 3);
            Assert.IsTrue(part2.Duration == 4);
            Assert.IsTrue(part3.Duration == 5);
            Assert.IsTrue(part1.End == 3);
            Assert.IsTrue(part2.End == 19);
            Assert.IsTrue(part3.End == 35);
            
            // test: move the split task itself - maintain interrupts
            var offset = 10 - split.Start;
            manager.SetStart(split, 10);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == 0 + offset);
            Assert.IsTrue(part2.Start == 15 + offset);
            Assert.IsTrue(part3.Start == 30 + offset);
            Assert.IsTrue(part1.Duration == 3);
            Assert.IsTrue(part2.Duration == 4);
            Assert.IsTrue(part3.Duration == 5);
            Assert.IsTrue(part1.End == 3 + offset);
            Assert.IsTrue(part2.End == 19 + offset);
            Assert.IsTrue(part3.End == 35 + offset);
        }

        [TestMethod]
        public void SetSplitTaskEndLastTaskMinimun1UnitTime()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a split task
            manager.Split(split, part1, part2, 1);
            manager.Split(part2, part3, 1);
            manager.SetStart(part1, 0);
            manager.SetStart(part2, 15);
            manager.SetStart(part3, 30);
            manager.SetDuration(part1, 3);
            manager.SetDuration(part2, 4);
            manager.SetDuration(part3, 5);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part2.Start == 15);
            Assert.IsTrue(part3.Start == 30);
            Assert.IsTrue(part1.Duration == 3);
            Assert.IsTrue(part2.Duration == 4);
            Assert.IsTrue(part3.Duration == 5);
            Assert.IsTrue(part1.End == 3);
            Assert.IsTrue(part2.End == 19);
            Assert.IsTrue(part3.End == 35);

            // test: set the split task end - last part end to increase or decrease,
            // resulting no less than 1 unit duration
            manager.SetEnd(split, 1);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == 0);
            Assert.IsTrue(part2.Start == 15);
            Assert.IsTrue(part3.Start == 30);
            Assert.IsTrue(part1.Duration == 3);
            Assert.IsTrue(part2.Duration == 4);
            Assert.IsTrue(part3.Duration == 1);
            Assert.IsTrue(part1.End == 3);
            Assert.IsTrue(part2.End == 19);
            Assert.IsTrue(part3.End == 31);
        }

        [TestMethod]
        public void DeleteResourceOnDeleteTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var task1 = new Task();
            var task2 = new Task();
            var r1 = new Task();
            manager.Add(task1);
            manager.Add(task2);

            // setup: assign resource
            manager.Assign(task1, r1);
            manager.Assign(task2, r1);
            Assert.IsTrue(manager.ResourcesOf(task1).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(task2).Contains(r1));
            Assert.IsTrue(manager.TasksOf(r1).Count() == 2);

            // test: delete task1
            manager.Delete(task1);
            Assert.IsTrue(!manager.ResourcesOf(task1).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(task2).Contains(r1));
            Assert.IsTrue(manager.TasksOf(r1).Count() == 1);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.Resources.Contains(r1));

            // test: delete task2
            manager.Delete(task2);
            Assert.IsTrue(!manager.ResourcesOf(task1).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(task2).Contains(r1));
            Assert.IsTrue(manager.TasksOf(r1).Count() == 0);
            Assert.IsTrue(manager.Resources.Count() == 0);
            Assert.IsTrue(!manager.Resources.Contains(r1));
        }
        
        [TestMethod]
        public void TransferResourceToSplitTaskOnMerge_ByDeleteInThisCase()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var r1 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, 1);
            manager.Assign(part1, r1);
            manager.Assign(part2, r1);
            Assert.IsTrue(!manager.TasksOf(r1).Contains(split));
            Assert.IsTrue(manager.TasksOf(r1).Contains(part1));
            Assert.IsTrue(manager.TasksOf(r1).Contains(part2));
            Assert.IsTrue(!manager.ResourcesOf(split).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(part1).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(part2).Contains(r1));
            Assert.IsTrue(manager.Resources.Count() == 1);

            // test: delete a part, expect a merge and resources transferred to split task
            manager.Delete(part1);
            Assert.IsTrue(manager.TasksOf(r1).Contains(split));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(part1));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(part2));
            Assert.IsTrue(manager.ResourcesOf(split).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(part1).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(part2).Contains(r1));
            Assert.IsTrue(manager.Resources.Count() == 1);
        }
        
        [TestMethod]
        public void DeletePartCausingMerge()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, 1);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));

            // test: delete a part, expect a merge to regular task
            manager.Delete(part1);
            Assert.IsTrue(!manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(part1));
            Assert.IsTrue(!manager.IsPart(part2));
        }
        
        [TestMethod]
        public void DeleteResourcesOnDeleteSplitTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var r1 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, 1);
            manager.Assign(part1, r1);
            manager.Assign(part2, r1);
            Assert.IsTrue(!manager.TasksOf(r1).Contains(split));
            Assert.IsTrue(manager.TasksOf(r1).Contains(part1));
            Assert.IsTrue(manager.TasksOf(r1).Contains(part2));
            Assert.IsTrue(!manager.ResourcesOf(split).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(part1).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(part2).Contains(r1));
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // test: delete the split task, everything should go
            manager.Delete(split);
            Assert.IsTrue(manager.Resources.Count() == 0);
            Assert.IsTrue(manager.Tasks.Count() == 0);
            Assert.IsTrue(!manager.TasksOf(r1).Contains(split));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(part1));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(part2));
            Assert.IsTrue(!manager.ResourcesOf(split).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(part1).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(part2).Contains(r1));

            // test: check that they are really deleted by adding them back again
            manager.Add(split);
            manager.Add(part1);
            manager.Add(part2);
            Assert.IsTrue(manager.Tasks.Count() == 3);
        }
        
        [TestMethod]
        public void DeletePartsOnDeleteSplitTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, 1);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // test: ensure that we cannot add the tasks parts and split again
            manager.Add(split);
            manager.Add(part1);
            manager.Add(part2);
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // test: delete a part, expect a merge to regular task
            manager.Delete(split);
            Assert.IsTrue(!manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(part1));
            Assert.IsTrue(!manager.IsPart(part2));
            Assert.IsTrue(manager.Tasks.Count() == 0);

            // test: if really deleted, we should be able to add them back
            manager.Add(split);
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // test: now we should be able to add again
            manager.Add(split);
            manager.Add(part1);
            manager.Add(part2);
            Assert.IsTrue(manager.Tasks.Count() == 3);
        }

        [TestMethod]
        public void SplitExistingSplitTaskNoEffect()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create split task
            manager.Split(split, part1, part2, 1);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(split));

            Assert.IsTrue(!manager.IsSplit(part1));
            Assert.IsTrue(manager.IsPart(part1));

            Assert.IsTrue(!manager.IsSplit(part2));
            Assert.IsTrue(manager.IsPart(part2));

            Assert.IsTrue(!manager.IsSplit(part3));
            Assert.IsTrue(!manager.IsPart(part3));

            // test: try to split the split task again - no effect.
            manager.Split(split, part3, 1);
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(split));

            Assert.IsTrue(!manager.IsSplit(part1));
            Assert.IsTrue(manager.IsPart(part1));

            Assert.IsTrue(!manager.IsSplit(part2));
            Assert.IsTrue(manager.IsPart(part2));

            Assert.IsTrue(!manager.IsSplit(part3));
            Assert.IsTrue(!manager.IsPart(part3));
            
        }

        [TestMethod]
        public void CannotStartPartBeforeSplitTaskPrecedentStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var precedent = new Task();
            manager.Add(split);
            manager.Add(precedent);

            // setup: create a split task with precendent
            manager.SetStart(precedent, 5);
            manager.SetDuration(precedent, 5);
            manager.SetStart(split, 15);
            manager.SetDuration(split, 5);
            manager.Relate(precedent, split);
            manager.Split(split, part1, part2, 2);
            Assert.IsTrue(manager.PrecedentsOf(split).Contains(precedent));
            Assert.IsTrue(manager.PartsOf(split).Contains(part1));
            Assert.IsTrue(manager.PartsOf(split).Contains(part2));
            Assert.IsTrue(precedent.End == 10);
            Assert.IsTrue(split.Start == 15);
            Assert.IsTrue(part1.Start == 15);

            // test: move part1 start before precendent start
            manager.SetStart(part1, 8);
            Assert.IsTrue(manager.PrecedentsOf(split).Contains(precedent));
            Assert.IsTrue(manager.PartsOf(split).Contains(part1));
            Assert.IsTrue(manager.PartsOf(split).Contains(part2));
            Assert.IsTrue(part1.Start == 11);
            Assert.IsTrue(split.Start == 11);
        }

        [TestMethod]
        public void SetPartCompleteUpdatesSplitTaskAndGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task() { Name = "split" };
            var part1 = new Task() { Name = "part1" };
            var part2 = new Task() { Name = "part2" };
            var group = new Task() { Name = "group" };
            var member = new Task() { Name = "member" };
            manager.Add(split);
            manager.Add(member);
            manager.Add(group);

            // setup: create split task under a group
            manager.SetDuration(split, 6);
            manager.SetDuration(member, 14);
            manager.Group(group, split);
            manager.Group(group, member);
            manager.Split(split, part1, part2, 2);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(manager.IsMember(member));
            Assert.IsTrue(manager.IsMember(split));

            Assert.IsTrue(group.Complete == 0);
            Assert.IsTrue(split.Complete == 0);
            Assert.IsTrue(part1.Complete == 0);
            Assert.IsTrue(part2.Complete == 0);

            Assert.IsTrue(group.Duration == 14);
            Assert.IsTrue(member.Duration == 14);
            Assert.IsTrue(split.Duration == 7);
            Assert.IsTrue(part1.Duration == 2);
            Assert.IsTrue(part2.Duration == 4);

            // test: set complete for part1 and see group and split complete increase
            manager.SetComplete(part1, 0.3f);
            Assert.IsTrue(Math.Abs(split.Complete - (0.3f * 2) / 6.0f) < 0.01f);
            Assert.IsTrue(Math.Abs(group.Complete - (split.Complete * 7) / 20.0f) < 0.01f);
            
        }
    }
}
