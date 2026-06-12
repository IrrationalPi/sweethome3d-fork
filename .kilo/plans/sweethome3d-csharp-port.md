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

#### Iteration 1.3: Furniture, Doors/Windows & Properties Parsing ✅ COMPLETED

- **Goal**: Parse furniture placements, wall openings (doors/windows), and custom home properties.
- **Tasks**:
  1. Expand C# models: 
     - `HomePieceOfFurniture` (`Id`, `Name`, `X`, `Y`, `Z`, `Angle`, `Width`, `Depth`, `Height`, `ModelName`).
     - `DoorOrWindow` (`Id`, `Name`, `X`, `Y`, `Angle`, `Width`, `Depth`, `Height`, `WallThickness`, `WallDistance`, `CutOutShape`, `WallCutOutOnBothSides`).
     - `HomeProperty` (`Name`, `Value`).
  2. Update `HomeXmlParser` to parse `<pieceOfFurniture ... />`, `<doorOrWindow ... />`, and `<property name="..." value="..." />` elements.
- **Completion Criteria**: Unit tests pass asserting that parsing XML with furniture and doors/windows correctly populates the respective collections with accurate spatial and dimensional data.
- **Documentation**: Custom properties are handled via the `HomeProperty` record. Door/window cutouts are preserved via `CutOutShape` for accurate 2D/3D rendering, maintaining 100% fidelity with files like `actual_sh3d_save.sh3d`.

#### Iteration 1.4: .sh3d ZIP Extraction & Content Mapping (Read)

- **Goal**: Handle the `.sh3d` file format as a ZIP archive, not just raw XML.
- **Tasks**:
  1. Create an `Sh3dIO` class using `System.IO.Compression.ZipFile`.
  2. Implement `Load(string filePath)`: Opens the ZIP, locates the `Home.xml` entry, extracts it, and passes it to `HomeXmlParser`.
  3. Build a dictionary of embedded content (e.g., `content/1.png`) referenced by the XML for future use.
- **Completion Criteria**: Unit test passes asserting that loading a valid `.sh3d` file successfully returns a populated `Home` object and a non-empty content map (if the file has embedded assets).
- **Documentation**: Add `.sh3d` structure notes to the `README.md`.

#### Iteration 1.5: XML Exporter & Roundtrip Validation ✅ COMPLETED

- **Goal**: Implement the exporter and prove 100% data fidelity through roundtrip testing, specifically validating against the provided `tests/OpenHome3D.Core.Tests/Resources/actual_sh3d_save.sh3d` fixture.
- **Tasks**:
  1. Implement `HomeXmlExporter` using `System.Xml.XmlWriter` to generate `Home.xml` from the C# `Home` model, mirroring the Java `HomeXMLExporter`.
  2. Update `Sh3dIO.Save(Home home, IReadOnlyDictionary<string, byte[]> content, string filePath)` to generate the XML and package it + referenced content into a new `.sh3d` ZIP.
  3. Write a parameterized xUnit roundtrip test. Key fixtures include empty homes, wall configurations, and the comprehensive `actual_sh3d_save.sh3d` (which includes 5 walls, 1 door, 3 fixed windows, and 5 pieces of furniture).
  4. Roundtrip assertion: Load `actual_sh3d_save.sh3d` -> Export to `actual_sh3d_save_roundtrip.sh3d` -> Load the new `.sh3d` -> Assert deep equality of core properties (Name="Office Layout.sh3d", Wall count=5, Furniture count=5, Door/Window count=4, all coordinates and dimensions match within floating-point tolerance).
- **Completion Criteria**: Roundtrip tests pass for all canonical `.sh3d` files with zero data loss, corruption, or floating-point degradation.
- **Documentation**: Phase 1 is now complete. The testing strategy has been updated with roundtrip test patterns, explicitly citing the `actual_sh3d_save.sh3d` validation.

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

- **Goal**: Set up the Avalonia UI project, main window, and basic layout (menus, toolbars), with an integrated "Open File" dialog.
- **Tasks**:
  1. Create `OpenHome3D.UI` project (`dotnet new avalonia.app -n OpenHome3D.UI -f net8.0`).
  2. Add reference to `OpenHome3D.Core`.
  3. Create `MainWindow.axaml` with a basic layout (Menu, Toolbar, Content area for 2D view).
  4. Wire up the `MainViewModel` from Phase 2 to the `MainWindow` via `DataContext`.
  5. Implement an `OpenFileCommand` that triggers a file open dialog, filters for `*.sh3d`, and uses `Sh3dIO.Load` to populate the `MainViewModel.Home`.
- **Completion Criteria**: `dotnet build` succeeds. `OpenHome3D.UI` launches, allows opening `actual_sh3d_save.sh3d`, and binds the loaded data to the ViewModel without crashing.
- **Unit Testing**: Basic xUnit test verifying `MainWindow` instantiation and `DataContext` binding, plus a mocked file-load test.
- **Documentation**: Update `README.md` with instructions on how to build and run the UI project (`dotnet run --project src/OpenHome3D.UI`) and open existing save files.

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

### Phase 4: 3D Rendering Engine (Broken into Testable Iterations)

**Overall Goal**: Replace Java 3D with a modern, hardware-accelerated 3D view using Godot, ensuring accurate scene representation and responsive user interaction. The primary integration test will be successfully rendering the `actual_sh3d_save.sh3d` file in 3D. Each iteration is designed to be completed in a single agent session with passing tests.

#### Iteration 4.1: Godot Project Scaffolding & IPC Protocol Definition
- **Goal**: Establish the Godot C# rendering project and define a robust communication protocol between the Avalonia UI and the renderer.
- **Tasks**:
  1. Create `src/OpenHome3D.Rendering/` as a Godot 4.x C# project (`project.godot`, `OpenHome3D.Rendering.csproj`).
  2. Define a C# record-based message protocol in `OpenHome3D.Core` (e.g., `RenderingCommand`, `SceneState`) for UI-to-Renderer communication (using JSON serialization).
  3. Create a basic Godot `Main.cs` script that initializes the 3D environment and can parse/acknowledge a dummy command (e.g., from a local JSON file or stdin).
  4. Write xUnit tests in `OpenHome3D.Core.Tests` verifying the serialization and deserialization of the message protocol.
- **Completion Criteria**: Godot project builds successfully (`godot --headless --build-solutions`). Message protocol unit tests pass with 100% coverage.
- **Documentation**: Update `README.md` with Godot project setup, build instructions, and the IPC protocol schema.

#### Iteration 4.2: Procedural Mesh Generation (Walls & Floors)
- **Goal**: Translate the C# `Home` model into 3D geometry for walls and floors.
- **Tasks**:
  1. Implement `WallMeshGenerator` in `OpenHome3D.Core` that takes a `Wall` and generates vertex/index arrays for a 3D extruded mesh (handling basic straight walls).
  2. Implement `FloorMeshGenerator` that creates a mesh for `Room` polygons using the existing 2D points and a fixed floor thickness.
  3. Create a Godot script `SceneBuilder.cs` that receives a serialized `SceneState` and instantiates `MeshInstance3D` nodes with `StandardMaterial3D` using `ArrayMesh`.
  4. Write xUnit tests verifying that a given `Wall` model produces the expected vertex count (e.g., 8 vertices for a simple rectangular wall) and correct bounding box dimensions.
- **Completion Criteria**: Unit tests pass asserting correct mesh dimensions and vertex counts for standard wall/floor configurations. Godot scene can successfully load and display a hardcoded test home state.
- **Documentation**: Document the mesh generation logic, coordinate system mapping (Y-up in Godot vs Y-down in SH3D), and Godot node hierarchy in `README.md`.

#### Iteration 4.3: Furniture Instantiation & Asset Loading
- **Goal**: Load and place 3D furniture models in the scene based on the `Home` model.
- **Tasks**:
  1. Implement logic to map `HomePieceOfFurniture.ModelName` to a `.gltf` or `.obj` file path within the project's asset directory.
  2. Create a `FurnitureLoader.cs` in Godot that uses `ResourceLoader.Load<PackedScene>` or `GLTFDocument` to dynamically load the model.
  3. Apply position (`X`, `Y`, `Z`) and rotation (`Angle`) from the `HomePieceOfFurniture` model to the instantiated Godot node, accounting for coordinate system differences.
  4. Write xUnit tests verifying the transformation matrix calculation (position + Y-axis rotation) for furniture placement.
- **Completion Criteria**: Unit tests pass for furniture transformation math. A test scene with furniture loads and positions items correctly in the Godot editor/runner.
- **Documentation**: Update documentation with supported 3D model formats, asset directory structure, and coordinate transformation rules.

#### Iteration 4.4: Camera Controls & Basic Lighting
- **Goal**: Implement interactive camera controls and basic scene lighting matching the original UX.
- **Tasks**:
  1. Add a `Camera3D` and `DirectionalLight3D` (with shadows enabled) to the Godot main scene.
  2. Implement an `OrbitCameraController.cs` in Godot that handles mouse input for orbit (left drag), pan (middle drag), and zoom (scroll wheel), matching Sweet Home 3D's UX.
  3. Write xUnit tests verifying camera boundary constraints, zoom limits, and target-centering math.
  4. Ensure the camera can be programmatically commanded to frame the entire home or a selected object.
- **Completion Criteria**: Camera controls are functional in the Godot editor and standalone run. Unit tests for camera math/constraints pass.
- **Documentation**: Document the camera control scheme, lighting setup, and keyboard/mouse mappings in `README.md`.

#### Iteration 4.5: Visual Regression Testing Setup
- **Goal**: Establish a baseline for visual regression testing of the 3D view to catch graphical regressions.
- **Tasks**:
  1. Create a standardized test scene (e.g., `TestHome.sh3d` with 1 room, 4 walls, 1 piece of furniture) and its corresponding `SceneState` JSON.
  2. Implement a headless Godot script (`VisualRegressionTest.cs`) that loads the test scene, waits for rendering to stabilize (e.g., 10 frames), and saves a screenshot to `tests/OpenHome3D.Rendering.Tests/Baselines/`.
  3. Write a test runner (xUnit test invoking Godot headless via CLI) that compares the new screenshot's SHA256 hash against the approved baseline hash.
  4. Add a script or Makefile target (e.g., `dotnet run --project tests/OpenHome3D.Rendering.Tests`) to execute the visual regression check.
- **Completion Criteria**: Headless Godot run successfully generates a screenshot. Test passes when the screenshot matches the baseline hash.
- **Documentation**: Add a "3D Visual Regression Testing" section to `README.md` explaining how to run the tests and update baselines when intentional visual changes are made.

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
