# Home Planner Fork (Sweet Home 3D 7.5)

GPL fork of [Sweet Home 3D](https://www.sweethome3d.com/) v7.5 with custom navigation and selection improvements.

**Upstream:** Sweet Home 3D is Copyright Space Mushrooms / Emmanuel Puybaret, licensed under GNU GPL v2+. This project modifies that source and must remain under the GPL if you distribute binaries.

## Custom features

| Preference | What it does |
|------------|----------------|
| Invert horizontal 3D navigation | Flips left/right look and strafe (mouse drag, keys, nav buttons) |
| Invert vertical 3D navigation | Flips up/down look, forward/back, wheel, and elevation |
| Enhanced touch input | Larger hit targets and touch-style selection timing on laptop touchscreens |
| Cycle overlapping selection | Repeated clicks at the same plan point cycle through stacked items |

Open **Preferences** in the app to enable these (English labels in `package.properties`).

## Quick start

```powershell
.\setup.ps1    # once: download Ant + source if missing
.\build.ps1    # after any code change (~20s)
.\run.ps1      # launch your build
```

Prerequisites: **JDK 17+** (`winget install Microsoft.OpenJDK.17`)

## GitHub

This folder is a git repo. To publish (replace `YOUR_USER` and repo name):

```bash
cd /c/Users/richa/Projects/sweethome3d-fork
git add .
git commit -m "Add custom navigation, touch, and overlap-selection preferences"
gh repo create YOUR_USER/sweethome3d-fork --public --source=. --remote=origin --push
```

Or create an empty repo on github.com, then:

```bash
git remote add origin https://github.com/YOUR_USER/sweethome3d-fork.git
git push -u origin main
```

Use a **distinct app name** if you distribute builds publicly (GPL still applies; trademark "Sweet Home 3D" belongs to the original project).

## C# Port (OpenHome3D)

This repository also contains an incremental C# port of the core logic.

### Prerequisites
- **.NET 10 SDK** (or latest .NET SDK)

### Build and Test
```bash
dotnet build
dotnet test
```

### `.sh3d` File Structure
The `.sh3d` file format is a standard ZIP archive containing:
- `Home.xml` (or `Home`): The core XML data describing the home layout, walls, rooms, furniture, and properties.
- Embedded content directories (e.g., `0`, `1`, `content/`): Binary assets like textures, 3D models (`.obj`, `.mtl`), and images referenced by the `Home.xml` data.

The `OpenHome3D.Core.IO.Sh3dIO` class handles extracting and parsing these archives, mapping embedded content to a dictionary for future rendering and export use. The `HomeXmlExporter` and `Sh3dIO.Save` methods enable 100% data-fidelity roundtrip testing (Load -> Save -> Load), verified by parameterized xUnit tests.

### Geometry Utilities
The `OpenHome3D.Core.Geometry` namespace provides the foundational 2D math primitives required for spatial calculations in the 2D plan view:
- `Point2D`: A lightweight, immutable record struct representing a 2D coordinate (`X`, `Y`).
- `Segment2D`: A record struct representing a line segment defined by a `Start` and `End` `Point2D`.
- `GeometryUtils`: A static class containing core mathematical operations, including:
  - `Distance` and `DistanceSquared`: Efficiently calculate the Euclidean distance between two points.
  - `ComputeIntersection`: Determines the intersection point of two lines (handling vertical and parallel lines gracefully by returning `null` when lines are parallel or coincident within a specified `limit`).

### Room Boundary & Area Calculation
The `OpenHome3D.Core.Models.Room` model uses the Shoelace formula to calculate room area and determine vertex ordering:
- `GetSignedArea`: Computes the signed area of a polygon defined by a list of `Point2D` vertices. A positive value indicates counter-clockwise ordering, while a negative value indicates clockwise ordering.
- `GetArea`: Returns the absolute value of the signed area.
- `IsClockwise`: Returns `true` if the room's vertices are ordered clockwise.
- `ContainsPoint`: Uses a ray-casting algorithm to determine if a given point lies inside the room polygon, with an optional margin for boundary tolerance.

### Catalog & Preferences
The `OpenHome3D.Core.IO` namespace provides parsers for user preferences and furniture catalogs:
- `UserPreferencesXmlParser`: Parses `<preferences>` elements with attributes like `unit`, `magnetismEnabled`, `newWallThickness`, and `newWallHeight`.
- `FurnitureCatalogXmlParser`: Parses `<catalog>` elements containing `<piece>` children. Each `<piece>` requires `id`, `name`, `category`, `width`, `depth`, and `height` attributes, with an optional `modelName` attribute for 3D model references.

### MVVM Architecture & State Management
The `OpenHome3D.Core.ViewModels` namespace implements the Model-View-ViewModel (MVVM) pattern using `CommunityToolkit.Mvvm`:
- `MainViewModel`: The root ViewModel managing the application state, including the `CurrentHome`.
- `HomeViewModel`: Wraps the immutable `Home` model, exposing `ObservableCollection<T>` for `Walls`, `Rooms`, and `Furniture` to support UI data binding.
- Relay Commands: `AddWallCommand` and `DeleteSelectedCommand` provide UI-triggered actions that modify the ViewModel collections and update selection state.

### 2D UI Shell & Application Layout
The `OpenHome3D.UI` project provides the cross-platform Avalonia UI shell:
- **Main Window**: Features a top Menu bar, a Toolbar with common actions (New, Add Wall, Delete), and a content area reserved for the 2D Plan View.
- **Data Binding**: The `MainWindow` is bound to `MainViewModel` via `DataContext`, enabling command execution directly from the UI.
- **Headless UI Testing**: The `OpenHome3D.UI.Tests` project uses `Avalonia.Headless.XUnit` to verify UI instantiation and `DataContext` bindings without requiring a display server.

### 2D Plan View Rendering
The `PlanViewControl` is a custom Avalonia `Control` that renders the 2D floor plan using native Avalonia drawing primitives:
- **Grid Background**: Draws a dashed light-gray grid to assist with spatial alignment.
- **Wall Rendering**: Utilizes `WallGeometryUtils.GetCornerPoints` to calculate the 4 corner points of each wall based on its start/end coordinates and thickness, rendering them as filled dark-gray polygons with black borders.
- **Reactive Invalidation**: The control subscribes to `CollectionChanged` events on the `HomeViewModel.Walls` and `HomeViewModel.Rooms` collections, automatically calling `InvalidateVisual()` to trigger a re-render whenever the underlying model changes.

### 2D Viewport Interactions (Pan, Zoom, Selection)
The `PlanViewControl` implements core 2D editing interactions:
- **Pan**: Middle-mouse drag updates the `PanX` and `PanY` properties on the `HomeViewModel`, shifting the view.
- **Zoom**: Mouse wheel events adjust the `Zoom` property (clamped between 0.1x and 5.0x), scaling the rendered output via `DrawingContext.PushTransform`.
- **Selection**: Left-clicks convert screen coordinates to world coordinates and use a ray-casting point-in-polygon algorithm to detect if the click intersects any wall geometry. Selected walls are highlighted with a light-blue fill and blue border, and the `SelectedWall` property on the `HomeViewModel` is updated to reflect the current selection state.

### Core Editing Tools
The `PlanViewControl` supports active editing modes managed by the `MainViewModel.CurrentTool` property:
- **Wall Creation Mode**: When `CurrentTool` is set to `"CreateWall"`, the first left-click sets the wall's start point, dragging shows a dashed preview line, and the second click finalizes the wall, adding it to the `HomeViewModel.Walls` collection and automatically reverting to `"Select"` mode.
- **Drag-and-Drop Furniture**: The control accepts drag-and-drop operations (`AllowDrop="True"`). Dropping text data (e.g., a furniture catalog ID) onto the control creates a new `HomePieceOfFurniture` instance in the `HomeViewModel.Furniture` collection, with its `X` and `Y` coordinates translated from screen space to world space based on the current pan and zoom state.

### Running the UI
To build and run the Avalonia UI application:
```bash
dotnet build
dotnet run --project src/OpenHome3D.UI
```
