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
            Project project = new Project();

            // create first task
            var first = project.CreateTask();
            Assert.IsTrue(project.Tasks.Count() == 1);
            Assert.IsTrue(first.Parent == project.Tasks);

            // create second task, remove first task
            var second = project.CreateTask();
            second.Name = "Apple Jack";
            project.Remove(first);
            var firstordefault = project.Tasks.FirstOrDefault();
            Assert.IsTrue(firstordefault != null);
            Assert.IsTrue(firstordefault.Name == "Apple Jack");
            Assert.IsTrue(firstordefault.Equals(second));

            // remove a task that is already removed
            project.Remove(first);
            Assert.IsTrue(project.Tasks.Count() == 1);

            // remove a null task
            project.Remove(null);
            Assert.IsTrue(project.Tasks.Count() == 1);
        }

        [TestMethod]
        public void CreateAndRemoveGroup()
        {
            var project = new Project();
            var group1 = project.CreateTask();
            var one = project.CreateTask();
            var two = project.CreateTask();

            // make group
            project.GroupTask(group1, one);
            project.GroupTask(group1, two);
            Assert.IsTrue(project.Tasks.Count() == 3);
            Assert.IsTrue(group1.Children.Count() == 2);
            Assert.IsTrue(one.Parent.Equals(group1));
            Assert.IsTrue(two.Parent.Equals(group1));
            

            // delete group task
            project.Remove(group1);
            Assert.IsTrue(project.Tasks.Count() == 2);
            Assert.IsTrue(group1.Children.Count() == 0);
            Assert.IsTrue(one.Parent.Equals(project.Tasks));
            Assert.IsTrue(two.Parent.Equals(project.Tasks));
            
        }

        [TestMethod]
        public void MoveSingleTaskToCheckForOutOfBoundHandling()
        {
            Project project = new Project();

            // create a task
            var first = project.CreateTask();
            Assert.IsTrue(project.Tasks.Count() == 1);

            // get task index
            var index = project.IndexOf(first);
            Assert.IsTrue(index == 0, string.Format("Task index should be {0}, but is {1}", 0, index));

            // move task by 0 offset
            project.Move(first, 0);
            index = project.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by negative offset
            project.Move(first, -1);
            index = project.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by count offset 
            project.Move(first, 1);
            index = project.IndexOf(first);
            Assert.IsTrue(index == 0);

            // move task by positive offset more than count
            project.Move(first, 2);
            index = project.IndexOf(first);
            Assert.IsTrue(index == 0);
        }

        [TestMethod]
        public void MoveTasksAroundSingleLevel()
        {
            var project = new Project();

            // create tasks
            var one = project.CreateTask();
            var two = project.CreateTask();
            var three = project.CreateTask();

            // get index of one
            Assert.IsTrue(project.IndexOf(one) == 0);
            Assert.IsTrue(project.IndexOf(two) == 1);
            Assert.IsTrue(project.IndexOf(three) == 2);
            // move by 1 offset each time
            project.Move(one, 1);
            Assert.IsTrue(project.IndexOf(two) == 0);
            Assert.IsTrue(project.IndexOf(one) == 1);
            Assert.IsTrue(project.IndexOf(three) == 2);
            project.Move(one, 1);
            Assert.IsTrue(project.IndexOf(two) == 0);
            Assert.IsTrue(project.IndexOf(three) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
            project.Move(one, 1);
            Assert.IsTrue(project.IndexOf(two) == 0);
            Assert.IsTrue(project.IndexOf(three) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
            
            // move by 1 offset each time
            project.Move(two, 1);
            Assert.IsTrue(project.IndexOf(three) == 0);
            Assert.IsTrue(project.IndexOf(two) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
            project.Move(two, 1);
            Assert.IsTrue(project.IndexOf(three) == 0);
            Assert.IsTrue(project.IndexOf(one) == 1);
            Assert.IsTrue(project.IndexOf(two) == 2);
            project.Move(two, 1);
            Assert.IsTrue(project.IndexOf(three) == 0);
            Assert.IsTrue(project.IndexOf(one) == 1);
            Assert.IsTrue(project.IndexOf(two) == 2);

            // move by -1 offset each time
            project.Move(two, -1);
            Assert.IsTrue(project.IndexOf(three) == 0);
            Assert.IsTrue(project.IndexOf(two) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
            project.Move(two, -1);
            Assert.IsTrue(project.IndexOf(two) == 0);
            Assert.IsTrue(project.IndexOf(three) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
            project.Move(two, -1);
            Assert.IsTrue(project.IndexOf(two) == 0);
            Assert.IsTrue(project.IndexOf(three) == 1);
            Assert.IsTrue(project.IndexOf(one) == 2);
        }

        [TestMethod]
        public void MoveGroupsAround()
        {
            var project = new Project();
            // groups
            var group1 = project.CreateTask();
            var group2 = project.CreateTask();
            var group3 = project.CreateTask();
            // group 1 tasks
            var g1t1 = project.CreateTask();
            var g1t2 = project.CreateTask();
            var g1t3 = project.CreateTask();
            // group 2 tasks
            var g2t1 = project.CreateTask();
            var g2t2 = project.CreateTask();
            var g2t3 = project.CreateTask();
            // group 3 tasks
            var g3t1 = project.CreateTask();
            var g3t2 = project.CreateTask();
            var g3t3 = project.CreateTask();
            // make groups
            project.GroupTask(group1, g1t1);
            project.GroupTask(group1, g1t2);
            project.GroupTask(group1, g1t3);
            // make groups
            project.GroupTask(group2, g2t1);
            project.GroupTask(group2, g2t2);
            project.GroupTask(group2, g2t3);
            // make groups
            project.GroupTask(group3, g3t1);
            project.GroupTask(group3, g3t2);
            project.GroupTask(group3, g3t3);

            // confirm the order
            foreach (var task in project.Tasks)
                Assert.IsTrue(task.Parent != null);

            Assert.IsTrue(project.IndexOf(group1) == 0);
            Assert.IsTrue(project.IndexOf(g1t1) == 1);
            Assert.IsTrue(project.IndexOf(g1t2) == 2);
            Assert.IsTrue(project.IndexOf(g1t3) == 3);
            Assert.IsTrue(project.IndexOf(group2) == 4);
            Assert.IsTrue(project.IndexOf(g2t1) == 5);
            Assert.IsTrue(project.IndexOf(g2t2) == 6);
            Assert.IsTrue(project.IndexOf(g2t3) == 7);
            Assert.IsTrue(project.IndexOf(group3) == 8);
            Assert.IsTrue(project.IndexOf(g3t1) == 9);
            Assert.IsTrue(project.IndexOf(g3t2) == 10);
            Assert.IsTrue(project.IndexOf(g3t3) == 11);

            // move group under itself 1 (not allowed)
            project.Move(group1, 1);
            Assert.IsTrue(project.IndexOf(group1) == 0);
            Assert.IsTrue(project.IndexOf(g1t1) == 1);
            Assert.IsTrue(project.IndexOf(g1t2) == 2);
            Assert.IsTrue(project.IndexOf(g1t3) == 3);
            Assert.IsTrue(project.IndexOf(group2) == 4);
            Assert.IsTrue(project.IndexOf(g2t1) == 5);
            Assert.IsTrue(project.IndexOf(g2t2) == 6);
            Assert.IsTrue(project.IndexOf(g2t3) == 7);
            Assert.IsTrue(project.IndexOf(group3) == 8);
            Assert.IsTrue(project.IndexOf(g3t1) == 9);
            Assert.IsTrue(project.IndexOf(g3t2) == 10);
            Assert.IsTrue(project.IndexOf(g3t3) == 11);

            // move group under itself 2 (not allowed)
            project.Move(group1, 2);
            Assert.IsTrue(project.IndexOf(group1) == 0);
            Assert.IsTrue(project.IndexOf(g1t1) == 1);
            Assert.IsTrue(project.IndexOf(g1t2) == 2);
            Assert.IsTrue(project.IndexOf(g1t3) == 3);
            Assert.IsTrue(project.IndexOf(group2) == 4);
            Assert.IsTrue(project.IndexOf(g2t1) == 5);
            Assert.IsTrue(project.IndexOf(g2t2) == 6);
            Assert.IsTrue(project.IndexOf(g2t3) == 7);
            Assert.IsTrue(project.IndexOf(group3) == 8);
            Assert.IsTrue(project.IndexOf(g3t1) == 9);
            Assert.IsTrue(project.IndexOf(g3t2) == 10);
            Assert.IsTrue(project.IndexOf(g3t3) == 11);

            // move group under itself 3 (not allowed)
            project.Move(group1, 3);
            Assert.IsTrue(project.IndexOf(group1) == 0);
            Assert.IsTrue(project.IndexOf(g1t1) == 1);
            Assert.IsTrue(project.IndexOf(g1t2) == 2);
            Assert.IsTrue(project.IndexOf(g1t3) == 3);
            Assert.IsTrue(project.IndexOf(group2) == 4);
            Assert.IsTrue(project.IndexOf(g2t1) == 5);
            Assert.IsTrue(project.IndexOf(g2t2) == 6);
            Assert.IsTrue(project.IndexOf(g2t3) == 7);
            Assert.IsTrue(project.IndexOf(group3) == 8);
            Assert.IsTrue(project.IndexOf(g3t1) == 9);
            Assert.IsTrue(project.IndexOf(g3t2) == 10);
            Assert.IsTrue(project.IndexOf(g3t3) == 11);

            // move group under itself 4 (not allowed)
            project.Move(group1, 4);
            Assert.IsTrue(project.IndexOf(group2) == 0);
            Assert.IsTrue(project.IndexOf(g2t1) == 1);
            Assert.IsTrue(project.IndexOf(g2t2) == 2);
            Assert.IsTrue(project.IndexOf(g2t3) == 3);
            Assert.IsTrue(project.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, project.IndexOf(group1)));
            Assert.IsTrue(project.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, project.IndexOf(g1t1)));
            Assert.IsTrue(project.IndexOf(g1t2) == 6, string.Format("{0} != {1}", 0, project.IndexOf(g1t2)));
            Assert.IsTrue(project.IndexOf(g1t3) == 7, string.Format("{0} != {1}", 0, project.IndexOf(g1t3)));
            Assert.IsTrue(project.IndexOf(group3) == 8);
            Assert.IsTrue(project.IndexOf(g3t1) == 9);
            Assert.IsTrue(project.IndexOf(g3t2) == 10);
            Assert.IsTrue(project.IndexOf(g3t3) == 11);

            // move group under another group
            project.Move(group3, -1);
            Assert.IsTrue(project.IndexOf(group2) == 0);
            Assert.IsTrue(project.IndexOf(g2t1) == 1);
            Assert.IsTrue(project.IndexOf(g2t2) == 2);
            Assert.IsTrue(project.IndexOf(g2t3) == 3);
            Assert.IsTrue(project.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, project.IndexOf(group1)));
            Assert.IsTrue(project.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, project.IndexOf(g1t1)));
            Assert.IsTrue(project.IndexOf(g1t2) == 6, string.Format("{0} != {1}", 0, project.IndexOf(g1t2)));
            Assert.IsTrue(project.IndexOf(group3) == 7);
            Assert.IsTrue(project.IndexOf(g3t1) == 8);
            Assert.IsTrue(project.IndexOf(g3t2) == 9);
            Assert.IsTrue(project.IndexOf(g3t3) == 10);
            Assert.IsTrue(project.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, project.IndexOf(g1t3)));

            // move group within group
            project.Move(group3, -1);
            Assert.IsTrue(project.IndexOf(group2) == 0);
            Assert.IsTrue(project.IndexOf(g2t1) == 1);
            Assert.IsTrue(project.IndexOf(g2t2) == 2);
            Assert.IsTrue(project.IndexOf(g2t3) == 3);
            Assert.IsTrue(project.IndexOf(group1) == 4, string.Format("{0} != {1}", 0, project.IndexOf(group1)));
            Assert.IsTrue(project.IndexOf(g1t1) == 5, string.Format("{0} != {1}", 0, project.IndexOf(g1t1)));
            Assert.IsTrue(project.IndexOf(group3) == 6);
            Assert.IsTrue(project.IndexOf(g3t1) == 7);
            Assert.IsTrue(project.IndexOf(g3t2) == 8);
            Assert.IsTrue(project.IndexOf(g3t3) == 9);
            Assert.IsTrue(project.IndexOf(g1t2) == 10, string.Format("{0} != {1}", 0, project.IndexOf(g1t2)));
            Assert.IsTrue(project.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, project.IndexOf(g1t3)));

            // move group out of another group
            project.Move(group3, -2);
            Assert.IsTrue(project.IndexOf(group2) == 0);
            Assert.IsTrue(project.IndexOf(g2t1) == 1);
            Assert.IsTrue(project.IndexOf(g2t2) == 2);
            Assert.IsTrue(project.IndexOf(g2t3) == 3);
            Assert.IsTrue(project.IndexOf(group3) == 4);
            Assert.IsTrue(project.IndexOf(g3t1) == 5);
            Assert.IsTrue(project.IndexOf(g3t2) == 6);
            Assert.IsTrue(project.IndexOf(g3t3) == 7);
            Assert.IsTrue(project.IndexOf(group1) == 8, string.Format("{0} != {1}", 0, project.IndexOf(group1)));
            Assert.IsTrue(project.IndexOf(g1t1) == 9, string.Format("{0} != {1}", 0, project.IndexOf(g1t1)));
            Assert.IsTrue(project.IndexOf(g1t2) == 10, string.Format("{0} != {1}", 0, project.IndexOf(g1t2)));
            Assert.IsTrue(project.IndexOf(g1t3) == 11, string.Format("{0} != {1}", 0, project.IndexOf(g1t3)));
        }

        [TestMethod]
        public void GroupAndUngroup()
        {
            var project = new Project();
            var group1 = project.CreateTask();
            var group2 = project.CreateTask();
            var one = project.CreateTask();

            // put null task into group
            project.GroupTask(group1, null);
            Assert.IsTrue(group1.Children.Count() == 0);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 3);

            // put task into null group
            project.GroupTask(null, one);
            Assert.IsTrue(group1.Children.Count() == 0);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 3);

            // group self
            project.GroupTask(group1, group1);
            Assert.IsTrue(group1.Children.Count() == 0);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 3);

            // grouping
            project.GroupTask(group1, one);
            Assert.IsTrue(one.Parent.Equals(group1));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 2);

            // grouping into parent
            project.GroupTask(group1, one);
            Assert.IsTrue(one.Parent.Equals(group1));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 2);

            // group to another group
            project.GroupTask(group2, one);
            Assert.IsTrue(one.Parent.Equals(group2));
            Assert.IsTrue(group1.Children.Count() == 0);
            Assert.IsTrue(group2.Children.Count() == 1);
            Assert.IsTrue(project.Tasks.Children.Count() == 2);

            // sub grouping
            project.GroupTask(group1, group2);
            Assert.IsTrue(one.Parent.Equals(group2));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 1);
            Assert.IsTrue(project.Tasks.Children.Count() == 1);

            // group into grandchild (not allowed)
            project.GroupTask(one, group1);
            Assert.IsTrue(one.Parent.Equals(group2));
            Assert.IsTrue(group2.Parent.Equals(group1));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 1);
            Assert.IsTrue(project.Tasks.Children.Count() == 1);

            // group into grandparent
            project.GroupTask(group1, one);
            Assert.IsTrue(group1.Children.Count() == 2);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 1);

            // group into child (not allowed)
            project.GroupTask(group2, group1);
            Assert.IsTrue(group1.Children.Count() == 2);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 1);            

            // remove from group
            project.UngroupTask(one);
            Assert.IsTrue(one.Parent.Equals(project.Tasks));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 2);

            // remove already removed task from group
            project.UngroupTask(one);
            Assert.IsTrue(one.Parent.Equals(project.Tasks));
            Assert.IsTrue(group1.Children.Count() == 1);
            Assert.IsTrue(group2.Children.Count() == 0);
            Assert.IsTrue(project.Tasks.Children.Count() == 2);
        }

        [TestMethod]
        public void GroupTaskLevelAndOrdering()
        {
            var project = new Project();
            var zero = project.CreateTask();
            var one = project.CreateTask();
            var two = project.CreateTask();
            var three = project.CreateTask();
            var four = project.CreateTask();
            var five = project.CreateTask();

            // single level ordering
            Assert.IsTrue(project.IndexOf(zero) == 0);
            Assert.IsTrue(project.IndexOf(one) == 1);
            Assert.IsTrue(project.IndexOf(two) == 2);
            Assert.IsTrue(project.IndexOf(three) == 3);
            Assert.IsTrue(project.IndexOf(four) == 4);
            Assert.IsTrue(project.IndexOf(five) == 5);
            Assert.IsTrue(project.Tasks.Count() == 6);

            // two level ordering
            project.GroupTask(zero, two);
            project.GroupTask(zero, three);
            Assert.IsTrue(project.IndexOf(zero) == 0);
                Assert.IsTrue(project.IndexOf(two) == 1);
                Assert.IsTrue(project.IndexOf(three) == 2);
            Assert.IsTrue(project.IndexOf(one) == 3);
            Assert.IsTrue(project.IndexOf(four) == 4);
            Assert.IsTrue(project.IndexOf(five) == 5);
            Assert.IsTrue(project.Tasks.Count() == 6);

            // three level ordering
            project.GroupTask(five, zero);
            Assert.IsTrue(project.IndexOf(one) == 0);
            Assert.IsTrue(project.IndexOf(four) == 1);
            Assert.IsTrue(project.IndexOf(five) == 2, string.Format("Assert index == {0}; but index == {1}", 0, project.IndexOf(five)));
                Assert.IsTrue(project.IndexOf(zero) == 3);
                    Assert.IsTrue(project.IndexOf(two) == 4);
                    Assert.IsTrue(project.IndexOf(three) == 5);
            Assert.IsTrue(project.Tasks.Count() == 6);

            // twin three level ordering
            project.GroupTask(four, one);
            Assert.IsTrue(project.IndexOf(four) == 0);
                Assert.IsTrue(project.IndexOf(one) == 1);
            Assert.IsTrue(project.IndexOf(five) == 2, string.Format("Assert index == {0}; but index == {1}", 0, project.IndexOf(five)));
                Assert.IsTrue(project.IndexOf(zero) == 3);
                    Assert.IsTrue(project.IndexOf(two) == 4);
                    Assert.IsTrue(project.IndexOf(three) == 5);
            Assert.IsTrue(project.Tasks.Count() == 6);

            // four level ordering
            project.GroupTask(two, four);
            Assert.IsTrue(project.IndexOf(five) == 0, string.Format("Assert index == {0}; but index == {1}", 0, project.IndexOf(five)));
                Assert.IsTrue(project.IndexOf(zero) == 1);
                    Assert.IsTrue(project.IndexOf(two) == 2);
                        Assert.IsTrue(project.IndexOf(four) == 3);
                            Assert.IsTrue(project.IndexOf(one) == 4);
                    Assert.IsTrue(project.IndexOf(three) == 5);
            Assert.IsTrue(project.Tasks.Count() == 6);

            // check parents
            Assert.IsTrue(zero.Parent.Equals(five));
            Assert.IsTrue(one.Parent.Equals(four));
            Assert.IsTrue(two.Parent.Equals(zero));
            Assert.IsTrue(three.Parent.Equals(zero));
            Assert.IsTrue(four.Parent.Equals(two));
            Assert.IsTrue(five.Parent.Equals(project.Tasks));
        }

        [TestMethod]
        public void RelationCreateDestroyManagement()
        {
            var project = new Project();

            var one = project.CreateTask();
            Assert.IsTrue(project.Relationships.Precedents(one) != null);
            Assert.IsTrue(project.Relationships.Precedents(one).Count() == 0);

            project.Remove(one);
            Assert.IsTrue(project.IndexOf(one) == -1);
            Assert.IsTrue(project.Relationships.Precedents(one) == null);

            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
        }

        [TestMethod]
        public void AddRemoveRelation()
        {
            var project = new Project();

            // initialization
            var one = project.CreateTask();
            var two = project.CreateTask();
            var three = project.CreateTask();
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // Add relation
            project.Relationships.Add(one, two);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // Add existing relation
            project.Relationships.Add(one, two);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // Remove relation
            project.Relationships.Delete(one, two);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // remove non-existing relation
            project.Relationships.Delete(one, two);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // prevent 1 level circular relation
            project.Relationships.Add(one, two);
            project.Relationships.Add(two, one);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);

            // prevent 2 level circular relation
            project.Relationships.Add(one, two);
            project.Relationships.Add(two, three);
            project.Relationships.Add(three, one);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 2);

            // delete all relation
            project.Relationships.Delete(one);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 1);

            // multi dependants
            project.Relationships.Add(two, one);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 2);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 1);

            // multi precedents
            project.Relationships.Delete(two, three);
            project.Relationships.Add(three, one);
            Assert.IsTrue(project.Relationships.Dependants(one).Count() == 0);
            Assert.IsTrue(project.Relationships.Dependants(two).Count() == 1);
            Assert.IsTrue(project.Relationships.Dependants(three).Count() == 1);
            Assert.IsTrue(project.Relationships.PrecedentTree(one).Count() == 2);
            Assert.IsTrue(project.Relationships.PrecedentTree(two).Count() == 0);
            Assert.IsTrue(project.Relationships.PrecedentTree(three).Count() == 0);
        }

        [TestMethod]
        public void ResourceCreateDestroyManagement()
        {
            var project = new Project();

            var one = project.CreateTask();
            Assert.IsTrue(project.Resources.GetResources(one) != null);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 0);

            project.Remove(one);
            Assert.IsTrue(project.IndexOf(one) == -1);
            Assert.IsTrue(project.Resources.GetResources(one) == null);

            Assert.IsTrue(project.Resources.GetResources().Count() == 0);
        }

        [TestMethod]
        public void ResourceTwoWayLookup()
        {
            var project = new Project();
            var one = project.CreateTask();
            var two = project.CreateTask();
            var r1 = project.CreateTask();
            var r2 = project.CreateTask();

            // check initializations are correct
            Assert.IsTrue(project.Resources.GetResources(one) != null);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 0);
            Assert.IsTrue(project.Resources.GetResources(two) != null);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 0);

            // assign resource
            project.Resources.AssignResource(one, r1);
            Assert.IsTrue(project.Resources.GetResources(one) != null);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 1);
            Assert.IsTrue(project.Resources.GetResources(one).FirstOrDefault().Equals(r1));
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r1).FirstOrDefault().Equals(one));

            // assign another resource
            project.Resources.AssignResource(one, r2);
            Assert.IsTrue(project.Resources.GetResources(one) != null);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 2);
            Assert.IsTrue(project.Resources.GetResources(one).ElementAtOrDefault(1).Equals(r2));
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r2).FirstOrDefault().Equals(one));

            // assign resource to another task
            project.Resources.AssignResource(two, r2);
            Assert.IsTrue(project.Resources.GetResources(two) != null);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 1);
            Assert.IsTrue(project.Resources.GetResources(two).FirstOrDefault().Equals(r2));
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r2).ElementAtOrDefault(1).Equals(two));
        }

        [TestMethod]
        public void AddRemoveResource()
        {
            var project = new Project();
            var one = project.CreateTask();
            var two = project.CreateTask();
            var r1 = project.CreateTask();
            var r2 = project.CreateTask();
            var r3 = project.CreateTask();

            // assign resource
            project.Resources.AssignResource(one, r1);
            project.Resources.AssignResource(one, r2);
            project.Resources.AssignResource(one, r3);
            project.Resources.AssignResource(two, r2);
            project.Resources.AssignResource(two, r3);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 2);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 3);
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r3).Count() == 2);

            // assign resource that is already assigned
            project.Resources.AssignResource(one, r1);
            project.Resources.AssignResource(one, r2);
            project.Resources.AssignResource(one, r3);
            project.Resources.AssignResource(two, r2);
            project.Resources.AssignResource(two, r3);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 2);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 3);
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r3).Count() == 2);

            // unassign resource that is not actually assigned
            project.Resources.UnassignResource(two, r1);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 1);

            // unassign resource
            project.Resources.UnassignResource(one, r3);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 2);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 2);
            Assert.IsTrue(project.Resources.GetTasks(r3).Count() == 1, string.Format("{0} != {1}", 1, project.Resources.GetTasks(r3).Count()));

            // unassign task
            project.Resources.UnassignResource(one);
            Assert.IsTrue(project.Resources.GetResources(two).Count() == 2);
            Assert.IsTrue(project.Resources.GetResources(one).Count() == 0);
            Assert.IsTrue(project.Resources.GetTasks(r1).Count() == 0);
            Assert.IsTrue(project.Resources.GetTasks(r2).Count() == 1);
            Assert.IsTrue(project.Resources.GetTasks(r3).Count() == 1);
        }
    }
}
