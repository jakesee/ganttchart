using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Braincase.GanttChart;

namespace GanttChartNUnitTests
{
    [TestFixture]
    public class TaskTest
    {
        [Test]
        public void SetTaskParameters()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set some initial values
            manager.SetStart(one, TimeSpan.FromDays(22));
            manager.SetEnd(one, TimeSpan.FromDays(24));
            manager.SetComplete(one, 0.3f);
            manager.SetCollapse(one, false);
            Assert.IsTrue(one.Start == TimeSpan.FromDays(22));
            Assert.IsTrue(one.End == TimeSpan.FromDays(24));
            Assert.IsTrue(one.Complete == 0.3f);
            Assert.IsTrue(one.IsCollapsed == false);
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(0));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(2));

            // test: change the values and check again
            manager.SetStart(one, TimeSpan.FromDays(34));
            manager.SetDuration(one, TimeSpan.FromDays(56));
            manager.SetComplete(one, 0.9f);
            manager.SetCollapse(one, true); // should have no effect unless on group
            Assert.IsTrue(one.Start == TimeSpan.FromDays(34));
            Assert.IsTrue(one.End == TimeSpan.FromDays(90));
            Assert.IsTrue(one.Complete == 0.9f);
            Assert.IsTrue(one.IsCollapsed == false);
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(0));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(56));
        }

        [Test]
        public void RelateBeforePrecedentEndsLate()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: set task parameters and set relation
            manager.Relate(one, two);
            manager.SetStart(one, TimeSpan.FromDays(10));
            manager.SetDuration(one, TimeSpan.FromDays(5));
            manager.SetStart(two, TimeSpan.FromDays(18));
            manager.SetDuration(two, TimeSpan.FromDays(12));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(one.End == TimeSpan.FromDays(15));
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(3), string.Format("Expected {0} != {1}", 2, one.Slack));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(18));
            Assert.IsTrue(two.End == TimeSpan.FromDays(30));
            Assert.IsTrue(two.Slack == TimeSpan.FromDays(0));
        }

        [Test]
        public void AdjustDependantScheduleOnRelate()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: make 2 independant tasks
            manager.SetStart(one, TimeSpan.FromDays(10));
            manager.SetDuration(one, TimeSpan.FromDays(6));
            manager.SetStart(two, TimeSpan.FromDays(10));
            manager.SetDuration(two, TimeSpan.FromDays(10));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(one.End == TimeSpan.FromDays(16));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(two.End == TimeSpan.FromDays(20));

            // test: relate one and two; one before two
            manager.Relate(one, two);
            Assert.IsTrue(one.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(one.End == TimeSpan.FromDays(16));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(16));
            Assert.IsTrue(two.End == TimeSpan.FromDays(26));
        }

        [Test]
        public void PrecendentPushingDependantLater()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "two" };
            manager.Add(one);
            manager.Add(two);

            // setup: set task parameters and set relation
            manager.Relate(one, two);
            manager.SetStart(one, TimeSpan.FromDays(10));
            manager.SetDuration(one, TimeSpan.FromDays(5));
            manager.SetStart(two, TimeSpan.FromDays(18));
            manager.SetDuration(two, TimeSpan.FromDays(12));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(one.End == TimeSpan.FromDays(15));
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(3), string.Format("Expected {0} != {1}", 3, one.Slack));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(18));
            Assert.IsTrue(two.End == TimeSpan.FromDays(30));
            Assert.IsTrue(two.Slack == TimeSpan.FromDays(0));
            
            // test: set one end date further than two start date
            manager.SetStart(one, TimeSpan.FromDays(15));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(one.End == TimeSpan.FromDays(20));
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(0), string.Format("Expected {0} != {1}", 0, one.Slack));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(two.End == TimeSpan.FromDays(32), string.Format("Expected {0} != {1}", 32, two.End));
            Assert.IsTrue(one.Slack == TimeSpan.FromDays(0));
        }

        [Test]
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
            manager.SetStart(one, TimeSpan.FromDays(10));
            manager.SetDuration(one, TimeSpan.FromDays(6));
            manager.SetStart(two, TimeSpan.FromDays(12));
            manager.SetEnd(two, TimeSpan.FromDays(20));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(one.End == TimeSpan.FromDays(16));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(two.Duration == TimeSpan.FromDays(8));
            Assert.IsTrue(group.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(group.End == TimeSpan.FromDays(20));
            Assert.IsTrue(group.Duration == TimeSpan.FromDays(10), string.Format("Expected {0} != {1}", 10, group.Duration));

            // test: change the member schedule and confirm if the group schedule updates approperiately
            manager.SetStart(one, TimeSpan.FromDays(26));
            Assert.IsTrue(group.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(group.End == TimeSpan.FromDays(32));

            // test: change the member schedule back
            manager.SetStart(one, TimeSpan.FromDays(10));
            Assert.IsTrue(group.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(group.End == TimeSpan.FromDays(20));
            Assert.IsTrue(group.Duration == TimeSpan.FromDays(10));
        }

        [Test]
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

        [Test]
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

        [Test]
        public void AdjustGroupScheduleOnGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var one = new Task();
            manager.Add(group);
            manager.Add(one);

            // setup: set initial task parameters
            manager.SetStart(group, TimeSpan.FromDays(6));
            manager.SetDuration(group, TimeSpan.FromDays(5));
            manager.SetStart(one, TimeSpan.FromDays(9));
            manager.SetEnd(one, TimeSpan.FromDays(15));
            Assert.IsTrue(group.Start == TimeSpan.FromDays(6));
            Assert.IsTrue(group.End == TimeSpan.FromDays(11));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(9));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(6));

            // test: make a group and group make have same parameters as one
            manager.Group(group, one);
            Assert.IsTrue(group.Start == one.Start);
            Assert.IsTrue(group.End == one.End);
            Assert.IsTrue(group.Duration == one.Duration);
            Assert.IsTrue(group.Complete == one.Complete);
        }

        [Test]
        public void DurationCannotLessThanZero()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set initial value
            manager.SetStart(one, TimeSpan.FromDays(14));
            manager.SetEnd(one, TimeSpan.FromDays(20));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(6));

            // test: moving the end point
            manager.SetEnd(one, TimeSpan.FromDays(10));
            Assert.IsTrue(one.Duration > TimeSpan.Zero);

            // test: moving the duration point
            manager.SetDuration(one, TimeSpan.FromDays(-5));
            Assert.IsTrue(one.Duration > TimeSpan.Zero);
        }

        [Test]
        public void CompleteMustBetweenZeroAndOne()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // setup: set initial value
            manager.SetStart(one, TimeSpan.FromDays(14));
            manager.SetEnd(one, TimeSpan.FromDays(20));
            manager.SetComplete(one, 0.5f);
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(6));
            Assert.IsTrue(one.Complete == 0.5f);

            // test: complete > 1 become 1
            manager.SetComplete(one, 25.4f);
            Assert.IsTrue(one.Complete == 1);

            // test: complete < 0 become 0
            manager.SetComplete(one, -33.8f);
            Assert.IsTrue(one.Complete == 0);
        }

        [Test]
        public void DependantRescheduledStartEarlierThanPrecedentEnd()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: one before two
            manager.Relate(one, two);
            manager.SetStart(one, TimeSpan.FromDays(5));
            manager.SetEnd(one, TimeSpan.FromDays(10));
            manager.SetStart(two, TimeSpan.FromDays(15));
            manager.SetDuration(two, TimeSpan.FromDays(25));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(5));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(two.End == TimeSpan.FromDays(40));

            // test: reschedule two to start before end of one
            manager.SetStart(two, TimeSpan.FromDays(8));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(5));
            Assert.IsTrue(one.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(two.Duration == TimeSpan.FromDays(25));
            Assert.IsTrue(two.End == TimeSpan.FromDays(35));
        }

        [Test]
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
            manager.SetStart(one, TimeSpan.FromDays(0)); manager.SetEnd(one, TimeSpan.FromDays(3));
            manager.SetStart(two, TimeSpan.FromDays(4)); manager.SetEnd(two, TimeSpan.FromDays(7));
            manager.SetStart(three, TimeSpan.FromDays(5)); manager.SetEnd(three, TimeSpan.FromDays(8));
            manager.SetStart(four, TimeSpan.FromDays(10)); manager.SetEnd(four, TimeSpan.FromDays(15));
            manager.SetStart(five, TimeSpan.FromDays(1)); manager.SetEnd(five, TimeSpan.FromDays(5));
            manager.SetStart(six, TimeSpan.FromDays(6)); manager.SetEnd(six, TimeSpan.FromDays(9));
            manager.SetStart(seven, TimeSpan.FromDays(12)); manager.SetEnd(seven, TimeSpan.FromDays(15));
            manager.SetStart(eight, TimeSpan.FromDays(11)); manager.SetEnd(eight, TimeSpan.FromDays(16));
            manager.SetStart(nine, TimeSpan.FromDays(17)); manager.SetEnd(nine, TimeSpan.FromDays(20));
            Assert.IsTrue(one.Start == TimeSpan.FromDays(0) && one.Duration == TimeSpan.FromDays(3), string.Format("Expected ({0}, {1}) != ({2}, {3})", 0, 3, one.Start, one.Duration));
            Assert.IsTrue(two.Start == TimeSpan.FromDays(4) && two.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(three.Start == TimeSpan.FromDays(5) && three.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(four.Start == TimeSpan.FromDays(10) && four.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(five.Start == TimeSpan.FromDays(1) && five.Duration == TimeSpan.FromDays(4));
            Assert.IsTrue(six.Start == TimeSpan.FromDays(6) && six.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(seven.Start == TimeSpan.FromDays(12) && seven.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(eight.Start == TimeSpan.FromDays(11) && eight.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(nine.Start == TimeSpan.FromDays(17) && nine.Duration == TimeSpan.FromDays(3), string.Format("Expected ({0}, {1}) != ({2}, {3})", 17, 3, nine.Start, nine.Duration));
            var iter_nine = manager.PrecedentsOf(nine).GetEnumerator(); // nine is critical in this setup
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(eight));
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(six));
            iter_nine.MoveNext(); Assert.IsTrue(iter_nine.Current.Equals(five));
            Assert.IsTrue(iter_nine.MoveNext() == false);

            // test: change the critical path and see if it gets updated correctly
            manager.SetStart(four, TimeSpan.FromDays(17)); // four now ends later than 9;
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

        [Test]
        public void TaskStartLessThanEnd()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var task = new Task();
            manager.Add(task);

            // test: ensure end and start must conserve precedence
            manager.SetStart(task, TimeSpan.FromDays(10));
            manager.SetEnd(task, TimeSpan.FromDays(5));
            Assert.IsTrue(task.Start < task.End);

            // test: ensure negative case holds
            manager.SetStart(task, TimeSpan.FromDays(-9));
            manager.SetEnd(task, TimeSpan.FromDays(-15));
            Assert.IsTrue(task.Start < task.End);
        }

        [Test]
        public void SetSplitSchedule()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a regular task of 30 duration
            manager.SetStart(split, TimeSpan.FromDays(12));
            manager.SetDuration(split, TimeSpan.FromDays(30));
            Assert.IsTrue(split.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(42));

            // test: split task and get the correct 10 unit duration back for each part
            manager.Split(split, part1, part2, TimeSpan.FromDays(10));
            manager.Split(part2, part3, TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(22));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(22));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(32));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(32));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(42));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(42));
        }

        [Test]
        public void MovePartLaterStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split
            manager.SetStart(split, TimeSpan.FromDays(0));
            manager.SetDuration(split, TimeSpan.FromDays(30));
            manager.Split(split, part1, part2, TimeSpan.FromDays(10));
            manager.Split(part2, part3, TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(30));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(30));

            // test: move part3 to a later start
            manager.SetStart(part3, TimeSpan.FromDays(40));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(40));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(50));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(50));
            Assert.IsTrue(split.End == TimeSpan.FromDays(50));
        }

        [Test]
        public void MovePartEarlierStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split with extra interrupt in between
            manager.SetStart(split, TimeSpan.FromDays(0));
            manager.SetDuration(split, TimeSpan.FromDays(30));
            manager.Split(split, part1, part2, TimeSpan.FromDays(10));
            manager.Split(part2, part3, TimeSpan.FromDays(10));
            manager.SetStart(part2, TimeSpan.FromDays(16));
            manager.SetStart(part3, TimeSpan.FromDays(32));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(16));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(26));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(32));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(42));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(42));
            Assert.IsTrue(split.End == TimeSpan.FromDays(42));

            // test: start part3 slight earlier
            manager.SetStart(part3, TimeSpan.FromDays(28));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(16));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(26));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(28));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(38));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(38));
            Assert.IsTrue(split.End == TimeSpan.FromDays(38));
        }

        [Test]
        public void MovePartEarlierOverlap()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split with extra interrupt in between
            manager.SetStart(split, TimeSpan.FromDays(0));
            manager.SetDuration(split, TimeSpan.FromDays(30));
            manager.Split(split, part1, part2, TimeSpan.FromDays(10));
            manager.Split(part2, part3, TimeSpan.FromDays(10));
            manager.SetStart(part2, TimeSpan.FromDays(16));
            manager.SetStart(part3, TimeSpan.FromDays(32));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(16));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(26));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(32));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(42));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(42));
            Assert.IsTrue(split.End == TimeSpan.FromDays(42));

            // test: move part3 earlier, should pack parts to get earliest end date while start date remain
            manager.SetStart(part3, TimeSpan.FromDays(24));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(14));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(24));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(24));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(34));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(34));
            Assert.IsTrue(split.End == TimeSpan.FromDays(34));

            // test: move part3 even earlier, tight packing
            manager.SetStart(part3, TimeSpan.FromDays(1));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(30));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(30));
        }

        [Test]
        public void MovePartLaterOverlap()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a 3 part split
            manager.SetStart(split, TimeSpan.FromDays(0));
            manager.SetDuration(split, TimeSpan.FromDays(30));
            manager.Split(split, part1, part2, TimeSpan.FromDays(10));
            manager.Split(part2, part3, TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(10));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(30));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(30));

            // test: move part1 to overlap with part2, should pack parts to get earliest end date
            manager.SetStart(part1, TimeSpan.FromDays(12));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(22));

            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(22));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(32));

            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(32));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(42));

            Assert.IsTrue(split.Start == TimeSpan.FromDays(12));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(30));
            Assert.IsTrue(split.End == TimeSpan.FromDays(42));
        }

        [Test]
        public void MovePartLaterThanDependantStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var dependant = new Task(); 
            manager.Add(split);
            manager.Add(dependant);

            // setup: create a dependant on the split task
            manager.SetDuration(split, TimeSpan.FromDays(20));
            manager.SetDuration(dependant, TimeSpan.FromDays(7));
            manager.Split(split, part1, part2, TimeSpan.FromDays(15));
            manager.Relate(split, dependant);
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));
            Assert.IsTrue(split.End == TimeSpan.FromDays(20));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(5));

            //// test: adjust part2 duration beyond dependant start
            manager.SetEnd(part2, TimeSpan.FromDays(25));
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(25));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(25));
            Assert.IsTrue(split.End == TimeSpan.FromDays(25));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));

            //// test: adjust part2 start such that end beyond dependant start
            manager.SetStart(part2, TimeSpan.FromDays(30));
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(40));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(40));
            Assert.IsTrue(split.End == TimeSpan.FromDays(40));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
        }
        [Test]
        public void SplitPartWithDependantAndAdjustDependantStart()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var dependant = new Task();
            manager.Add(split);
            manager.Add(dependant);

            // setup: create a dependant on the split task
            manager.SetDuration(split, TimeSpan.FromDays(20));
            manager.SetDuration(dependant, TimeSpan.FromDays(7));
            manager.Relate(split, dependant);
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(20));

            // test: split and dependant should not be affected
            manager.Split(split, part1, part2, TimeSpan.FromDays(15));
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(20));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(20));
            Assert.IsTrue(split.End == TimeSpan.FromDays(20));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(5));

            // test: increase split end, expect part2 to increase duration and dependant to start later
            manager.SetEnd(split, TimeSpan.FromDays(25));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(25));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(10));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(dependant.Start == TimeSpan.FromDays(25));

        }

        [Test]
        public void SetSplitTaskStartMaintainPartsRelativeSchedule()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a split task
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
            manager.Split(part2, part3, TimeSpan.FromDays(1));
            manager.SetStart(part1, TimeSpan.FromDays(0));
            manager.SetStart(part2, TimeSpan.FromDays(15));
            manager.SetStart(part3, TimeSpan.FromDays(30));
            manager.SetDuration(part1, TimeSpan.FromDays(3));
            manager.SetDuration(part2, TimeSpan.FromDays(4));
            manager.SetDuration(part3, TimeSpan.FromDays(5));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(30));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(4));
            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(19));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(35));
            
            // test: move the split task itself - maintain interrupts
            var offset = TimeSpan.FromDays(10) - split.Start;
            manager.SetStart(split, TimeSpan.FromDays(10));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0) + offset);
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(15) + offset);
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(30) + offset);
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(4));
            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(3) + offset);
            Assert.IsTrue(part2.End == TimeSpan.FromDays(19) + offset);
            Assert.IsTrue(part3.End == TimeSpan.FromDays(35) + offset);
        }

        [Test]
        public void SetSplitTaskEndLastTaskMinimun1UnitTime()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create a split task
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
            manager.Split(part2, part3, TimeSpan.FromDays(1));
            manager.SetStart(part1, TimeSpan.FromDays(0));
            manager.SetStart(part2, TimeSpan.FromDays(15));
            manager.SetStart(part3, TimeSpan.FromDays(30));
            manager.SetDuration(part1, TimeSpan.FromDays(3));
            manager.SetDuration(part2, TimeSpan.FromDays(4));
            manager.SetDuration(part3, TimeSpan.FromDays(5));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(30));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(4));
            Assert.IsTrue(part3.Duration == TimeSpan.FromDays(5));
            Assert.IsTrue(part1.End == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(19));
            Assert.IsTrue(part3.End == TimeSpan.FromDays(35));

            // test: set the split task end - last part end to increase or decrease,
            // resulting no less than zero time unit
            manager.SetEnd(split, TimeSpan.FromDays(1));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));
            Assert.IsTrue(manager.IsPart(part3));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(0));
            Assert.IsTrue(part2.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(part3.Start == TimeSpan.FromDays(30));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(4));
            Assert.IsTrue(part3.Duration > TimeSpan.Zero);
            Assert.IsTrue(part1.End == TimeSpan.FromDays(3));
            Assert.IsTrue(part2.End == TimeSpan.FromDays(19));
            Assert.IsTrue(part3.End > part3.Start);

            //// test: increase split task end and last part adjust accordingly
            //manager.SetEnd(split, TimeSpan.FromDays(100));
            //Assert.IsTrue(part3.End == TimeSpan.FromDays(100));
            //Assert.IsTrue(part3.Duration == TimeSpan.FromDays(70));
            //Assert.IsTrue(part3.Start == TimeSpan.FromDays(30));
        }

        [Test]
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
        
        [Test]
        public void TransferResourceToSplitTaskOnMerge_ByDeleteInThisCase()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var r1 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
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
        
        [Test]
        public void DeletePartCausingMerge()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(manager.IsPart(part1));
            Assert.IsTrue(manager.IsPart(part2));

            // test: delete a part, expect a merge to regular task
            manager.Delete(part1);
            Assert.IsTrue(!manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(part1));
            Assert.IsTrue(!manager.IsPart(part2));
        }
        
        [Test]
        public void DeleteResourcesOnDeleteSplitTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var r1 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
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
        
        [Test]
        public void DeletePartsOnDeleteSplitTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            manager.Add(split);

            // setup: create a split task with resources on parts
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
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

        [Test]
        public void SplitExistingSplitTaskNoEffect()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var split = new Task();
            var part1 = new Task();
            var part2 = new Task();
            var part3 = new Task();
            manager.Add(split);

            // setup: create split task
            manager.Split(split, part1, part2, TimeSpan.FromDays(1));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(split));

            Assert.IsTrue(!manager.IsSplit(part1));
            Assert.IsTrue(manager.IsPart(part1));

            Assert.IsTrue(!manager.IsSplit(part2));
            Assert.IsTrue(manager.IsPart(part2));

            Assert.IsTrue(!manager.IsSplit(part3));
            Assert.IsTrue(!manager.IsPart(part3));

            // test: try to split the split task again - no effect.
            manager.Split(split, part3, TimeSpan.FromDays(1));
            Assert.IsTrue(manager.IsSplit(split));
            Assert.IsTrue(!manager.IsPart(split));

            Assert.IsTrue(!manager.IsSplit(part1));
            Assert.IsTrue(manager.IsPart(part1));

            Assert.IsTrue(!manager.IsSplit(part2));
            Assert.IsTrue(manager.IsPart(part2));

            Assert.IsTrue(!manager.IsSplit(part3));
            Assert.IsTrue(!manager.IsPart(part3));
            
        }

        [Test]
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
            manager.SetStart(precedent, TimeSpan.FromDays(5));
            manager.SetDuration(precedent, TimeSpan.FromDays(5));
            manager.SetStart(split, TimeSpan.FromDays(15));
            manager.SetDuration(split, TimeSpan.FromDays(5));
            manager.Relate(precedent, split);
            manager.Split(split, part1, part2, TimeSpan.FromDays(2));
            Assert.IsTrue(manager.PrecedentsOf(split).Contains(precedent));
            Assert.IsTrue(manager.PartsOf(split).Contains(part1));
            Assert.IsTrue(manager.PartsOf(split).Contains(part2));
            Assert.IsTrue(precedent.End == TimeSpan.FromDays(10));
            Assert.IsTrue(split.Start == TimeSpan.FromDays(15));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(15));

            // test: move part1 start before precendent end
            manager.SetStart(part1, TimeSpan.FromDays(8));
            Assert.IsTrue(manager.PrecedentsOf(split).Contains(precedent));
            Assert.IsTrue(manager.PartsOf(split).Contains(part1));
            Assert.IsTrue(manager.PartsOf(split).Contains(part2));
            Assert.IsTrue(part1.Start == TimeSpan.FromDays(10));
            Assert.IsTrue(split.Start == TimeSpan.FromDays(10));
        }

        [Test]
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
            manager.SetDuration(split, TimeSpan.FromDays(6));
            manager.SetDuration(member, TimeSpan.FromDays(14));
            manager.Group(group, split);
            manager.Group(group, member);
            manager.Split(split, part1, part2, TimeSpan.FromDays(2));
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(manager.IsMember(member));
            Assert.IsTrue(manager.IsMember(split));

            Assert.IsTrue(group.Complete == 0);
            Assert.IsTrue(split.Complete == 0);
            Assert.IsTrue(part1.Complete == 0);
            Assert.IsTrue(part2.Complete == 0);

            Assert.IsTrue(group.Duration == TimeSpan.FromDays(14));
            Assert.IsTrue(member.Duration == TimeSpan.FromDays(14));
            Assert.IsTrue(split.Duration == TimeSpan.FromDays(6));
            Assert.IsTrue(part1.Duration == TimeSpan.FromDays(2));
            Assert.IsTrue(part2.Duration == TimeSpan.FromDays(4));

            // test: set complete for part1 and see group and split complete increase
            manager.SetComplete(part1, 0.3f);
            Assert.IsTrue(Math.Abs(split.Complete - (0.3f * 2) / 6.0f) < 0.01f);
            Assert.IsTrue(Math.Abs(group.Complete - (split.Complete * 7) / 20.0f) < 0.01f);

        }
    }
}
