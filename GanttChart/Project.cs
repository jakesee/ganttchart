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
            if (item == null || item == this || this.Ancestors.Contains(item) || item.Parent == this)
            {
                return false;
            }
            else
            {
                item.Leave();
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
            if (this.Parent != null)
            {
                this.Parent.Remove(this as T);
                this.Parent = null;
            }
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
            else if (this._mProject.Relationships.Precedents(this).FirstOrDefault() == null)
                 start = _mStart;
            // A normal task with predecessor starts after the predecessor ends after Delay.
            else
                start = this._mProject.Relationships.Precedents(this).Max(x => x.End) + this.Delay + 1;

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

        /// <summary>
        /// Create the precedent list for the specified dependant
        /// </summary>
        /// <param name="dependant"></param>
        internal void Create(Task dependant)
        {
            _mPrecedents[dependant] = new List<Task>();
        }

        /// <summary>
        /// Delete the precedent list for the specified dependant; subsequent attempt to get the list will give null
        /// </summary>
        /// <param name="dependant"></param>
        internal void Delete(Task dependant)
        {
            _mPrecedents.Remove(dependant);
            var query = _mPrecedents.Where(x => x.Value.Contains(dependant)).Select(x => x.Value);
            foreach (var list in query) list.Remove(dependant);
        }

        /// <summary>
        /// Add a relation between precedent and dependant, saving the relation is the precedent list for the dependant
        /// </summary>
        /// <param name="precedent"></param>
        /// <param name="dependant"></param>
        public void Add(Task precedent, Task dependant)
        {
            if (precedent == dependant
                || precedent == null
                || dependant == null
                || precedent.IsGroup
                || dependant.IsGroup
                || (this._mPrecedents[dependant] != null && this._mPrecedents[dependant].Contains(precedent)) // precedent not current already precendent of dependant
                || (this.PrecedentTree(precedent) != null && this.PrecedentTree(precedent).Contains(dependant)) // precedent cannot have dependant as an existing precedent
                )
            {
                // not allowed
            }
            else
            {
                // Bold assumption: the list must have been created the moment task is created.
                _mPrecedents[dependant].Add(precedent);
            }
        }

        /// <summary>
        /// Remove the relation between precedent and dependant, from the dependant's precendent list
        /// </summary>
        /// <param name="precedent"></param>
        /// <param name="dependant"></param>
        public void Remove(Task precedent, Task dependant)
        {
            _mPrecedents[dependant].Remove(precedent);
        }

        /// <summary>
        /// Remove all relations associated with the specified task
        /// </summary>
        /// <param name="dependant"></param>
        public void Remove(Task task)
        {
            _mPrecedents[task].Clear();
            var query = _mPrecedents.Where(x => x.Value.Contains(task)).Select(x => x.Value);
            foreach (var list in query) list.Remove(task);
        }

        /// <summary>
        /// Get whether the specified task has any relations defined
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool HasRelations(Task task)
        {
            return (_mPrecedents.ContainsKey(task) && _mPrecedents[task].Count > 0) || _mPrecedents.Any(x => x.Value.Contains(task));
        }

        /// <summary>
        /// Enumerate through each direct precedent of the specified dependant
        /// </summary>
        /// <param name="dependant"></param>
        /// <returns></returns>
        public IEnumerable<Task> Precedents(Task dependant)
        {
            List<Task> precedents;
            return _mPrecedents.TryGetValue(dependant, out precedents) ? precedents.ToArray() : null;
        }

        /// <summary>
        /// Enumerate through every precendent of the specified dependant
        /// </summary>
        /// <param name="dependant"></param>
        /// <returns></returns>
        public IEnumerable<Task> PrecedentTree(Task dependant)
        {
            List<Task> precedents;
            if (_mPrecedents.TryGetValue(dependant, out precedents))
            {
                Stack<Task> destinations = new Stack<Task>();
                Task current;
                foreach (var p in precedents)
                {
                    destinations.Push(p);
                    while (destinations.Count > 0)
                    {
                        current = destinations.Pop();
                        foreach (var pp in this.Precedents(current)) destinations.Push(pp);
                        yield return current;
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        /// <summary>
        /// Enumerate through the dependant of the specified task
        /// </summary>
        /// <param name="precedent"></param>
        /// <returns></returns>
        public IEnumerable<Task> Dependants(Task precedent)
        {
            return _mPrecedents.Where(x => x.Value.Contains(precedent)).Select(x => x.Key);
        }

        /// <summary>
        /// Enumerate through each task that is a dependant of another task
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Task> Dependants()
        {
            return _mPrecedents.Keys;
        }
    }

    public class ResourceManager
    {
        Dictionary<object, List<Task>> _mResourceToTasks = new Dictionary<object, List<Task>>();
        Dictionary<Task, List<object>> _mTasksToResource = new Dictionary<Task, List<object>>();

        /// <summary>
        /// Create a resource list for the specified task
        /// </summary>
        /// <param name="task"></param>
        internal void Create(Task task)
        {
            _GetOrCreateTaskResources(task);
        }

        /// <summary>
        /// Delete the resource list from the ResourceManager; Will return null when on next get attempt
        /// </summary>
        /// <param name="task"></param>
        internal void Delete(Task task)
        {
            this.UnassignResource(task);
            _mTasksToResource.Remove(task);
        }

        /// <summary>
        /// Assign a resource to a task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void AssignResource(Task task, object resource)
        {
            var tasks = _GetOrCreateResourceTasks(resource);
            var resources = _GetOrCreateTaskResources(task);

            if (!resources.Contains(resource))
            {
                tasks.Add(task);
                resources.Add(resource);
            }
        }

        /// <summary>
        /// Unassign the specified resource from the specified task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resource"></param>
        public void UnassignResource(Task task, object resource)
        {
            _GetOrCreateTaskResources(task).Remove(resource);
            _GetOrCreateResourceTasks(resource).Remove(task);
        }

        /// <summary>
        /// Unassign all resources from the specified task
        /// </summary>
        /// <param name="task"></param>
        public void UnassignResource(Task task)
        {
            var resources = _GetOrCreateTaskResources(task);
            foreach (var r in resources)
                _mResourceToTasks[r].Remove(task);
            resources.Clear();
        }

        /// <summary>
        /// Delete the task lists for the specified resource
        /// </summary>
        /// <param name="resource"></param>
        public void Delete(object resource)
        {
            var tasks = _GetOrCreateResourceTasks(resource);
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
            return _GetOrCreateResourceTasks(resource).ToArray();
        }

        /// <summary>
        /// Enumerate through the resources that the task uses
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public IEnumerable<object> GetResources(Task task)
        {
            List<object> resources;
            return _mTasksToResource.TryGetValue(task, out resources) ? resources.ToArray() : null;
        }

        /// <summary>
        /// Enumerate through all resources
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetResources()
        {
            return _mResourceToTasks.Keys;
        }

        /// <summary>
        /// Clear all task and resource data in the ResourceManager
        /// </summary>
        public void Clear()
        {
            _mResourceToTasks.Clear();
            _mTasksToResource.Clear();
        }

        private List<Task> _GetOrCreateResourceTasks(object resource)
        {
            List<Task> tasks;
            if (!_mResourceToTasks.TryGetValue(resource, out tasks))
                _mResourceToTasks[resource] = tasks = new List<Task>();

            return tasks;
        }

        private List<object> _GetOrCreateTaskResources(Task task)
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
            Relationships.Create(task);
            Resources.Create(task);
            return task;
        }

        /// <summary>
        /// Add the member Task to the group Task
        /// </summary>
        /// <param name="group"></param>
        /// <param name="member"></param>
        public void GroupTask(Task group, Task member)
        {
            if (group != null && !Relationships.HasRelations(group))
                group.Add(member);
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
        public void Delete(Task task)
        {
            if (task != null)
            {
                // If task is a group we need to pass the orphans to the grand parent
                if (task.IsGroup)
                {
                    var parent = task.Parent;
                    var index = parent.IndexOf(task);
                    foreach (var child in task.Children)
                        parent.Insert(index++, child);
                }

                task.Leave();
                task.Clear();
                Relationships.Delete(task);
                Resources.Delete(task);
            }
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
            if (task != null && offset != 0)
            {
                int indexoftask = IndexOf(task);
                int newindexoftask = indexoftask + offset;
                if (newindexoftask < 0) newindexoftask = 0;
                else if (newindexoftask > Tasks.Count()) newindexoftask = Tasks.Count();
                var taskatnewindex = Tasks.ElementAtOrDefault(newindexoftask);

                if (taskatnewindex == null)
                {
                    // adding to the end of the task list
                    Tasks.Add(task);
                }
                else if (!taskatnewindex.Equals(task))
                {
                    var indexofdestinationtask = taskatnewindex.Parent.IndexOf(taskatnewindex);
                    // if moving down, we need to move further down before swap places
                    taskatnewindex.Parent.Insert(indexofdestinationtask, task);
                }
            }
        }

        /// <summary>
        /// Get the PrecedentManager
        /// </summary>
        public RelationshipManager Relationships { get; private set; }

        /// <summary>
        /// Get the ResourceManager
        /// </summary>
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
