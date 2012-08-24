using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Braincase.GanttChart
{
    /// <summary>
    /// Generic rooted DFS tree where all nodes are unique references and no circular referencing allowed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Tree<T> : IEnumerable<T> where T : Tree<T>
    {
        protected List<T> mChildren = new List<T>();

        internal bool Add(T item)
        {
            if (item == this || this.Ancestors.Contains(item))
            {
                return false;
            }
            else
            {
                if (item.Parent != null) item.Leave();
                item.Parent = this as T;
                mChildren.Add(item);
                return true;
            }
        }

        internal bool Insert(int index, T item)
        {
            if (item == this) return false;
            if (item.Parent != null) item.Leave();
            item.Parent = this as T;
            mChildren.Insert(index, item);
            return true;
        }

        internal void Leave()
        {
            this.Parent.Remove(this as T);
        }

        internal bool Remove(T item)
        {
            return mChildren.Remove(item);
        }

        internal void Clear()
        {
            mChildren.Clear();
        }

        internal int IndexOf(T item)
        {
            return mChildren.IndexOf(item);
        }

        /// <summary>
        /// Get the parent node
        /// </summary>
        public T Parent { get; private set; }

        /// <summary>
        /// Get the root node
        /// </summary>
        public T Root
        {
            get
            {
                return Ancestors.Last();
            }
        }

        /// <summary>
        /// Eumerate up the tree through all the parents
        /// </summary>
        public IEnumerable<T> Ancestors
        {
            get
            {
                T parent = this.Parent;
                while (parent != null)
                {
                    yield return parent;
                    parent = parent.Parent;
                }
            }
        }

        /// <summary>
        /// Enumerate direct children nodes
        /// </summary>
        public IEnumerable<T> Children
        {
            get
            {
                return mChildren.ToArray();
            }
        }

        /// <summary>
        /// Enumerator down the tree through all decendants
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            Stack<T> destinations = new Stack<T>();
            T current;
            foreach (var child in mChildren)
            {
                destinations.Push(child);

                while (destinations.Count > 0)
                {
                    current = destinations.Pop();
                    var tempstack = new Stack<T>();
                    foreach (var dest in current.mChildren) tempstack.Push(dest);
                    tempstack.ToList().ForEach(x => destinations.Push(x));
                    yield return current;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class Task : Tree<Task>
    {
        internal Task(Project project)
        {
            Complete = 0.0f;
            Start = 0;
            Duration = 1;
            Delay = 0;
            IsCollapsed = false;
            _mProject = project;
        }

        /// <summary>
        /// Custom user state for storing additional information regarding the task
        /// </summary>
        public Object UserState { get; set; }

        /// <summary>
        /// Indicate whether this task is collapsed such that sub tasks are hidden from view. Only groups can be collasped.
        /// </summary>
        public bool IsCollapsed { get {
            return this.IsGroup && _mIsCollapsed;
        } set{

            if (this.IsGroup) _mIsCollapsed = value;
            else _mIsCollapsed = false;
        }
        }

        /// <summary>
        /// Get or set the pecentage complete of this task, expressed in decimal between 0.0 and 1.0f.
        /// </summary>
        public float Complete
        {
            get
            {
                return _GetComplete();
            }
            set
            {
                _mComplete = value;
                if (_mComplete < 0) _mComplete = 0;
                else if (_mComplete > 1) _mComplete = 1;
            }
        }

        /// <summary>
        /// Get whether this Task is a task group with sub tasks.
        /// </summary>
        public bool IsGroup { get { return this.mChildren.Count > 0; } }

        /// <summary>
        /// Get or set the Name of this Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set the number of period after the end of all precedent Tasks, on which this Task will begin.
        /// </summary>
        public int Delay { 
            get { return _mDelay; } 
            set
            { 
                _mDelay = value;
                if (_mDelay < 0) _mDelay = 0;
            } 
        }
        
        /// <summary>
        /// Get or set the number of period after which this Task will begin in the absence of any precedent Tasks
        /// </summary>
        public int Start
        {
            get
            {
                return _GetStart();
            }
            set
            {
                _mStart = value;
                if (_mStart < 0) _mStart = 0;
            }
        }

        public int End
        {
            get
            {
                return this.Start + this.Duration;
            }
            set
            {
                _mDuration = value - this.Start;
                if (_mDuration < 1) _mDuration = 1;
            }   
        }

        public int Duration
        {
            get
            {
                return _GetDuration();
            }
            set
            {
                _mDuration = value;
                if (_mDuration < 1) _mDuration = 1;
            }
        }

        public int Slack
        {
            get
            {
                if (!this.IsGroup && _mProject.Relationships.Dependants(this).FirstOrDefault() != null)
                {
                    return _mProject.Relationships.Dependants(this).Aggregate((x1, x2) => !x1.IsGroup && (x1.Start < x2.Start) ? x1 : x2).Start - this.End - 1;
                }
                else if (this.IsGroup)
                {
                    var t = this.Aggregate((x1, x2) => x1.End + x1.Slack > x2.End + x2.Slack ? x1 : x2);
                    return t.End + t.Slack - this.End;
                }
                else
                {
                    return _mProject.Tasks.Aggregate((x1, x2) => !x1.IsGroup && (x1.End > x2.End) ? x1 : x2).End - this.End;
                }
            }
        }

        private int _GetStart()
        {
            int start = 0;

            // A task group, does not have its own start, uses the ealiest start in subtask.
            if (this.Count() > 0)
                start = this.Min(x => x.Start);
            // A normal task without predecessor uses own start
            else if (this._mProject.Relationships[this].FirstOrDefault() == null)
                 start = _mStart;
            // A normal task with predecessor starts after the predecessor ends after Delay.
            else
                start = this._mProject.Relationships[this].Max(x => x.End) + this.Delay + 1;

            return start;
        }

        private float _GetComplete()
        {
            if (this.Count() > 0)
            {
                return this.Sum(x => x.Complete * x.Duration) / this.Sum(x => x.Duration);
            }
            else
            {
                return _mComplete;
            }
        }
        
        private int _GetDuration()
        {
            if (this.Count() > 0)
            {
                return this.Max(x => x.End) - this.Min(x => x.Start);
            }
            else
            {
                return _mDuration;
            }
        }

        private float _mComplete = 0.0f;

        private int _mDuration = 0;

        private int _mStart = 0;

        private int _mDelay = 0;

        private bool _mIsCollapsed = false;

        private Project _mProject = null;
    }

    public class RelationshipManager
    {
        Dictionary<Task, List<Task>> _mPrecedents = new Dictionary<Task, List<Task>>();

        public void Add(Task before, Task after)
        {
            if (before == after
                || this.PrecedentTree(after).Contains(before)
                || this.PrecedentTree(before).Contains(after)
                || after.Count() > 0
                || before.Count() > 0)
            {
                // not allowed
            }
            else
            {
                // Hard Adding
                var precedents = _GetOrCreatePrecedentList(before);
                precedents.Add(after);
            }
        }

        public void Delete(Task before, Task after)
        {
            _mPrecedents[before].Remove(after);
        }

        public void Delete(Task beforeOrAfter)
        {
            _mPrecedents.Remove(beforeOrAfter);
            var query = _mPrecedents.Where(x => x.Value.Contains(beforeOrAfter)).Select(x => x.Value);
            foreach (var list in query) list.Remove(beforeOrAfter);
        }

        public IEnumerable<Task> this[Task before]
        {
            get { return _GetOrCreatePrecedentList(before).ToArray(); }
        }

        public IEnumerable<Task> PrecedentTree(Task before)
        {
            var precedents = _GetOrCreatePrecedentList(before);

            Stack<Task> destinations = new Stack<Task>();
            Task current;
            foreach (var p in precedents)
            {
                destinations.Push(p);
                while (destinations.Count > 0)
                {
                    current = destinations.Pop();
                    foreach(var pp in this[current]) destinations.Push(pp);
                    yield return current;
                }
            }
        }

        /// <summary>
        /// Enumerate through the dependant tasks that must occur after the specified task
        /// </summary>
        /// <param name="after"></param>
        /// <returns></returns>
        public IEnumerable<Task> Dependants(Task after)
        {
            return _mPrecedents.Where(x => x.Value.Contains(after)).Select(x => x.Key);
        }

        public void Clear()
        {
            _mPrecedents.Clear();
        }

        private List<Task> _GetOrCreatePrecedentList(Task before)
        {
            List<Task> precedents;
            if (!_mPrecedents.TryGetValue(before, out precedents) || precedents == null)
                precedents = _mPrecedents[before] = new List<Task>();

            return precedents;
        }
    }

    public class ResourceManager
    {
        Dictionary<object, List<Task>> _mResourceToTasks = new Dictionary<object, List<Task>>();
        Dictionary<Task, List<object>> _mTasksToResource = new Dictionary<Task, List<object>>();

        /// <summary>
        /// Assign a resource to a task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void AssignResource(Task task, object resource)
        {
            var tasks = _GetResourceTasks(resource);
            var resources = _GetTaskResources(task);

            tasks.Add(task);
            resources.Add(resource);
        }

        /// <summary>
        /// Unassign the specified resource from the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void UnassignResource(Task task, object resource)
        {
            _GetTaskResources(task).Remove(resource);
        }

        /// <summary>
        /// Unassign all resources from the specified task
        /// </summary>
        /// <param name="task"></param>
        public void UnassignResource(Task task)
        {
            var resources = _GetTaskResources(task);
            foreach (var r in resources)
                _mResourceToTasks[r].Remove(task);
            _mTasksToResource.Remove(task);
        }

        /// <summary>
        /// Remove the specified resource from all tasks
        /// </summary>
        /// <param name="resource"></param>
        public void Remove(object resource)
        {
            var tasks = _GetResourceTasks(resource);
            foreach (var t in tasks)
                _mTasksToResource[t].Remove(resource);

            _mResourceToTasks.Remove(resource);
        }

        /// <summary>
        /// Enumerate through the tasks that uses the specified resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public IEnumerable<Task> GetTasks(object resource)
        {
            return _GetResourceTasks(resource).ToArray();
        }

        /// <summary>
        /// Enumerate through the resources that the task uses
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<object> GetResources(Task task)
        {
            return _GetTaskResources(task).ToArray();
        }

        /// <summary>
        /// Clear all task and resource data in the ResourceManager
        /// </summary>
        public void Clear()
        {
            _mResourceToTasks.Clear();
            _mTasksToResource.Clear();
        }

        private List<Task> _GetResourceTasks(object resource)
        {
            List<Task> tasks;
            if (!_mResourceToTasks.TryGetValue(resource, out tasks))
                _mResourceToTasks[resource] = tasks = new List<Task>();

            return tasks;
        }

        private List<object> _GetTaskResources(Task task)
        {
            List<object> resources;
            if (!_mTasksToResource.TryGetValue(task, out resources))
                _mTasksToResource[task] = resources = new List<object>();

            return resources;
        }
    }

    public enum TimeScale
    {
        Day, Week
    }

    public interface Object
    {
        string Name { get; set; }
    }

    public class Project
    {
        /// <summary>
        /// Create a new Project
        /// </summary>
        public Project()
        {
            Tasks = new Task(this);
            Relationships = new RelationshipManager();
            Resources = new ResourceManager();
            Now = 0;
            Start = DateTime.Now;
            TimeScale = GanttChart.TimeScale.Day;
        }

        /// <summary>
        /// Get the Task tree
        /// </summary>
        public Task Tasks { get; private set; }

        /// <summary>
        /// Create a new Task for this Project and add it to the Task tree
        /// </summary>
        /// <returns></returns>
        public Task CreateTask()
        {
            var task = new Task(this);
            Tasks.Add(task);
            return task;
        }

        /// <summary>
        /// Add the member Task to the group Task
        /// </summary>
        /// <param name="group"></param>
        /// <param name="member"></param>
        public void GroupTask(Task group, Task member)
        {
            if(group.Add(member))
                Relationships.Delete(group);
        }

        /// <summary>
        /// Remove the member task from its group
        /// </summary>
        /// <param name="member"></param>
        public void UngroupTask(Task member)
        {
            member.Leave();
            Tasks.Add(member);
        }

        /// <summary>
        /// Remove task from this Project
        /// </summary>
        /// <param name="task"></param>
        public void Remove(Task task)
        {
            task.Leave();
            Relationships.Delete(task);
            Resources.UnassignResource(task);
        }

        /// <summary>
        /// Get the zero-based index of the task in this Project
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int IndexOf(Task task)
        {
            int i = 0;
            foreach (var x in Tasks)
            {
                if (x.Equals(task)) return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Re-position the task by offset amount of places
        /// </summary>
        /// <param name="task"></param>
        /// <param name="offset"></param>
        public void Move(Task task, int offset)
        {
            int i = IndexOf(task);
            int index = i + offset;
            if(index < 0) offset = 0;
            else if(index > Tasks.Count()) index = Tasks.Count();
            var dest = Tasks.ElementAtOrDefault(index);

            if (dest == null) Tasks.Add(task);
            else if (!dest.Equals(task))
            {
                var p = dest.Parent.IndexOf(dest);
                if (offset <= 0) dest.Parent.Insert(p, task);
                else dest.Parent.Insert(Math.Max(0, p - 1), task);
            }
        }

        /// <summary>
        /// Get the PrecedentManager
        /// </summary>
        public RelationshipManager Relationships { get; private set; }

        public ResourceManager Resources { get; private set; }

        /// <summary>
        /// Get or set the period we are at now
        /// </summary>
        public int Now { get; set; }

        /// <summary>
        /// Get or set the starting date for this project
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Get or set the time scale on this project. Each period on the task represents one unit of TimeScale.
        /// </summary>
        public TimeScale TimeScale { get; set; }

        /// <summary>
        /// Get the date after the specified period based on TimeScale
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int period)
        {
            DateTime datetime = DateTime.Now;
            if (this.TimeScale == TimeScale.Day)
            {
                datetime = this.Start.AddDays(period);
            }
            else if(this.TimeScale == TimeScale.Week)
            {
                datetime = this.Start.AddDays(period * 7 - (int)this.Start.DayOfWeek);
            }
            return datetime;
        }

        /// <summary>
        /// Enumerate list of critical paths in Project
        /// </summary>
        public IEnumerable<IEnumerable<Task>> CriticalPaths
        {
            get
            {
                int max = Tasks.Max(x => x.End);
                var paths = Tasks.Where(x => x.End == max);
                foreach (var path in paths)
                {
                    var list = new List<Task>() { path };
                    yield return list.Concat(Relationships.PrecedentTree(path));
                }
            }
        }

        
    }
}
