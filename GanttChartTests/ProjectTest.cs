using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Braincase.GanttChart;

namespace GanttChartTests
{
    [TestClass]
    public class ProjectTest
    {
        [TestMethod]
        public void CreateAndRemoveTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();

            // create first task
            var first = new Task();
            manager.Add(first);
            Assert.IsTrue(manager.Tasks.Count() == 1, string.Format("{0} != {1}", 1, manager.Tasks.Count()));
            Assert.IsTrue(manager.ParentOf(first) == null);

            // create second task, remove first task
            var second = new Task();
            second.Name = "Apple Jack";
            manager.Add(second);
            manager.Delete(first);
            var firstordefault = manager.Tasks.FirstOrDefault();
            Assert.IsTrue(firstordefault != null);
            Assert.IsTrue(firstordefault.Name == "Apple Jack");
            Assert.IsTrue(firstordefault.Equals(second));

            // remove a task that is already removed
            manager.Delete(first);
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // remove a null task
            manager.Delete(null);
            Assert.IsTrue(manager.Tasks.Count() == 1);
        }

        [TestMethod]
        public void CreateAndRemoveGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var one = new Task();
            var two = new Task();
            manager.Add(group1);
            manager.Add(one);
            manager.Add(two);

            // make group
            manager.Group(group1, one);
            manager.Group(group1, two);
            Assert.IsTrue(manager.Tasks.Count() == 3);
            Assert.IsTrue(manager.ChildrenOf(group1).Count() == 2);
            Assert.IsTrue(manager.ParentOf(one).Equals(group1));
            Assert.IsTrue(manager.ParentOf(two).Equals(group1));
            
            // delete group task
            manager.Delete(group1);
            Assert.IsTrue(manager.Tasks.Count() == 2);
            Assert.IsTrue(manager.ChildrenOf(group1).Count() == 0);
            Assert.IsTrue(manager.ParentOf(one) == null);
            Assert.IsTrue(manager.ParentOf(two) == null);   
        }

        [TestMethod]
        public void ProjectEmptyEnumerators()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();

            Assert.IsTrue(manager.Tasks.Count() == 0, string.Format("count == {0} != {1}",manager.Tasks.Count(), 0) );
            Assert.IsTrue(manager.Resources.Count() == 0);
        }

        [TestMethod]
        public void EnumerationShouldReturnEmptySet()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var alien = new Task();

            // test: Enumerators should at least return empty sets
            Assert.IsNotNull(manager.ChildrenOf(alien), "ChildrenOf is null");
            Assert.IsNotNull(manager.AncestorsOf(alien), "AncestorsOf is null");
            Assert.IsNotNull(manager.DecendantsOf(alien), "DecendantsOf is null");
            Assert.IsNotNull(manager.DependantsOf(alien), "DependantsOf is null");
            Assert.IsNotNull(manager.PrecedentsOf(alien), "PrecedentsOf is null");
            Assert.IsNotNull(manager.DirectDependantsOf(alien), "DirectDependantsOf is null");
            Assert.IsNotNull(manager.DirectPrecedentsOf(alien), "DirectPrecedentsOf is null");
            Assert.IsNotNull(manager.ResourcesOf(alien), "ResourcesOf is null");
            Assert.IsNotNull(manager.TasksOf(alien), "TasksOf is null");
        }

        [TestMethod]
        public void KnownTasksEnumeration()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var local = new Task();
            manager.Add(local);

            Assert.IsNotNull(manager.ChildrenOf(local));
            Assert.IsNotNull(manager.AncestorsOf(local));
            Assert.IsNotNull(manager.DecendantsOf(local));
            Assert.IsNotNull(manager.DependantsOf(local));
            Assert.IsNotNull(manager.PrecedentsOf(local));
            Assert.IsNotNull(manager.DirectDependantsOf(local));
            Assert.IsNotNull(manager.DirectPrecedentsOf(local));
            Assert.IsNotNull(manager.ResourcesOf(local));
            Assert.IsNotNull(manager.TasksOf(local));
        }

        [TestMethod]
        public void MoveSingleTaskToCheckForOutOfBoundHandling()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();

            // create a task
            var first = new Task();
            manager.Add(first);
            Assert.IsTrue(manager.Tasks.Count() == 1);

            // get task index
            var index = manager.IndexOf(first);
            Assert.IsTrue(index == 0, string.Format("Task index should be {0}, but is {1}", 0, index));

            // move task by 0 offset
            manager.Move(first, 0);
            index = manager.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by negative offset
            manager.Move(first, -1);
            index = manager.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by count offset 
            manager.Move(first, 1);
            index = manager.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by positive offset more than count
            manager.Move(first, 2);
            index = manager.IndexOf(first);
            Assert.IsTrue(index == 0);
        }

        [TestMethod]
        public void MoveNonExistingTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var task = new Task();
            manager.Move(task, 1);
            Assert.IsTrue(manager.IndexOf(task) == -1);
        }

        [TestMethod]
        public void MoveTasksAroundSingleLevel()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();

            // create tasks
            var one = new Task() { Name = "one" };
            var two = new Task() { Name = "one" };
            var three = new Task() { Name = "one" };

            manager.Add(one);
            manager.Add(two);
            manager.Add(three);

            // get index of one
            Assert.IsTrue(manager.IndexOf(one) == 0);
            Assert.IsTrue(manager.IndexOf(two) == 1);
            Assert.IsTrue(manager.IndexOf(three) == 2);
            // move by 1 offset each time
            manager.Move(one, 1);
            Assert.IsTrue(manager.IndexOf(two) == 0, string.Format("{0} != {1}", 0, manager.IndexOf(two)));
            Assert.IsTrue(manager.IndexOf(one) == 1, string.Format("{0} != {1}", 1, manager.IndexOf(one)));
            Assert.IsTrue(manager.IndexOf(three) == 2, string.Format("{0} != {1}", 2, manager.IndexOf(three)));
            manager.Move(one, 1);
            Assert.IsTrue(manager.IndexOf(two) == 0);
            Assert.IsTrue(manager.IndexOf(three) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
            manager.Move(one, 1);
            Assert.IsTrue(manager.IndexOf(two) == 0);
            Assert.IsTrue(manager.IndexOf(three) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
            
            // move by 1 offset each time
            manager.Move(two, 1);
            Assert.IsTrue(manager.IndexOf(three) == 0);
            Assert.IsTrue(manager.IndexOf(two) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
            manager.Move(two, 1);
            Assert.IsTrue(manager.IndexOf(three) == 0);
            Assert.IsTrue(manager.IndexOf(one) == 1);
            Assert.IsTrue(manager.IndexOf(two) == 2);
            manager.Move(two, 1);
            Assert.IsTrue(manager.IndexOf(three) == 0);
            Assert.IsTrue(manager.IndexOf(one) == 1, string.Format("{0} != {1}", 1, manager.IndexOf(one)));
            Assert.IsTrue(manager.IndexOf(two) == 2, string.Format("{0} != {1}", 2, manager.IndexOf(two)));

            // move by -1 offset each time
            manager.Move(two, -1);
            Assert.IsTrue(manager.IndexOf(three) == 0);
            Assert.IsTrue(manager.IndexOf(two) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
            manager.Move(two, -1);
            Assert.IsTrue(manager.IndexOf(two) == 0);
            Assert.IsTrue(manager.IndexOf(three) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
            manager.Move(two, -1);
            Assert.IsTrue(manager.IndexOf(two) == 0);
            Assert.IsTrue(manager.IndexOf(three) == 1);
            Assert.IsTrue(manager.IndexOf(one) == 2);
        }

        [TestMethod]
        public void MoveGroupsAround()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            // groups
            var group1 = new Task() { Name = "group1" };
            var group2 = new Task() { Name = "group2" };
            var group3 = new Task() { Name = "group3" };
            // group 1 tasks
            var g1t1 = new Task() { Name = "g1t1" };
            var g1t2 = new Task() { Name = "g1t2" };
            var g1t3 = new Task() { Name = "g1t3" };
            // group 2 tasks
            var g2t1 = new Task() { Name = "g2t1" };
            var g2t2 = new Task() { Name = "g2t2" };
            var g2t3 = new Task() { Name = "g2t3" };
            // group 3 tasks
            var g3t1 = new Task() { Name = "g3t1" };
            var g3t2 = new Task() { Name = "g3t2" };
            var g3t3 = new Task() { Name = "g3t3" };

            manager.Add(group1);
            manager.Add(group2);
            manager.Add(group3);
            manager.Add(g1t1);
            manager.Add(g1t2);
            manager.Add(g1t3);
            manager.Add(g2t1);
            manager.Add(g2t2);
            manager.Add(g2t3);
            manager.Add(g3t1);
            manager.Add(g3t2);
            manager.Add(g3t3);

            // make groups
            manager.Group(group1, g1t1);
            manager.Group(group1, g1t2);
            manager.Group(group1, g1t3);
            // make groups
            manager.Group(group2, g2t1);
            manager.Group(group2, g2t2);
            manager.Group(group2, g2t3);
            // make groups
            manager.Group(group3, g3t1);
            manager.Group(group3, g3t2);
            manager.Group(group3, g3t3);

            // confirm parents
            Assert.IsTrue(manager.ParentOf(group1) == null);
            Assert.IsTrue(manager.ParentOf(group2) == null);
            Assert.IsTrue(manager.ParentOf(group3) == null);
            Assert.IsTrue(manager.ParentOf(g1t1) == group1);
            Assert.IsTrue(manager.ParentOf(g1t2) == group1);
            Assert.IsTrue(manager.ParentOf(g1t3) == group1);
            Assert.IsTrue(manager.ParentOf(g2t1) == group2);
            Assert.IsTrue(manager.ParentOf(g2t2) == group2);
            Assert.IsTrue(manager.ParentOf(g2t3) == group2);
            Assert.IsTrue(manager.ParentOf(g3t1) == group3);
            Assert.IsTrue(manager.ParentOf(g3t2) == group3);
            Assert.IsTrue(manager.ParentOf(g3t3) == group3);

            // confirm order
            Assert.IsTrue(manager.IndexOf(group1) == 0);
            Assert.IsTrue(manager.IndexOf(g1t1) == 1);
            Assert.IsTrue(manager.IndexOf(g1t2) == 2);
            Assert.IsTrue(manager.IndexOf(g1t3) == 3);
            Assert.IsTrue(manager.IndexOf(group2) == 4);
            Assert.IsTrue(manager.IndexOf(g2t1) == 5);
            Assert.IsTrue(manager.IndexOf(g2t2) == 6);
            Assert.IsTrue(manager.IndexOf(g2t3) == 7);
            Assert.IsTrue(manager.IndexOf(group3) == 8);
            Assert.IsTrue(manager.IndexOf(g3t1) == 9);
            Assert.IsTrue(manager.IndexOf(g3t2) == 10);
            Assert.IsTrue(manager.IndexOf(g3t3) == 11);

            // move group under itself 1 (not allowed)
            manager.Move(group1, 1);
            Assert.IsTrue(manager.IndexOf(group1) == 0, string.Format("{0} != {1}", 0, manager.IndexOf(group1)));
            Assert.IsTrue(manager.IndexOf(g1t1) == 1);
            Assert.IsTrue(manager.IndexOf(g1t2) == 2);
            Assert.IsTrue(manager.IndexOf(g1t3) == 3);
            Assert.IsTrue(manager.IndexOf(group2) == 4);
            Assert.IsTrue(manager.IndexOf(g2t1) == 5);
            Assert.IsTrue(manager.IndexOf(g2t2) == 6);
            Assert.IsTrue(manager.IndexOf(g2t3) == 7);
            Assert.IsTrue(manager.IndexOf(group3) == 8);
            Assert.IsTrue(manager.IndexOf(g3t1) == 9);
            Assert.IsTrue(manager.IndexOf(g3t2) == 10);
            Assert.IsTrue(manager.IndexOf(g3t3) == 11);

            // move group under itself 2 (not allowed)
            manager.Move(group1, 2);
            Assert.IsTrue(manager.IndexOf(group1) == 0);
            Assert.IsTrue(manager.IndexOf(g1t1) == 1);
            Assert.IsTrue(manager.IndexOf(g1t2) == 2);
            Assert.IsTrue(manager.IndexOf(g1t3) == 3);
            Assert.IsTrue(manager.IndexOf(group2) == 4);
            Assert.IsTrue(manager.IndexOf(g2t1) == 5);
            Assert.IsTrue(manager.IndexOf(g2t2) == 6);
            Assert.IsTrue(manager.IndexOf(g2t3) == 7);
            Assert.IsTrue(manager.IndexOf(group3) == 8);
            Assert.IsTrue(manager.IndexOf(g3t1) == 9);
            Assert.IsTrue(manager.IndexOf(g3t2) == 10);
            Assert.IsTrue(manager.IndexOf(g3t3) == 11);

            // move group under itself 3 (not allowed)
            manager.Move(group1, 3);
            Assert.IsTrue(manager.IndexOf(group1) == 0);
            Assert.IsTrue(manager.IndexOf(g1t1) == 1);
            Assert.IsTrue(manager.IndexOf(g1t2) == 2);
            Assert.IsTrue(manager.IndexOf(g1t3) == 3);
            Assert.IsTrue(manager.IndexOf(group2) == 4);
            Assert.IsTrue(manager.IndexOf(g2t1) == 5);
            Assert.IsTrue(manager.IndexOf(g2t2) == 6);
            Assert.IsTrue(manager.IndexOf(g2t3) == 7);
            Assert.IsTrue(manager.IndexOf(group3) == 8);
            Assert.IsTrue(manager.IndexOf(g3t1) == 9);
            Assert.IsTrue(manager.IndexOf(g3t2) == 10);
            Assert.IsTrue(manager.IndexOf(g3t3) == 11);

            // move group under itself 4 (not allowed)
            manager.Move(group1, 4);
            Assert.IsTrue(manager.IndexOf(group2) == 0);
            Assert.IsTrue(manager.IndexOf(g2t1) == 1);
            Assert.IsTrue(manager.IndexOf(g2t2) == 2);
            Assert.IsTrue(manager.IndexOf(g2t3) == 3);
            Assert.IsTrue(manager.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, manager.IndexOf(group1)));
            Assert.IsTrue(manager.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, manager.IndexOf(g1t1)));
            Assert.IsTrue(manager.IndexOf(g1t2) == 6, string.Format("{0} != {1}", 0, manager.IndexOf(g1t2)));
            Assert.IsTrue(manager.IndexOf(g1t3) == 7, string.Format("{0} != {1}", 0, manager.IndexOf(g1t3)));
            Assert.IsTrue(manager.IndexOf(group3) == 8);
            Assert.IsTrue(manager.IndexOf(g3t1) == 9);
            Assert.IsTrue(manager.IndexOf(g3t2) == 10);
            Assert.IsTrue(manager.IndexOf(g3t3) == 11);

            // move group under another group
            manager.Move(group3, -1);
            Assert.IsTrue(manager.IndexOf(group2) == 0);
            Assert.IsTrue(manager.IndexOf(g2t1) == 1);
            Assert.IsTrue(manager.IndexOf(g2t2) == 2);
            Assert.IsTrue(manager.IndexOf(g2t3) == 3);
            Assert.IsTrue(manager.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, manager.IndexOf(group1)));
            Assert.IsTrue(manager.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, manager.IndexOf(g1t1)));
            Assert.IsTrue(manager.IndexOf(g1t2) == 6, string.Format("{0} != {1}", 0, manager.IndexOf(g1t2)));
            Assert.IsTrue(manager.IndexOf(group3) == 7);
            Assert.IsTrue(manager.IndexOf(g3t1) == 8);
            Assert.IsTrue(manager.IndexOf(g3t2) == 9);
            Assert.IsTrue(manager.IndexOf(g3t3) == 10);
            Assert.IsTrue(manager.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, manager.IndexOf(g1t3)));

            // move group within group
            manager.Move(group3, -1);
            Assert.IsTrue(manager.IndexOf(group2) == 0);
            Assert.IsTrue(manager.IndexOf(g2t1) == 1);
            Assert.IsTrue(manager.IndexOf(g2t2) == 2);
            Assert.IsTrue(manager.IndexOf(g2t3) == 3);
            Assert.IsTrue(manager.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, manager.IndexOf(group1)));
            Assert.IsTrue(manager.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, manager.IndexOf(g1t1)));
            Assert.IsTrue(manager.IndexOf(group3) == 6);
            Assert.IsTrue(manager.IndexOf(g3t1) == 7);
            Assert.IsTrue(manager.IndexOf(g3t2) == 8);
            Assert.IsTrue(manager.IndexOf(g3t3) == 9);
            Assert.IsTrue(manager.IndexOf(g1t2) == 10, string.Format("{0} != {1}", 0, manager.IndexOf(g1t2)));
            Assert.IsTrue(manager.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, manager.IndexOf(g1t3)));

            // move group out of another group
            manager.Move(group3, -2);
            Assert.IsTrue(manager.IndexOf(group2) == 0);
            Assert.IsTrue(manager.IndexOf(g2t1) == 1);
            Assert.IsTrue(manager.IndexOf(g2t2) == 2);
            Assert.IsTrue(manager.IndexOf(g2t3) == 3);
            Assert.IsTrue(manager.IndexOf(group3) == 4);
            Assert.IsTrue(manager.IndexOf(g3t1) == 5);
            Assert.IsTrue(manager.IndexOf(g3t2) == 6);
            Assert.IsTrue(manager.IndexOf(g3t3) == 7);
            Assert.IsTrue(manager.IndexOf(group1) == 8, string.Format("{0} != {1}", 0, manager.IndexOf(group1)));
            Assert.IsTrue(manager.IndexOf(g1t1) == 9, string.Format("{0} != {1}", 0, manager.IndexOf(g1t1)));
            Assert.IsTrue(manager.IndexOf(g1t2) == 10, string.Format("{0} != {1}", 0, manager.IndexOf(g1t2)));
            Assert.IsTrue(manager.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, manager.IndexOf(g1t3)));
        }

        #region groups

        [TestMethod]
        public void GroupKnownTasksAndGroups()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            var one = new Task();
            var two = new Task();
            var three = new Task();
            var four = new Task();
            var five = new Task();
            var six = new Task();

            manager.Add(one);
            manager.Add(two);
            manager.Add(three);
            manager.Add(four);
            manager.Add(five);
            manager.Add(six);

            // test: groupings
            manager.Group(one, two);
            manager.Group(two, three);
            manager.Group(two, six);
            manager.Group(four, five);
            Assert.IsTrue(manager.IsGroup(one));
            Assert.IsTrue(manager.IsGroup(two));
            Assert.IsTrue(!manager.IsGroup(three));
            Assert.IsTrue(manager.IsGroup(four));
            Assert.IsTrue(!manager.IsGroup(five));
            Assert.IsTrue(!manager.IsGroup(six));

            // test: check task
            Assert.IsTrue(manager.Tasks.Contains(one));
            Assert.IsTrue(manager.Tasks.Contains(two));
            Assert.IsTrue(manager.Tasks.Contains(three));
            Assert.IsTrue(manager.Tasks.Contains(four));
            Assert.IsTrue(manager.Tasks.Contains(five));
            Assert.IsTrue(manager.Tasks.Contains(six));

            // test: check decendants
            Assert.IsTrue(manager.DecendantsOf(four).Contains(five));
            Assert.IsTrue(manager.DecendantsOf(one).Contains(two));
            Assert.IsTrue(manager.DecendantsOf(one).Contains(three));
            Assert.IsTrue(manager.DecendantsOf(one).Contains(six));
            Assert.IsTrue(manager.DecendantsOf(one).Count() == 3);

            // test: check ancestors
            Assert.IsTrue(manager.AncestorsOf(six).Contains(two));
            Assert.IsTrue(manager.AncestorsOf(six).Contains(one));
            Assert.IsTrue(manager.AncestorsOf(six).Count() == 2);
        }

        [TestMethod]
        public void GroupUnknownTasksIntoGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var one = new Task();

            // setup: make only group added
            manager.Add(group);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(group));

            // test: check that we cannot add unknown tasks to group
            manager.Group(group, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(group));
        }

        [TestMethod]
        public void GroupTaskIntoUnknownGroups()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var one = new Task();

            // setup: make only one added
            manager.Add(one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));

            // test: check that we cannot add tasks to unknown group
            manager.Group(group, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));
        }

        [TestMethod]
        public void UngroupUnknownTasksFromGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var member = new Task();
            var one = new Task();

            // setup: make only group added
            manager.Add(group);
            manager.Group(group, member);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(group));

            // test: check that we cannot add unknown tasks to group
            manager.Ungroup(group, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(group));
        }

        [TestMethod]
        public void UnroupTaskFromUnknownGroups()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            IProjectManager<Task, object> other = new ProjectManager<Task, object>();
            var group = new Task();
            var member = new Task();
            var one = new Task();
            other.Add(group);
            other.Add(member);
            

            // setup: make only one added and foreign group
            manager.Add(one);
            other.Group(group, member);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));

            // test: check that we cannot add tasks to unknown group
            manager.Ungroup(group, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));
        }

        [TestMethod]
        public void UngroupUnknownGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            IProjectManager<Task, object> other = new ProjectManager<Task, object>();
            var group = new Task();
            var one = new Task();
            other.Add(group);
            other.Add(one);

            // setup: make a foreign group
            other.Group(group, one);
            Assert.IsTrue(other.IsGroup(group));
            Assert.IsTrue(manager.Tasks.Count() == 0);

            // test: check whether can ungroup unknown group
            manager.Ungroup(group);
            Assert.IsTrue(other.IsGroup(group));
            Assert.IsTrue(manager.Tasks.Count() == 0);
        }

        [TestMethod]
        public void UngroupAGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var group2 = new Task();
            var one =  new Task();
            var two = new Task();
            manager.Add(group1);
            manager.Add(group2);
            manager.Add(one);
            manager.Add(two);

            // setup: group1 contains group2; group2 contains one and two
            manager.Group(group1, group2);
            manager.Group(group2, one);
            manager.Group(group2, two);

            // test: group2 is no longer a group; one and two goes into group 1 (parent of group 2)
            manager.Ungroup(group1);
            Assert.IsTrue(!manager.IsGroup(group1));
            Assert.IsTrue(manager.IsGroup(group2));
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(!manager.IsGroup(two));
            Assert.IsTrue(manager.ChildrenOf(group1).Count() == 0);
            Assert.IsTrue(manager.ChildrenOf(group2).Count() == 2);
            Assert.IsTrue(manager.Tasks.Count() == 4);
        }

        [TestMethod]
        public void UngroupNonGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            manager.Add(group1);

            // test: ungroup a task that wasn't a group in the first place (no effect)
            manager.Ungroup(group1);
            Assert.IsTrue(!manager.IsGroup(group1));
        }

        [TestMethod]
        public void UngroupNullGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // test: ungroup a null group (no effect, should not throw)
            manager.Ungroup(null);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));

            // test: ungroup a null group with task (no effect, should not throw)
            manager.Ungroup(null, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
            Assert.IsTrue(manager.Tasks.Contains(one));
        }

        [TestMethod]
        public void UngroupNullTaskFromGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(one);

            // setup: make an actual group first
            manager.Group(group1, one);
            Assert.IsTrue(manager.IsGroup(group1));
            Assert.IsTrue(manager.Tasks.Count() == 2);
            Assert.IsTrue(manager.Tasks.Contains(one));
            Assert.IsTrue(manager.Tasks.Contains(group1));

            // test: ungroup null (no effect)
            manager.Ungroup(group1, null);
            Assert.IsTrue(manager.IsGroup(group1));
            Assert.IsTrue(manager.Tasks.Count() == 2);
            Assert.IsTrue(manager.Tasks.Contains(one));
            Assert.IsTrue(manager.Tasks.Contains(group1));
        }

        [TestMethod]
        public void GroupNullTaskIntoGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            var group1 = new Task();
            manager.Add(group1);

            // test: put task into null group (no effect)
            manager.Group(group1, null);
            Assert.IsTrue(manager.Tasks.Count() == 1);
        }

        [TestMethod]
        public void GroupTaskIntoNullGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            var one = new Task();
            manager.Add(one);

            // test: put task into null group (no effect)
            manager.Group(null, one);
            Assert.IsTrue(manager.Tasks.Count() == 1);
        }

        [TestMethod]
        public void GroupIntoSelf()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            var group1 = new Task();

            // test: group into self (no effect)
            manager.Group(group1, group1);
            Assert.IsTrue(!manager.IsGroup(group1));
            Assert.IsTrue(manager.ParentOf(group1) == null);
        }

        [TestMethod]
        public void GroupChildIntoSelf()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(one);

            // setup: group1 contain group2 contain one
            manager.Group(group1, one);

            // test: group into self (no effect)
            manager.Group(one, one);
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(manager.ParentOf(one) == group1);
        }

        [TestMethod]
        public void GroupIntoParent()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(one);

            // setup: group1 contains one
            manager.Group(group1, one);

            // test: grouping into parent (no effect, since already grouped)
            manager.Group(one, group1);
            Assert.IsTrue(manager.IsGroup(group1));
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(manager.ParentOf(one).Equals(group1));
        }

        [TestMethod]
        public void GroupIntoAnotherGroupWhenAlreadyHaveGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var group2 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(group2);
            manager.Add(one);

            // setup: group1 contains one
            manager.Group(group1, one);

            // test: group one into group2, leaving group1
            manager.Group(group2, one);
            Assert.IsTrue(!manager.IsGroup(group1), string.Format("{0} != {1}", true, manager.IsGroup(group1)));
            Assert.IsTrue(manager.IsGroup(group2));
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(manager.ParentOf(one).Equals(group2));
            Assert.IsTrue(manager.ChildrenOf(group1).Count() == 0);
            Assert.IsTrue(manager.ChildrenOf(group2).Count() == 1);
        }

        [TestMethod]
        public void SubGrouping()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            // create tasks in random order
            var a = new Task();
            var b = new Task();
            var c1 = new Task();
            var c2 = new Task();
            var d1 = new Task();
            var d2 = new Task();
            var d3 = new Task();
            var e1 = new Task();
            var e2 = new Task();
            var e3 = new Task();

            manager.Add(b);
            manager.Add(e1);
            manager.Add(a);
            manager.Add(e2);
            manager.Add(c1);
            manager.Add(d1);
            manager.Add(d2);
            manager.Add(e3);
            manager.Add(d3);
            manager.Add(c2);

            // setup: make sub groups
            manager.Group(a, b);
            manager.Group(b, c1);
            manager.Group(b, c2);
            manager.Group(c1, d1);
            manager.Group(c1, d2);
            manager.Group(c2, d3);
            manager.Group(d1, e1);
            manager.Group(d2, e2);
            manager.Group(d2, e3);

            // test: check sub groups are correct
            Assert.IsTrue(manager.ChildrenOf(a).Contains(b));
            Assert.IsTrue(manager.ChildrenOf(b).Contains(c1));
            Assert.IsTrue(manager.ChildrenOf(b).Contains(c2));
            Assert.IsTrue(manager.ChildrenOf(c1).Contains(d1));
            Assert.IsTrue(manager.ChildrenOf(c1).Contains(d2));
            Assert.IsTrue(manager.ChildrenOf(c2).Contains(d3));
            Assert.IsTrue(manager.ChildrenOf(d1).Contains(e1));
            Assert.IsTrue(manager.ChildrenOf(d2).Contains(e2));
            Assert.IsTrue(manager.ChildrenOf(d2).Contains(e3));
        }

        [TestMethod]
        public void SubGroupOrdering()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            // create tasks in random order
            var a = new Task();
            var b = new Task();
            var c1 = new Task();
            var c2 = new Task();
            var d1 = new Task();
            var d2 = new Task();
            var d3 = new Task();
            var e1 = new Task();
            var e2 = new Task();
            var e3 = new Task();

            manager.Add(b);
            manager.Add(e1);
            manager.Add(a);
            manager.Add(e2);
            manager.Add(c1);
            manager.Add(d1);
            manager.Add(d2);
            manager.Add(e3);
            manager.Add(d3);
            manager.Add(c2);

            // setup: make sub groups
            manager.Group(a, b);
            manager.Group(b, c1);
            manager.Group(b, c2);
            manager.Group(c1, d1);
            manager.Group(c1, d2);
            manager.Group(c2, d3);
            manager.Group(d1, e1);
            manager.Group(d2, e2);
            manager.Group(d2, e3);

            // setup: make sub groups
            manager.Group(a, b);
            manager.Group(b, c1);
            manager.Group(b, c2);
            manager.Group(c1, d1);
            manager.Group(c1, d2);
            manager.Group(c2, d3);
            manager.Group(d1, e1);
            manager.Group(d2, e2);
            manager.Group(d2, e3);

            // test: check that order is correct
            Assert.IsTrue(manager.IndexOf(a) == 0, string.Format("Order a = {0} != {1}", manager.IndexOf(a), 0));
            Assert.IsTrue(manager.IndexOf(b) == 1, string.Format("Order a = {0} != {1}", manager.IndexOf(b), 1));
            Assert.IsTrue(manager.IndexOf(c1) == 2, string.Format("Order a = {0} != {1}", manager.IndexOf(c1), 2));
            Assert.IsTrue(manager.IndexOf(d1) == 3, string.Format("Order a = {0} != {1}", manager.IndexOf(d1), 3));
            Assert.IsTrue(manager.IndexOf(e1) == 4, string.Format("Order a = {0} != {1}", manager.IndexOf(d2), 4));
            Assert.IsTrue(manager.IndexOf(d2) == 5, string.Format("Order a = {0} != {1}", manager.IndexOf(d3), 5));
            Assert.IsTrue(manager.IndexOf(e2) == 6, string.Format("Order a = {0} != {1}", manager.IndexOf(e1), 6));
            Assert.IsTrue(manager.IndexOf(e3) == 7, string.Format("Order a = {0} != {1}", manager.IndexOf(e2), 7));
            Assert.IsTrue(manager.IndexOf(c2) == 8, string.Format("Order a = {0} != {1}", manager.IndexOf(e3), 8));
            Assert.IsTrue(manager.IndexOf(d3) == 9, string.Format("Order a = {0} != {1}", manager.IndexOf(a), 9));
        }

        [TestMethod]
        public void GroupIntoGrandChild()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var group2 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(group2);
            manager.Add(one);

            // setup: group1 contain group2 contain one
            manager.Group(group1, group2);
            manager.Group(group2, one);

            // group into grandchild (no effect)
            manager.Group(one, group1);
            Assert.IsTrue(manager.ChildrenOf(group1).Contains(group2));
            Assert.IsTrue(manager.ChildrenOf(group2).Contains(one));
            Assert.IsTrue(manager.ChildrenOf(one).Count() == 0);
        }

        [TestMethod]
        public void GroupIntoGrandParent()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var group2 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(group2);
            manager.Add(one);

            // setup: group1 contain group2 contain one
            manager.Group(group1, group2);
            manager.Group(group2, one);

            // test: group into grandparent (allowed)
            manager.Group(group1, one);
            Assert.IsTrue(manager.IsGroup(group1));
            Assert.IsTrue(!manager.IsGroup(group2));
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(manager.ParentOf(one).Equals(group1));
            Assert.IsTrue(manager.ParentOf(group2).Equals(group1));
            Assert.IsTrue(manager.ParentOf(group1) == null);
        }

        [TestMethod]
        public void GroupIntoChild()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group1 = new Task();
            var one = new Task();
            manager.Add(group1);
            manager.Add(one);

            // setup: group1 contains one
            manager.Group(group1, one);

            // test: group into child (no effect)
            manager.Group(one, group1);
            Assert.IsTrue(manager.DecendantsOf(group1).Contains(one));
            Assert.IsTrue(manager.ParentOf(one).Equals(group1));
            Assert.IsTrue(manager.ParentOf(group1) == null);
        }

        #endregion groups

        [TestMethod]
        public void GroupTaskLevelAndOrdering()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            
            var zero = new Task();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            var four = new Task();
            var five = new Task();

            manager.Add(zero);
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);
            manager.Add(four);
            manager.Add(five);

            // single level ordering
            Assert.IsTrue(manager.IndexOf(zero) == 0);
            Assert.IsTrue(manager.IndexOf(one) == 1);
            Assert.IsTrue(manager.IndexOf(two) == 2);
            Assert.IsTrue(manager.IndexOf(three) == 3);
            Assert.IsTrue(manager.IndexOf(four) == 4);
            Assert.IsTrue(manager.IndexOf(five) == 5);
            Assert.IsTrue(manager.Tasks.Count() == 6);

            // two level ordering
            manager.Group(zero, two);
            manager.Group(zero, three);
            Assert.IsTrue(manager.IndexOf(zero) == 0);
            Assert.IsTrue(manager.IndexOf(two) == 1);
            Assert.IsTrue(manager.IndexOf(three) == 2);
            Assert.IsTrue(manager.IndexOf(one) == 3);
            Assert.IsTrue(manager.IndexOf(four) == 4);
            Assert.IsTrue(manager.IndexOf(five) == 5);
            Assert.IsTrue(manager.Tasks.Count() == 6);

            // three level ordering
            manager.Group(five, zero);
            Assert.IsTrue(manager.IndexOf(one) == 0);
            Assert.IsTrue(manager.IndexOf(four) == 1);
            Assert.IsTrue(manager.IndexOf(five) == 2, string.Format("Assert index == {0}; but index == {1}", 0, manager.IndexOf(five)));
            Assert.IsTrue(manager.IndexOf(zero) == 3);
            Assert.IsTrue(manager.IndexOf(two) == 4);
            Assert.IsTrue(manager.IndexOf(three) == 5);
            Assert.IsTrue(manager.Tasks.Count() == 6);

            // twin three level ordering
            manager.Group(four, one);
            Assert.IsTrue(manager.IndexOf(four) == 0);
            Assert.IsTrue(manager.IndexOf(one) == 1);
            Assert.IsTrue(manager.IndexOf(five) == 2, string.Format("Assert index == {0}; but index == {1}", 0, manager.IndexOf(five)));
            Assert.IsTrue(manager.IndexOf(zero) == 3);
            Assert.IsTrue(manager.IndexOf(two) == 4);
            Assert.IsTrue(manager.IndexOf(three) == 5);
            Assert.IsTrue(manager.Tasks.Count() == 6);

            // four level ordering
            manager.Group(two, four);
            Assert.IsTrue(manager.IndexOf(five) == 0, string.Format("Assert index == {0}; but index == {1}", 0, manager.IndexOf(five)));
            Assert.IsTrue(manager.IndexOf(zero) == 1);
            Assert.IsTrue(manager.IndexOf(two) == 2);
            Assert.IsTrue(manager.IndexOf(four) == 3);
            Assert.IsTrue(manager.IndexOf(one) == 4);
            Assert.IsTrue(manager.IndexOf(three) == 5);
            Assert.IsTrue(manager.Tasks.Count() == 6, string.Format("{0} != {1}", 6, manager.Tasks.Count()));

            // check parents
            Assert.IsTrue(manager.ParentOf(zero).Equals(five));
            Assert.IsTrue(manager.ParentOf(one).Equals(four));
            Assert.IsTrue(manager.ParentOf(two).Equals(zero));
            Assert.IsTrue(manager.ParentOf(three).Equals(zero));
            Assert.IsTrue(manager.ParentOf(four).Equals(two));
            Assert.IsTrue(manager.ParentOf(five) == null);
        }

        [TestMethod]
        public void CreateRelation()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);
            
            // setup: confirms no relations
            Assert.IsTrue(manager.DependantsOf(one).Count() == 0);
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 0);

            // test: create a relationship
            manager.Relate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
        }
        
        [TestMethod]
        public void CreateRelationWithNull()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            manager.Add(one);

            // test: null precedent (no effect)
            manager.Relate(null, one);
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(null) == false);

            // test: null dependant (no effect)
            manager.Relate(one, null);
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(null) == false);
        }
        
        [TestMethod]
        public void CreateRelationWithUnknownTasks()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);

            // test: non-added dependant (no effect)
            manager.Relate(one, two);
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(two) == false);

            // test: non-added precedent (no effect)
            manager.Relate(two, one);
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(two) == false);
        }
       
        [TestMethod]
        public void CreateRelationThatAlreadyExist()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: create a relationship
            manager.Relate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));

            // test: re-create the relationship (no effect)
            manager.Relate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));
        }
        
        [TestMethod]
        public void RemoveExistingRelation()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: create a relationship
            manager.Relate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));

            // test: remove relation
            manager.Unrelate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Count() == 0);
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 0);
        }
       
        [TestMethod]
        public void RemoveAllDependantsRelation()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            var four = new Task();
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);
            manager.Add(four);

            // setup: create a relationship
            manager.Relate(four, one);
            manager.Relate(one, two);
            manager.Relate(one, three);
            Assert.IsTrue(manager.DependantsOf(one).Contains(three));
            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(three).Contains(one));
            Assert.IsTrue(manager.DependantsOf(four).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(one).Contains(four));

            // test: remove dependants
            manager.Unrelate(one);
            Assert.IsTrue(manager.DependantsOf(one).Count() == 0);
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 0);
            Assert.IsTrue(manager.PrecedentsOf(three).Count() == 0);
            Assert.IsTrue(manager.DependantsOf(four).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(one).Contains(four));
        }
       
        [TestMethod]
        public void RemoveNonExistingRelation()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: confirm current relations
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(two) == false);

            // test: check that there is no effect
            manager.Unrelate(one, two);
            Assert.IsTrue(manager.HasRelations(one) == false);
            Assert.IsTrue(manager.HasRelations(two) == false);
        }
        
        [TestMethod]
        public void Create3LevelRelations()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);

            // setup: one before two before three
            manager.Relate(one, two);
            manager.Relate(two, three);

            // test: check enumerations are established
            Assert.IsTrue(manager.DependantsOf(one).Count() == 2);
            Assert.IsTrue(manager.DependantsOf(two).Count() == 1);
            Assert.IsTrue(manager.DependantsOf(three).Count() == 0);

            Assert.IsTrue(manager.DirectDependantsOf(one).Count() == 1);
            Assert.IsTrue(manager.DirectDependantsOf(two).Count() == 1);
            Assert.IsTrue(manager.DirectDependantsOf(three).Count() == 0);

            Assert.IsTrue(manager.PrecedentsOf(one).Count() == 0);
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 1, string.Format("expected {0} != {1}", 1, manager.PrecedentsOf(two).Count()));
            Assert.IsTrue(manager.PrecedentsOf(three).Count() == 2);

            Assert.IsTrue(manager.DirectPrecedentsOf(one).Count() == 0);
            Assert.IsTrue(manager.DirectPrecedentsOf(two).Count() == 1);
            Assert.IsTrue(manager.DirectPrecedentsOf(three).Count() == 1);
        }
        
        [TestMethod]
        public void CircularRelationLevel1()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: create a relationship
            manager.Relate(one, two);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.DependantsOf(one).Count() == 1);
            Assert.IsTrue(manager.PrecedentsOf(one).Count() == 0);

            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 1);
            Assert.IsTrue(manager.DependantsOf(two).Count() == 0);

            // test: cycle the relationship (no effect)
            manager.Relate(two, one);
            Assert.IsTrue(manager.DependantsOf(one).Contains(two));
            Assert.IsTrue(manager.DependantsOf(one).Count() == 1);
            Assert.IsTrue(manager.PrecedentsOf(one).Count() == 0);

            Assert.IsTrue(manager.PrecedentsOf(two).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(two).Count() == 1);
            Assert.IsTrue(manager.DependantsOf(two).Count() == 0);
        }
        
        [TestMethod]
        public void CircularRelationLevel2()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);

            // setup: one before two before three;
            manager.Relate(one, two);
            manager.Relate(two, three);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
            Assert.IsTrue(manager.HasRelations(three));
            Assert.IsTrue(manager.DirectDependantsOf(one).Contains(two));
            Assert.IsTrue(manager.DirectDependantsOf(two).Contains(three));
            Assert.IsTrue(manager.PrecedentsOf(three).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(three).Contains(two));

            // test: prevent circular relation (no effect)
            manager.Relate(three, one);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
            Assert.IsTrue(manager.HasRelations(three));
            Assert.IsTrue(manager.DirectDependantsOf(one).Contains(two));
            Assert.IsTrue(manager.DirectDependantsOf(two).Contains(three));
            Assert.IsTrue(manager.PrecedentsOf(three).Contains(one));
            Assert.IsTrue(manager.PrecedentsOf(three).Contains(two));
        }
        
        [TestMethod]
        public void RelateMultipleDependants()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var three = new Task();
            manager.Add(one);
            manager.Add(two);
            manager.Add(three);

            // setup: confirm no relations exists
            Assert.IsTrue(!manager.HasRelations(one));
            Assert.IsTrue(!manager.HasRelations(two));
            Assert.IsTrue(!manager.HasRelations(three));
            
            // test: multiple relation setup
            manager.Relate(one, two);
            manager.Relate(one, three);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
            Assert.IsTrue(manager.HasRelations(three));
            Assert.IsTrue(manager.DirectDependantsOf(one).Count() == 2);
            Assert.IsTrue(manager.DirectPrecedentsOf(two).Count() == 1);
            Assert.IsTrue(manager.DirectPrecedentsOf(three).Count() == 1);
        }
        
        [TestMethod]
        public void AssignResource()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var res = new Task() { Name = "Resource" };
            manager.Add(one);

            // setup: confirm there is no resource
            Assert.IsTrue(manager.Resources.Count() == 0);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 0);
            Assert.IsTrue(manager.TasksOf(res).Count() == 0);

            // test: assign resource
            manager.Assign(one, res);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.TasksOf(res).Count() == 1);
            Assert.IsTrue(manager.Resources.Contains(res));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(res));
            Assert.IsTrue(manager.TasksOf(res).Contains(one));
        }
        
        [TestMethod]
        public void AssignResourceForUnknownTasks()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var res = new Task() { Name = "Resource" };

            // setup: confirm there is no resource
            Assert.IsTrue(manager.Resources.Count() == 0);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 0);
            Assert.IsTrue(manager.TasksOf(res).Count() == 0);

            // test: assign resource
            manager.Assign(one, res);
            Assert.IsTrue(manager.Resources.Count() == 0);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 0);
            Assert.IsTrue(manager.TasksOf(res).Count() == 0);
        }

        [TestMethod]
        public void AssignSameResourceToSameTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var res = new Task() { Name = "Resource" };
            manager.Add(one);

            // setup: assign resource
            manager.Assign(one, res);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.TasksOf(res).Count() == 1);
            Assert.IsTrue(manager.Resources.Contains(res));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(res));
            Assert.IsTrue(manager.TasksOf(res).Contains(one));

            // test: assign the same resource to the same task (no effect, as resource is already assigned)
            manager.Assign(one, res);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.TasksOf(res).Count() == 1);
            Assert.IsTrue(manager.Resources.Contains(res));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(res));
            Assert.IsTrue(manager.TasksOf(res).Contains(one));
        }

        [TestMethod]
        public void AssignSameResourceToDifferentTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var res = new Task() { Name = "Resource" };
            manager.Add(one);
            manager.Add(two);

            // setup: assign resource
            manager.Assign(one, res);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.TasksOf(res).Count() == 1);
            Assert.IsTrue(manager.Resources.Contains(res));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(res));
            Assert.IsTrue(manager.TasksOf(res).Contains(one));

            // setup: assign resource. no extra resource should be created. resource reference shared by 2 tasks
            manager.Assign(two, res);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(two).Count() == 1);
            Assert.IsTrue(manager.TasksOf(res).Count() == 2);
            Assert.IsTrue(manager.Resources.Contains(res));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(res));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(res));
            Assert.IsTrue(manager.TasksOf(res).Contains(one));
            Assert.IsTrue(manager.TasksOf(res).Contains(two));
        }

        [TestMethod]
        public void TwoWayResourceLookup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var r1 = new Task();
            var r2 = new Task();
            manager.Add(one);
            manager.Add(two);

            // check initializations are correct
            Assert.IsTrue(manager.ResourcesOf(one) != null);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 0);
            Assert.IsTrue(manager.ResourcesOf(two) != null);
            Assert.IsTrue(manager.ResourcesOf(two).Count() == 0);

            // assign resource
            manager.Assign(one, r1);
            Assert.IsTrue(manager.ResourcesOf(one) != null);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).FirstOrDefault().Equals(r1));
            Assert.IsTrue(manager.TasksOf(r1).Count() == 1);
            Assert.IsTrue(manager.TasksOf(r1).FirstOrDefault().Equals(one));

            // assign another resource
            manager.Assign(one, r2);
            Assert.IsTrue(manager.ResourcesOf(one) != null);
            Assert.IsTrue(manager.ResourcesOf(one).Count() == 2);
            Assert.IsTrue(manager.ResourcesOf(one).ElementAtOrDefault(1).Equals(r2));
            Assert.IsTrue(manager.TasksOf(r2).Count() == 1);
            Assert.IsTrue(manager.TasksOf(r2).FirstOrDefault().Equals(one));

            // assign resource to another task
            manager.Assign(two, r2);
            Assert.IsTrue(manager.ResourcesOf(two) != null);
            Assert.IsTrue(manager.ResourcesOf(two).Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(two).FirstOrDefault().Equals(r2));
            Assert.IsTrue(manager.TasksOf(r2).Count() == 2);
            Assert.IsTrue(manager.TasksOf(r2).ElementAtOrDefault(1).Equals(two));
        }

        [TestMethod]
        public void UnassignSpecificResource()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var r1 = new Task();
            var r2 = new Task();
            manager.Add(one);

            // setup: assign some resource
            manager.Assign(one, r1);
            manager.Assign(one, r2);
            Assert.IsTrue(manager.Resources.Count() == 2);
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(manager.TasksOf(r2).Contains(one));

            // test: unassign resource
            manager.Unassign(one, r2);
            Assert.IsTrue(manager.Resources.Count() == 1);
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(!manager.TasksOf(r2).Contains(one));
        }

        [TestMethod]
        public void UnassignAllResourceFromTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var r1 = new Task();
            var r2 = new Task();
            manager.Add(one);
            manager.Add(two);

            // setup: assign some resource
            manager.Assign(one, r1);
            manager.Assign(one, r2);
            manager.Assign(two, r1);
            manager.Assign(two, r2);
            Assert.IsTrue(manager.Resources.Count() == 2);
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r2));
            Assert.IsTrue(manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(manager.TasksOf(r2).Contains(one));
            Assert.IsTrue(manager.TasksOf(r1).Contains(two));
            Assert.IsTrue(manager.TasksOf(r2).Contains(two));

            manager.Unassign(one);
            Assert.IsTrue(manager.Resources.Count() == 2);
            Assert.IsTrue(!manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(!manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r2));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(!manager.TasksOf(r2).Contains(one));
            Assert.IsTrue(manager.TasksOf(r1).Contains(two));
            Assert.IsTrue(manager.TasksOf(r2).Contains(two));
        }

        [TestMethod]
        public void UnassignResourceFromAllTask()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var r1 = new object();
            var r2 = new object();
            manager.Add(one);
            manager.Add(two);

            // setup: assign some resource
            manager.Assign(one, r1);
            manager.Assign(one, r2);
            manager.Assign(two, r1);
            manager.Assign(two, r2);
            Assert.IsTrue(manager.Resources.Count() == 2);
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r2));
            Assert.IsTrue(manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(manager.TasksOf(r2).Contains(one));
            Assert.IsTrue(manager.TasksOf(r1).Contains(two));
            Assert.IsTrue(manager.TasksOf(r2).Contains(two));

            manager.Unassign(r1);
            Assert.IsTrue(manager.Resources.Count() == 1, string.Format("Expected {0} != {1}", 1, manager.Resources.Count()));
            Assert.IsTrue(!manager.ResourcesOf(one).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(one).Contains(r2));
            Assert.IsTrue(!manager.ResourcesOf(two).Contains(r1));
            Assert.IsTrue(manager.ResourcesOf(two).Contains(r2));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(one));
            Assert.IsTrue(manager.TasksOf(r2).Contains(one));
            Assert.IsTrue(!manager.TasksOf(r1).Contains(two));
            Assert.IsTrue(manager.TasksOf(r2).Contains(two));
        }
       
        [TestMethod]
        public void GroupCannotBeRelated()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var group = new Task();
            var member = new Task();
            var one = new Task();
            manager.Add(group);
            manager.Add(member);
            manager.Add(one);

            // setup: make a group
            manager.Group(group, member);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(manager.IsMember(member));
            Assert.IsTrue(!manager.HasRelations(one));

            // test: relate a task and a group (not allowed)
            manager.Relate(group, one);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(manager.IsMember(member));
            Assert.IsTrue(!manager.HasRelations(group));
            Assert.IsTrue(!manager.HasRelations(one));

            // test: relate a task and a group, now in another order (not allowed)
            manager.Relate(one, group);
            Assert.IsTrue(manager.IsGroup(group));
            Assert.IsTrue(manager.IsMember(member));
            Assert.IsTrue(!manager.HasRelations(group));
            Assert.IsTrue(!manager.HasRelations(one));
        }
        
        [TestMethod]
        public void HasRelationTaskCannotBecomeGroup()
        {
            IProjectManager<Task, object> manager = new ProjectManager<Task, object>();
            var one = new Task();
            var two = new Task();
            var member = new Task();
            manager.Add(one);
            manager.Add(two);
            manager.Add(member);

            // setup: create a relation
            manager.Relate(one, two);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));

            // test: make a group with another task (not allowed)
            manager.Group(one, member);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
            Assert.IsTrue(!manager.IsGroup(one));
            Assert.IsTrue(!manager.IsMember(member));

            // test: make a group with another task (not allowed)
            manager.Group(two, member);
            Assert.IsTrue(manager.HasRelations(one));
            Assert.IsTrue(manager.HasRelations(two));
            Assert.IsTrue(!manager.IsGroup(two));
            Assert.IsTrue(!manager.IsMember(member));
        }

        
    }
}
