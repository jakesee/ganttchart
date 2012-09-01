.NET C# Winforms Gantt Chart Control
Blog: http://www.jakesee.com/net-c-winforms-gantt-chart-control/
Doxygen Documentation: http://jakesee.com/docs/ganttchart/
Source/Download: http://ganttchart.codeplex.com

1 Sep 2012 v1.2.0.0
- Added example to show how to use PrintDocument.BeginPrint and EndPrint to handle custom drawing.
- Fixed issue in the timeline date now example where the date for ProjectManager.Now is not shown correctly on the chart.
- Added Chart.Print with scaling. Respects margin and paper orientation.
- Chart coordinates are now expressed in floats (this causing breaking type conversion changes)
- Refactored example application to show new features

31 Aug 2012 v1.1.0.1
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