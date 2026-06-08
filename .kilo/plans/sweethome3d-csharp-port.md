# Sweet Home 3D C# Port: Long-Term Technical Plan

## 1. Executive Summary

This document outlines a long-term, incremental strategy to port the Sweet Home 3D desktop application from Java to C#. The goal is to achieve a fully cross-platform application (Windows, macOS, Linux) with a modern, native-looking UI/UX, while preserving 100% compatibility with existing `.sh3d` save files and core user workflows.

## 2. Licensing & Legal Compatibility

- **Core License**: Sweet Home 3D is licensed under **GNU GPL v2 or later**. A reimplementation in C# must also be released under GPL v2+ (or a compatible license like GPL v3) to comply with the original author's terms.
- **Third-Party Libraries**: The current project uses libraries with Apache 2.0, BSD, MIT, and LGPL licenses (e.g., iText, Batik, SunFlow, YafaRay). These are generally compatible with GPL. The C# port must use equivalent GPL-compatible or permissively licensed alternatives (e.g., MIT/Apache 2.0) to maintain compliance.
- **Branding**: The project will be **rebranded** (e.g., "OpenHome3D" or a new name) to avoid trademark conflicts with Space Mushrooms' "Sweet Home 3D".

## 3. Target Technology Stack

Given your C# experience, the .NET ecosystem is an excellent fit for this modernization effort.

| Domain | Java (Current) | C# / .NET (Proposed) | Rationale |
| :--- | :--- | :--- | :--- |
| **Language** | Java 8+ | C# 12+ (.NET 8/9) | Modern syntax, excellent cross-platform performance, strong typing. |
| **UI Framework** | Java Swing | **Avalonia UI** | True cross-platform (Win/macOS/Linux), XAML-based, highly customizable, native look-and-feel support. |
| **2D Rendering** | Java 2D / Batik | **SkiaSharp** / **Svg.Skia** | Hardware-accelerated 2D drawing and robust SVG parsing. |
| **3D Rendering** | Java 3D (JOGL) | **Godot** (via GodotSharp / C# bindings) | High-level, modern, cross-platform 3D engine with excellent C# support. Can be embedded in Avalonia or run as a separate window. |
| **Ray Tracing** | SunFlow / YafaRay | **Deferred to post-V1** | Photo-realistic rendering is complex. V1 will focus on core 2D/3D editing parity. Advanced rendering will be evaluated in a later phase. |
| **File I/O / XML** | Custom SAX Parser | **System.Xml.Linq** / **System.Text.Xml** | Robust, built-in, and highly performant for `.sh3d` roundtrip parsing. |
| **PDF Export** | iText 2.1.7 | **QuestPDF** or **iText 7 for .NET** | Modern, actively maintained, and permissive (QuestPDF is MIT, iText 7 is AGPL/commercial). |
| **Build / CI** | Ant | **.NET SDK** / **GitHub Actions** | Standardized, fast, and easily integrates with cross-platform packaging. |
| **Testing** | Abbot / JUnit | **xUnit** / **NUnit** + **Avalonia.Headless** | Modern, fast, and supports headless UI testing. |

## 4. Architecture & Domain Model Mapping

The Java codebase is well-structured into MVC-like packages. This maps cleanly to C#:

- `com.eteks.sweethome3d.model.*` → `OpenHome3D.Core.Models.*` (Pure C# records/classes, `INotifyPropertyChanged` for data binding, e.g., `Home`, `Wall`, `Room`, `Point2D`, `HomePieceOfFurniture`, `HomeProperty`).
- `com.eteks.sweethome3d.io.*` → `OpenHome3D.Core.IO.*` (XML serialization/deserialization, ZIP handling for `.sh3d`).
- `com.eteks.sweethome3d.viewcontroller.*` → `OpenHome3D.Core.ViewModels.*` (MVVM pattern, replacing Swing controllers).
- `com.eteks.sweethome3d.swing.*` → `OpenHome3D.UI.Views.*` (Avalonia UserControls and Windows).
- `com.eteks.sweethome3d.j3d.*` → `OpenHome3D.Rendering.*` (3D scene management, mesh generation, Godot engine integration).

## 5. Incremental Migration Strategy

To avoid a "big bang" rewrite, the port will be executed in distinct, testable phases.

### Phase 1: Foundation & File I/O (Broken into Testable Iterations) ✅ COMPLETED

**Overall Goal**: Prove that C# can read and write existing `.sh3d` files perfectly, establishing the core data model and I/O pipeline. Each iteration is designed to be completed in a single agent session with passing tests.

#### Iteration 1.1: Project Scaffolding & Minimal XML Parsing

- **Goal**: Set up the .NET solution and parse a minimal `Home.xml` structure.
- **Tasks**:
  1. **Solution Setup**:
     - `dotnet new sln -n OpenHome3D`
     - `dotnet new classlib -n OpenHome3D.Core -f net8.0`
     - `dotnet new xunit -n OpenHome3D.Core.Tests -f net8.0`
     - `dotnet sln add OpenHome3D.Core OpenHome3D.Core.Tests`
     - `dotnet add OpenHome3D.Core.Tests reference OpenHome3D.Core`
  2. **Model Definition** (`src/OpenHome3D.Core/Models/Home.cs`):
     - Define `public record Home(string Name, string Version);`
  3. **Parser Implementation** (`src/OpenHome3D.Core/IO/HomeXmlParser.cs`):
     - Define `public static class HomeXmlParser`
     - Implement `public static Home Parse(string xmlContent)` using `XDocument.Parse(xmlContent)`.
     - Extract `version` and `name` attributes from the root `<home>` element.
  4. **Unit Testing** (`tests/OpenHome3D.Core.Tests/HomeXmlParserTests.cs`):
     - Define `public class HomeXmlParserTests`
     - Implement `[Fact] public void Parse_MinimalXml_ReturnsCorrectHome()`
     - Arrange: `string xml = "<home version=\"7.5\" name=\"Test\" />";`
     - Act: `var home = HomeXmlParser.Parse(xml);`
     - Assert: `Assert.Equal("Test", home.Name);` and `Assert.Equal("7.5", home.Version);`
- **Completion Criteria**:
  - `dotnet build` succeeds with 0 errors.
  - `dotnet test` succeeds with the new test passing.
- **Documentation**:
  - Create or update `README.md` with .NET build (`dotnet build`) and test (`dotnet test`) instructions.

#### Iteration 1.2: Core Model Expansion (Walls & Rooms)

- **Goal**: Expand the data model and parse structural elements.
- **Tasks**:
  1. Expand C# models: `Wall` (`XStart`, `YStart`, `XEnd`, `YEnd`, `Thickness`, `Height`), `Room`, `Point2D`.
  2. Update `HomeXmlParser` to parse `<wall ... />` and `<room ... />` elements, populating the `Home.Walls` and `Home.Rooms` collections.
- **Completion Criteria**: Unit tests pass asserting that parsing XML with walls/rooms correctly populates the collections with accurate coordinates and dimensions.
- **Documentation**: Update the domain model mapping section to reflect the new C# types.

#### Iteration 1.3: Furniture & Properties Parsing ✅ COMPLETED

- **Goal**: Parse furniture placements and custom home properties.
- **Tasks**:
  1. Expand C# models: `HomePieceOfFurniture` (`Id`, `X`, `Y`, `Z`, `Angle`, `Width`, `Depth`, `Height`, `ModelName`) and `HomeProperty` (`Name`, `Value`).
  2. Update `HomeXmlParser` to parse `<pieceOfFurniture ... />` and `<property name="..." value="..." />` elements.
- **Completion Criteria**: Unit tests pass asserting that parsing XML with furniture correctly populates the `Home.Furniture` collection with accurate spatial data.
- **Documentation**: Custom properties are now handled via the `HomeProperty` record and stored in the `Home.Properties` collection, preserving arbitrary key-value metadata from the original `.sh3d` files.

#### Iteration 1.4: .sh3d ZIP Extraction & Content Mapping (Read)

- **Goal**: Handle the `.sh3d` file format as a ZIP archive, not just raw XML.
- **Tasks**:
  1. Create an `Sh3dIO` class using `System.IO.Compression.ZipFile`.
  2. Implement `Load(string filePath)`: Opens the ZIP, locates the `Home.xml` entry, extracts it, and passes it to `HomeXmlParser`.
  3. Build a dictionary of embedded content (e.g., `content/1.png`) referenced by the XML for future use.
- **Completion Criteria**: Unit test passes asserting that loading a valid `.sh3d` file successfully returns a populated `Home` object and a non-empty content map (if the file has embedded assets).
- **Documentation**: Add `.sh3d` structure notes to the `README.md`.

#### Iteration 1.5: XML Exporter & Roundtrip Validation (Write) ✅ COMPLETED

- **Goal**: Implement the exporter and prove 100% data fidelity through roundtrip testing.
- **Tasks**:
  1. Implement `HomeXmlExporter` using `System.Xml.XmlWriter` to generate `Home.xml` from the C# `Home` model, mirroring the Java `HomeXMLExporter`.
  2. Update `Sh3dIO.Save(Home home, IReadOnlyDictionary<string, byte[]> content, string filePath)` to generate the XML and package it + referenced content into a new `.sh3d` ZIP.
  3. Write a parameterized xUnit roundtrip test: Load a canonical `.sh3d` -> Export to a new `.sh3d` -> Load the new `.sh3d` -> Assert deep equality of core properties (Name, Wall count/coords, Furniture count/coords).
- **Completion Criteria**: Roundtrip tests pass for a provided set of 3 canonical `.sh3d` files (empty, with walls, with furniture). No data loss or corruption.
- **Documentation**: Phase 1 is now complete. The testing strategy has been updated with roundtrip test patterns.

### Phase 2: Core Business Logic & Geometry (Broken into Testable Iterations)

**Overall Goal**: Port the mathematical and spatial logic that powers the 2D plan view, ensuring robust geometry, catalog loading, and MVVM state management. Each iteration is designed to be completed in a single agent session with passing tests.

#### Iteration 2.1: 2D Geometry Primitives

- **Goal**: Implement core 2D math utilities to serve as the foundation for all spatial calculations.
- **Tasks**:
  1. Create `OpenHome3D.Core/Geometry/Point2D.cs` (record struct with `X`, `Y`).
  2. Create `OpenHome3D.Core/Geometry/Segment2D.cs` (record struct with `Start`, `End`).
  3. Implement static methods in `OpenHome3D.Core/Geometry/GeometryUtils.cs` for:
     - `Distance(Point2D a, Point2D b)` and `DistanceSquared`.
     - `ComputeIntersection(Point2D p1, Point2D p2, Point2D p3, Point2D p4, float limit)` (handling vertical/parallel lines, returning `Point2D?`).
  4. Write xUnit tests in `tests/OpenHome3D.Core.Tests/Geometry/GeometryUtilsTests.cs` covering parallel lines, intersecting lines, vertical lines, and distance checks.
- **Completion Criteria**: `dotnet test` passes with >90% coverage on `GeometryUtils`. No floating-point precision failures.
- **Documentation**: Add a "Geometry Utilities" section to `README.md` explaining the 2D math foundation.

#### Iteration 2.2: Wall Geometry & Snapping Logic

- **Goal**: Port wall-specific spatial logic (corner generation, joining, snapping).
- **Tasks**:
  1. Expand the `Wall` model (`src/OpenHome3D.Core/Models/Wall.cs`) to include `Thickness`, `Height`, `WallAtStart`, and `WallAtEnd`.
  2. Implement `GetCornerPoints()` in `Wall` (or `WallGeometryUtils`) to generate the 4 (or more, if joined/arc) corner points of the wall based on start/end coordinates and thickness using vector math.
  3. Implement snapping logic: `SnapStartTo(Point2D target, float margin)` and `SnapToWall(Wall other, float margin)`.
  4. Write xUnit tests verifying wall corner generation (e.g., horizontal/vertical walls) and snapping behavior (walls merging endpoints within margin).
- **Completion Criteria**: `dotnet test` passes for wall geometry and snapping tests. Corner points accurately reflect thickness and orientation.
- **Documentation**: Update the domain model mapping section to reflect the expanded `Wall` properties and geometry methods.

#### Iteration 2.3: Room Boundary & Area Calculation

- **Goal**: Port room polygon logic and area calculation.
- **Tasks**:
  1. Expand the `Room` model (`src/OpenHome3D.Core/Models/Room.cs`) to include `Points` (`IReadOnlyList<Point2D>`).
  2. Implement `GetSignedArea(IReadOnlyList<Point2D> points)` using the Shoelace formula.
  3. Implement `GetArea()` returning the absolute value of the signed area.
  4. Implement `IsClockwise()` and `ContainsPoint(Point2D point, float margin)` using a ray-casting algorithm.
  5. Write xUnit tests for area calculation (simple square, complex polygon, clockwise vs. counter-clockwise vertex ordering) and point-in-polygon checks.
- **Completion Criteria**: `dotnet test` passes for room area and point-in-polygon tests. Area calculations match expected mathematical results exactly.
- **Documentation**: Document the room area calculation algorithm (Shoelace formula) in the code comments or `README.md`.

#### Iteration 2.4: User Preferences & Catalog I/O

- **Goal**: Port the loading of user preferences and the furniture catalog.
- **Tasks**:
  1. Create `UserPreferences` model (`src/OpenHome3D.Core/Models/UserPreferences.cs`) with properties like `Unit`, `MagnetismEnabled`, `NewWallThickness`, `NewWallHeight`.
  2. Create `FurnitureCatalog` and `CatalogPieceOfFurniture` models (`src/OpenHome3D.Core/Models/CatalogPieceOfFurniture.cs`).
  3. Implement `UserPreferencesXmlParser` and `FurnitureCatalogXmlParser` (`src/OpenHome3D.Core/IO/`) using `System.Xml.Linq` to parse standard Sweet Home 3D catalog XML structures.
  4. Write xUnit tests verifying the parsing of sample XML preference and catalog strings, asserting correct property mapping.
- **Completion Criteria**: `dotnet test` passes for catalog and preferences parsing tests. Invalid XML gracefully throws or returns default states.
- **Documentation**: Add a "Catalog & Preferences" section to `README.md` detailing the expected XML schema for custom catalogs.

#### Iteration 2.5: Core MVVM State Management

- **Goal**: Implement the foundational ViewModels bridging the core model to the future Avalonia UI.
- **Tasks**:
  1. Create `OpenHome3D.Core/ViewModels/MainViewModel.cs` implementing `INotifyPropertyChanged` (or using a community toolkit MVVM source generator).
  2. Create `HomeViewModel.cs` wrapping the `Home` model, exposing `ObservableCollection<WallViewModel>`, `ObservableCollection<RoomViewModel>`, etc.
  3. Implement basic relay commands: `AddWallCommand`, `DeleteSelectedCommand`.
  4. Write xUnit tests verifying property change notifications and command execution (e.g., executing `AddWallCommand` adds a wall to the collection and raises `PropertyChanged`).
- **Completion Criteria**: `dotnet test` passes for ViewModel state and command tests. MVVM patterns are strictly followed with no direct UI dependencies in the Core project.
- **Documentation**: Update the architecture section to document the MVVM structure and command patterns used.

### Phase 3: 2D UI & Plan View (Broken into Testable Iterations)

**Overall Goal**: A functional, modern 2D floor plan editor with hardware-accelerated rendering and core editing interactions. Each iteration is designed to be completed in a single agent session with passing tests.

#### Iteration 3.1: Avalonia UI Shell & Basic Window Setup

- **Goal**: Set up the Avalonia UI project, main window, and basic layout (menus, toolbars).
- **Tasks**:
  1. Create `OpenHome3D.UI` project (`dotnet new avalonia.app -n OpenHome3D.UI -f net8.0`).
  2. Add reference to `OpenHome3D.Core`.
  3. Create `MainWindow.axaml` with a basic layout (Menu, Toolbar, Content area for 2D view).
  4. Wire up the `MainViewModel` from Phase 2 to the `MainWindow` via `DataContext`.
- **Completion Criteria**: `dotnet build` succeeds. `OpenHome3D.UI` launches and displays the empty application shell.
- **Unit Testing**: Basic xUnit test verifying `MainWindow` instantiation and `DataContext` binding to `MainViewModel`.
- **Documentation**: Update `README.md` with instructions on how to build and run the UI project (`dotnet run --project src/OpenHome3D.UI`).

#### Iteration 3.2: SkiaSharp 2D Plan View Control (Rendering)

- **Goal**: Implement a custom Avalonia `Control` that renders the 2D plan using `SkiaSharp`.
- **Tasks**:
  1. Add `SkiaSharp` and `Avalonia.Skia` packages to `OpenHome3D.UI`.
  2. Create `PlanViewControl.axaml.cs` inheriting from `Control`.
  3. Override `OnRender` (or use `ICustomDrawOperation`) to draw a background grid, walls (as filled rectangles based on `Wall` corner points), and rooms (as polygons).
  4. Bind the control to `HomeViewModel` to react to model changes and invalidate the visual state.
- **Completion Criteria**: The control successfully renders a static set of walls and rooms provided by a mock `HomeViewModel` without throwing exceptions.
- **Unit Testing**: Use `Avalonia.Headless` to verify that the `PlanViewControl` renders correctly and triggers visual invalidation when the underlying `HomeViewModel` collection changes.
- **Documentation**: Add a "2D Plan View Rendering" section to `README.md` explaining the SkiaSharp integration and custom control architecture.

#### Iteration 3.3: 2D Viewport Interactions (Pan, Zoom, Selection)

- **Goal**: Implement basic mouse and keyboard interactions for the 2D plan view.
- **Tasks**:
  1. Implement `PointerPressed`, `PointerMoved`, and `PointerReleased` handlers in `PlanViewControl`.
  2. Add pan (middle mouse or space+drag) and zoom (mouse wheel) logic, updating a `Transform` or camera state in the ViewModel.
  3. Implement basic selection logic: clicking a wall or room highlights it and updates `SelectedItem` in `HomeViewModel`.
- **Completion Criteria**: User can pan, zoom, and select items in the running application, with visual feedback (e.g., highlight color) for selected items.
- **Unit Testing**: `Avalonia.Headless` tests simulating pointer events (e.g., `pointer.MoveTo(...)`, `pointer.Click()`) and asserting that the `HomeViewModel.SelectedItem` is updated correctly.
- **Documentation**: Update the architecture section in `README.md` to document the interaction handling pattern (e.g., command pattern or direct ViewModel updates via pointer events).

#### Iteration 3.4: Drag-and-Drop Furniture & Wall Creation Tools

- **Goal**: Implement the core editing tools: adding walls and placing furniture.
- **Tasks**:
  1. Implement a "Wall Creation" mode: clicking sets start point, moving shows preview, clicking again sets end point and adds to `HomeViewModel.Walls` (utilizing snapping logic from Phase 2).
  2. Implement Drag-and-Drop from a mock catalog sidebar to the `PlanViewControl`, creating a new `HomePieceOfFurniture` in the `HomeViewModel` with coordinates translated from screen space to world space.
- **Completion Criteria**: User can draw new walls and drag-and-drop furniture into the 2D view, with changes immediately reflected in the `HomeViewModel` and re-rendered.
- **Unit Testing**: `Avalonia.Headless` tests simulating a drag-and-drop operation and asserting that a new furniture item is added to the `HomeViewModel.Furniture` collection with correct world coordinates.
- **Documentation**: Update `README.md` with a "Core Editing Tools" section detailing the supported interactions and how to extend them.

### Phase 4: 3D Rendering Engine (Weeks 11-16)

- **Goal**: Replace Java 3D with a modern, hardware-accelerated 3D view using Godot.
- **Tasks**:
  1. Set up a Godot C# project and establish communication between the Avalonia UI and the Godot viewport (via GodotSharp or IPC/embedding).
  2. Port the scene graph logic: instantiate Godot nodes for walls, floors, and furniture (parsing `.obj` / `.3ds` / `.gltf`).
  3. Implement camera controls (orbit, pan, zoom) matching the original UX using Godot's `Camera3D`.
  4. Add basic lighting (DirectionalLight3D, OmniLight3D) and StandardMaterial3D support.
- **Testing**: Visual regression testing (render a scene and compare to a baseline image).

### Phase 5: Advanced Features & Export (Weeks 17-20)

- **Goal**: Parity with core advanced features (deferring complex photo rendering).
- **Tasks**:
  1. Implement PDF export (via QuestPDF/iText 7) and SVG export (via Svg.Skia).
  2. Port the video creation workflow (e.g., using `FFMpegCore` to stitch rendered frames).
  3. **Defer** advanced ray-traced photo rendering to a post-V1 phase to ensure the core editor is stable and feature-complete first.
- **Testing**: End-to-end integration tests for export pipelines.

### Phase 6: Packaging & Distribution (Weeks 21-24)

- **Goal**: Deliver native installers for all target platforms.
- **Tasks**:
  1. Configure GitHub Actions for CI/CD.
  2. Package for Windows: MSIX or standard NSIS/Inno Setup installer.
  3. Package for macOS: `.dmg` with universal binary (x86_64 + arm64) support.
  4. Package for Linux: AppImage, Flatpak, or native `.deb`/`.rpm`.
- **Testing**: Installation and smoke tests on clean VMs for each OS.

## 6. Testing Strategy

- **Unit Testing**: `xUnit` for all pure C# logic (models, geometry, XML parsing). Target >85% coverage on the `Core` project.
- **Roundtrip Testing**: Parameterized tests that Load -> Save -> Load canonical `.sh3d` files, asserting deep equality of core properties (Name, Wall count/coords, Furniture count/coords) to guarantee 100% data fidelity.
- **UI Testing**: `Avalonia.Headless` for automated testing of view models and basic control interactions.
- **Visual Regression**: For the 3D and 2D renderers, use a tool to capture rendered frames and compare them against approved baseline images to catch graphical regressions.
- **Compatibility Testing**: Maintain a repository of diverse `.sh3d` files (from different SH3D versions) to test backward compatibility continuously.

## 7. Risks & Mitigations

| Risk | Impact | Mitigation |
| :--- | :--- | :--- |
| **3D Performance Parity** | High | Godot provides excellent out-of-the-box performance. Focus on efficient mesh generation (e.g., merging wall meshes) and proper LOD management. |
| **Exact Visual Parity** | Medium | The 2D/3D rendering will look *cleaner* and more modern, but dimensions and proportions must be mathematically identical. Rely on unit tests for geometry. |
| **Third-Party Library Gaps** | Medium | Some niche Java libraries (e.g., specific JMF video handling) may lack direct C# equivalents. Plan to use modern .NET alternatives (e.g., `FFMpegCore` for video). |
| **Scope Creep** | High | Strictly adhere to the phased approach. Do not add new features until feature parity with SH3D 7.5 is achieved. |

## 8. Confirmed Decisions & Next Steps

Based on your feedback, the following decisions are locked in:

1. **3D Engine**: **Godot** (via GodotSharp/C#) will be used for the 3D viewport, providing a modern, high-level rendering pipeline.
2. **Branding**: The project will be **rebranded** (e.g., "OpenHome3D") to avoid any trademark conflicts with the original "Sweet Home 3D".
3. **Ray Tracing**: Advanced photo-realistic rendering is **deferred to post-V1**, allowing us to focus on core 2D/3D editing parity first.

**Next Step**: We are ready to begin **Phase 2, Iteration 2.1**. This involves porting geometry utilities (intersection, wall joining, room area calculation) to support the 2D plan view logic.
