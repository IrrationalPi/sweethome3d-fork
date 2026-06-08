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
