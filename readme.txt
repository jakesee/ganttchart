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