# GanttChart C# WinForms Control - AI Coding Instructions

## Project Architecture Overview

This is a .NET 4.8 WinForms library (`Braincase.GanttChart` namespace) providing a custom UserControl for rendering Gantt charts using native GDI+. The architecture follows a classic separation of concerns:

- **Chart.cs** (~2000 lines): Main UserControl handling all rendering, mouse interactions, and UI events
- **ProjectManager<T,R>**: Generic project management facade implementing `IProjectManager<T,R>`
- **Task**: Passive data class for schedule information (Start, End, Duration, Complete, etc.)
- **Viewport.cs**: Handles chart scrolling, zooming, and coordinate transformations

Key insight: The library uses a **manager pattern** where `ProjectManager` orchestrates all task relationships (grouping, dependencies, resources, splitting) while `Chart` handles pure visualization.

## Essential Development Patterns

### Task Management API
The `ProjectManager` provides fluent methods for task manipulation:
```csharp
_mManager.Add(task);                          // Add tasks
_mManager.Group(parentTask, childTask);       // Create task hierarchies
_mManager.Relate(precedent, dependent);       // Set dependencies
_mManager.Assign(task, resource);             // Assign resources
_mManager.Split(task, part1, part2, delay);   // Split tasks
_mManager.SetDuration(task, TimeSpan.FromDays(5));
```

### Chart Initialization Pattern
Always follow this sequence:
```csharp
var manager = new ProjectManager();
// Add tasks to manager
var chart = new Chart();
chart.Init(manager);  // CRITICAL: Must call Init() after creating manager
```

### Custom Task Data Extension
Extend `Task` class for custom business data, then use `Chart.PaintTask` event:
```csharp
public class ColoredTask : Task { public Color Color { get; set; } }

_mChart.PaintTask += (s, e) => {
    if (e.Task is ColoredTask ctask) {
        var format = e.Format;
        format.BackFill = new SolidBrush(ctask.Color);
        e.Format = format;
    }
};
```

## Build & Test Workflow

### Solution Structure
- **GanttChart.csproj**: Core library (builds to `bin/Debug/GanttChart.dll`)
- **GanttChartExample.csproj**: Demo application with ExampleSimple.cs and ExampleFull.cs
- **GanttChartNUnitTests.csproj**: NUnit 3.9.0 test suite

### Build Commands
```bash
# Build solution (requires Visual Studio 2017+)
msbuild GanttChart.sln /p:Configuration=Debug
msbuild GanttChart.sln /p:Configuration=Release

# Run tests (from VS Test Explorer or)
nunit3-console.exe GanttChartNUnitTests\bin\Debug\GanttChartNUnitTests.dll
```

### Documentation Generation
Uses Doxygen for API docs:
```bash
cd docs
doxygen_gen_docs.bat  # Generates docs/html/ from XML comments
```
- The `docs` folder is the root for github pages website, entry point is `docs/index.html`.
- The resource folder contains images and other assets used in documentation.
- The js and css folders contains javascript and stylesheets respectively for the documentation website.

## Critical Implementation Details

### Generic Type System
Core classes use generic constraints:
- `ProjectManager<T,R> where T : Task where R : class`
- Default concrete type: `ProjectManager : ProjectManager<Task, object>`
- Resources can be any reference type (common: custom resource classes)

### Time Handling
- All times are `TimeSpan` relative to `ProjectManager.Start` (DateTime)
- Default time unit: Days (supports Hours, Weeks via customization)
- Task properties: Start, End, Duration, Slack (all calculated by manager)

### Performance Considerations
- Designed for 1000+ tasks (see ExampleFull.cs stress test)
- Uses HashSet/Dictionary for O(1) lookups in relationship mappings
- Custom GDI+ extensions in Chart.cs for optimized rendering

### Event-Driven Customization
Key events for extending functionality:
- `Chart.PaintTask`: Custom task rendering
- `Chart.TaskSelected`: Handle task selection
- `Chart.MouseDown/MouseMove`: Custom mouse behaviors

## Common Gotchas

1. **Must call `Chart.Init(manager)` after creating ProjectManager** - Chart won't render without this
2. **Task properties are internal set** - Use ProjectManager methods, not direct property assignment
3. **Resource assignment requires reference types** - Primitives won't work as resources
4. **Split tasks create new Task instances** - Original task becomes container, parts are separate objects

## Testing Patterns
- Tests focus on Chart integration (`ChartTest.cs`) and core functionality
- Use Form containers for UserControl testing: `form.Controls.Add(chart)`
- Deferred initialization pattern supported: create Chart, add to Form, then Init with ProjectManager
