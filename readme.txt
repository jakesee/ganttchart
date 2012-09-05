.NET C# Winforms Gantt Chart Control
Copyright 2012 Jake See 

Blog: http://www.jakesee.com/net-c-winforms-gantt-chart-control/
Doxygen Documentation: http://jakesee.com/docs/ganttchart/
Source/Download: http://ganttchart.codeplex.com
Email: jakesee@gmail.com


=============================
CHANGE LOG
=============================
5 Sep 2012 v1.3.0.0(tagged)
- Added feature to split tasks and join tasks
- Added mouse commands for split and join tasks
- Added view instructions toggle
- Added associated unit tests. (119 total)
- Task bars are now aligned to the middle of the chart rows

3 Sep 2012 v1.2.0.2 (tagged)
- Added save and open binary serializer in example application
- Added comments for all public interfaces

2 Sep 2012
- Fix issue where tool tip is not automatically showing. Have to call Chart.Invalidate() on TaskMouseOver and TaskMoveOut to work.

1 Sep 2012 v1.2.0.0
- Added example to show how to use PrintDocument.BeginPrint and EndPrint to handle custom drawing.
- Fixed issue in the timeline date now example where the date for ProjectManager.Now is not shown correctly on the chart.
- Added Chart.Print with scaling. Respects margin and paper orientation.
- Chart coordinates are now expressed in floats (this causing breaking type conversion changes)
- Refactored example application to show new features

31 Aug 2012 v1.1.0.1 (tagged)
- Rewrite entire drawing routine to overcome Winforms control size limits
- Improved drawing speed and culling
- Header now stays on top of view area
- Added Chart.ScrollTo date/time and task
- Added billboard overlay drawing and examples
- Added feature to set tooltip on tasks
- Added DataGridView binding example

30 Aug 2012 v1.1.0.0
- Completely improved the underlying engine to cache task schedules
- Runs many times faster then previous version
- New interfaces for more more flexibility and customisation
- Various default UI behavior changes to cater for more intuitive usage.
- Includes 68 unit tests.

29 Aug 2012 v1.0.0.2
- Fix an exception while grouping tasks with relations.
- Grouping is now disallowed when involved task have relations. User must explicitly remove relations before grouping.
- End of branch

27 Aug 2012
- Added Unit Test
- Fixed more bugs as a result

24 Aug 2012
- Fix bug where cannot add task relationship when precedent task is already an indirect precedent of the dependant task
- Fix a crash when moving a group-task under itself
- Improve rendering by culling relationship and smaller iteration loops
- Other minior refactoring

=============================
ISSUE TRACKER
=============================
- Middle align tasks (COMPLETE)
- Split Tasks (BETA) 5 Sep 2012
- Option to disable/enable mouse commands (lock chart)
- Option to draw horizontal row lines
- Add support down to hour
- Task schedule conflict events
	- dependant start earlier than precedent end
	- precedent end later than dependant start
- Draggable header to change timescale
- ProjectManager option to scale task schedules according to timescale changes
- Collapsable rows
- Resizable row heights
- Print Page headers, page number etc.
- PERT Chart