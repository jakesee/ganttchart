using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Braincase.GanttChart
{
    public class Task
    {
        public Task()
        {
            Complete = 0.0f;
            Start = 0;
            End = 1;
            Duration = 1;
            Slack = 0;
        }

        /// <summary>
        /// Get or set the Name of this Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicate whether this task is collapsed such that sub tasks are hidden from view. Only groups can be collasped.
        /// </summary>
        public bool IsCollapsed { get; internal set; }

        /// <summary>
        /// Get or set the pecentage complete of this task, expressed in float between 0.0 and 1.0f.
        /// </summary>
        public float Complete { get; internal set; }
        
        /// <summary>
        /// Get the start time of this Task relative to the project start
        /// </summary>
        public int Start { get; internal set; }

        /// <summary>
        /// Get the end time of this Task relative to the project start
        /// </summary>
        public int End { get; internal set; }

        /// <summary>
        /// Get the duration of this Task
        /// </summary>
        public int Duration { get; internal set; }

        /// <summary>
        /// Get the amount of slack (free float)
        /// </summary>
        public int Slack { get; internal set; }

        /// <summary>
        /// Convert this Task to a descriptive string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Name = {0}, Start = {1}, End = {2}, Duration = {3}, Complete = {4}]", Name, Start, End, Duration, Complete);
        }
    }

    public enum TimeScale
    {
        Day, Week
    }

    public interface IProjectManager<T, R>
        where T : Task
        where R : class
    {
        // Relating to all the Tasks
        void Add(T task);
        void Delete(T task);
        void Group(T group, T member);
        void Ungroup(T group, T member);
        void Ungroup(T group);
        void Move(T task, int offset);
        void Relate(T precedent, T dependant);
        void Unrelate(T precedent, T dependant);
        void Unrelate(T precedent);
        IEnumerable<T> Precedents { get; }
        IEnumerable<T> Tasks { get; }

        // set schedules and timelines
        void SetStart(T task, int start);
        void SetEnd(T task, int end);
        void SetDuration(T task, int duration);
        void SetComplete(T task, float complete);
        void SetCollapse(T task, bool collasped);
        int Now { get; }

        // Relating to single Tasks
        int IndexOf(T task);
        IEnumerable<T> AncestorsOf(T task);
        IEnumerable<T> DecendantsOf(T task);
        IEnumerable<T> ChildrenOf(T task);
        IEnumerable<T> PrecedentsOf(T task);
        IEnumerable<T> DependantsOf(T task);
        IEnumerable<T> DirectPrecedentsOf(T task);
        IEnumerable<T> DirectDependantsOf(T task);
        IEnumerable<IEnumerable<T>> CriticalPaths { get; }
        T ParentOf(T task);

        // Queries
        bool IsGroup(T task);
        bool IsMember(T task);
        bool HasRelations(T task);

        // Resources
        void Assign(T task, R resource);
        void Unassign(T task, R resource);
        void Unassign(T task);
        void Unassign(R resource);
        IEnumerable<R> Resources { get; }
        IEnumerable<R> ResourcesOf(T task);
        IEnumerable<T> TasksOf(R resource);
    }

    public class ProjectManager<T, R> : IProjectManager<T, R>
        where T : Task
        where R : class
    {
        HashSet<T> _mRegister = new HashSet<T>();
        List<T> _mRootTasks = new List<T>();
        Dictionary<T, List<T>> _mTaskGroups = new Dictionary<T, List<T>>();
        Dictionary<T, HashSet<T>> _mDependents = new Dictionary<T, HashSet<T>>();
        Dictionary<T, HashSet<R>> _mResources = new Dictionary<T, HashSet<R>>();
        Dictionary<T, T> _mParentOfChild = new Dictionary<T, T>();

        /// <summary>
        /// Create a new Project
        /// </summary>
        public ProjectManager()
        {
            Now = 0;
            Start = DateTime.Now;
            TimeScale = GanttChart.TimeScale.Day;
        }
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
            else if (this.TimeScale == TimeScale.Week)
            {
                datetime = this.Start.AddDays(period * 7 - (int)this.Start.DayOfWeek);
            }
            return datetime;
        }

        /// <summary>
        /// Create a new T for this Project and add it to the T tree
        /// </summary>
        /// <returns></returns>
        public void Add(T task)
        {
            if (!this._mRegister.Contains(task))
            {
                _mRegister.Add(task);
                _mRootTasks.Add(task);
                _mTaskGroups[task] = new List<T>();
                _mDependents[task] = new HashSet<T>();
                _mResources[task] = new HashSet<R>();
                _mParentOfChild[task] = null;
            }
        }

        /// <summary>
        /// Remove task from this Project
        /// </summary>
        /// <param name="task"></param>
        public void Delete(T task)
        {
            if (task != null)
            {
                // Check if is group so can ungroup the task
                if (this.IsGroup(task))
                    this.Ungroup(task);

                // Really delete all references
                _mRootTasks.Remove(task);
                _mTaskGroups.Remove(task);
                _mDependents.Remove(task);
                _mResources.Remove(task);
                _mParentOfChild.Remove(task);
                foreach (var g in _mTaskGroups) g.Value.Remove(task); // optimised: no need to check for contains
                foreach (var g in _mDependents) g.Value.Remove(task);
                _mRegister.Remove(task);
            }
        }

        /// <summary>
        /// Add the member T to the group T
        /// </summary>
        /// <param name="group"></param>
        /// <param name="member"></param>
        public void Group(T group, T member)
        {
            if (group != null
                && member != null
                && !group.Equals(member)
                && _mRegister.Contains(group)
                && _mRegister.Contains(member)
                && !this.DecendantsOf(member).Contains(group)
                && !this.HasRelations(group))
            {
                _LeaveParent(member);
                _mTaskGroups[group].Add(member);
                _mParentOfChild[member] = group;

                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Remove the member task from its group
        /// </summary>
        /// <param name="member"></param>
        public void Ungroup(T group, T member)
        {
            if (group != null
                && member != null
                && _mRegister.Contains(group)
                && _mRegister.Contains(group)
                && this.IsGroup(group))
            {
                _mRootTasks.Add(member);
                _mTaskGroups[group].Remove(member);
                _mParentOfChild[member] = null;
            }
        }

        public void Ungroup(T group)
        {
            List<T> list;
            if (group != null
                //&& _mRegister.Contains(group)
                && _mTaskGroups.TryGetValue(group, out list))
            {
                var newgroup = this.ParentOf(group);
                if (newgroup == null)
                {
                    foreach (var member in list)
                    {
                        _mRootTasks.Add(member);
                        _mParentOfChild[member] = null;
                    }
                }
                else
                {
                    foreach (var member in list)
                    {
                        _mTaskGroups[newgroup].Add(member);
                        _mParentOfChild[member] = null;
                    }
                }

                list.Clear();
            }
        }
        
        /// <summary>
        /// Get the zero-based index of the task in this Project
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int IndexOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                int i = 0;
                foreach (var x in Tasks)
                {
                    if (x.Equals(task)) return i;
                    i++;
                }
            }
            return -1;
        }

        /// <summary>
        /// Re-position the task by offset amount of places
        /// </summary>
        /// <param name="task"></param>
        /// <param name="offset"></param>
        public void Move(T task, int offset)
        {
            if (task != null && _mRegister.Contains(task) && offset != 0)
            {
                int indexoftask = IndexOf(task);
                if (indexoftask > -1)
                {
                    int newindexoftask = indexoftask + offset;
                    // check for out of index bounds
                    if (newindexoftask < 0) newindexoftask = 0;
                    else if (newindexoftask > Tasks.Count()) newindexoftask = Tasks.Count();
                    // get the index of the task that will be displaced
                    var displacedtask = Tasks.ElementAtOrDefault(newindexoftask);

                    if (displacedtask == null)
                    {
                        // adding to the end of the task list
                        _LeaveParent(task);
                        _mRootTasks.Add(task);
                    }
                    else if (!displacedtask.Equals(task))
                    {
                        int indexofdestinationtask;
                        var displacedtaskparent = this.ParentOf(displacedtask);
                        if (displacedtaskparent == null) // displacedtask is in root
                        {
                            indexofdestinationtask = _mRootTasks.IndexOf(displacedtask);
                            _LeaveParent(task);
                            _mRootTasks.Insert(indexofdestinationtask, task);
                        }
                        else if (!displacedtaskparent.Equals(task)) // displaced task is not under the moving task
                        {
                            var memberlist = _mTaskGroups[displacedtaskparent];
                            indexofdestinationtask = memberlist.IndexOf(displacedtask);
                            _LeaveParent(task);
                            memberlist.Insert(indexofdestinationtask, task);
                            _mParentOfChild[task] = displacedtaskparent;
                        }
                    }
                    else // displacedtask == task, no need to move    
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Get the T tree
        /// </summary>
        public IEnumerable<T> Tasks
        {
            get
            {
                var stack = new Stack<T>(1024);
                var rstack = new Stack<T>(30);
                foreach (var task in _mRootTasks)
                {
                    stack.Push(task);
                    while (stack.Count > 0)
                    {
                        var visited = stack.Pop();
                        yield return visited;
                        foreach (var member in _mTaskGroups[visited])
                            rstack.Push(member);

                        while (rstack.Count > 0) stack.Push(rstack.Pop());
                    }
                }
            }
        }

        public IEnumerable<T> AncestorsOf(T task)
        {
            T parent = ParentOf(task);
            while (parent != null)
            {
                yield return parent;
                parent = ParentOf(parent);
            }
        }

        public IEnumerable<T> DecendantsOf(T task)
        {
            if(_mRegister.Contains(task))
            {
                Stack<T> stack = new Stack<T>(20);
                Stack<T> rstack = new Stack<T>(10);
                foreach (var child in _mTaskGroups[task])
                {
                    stack.Push(child);
                    while (stack.Count > 0)
                    {
                        var visitedchild = stack.Pop();
                        yield return visitedchild;

                        // push the grandchild
                        rstack.Clear();
                        foreach (var grandchild in _mTaskGroups[visitedchild])
                            rstack.Push(grandchild);

                        // put in the right visiting order
                        while (rstack.Count > 0)
                            stack.Push(rstack.Pop());
                    }
                }
            }
        }

        public IEnumerable<T> ChildrenOf(T task)
        {
            if (task == null) yield break;

            List<T> list;
            if (_mTaskGroups.TryGetValue(task, out list))
            {
                foreach (var item in list)
                    yield return item;
            }
        }

        public IEnumerable<T> PrecedentsOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                var stack = new Stack<T>(20);
                foreach (var p in DirectPrecedentsOf(task))
                {
                    stack.Push(p);
                    while (stack.Count > 0)
                    {
                        var visited = stack.Pop();
                        yield return visited;
                        foreach (var grandp in DirectPrecedentsOf(visited))
                            stack.Push(grandp);
                    }
                }
            }
        }

        public IEnumerable<T> DependantsOf(T task)
        {
            var stack = new Stack<T>(20);
            foreach (var d in _mDependents[task])
            {
                stack.Push(d);
                while (stack.Count > 0)
                {
                    var visited = stack.Pop();
                    yield return visited;
                    foreach (var grandd in _mDependents[visited])
                        stack.Push(grandd);
                }
            }
        }

        public IEnumerable<T> DirectPrecedentsOf(T task)
        {
            return _mDependents.Where(x => x.Value.Contains(task)).Select(x => x.Key);
        }

        public IEnumerable<T> DirectDependantsOf(T task)
        {
            if (task == null) yield break;

            HashSet<T> list;
            if (_mDependents.TryGetValue(task, out list))
            {
                foreach (var item in list)
                    yield return item;
            }
        }

        public IEnumerable<T> Precedents
        {
            get { foreach(var item in _mDependents.Where(x => _mDependents[x.Key].Count > 0).Select(x => x.Key)) yield return item; }
        }

        /// <summary>
        /// Enumerate list of critical paths in Project
        /// </summary>
        public IEnumerable<IEnumerable<T>> CriticalPaths
        {
            get
            {
                Dictionary<int, List<T>> endtimelookp = new Dictionary<int, List<T>>(1024);
                List<T> list;
                var max_end = int.MinValue;
                foreach (var task in this.Tasks)
                {
                    if (!endtimelookp.TryGetValue(task.End, out list))
                        endtimelookp[task.End] = new List<T>(10);
                    endtimelookp[task.End].Add(task);

                    if(task.End > max_end) max_end = task.End;
                }

                if (max_end != int.MinValue)
                {
                    foreach (var task in endtimelookp[max_end])
                    {
                        yield return new T[] { task }.Concat(PrecedentsOf(task));
                    }
                }

                /*
                var max = this.Tasks.Max(x => x.End);
                var paths = this.Tasks.Where(x => x.End == max);
                foreach (var task in paths)
                {
                    yield return new T[] { task }.Concat(PrecedentsOf(task));
                }
                 */
            }
        }

        public T ParentOf(T task)
        {
            if (_mRegister.Contains(task))
            {
                /*
                var kvp = _mTaskGroups.FirstOrDefault(x => x.Value.Contains(task));
                return kvp.Equals(default(KeyValuePair<T, List<T>>)) ? null : kvp.Key;
                 */
                return _mParentOfChild[task];
            }
            else
            {
                return null;
            }
        }

        public bool IsGroup(T task)
        {
            List<T> list;
            if (_mTaskGroups.TryGetValue(task, out list))
                return list.Count > 0;
            else
                return false;
        }

        public bool IsMember(T task)
        {
            return this.ParentOf(task) != null;
        }

        public bool HasRelations(T task)
        {
            if (_mRegister.Contains(task))
            {
                return _mDependents[task].Count > 0 || DirectPrecedentsOf(task).FirstOrDefault() != null;
            }
            else
            {
                return false;
            }
        }

        public void Relate(T precedent, T dependant)
        {
            if (_mRegister.Contains(precedent)
                && _mRegister.Contains(dependant)
                && !this.DependantsOf(dependant).Contains(precedent)
                && !this.IsGroup(precedent)
                && !this.IsGroup(dependant))
            {
                _mDependents[precedent].Add(dependant);

                _RecalculateDependantsOf(precedent);
                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        public void Unrelate(T precedent, T dependant)
        {
            if (_mRegister.Contains(precedent) && _mRegister.Contains(dependant))
            {
                _mDependents[precedent].Remove(dependant);

                _RecalculateSlack();
            }
        }

        public void Unrelate(T precedent)
        {
            if (_mRegister.Contains(precedent))
            {
                _mDependents[precedent].Clear();

                _RecalculateSlack();
            }
        }

        public void Assign(T task, R resource)
        {
            if (_mRegister.Contains(task) && !_mResources[task].Contains(resource))
                _mResources[task].Add(resource);
        }

        public void Unassign(T task, R resource)
        {
            _mResources[task].Remove(resource);
        }

        public void Unassign(T task)
        {
            if(_mRegister.Contains(task))
                _mResources[task].Clear();
        }

        public void Unassign(R resource)
        {
            foreach (var r in _mResources.Where(x => x.Value.Contains(resource)))
                r.Value.Remove(resource);
        }

        public IEnumerable<R> Resources
        {
            get
            {
                return _mResources.SelectMany(x => x.Value).Distinct();
            }
        }

        public IEnumerable<R> ResourcesOf(T task)
        {
            if (task == null || !_mRegister.Contains(task))
                yield break;

            HashSet<R> list;
            if (_mResources.TryGetValue(task, out list))
            {
                foreach (var item in list)
                    yield return item;
            }
        }

        public IEnumerable<T> TasksOf(R resource)
        {
            return _mResources.Where(x => x.Value.Contains(resource)).Select(x => x.Key);
        }

        /// <summary>
        /// Set the start value. Affects group start/end and dependants start time.
        /// </summary>
        /// <param name="value"></param>
        public void SetStart(T task, int value)
        {
            if (_mRegister.Contains(task) && value != task.Start && !this.IsGroup(task))
            {
                _SetStartHelper(task, value);

                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        /// <summary>
        /// Set the end time. Affects group end and dependants start time.
        /// </summary>
        /// <param name="value"></param>
        public void SetEnd(T task, int value)
        {
            if (_mRegister.Contains(task) && value != task.End && !this.IsGroup(task))
            {
                this._SetEndHelper(task, value);

                _RecalculateAncestorsSchedule();
                _RecalculateSlack();
            }
        }

        public void SetDuration(T task, int duration)
        {
            this.SetEnd(task, task.Start + duration);
        }

        public void SetComplete(T task, float value)
        {
            if (_mRegister.Contains(task) && value != task.Complete && !this.IsGroup(task))
            {
                _SetCompleteHelper(task, value);

                _RecalculateComplete();
            }
        }

        public void SetCollapse(T task, bool collasped)
        {
            if (_mRegister.Contains(task) && this.IsGroup(task))
            {
                task.IsCollapsed = collasped;
            }
        }

        private void _LeaveParent(T task)
        {
            var parent = this.ParentOf(task);
            if (parent == null)
                _mRootTasks.Remove(task);
            else
            {
                _mTaskGroups[parent].Remove(task);
                _mParentOfChild[task] = null;
            }
        }

        private void _SetStartHelper(T task, int value)
        {
            if (task.Start != value)
            {
                // check out of bounds
                if (value < 0) value = 0;
                if (this.DirectPrecedentsOf(task).Any())
                {
                    var max_end = this.DirectPrecedentsOf(task).Max(x => x.End);
                    if (value <= max_end) value = max_end + 1;
                }

                // cache value
                task.Duration = task.End - task.Start;
                task.Start = value;


                // affect self
                task.End = task.Start + task.Duration;

                _RecalculateDependantsOf(task);
            }
        }

        private void _SetEndHelper(T task, int value)
        {
            if (task.End != value)
            {
                // cache value
                task.End = value;
                
                // check bounds
                if (task.End <= task.Start) task.End = task.Start + 1;
                task.Duration = task.End - task.Start;

                _RecalculateDependantsOf(task);
            }
        }

        private void _SetCompleteHelper(T task, float value)
        {
            if (task.Complete != value)
            {
                if (value > 1) value = 1;
                else if (value < 0) value = 0;
                task.Complete = value;
            }
        }

        private void _RecalculateComplete()
        {
            Stack<T> groups = new Stack<T>();
            foreach (var task in _mRootTasks.Where(x => this.IsGroup(x)))
            {
                _RecalculateCompletedHelper(task);
            }
        }

        private float _RecalculateCompletedHelper(T group)
        {
            float t_complete = 0;
            int t_duration = 0;
            foreach (var member in this.ChildrenOf(group))
            {
                t_duration += member.Duration;
                if (this.IsGroup(member)) t_complete += _RecalculateCompletedHelper(member) * member.Duration;
                else t_complete += member.Complete * member.Duration;
            }

            group.Complete = t_complete / t_duration;

            return group.Complete;
        }

        private void _RecalculateDependantsOf(T precedent)
        {
            // affect decendants
            foreach (var dependant in this.DirectDependantsOf(precedent))
            {
                if (dependant.Start <= precedent.End)
                    this._SetStartHelper(dependant, precedent.End + 1);
            }
        }

        private void _RecalculateAncestorsSchedule()
        {
            // affects parent group
            foreach(var group in _mRootTasks.Where(x => this.IsGroup(x)))
            {
                _RecalculateAncestorsScheduleHelper(group);
            }
        }

        private void _RecalculateAncestorsScheduleHelper(T group)
        {
            float t_complete = 0;
            int t_duration = 0;
            var start = int.MaxValue;
            var end = int.MinValue;
            foreach (var member in this.ChildrenOf(group))
            {
                if (this.IsGroup(member))
                    _RecalculateAncestorsScheduleHelper(member);

                t_duration += member.Duration;
                t_complete += member.Complete * member.Duration;
                if (member.Start < start) start = member.Start;
                if (member.End > end) end = member.End;
            }

            this._SetStartHelper(group, start);
            this._SetEndHelper(group, end);
            this._SetCompleteHelper(group, t_complete / t_duration);
        }

        private void _RecalculateSlack()
        {
            var max_end = this.Tasks.Max(x => x.End);
            foreach (var task in this.Tasks)
            {
                // affects slack for current task
                if (this.DirectDependantsOf(task).Any())
                {
                    // slack until the earliest dependant needs to start
                    var min = this.DirectDependantsOf(task).Min(x => x.Start);
                    task.Slack = min - task.End - 1;
                }
                else
                {
                    // no dependants, so we have all the time until the last task ends
                    task.Slack = max_end - task.End;
                }
            }
        }
    }
}
