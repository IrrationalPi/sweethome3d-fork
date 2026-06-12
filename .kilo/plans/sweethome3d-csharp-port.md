# Sweet Home 3D C# Port: Implementation & Progress Tracking Plan

## 1. Executive Summary
This document outlines a long-term, incremental strategy to port the Sweet Home 3D desktop application from Java to C#. The goal is a fully cross-platform application (Windows, macOS, Linux) with a modern, native UI/UX, preserving 100% compatibility with existing `.sh3d` save files and core user workflows. This plan strictly adheres to the project's atomic task lifecycle, test-driven validation, and centralized progress tracking.

## 2. Licensing & Legal Compatibility
- **Core License**: GNU GPL v2 or later. The C# port must remain GPL v2+ (or compatible).
- **Third-Party Libraries**: Must use GPL-compatible or permissively licensed alternatives (e.g., MIT/Apache 2.0).
- **Branding**: Rebranded (e.g., "OpenHome3D") to avoid trademark conflicts.

## 3. Target Technology Stack
| Domain | Java (Current) | C# / .NET (Proposed) | Rationale |
| :--- | :--- | :--- | :--- |
| **Language** | Java 8+ | C# 12+ (.NET 10) | Modern syntax, cross-platform performance. |
| **UI Framework** | Java Swing | **Avalonia UI** | True cross-platform, XAML-based, customizable. |
| **2D Rendering** | Java 2D / Batik | **SkiaSharp** / **Svg.Skia** | Hardware-accelerated 2D drawing and SVG parsing. |
| **3D Rendering** | Java 3D (JOGL) | **Godot** (GodotSharp) | High-level, modern, cross-platform 3D engine with C# support. |
| **Ray Tracing** | SunFlow / YafaRay | **Deferred to post-V1** | Focus on core 2D/3D editing parity first. |
| **File I/O / XML** | Custom SAX Parser | **System.Xml.Linq** / **System.IO.Compression** | Robust, built-in, performant for `.sh3d` roundtrip. |
| **Build / CI** | Ant | **.NET SDK** / **GitHub Actions** | Standardized, cross-platform packaging. |
| **Testing** | Abbot / JUnit | **xUnit** + **Avalonia.Headless** | Modern, fast, supports headless UI testing. |

## 4. Architecture & Domain Model Mapping
- `com.eteks.sweethome3d.model.*` → `OpenHome3D.Core.Models.*` (C# `record` types, immutable, e.g., `Home`, `Wall`, `Room`, `HomePieceOfFurniture`).
- `com.eteks.sweethome3d.io.*` → `OpenHome3D.Core.IO.*` (XML serialization, ZIP handling).
- `com.eteks.sweethome3d.viewcontroller.*` → `OpenHome3D.Core.ViewModels.*` (MVVM pattern).
- `com.eteks.sweethome3d.swing.*` → `OpenHome3D.UI.Views.*` (Avalonia UserControls/Windows).
- `com.eteks.sweethome3d.j3d.*` → `OpenHome3D.Rendering.*` (3D scene management, Godot integration).

## 5. Progress Tracking Master Checklist
- [x] **Phase 1**: Foundation & File I/O (Iterations 1.1 - 1.5)
- [ ] **Phase 2**: Core Business Logic & Geometry (Iterations 2.1 - 2.5)
  - [ ] 2.1: 2D Geometry Primitives (ACTIVE)
  - [ ] 2.2: Wall Geometry & Snapping Logic
  - [ ] 2.3: Room Boundary & Area Calculation
  - [ ] 2.4: User Preferences & Catalog I/O
  - [ ] 2.5: Core MVVM State Management
- [ ] **Phase 3**: 2D UI & Plan View (Iterations 3.1 - 3.4)
- [ ] **Phase 4**: 3D Rendering Engine (Iterations 4.1 - 4.5)
- [ ] **Phase 5**: Advanced Features & Export (Weeks 17-20)
- [ ] **Phase 6**: Packaging & Distribution (Weeks 21-24)

---

## 6. Phased Implementation Plan (Atomic Tasks)

### Phase 1: Foundation & File I/O ✅ COMPLETED
*Goal: Prove C# can read/write `.sh3d` files perfectly, establishing core data model and I/O pipeline.*
- [x] **1.1 Project Scaffolding & Minimal XML Parsing**: Set up solution, define `Home` record, implement `HomeXmlParser`, write unit test.
- [x] **1.2 Core Model Expansion**: Expand `Wall`, `Room`, `Point2D` models; update parser for `<wall>` and `<room>`.
- [x] **1.3 Furniture, Doors/Windows & Properties**: Expand `HomePieceOfFurniture`, `DoorOrWindow`, `HomeProperty` models; update parser.
- [x] **1.4 .sh3d ZIP Extraction**: Implement `Sh3dIO.Load` using `System.IO.Compression.ZipFile` to extract `Home.xml` and map content.
- [x] **1.5 XML Exporter & Roundtrip Validation**: Implement `HomeXmlExporter` and `Sh3dIO.Save`. Write parameterized roundtrip tests using `actual_sh3d_save.sh3d`.

### Phase 2: Core Business Logic & Geometry
*Goal: Port mathematical/spatial logic for 2D plan view, ensuring robust geometry, catalog loading, and MVVM state management.*

#### [ ] Iteration 2.1: 2D Geometry Primitives (ACTIVE)
- **Tasks**:
  - [ ] Create `OpenHome3D.Core/Geometry/Point2D.cs` (record struct with `X`, `Y`).
  - [ ] Create `OpenHome3D.Core/Geometry/Segment2D.cs` (record struct with `Start`, `End`).
  - [ ] Implement static methods in `OpenHome3D.Core/Geometry/GeometryUtils.cs`: `Distance`, `DistanceSquared`, `ComputeIntersection` (handling vertical/parallel lines).
  - [ ] Write xUnit tests in `tests/OpenHome3D.Core.Tests/Geometry/GeometryUtilsTests.cs` covering parallel lines, intersecting lines, vertical lines, and distance checks.
- **Validation**: `dotnet build` succeeds. `dotnet test` passes with >90% coverage on `GeometryUtils`. No floating-point precision failures.
- **Documentation**: Add "Geometry Utilities" section to `README.md`.

#### [ ] Iteration 2.2: Wall Geometry & Snapping Logic
- **Tasks**:
  - [ ] Expand `Wall` model (`Thickness`, `Height`, `WallAtStart`, `WallAtEnd`).
  - [ ] Implement `GetCornerPoints()` for 4+ corner generation based on thickness and vector math.
  - [ ] Implement snapping logic: `SnapStartTo` and `SnapToWall`.
  - [ ] Write xUnit tests verifying wall corner generation and snapping behavior within margin.
- **Validation**: `dotnet test` passes. Corner points accurately reflect thickness and orientation.
- **Documentation**: Update domain model mapping section.

#### [ ] Iteration 2.3: Room Boundary & Area Calculation
- **Tasks**:
  - [ ] Expand `Room` model to include `Points` (`IReadOnlyList<Point2D>`).
  - [ ] Implement `GetSignedArea` (Shoelace formula) and `GetArea` (absolute value).
  - [ ] Implement `IsClockwise` and `ContainsPoint` (ray-casting algorithm).
  - [ ] Write xUnit tests for area calculation and point-in-polygon checks.
- **Validation**: `dotnet test` passes. Area calculations match expected mathematical results.
- **Documentation**: Document room area calculation algorithm in `README.md`.

#### [ ] Iteration 2.4: User Preferences & Catalog I/O
- **Tasks**:
  - [ ] Create `UserPreferences` model (`Unit`, `MagnetismEnabled`, `NewWallThickness`, `NewWallHeight`).
  - [ ] Create `FurnitureCatalog` and `CatalogPieceOfFurniture` models.
  - [ ] Implement `UserPreferencesXmlParser` and `FurnitureCatalogXmlParser`.
  - [ ] Write xUnit tests verifying parsing of sample XML preference and catalog strings.
- **Validation**: `dotnet test` passes. Invalid XML gracefully throws or returns default states.
- **Documentation**: Add "Catalog & Preferences" section to `README.md`.

#### [ ] Iteration 2.5: Core MVVM State Management
- **Tasks**:
  - [ ] Create `MainViewModel` and `HomeViewModel` implementing `INotifyPropertyChanged` (or CommunityToolkit.Mvvm).
  - [ ] Expose `ObservableCollection<WallViewModel>`, etc.
  - [ ] Implement basic relay commands: `AddWallCommand`, `DeleteSelectedCommand`.
  - [ ] Write xUnit tests verifying property change notifications and command execution.
- **Validation**: `dotnet test` passes. MVVM patterns strictly followed, no direct UI dependencies in Core.
- **Documentation**: Update architecture section documenting MVVM structure.

### Phase 3: 2D UI & Plan View
*Goal: A functional, modern 2D floor plan editor with hardware-accelerated rendering and core editing interactions.*
- [ ] **3.1 Avalonia UI Shell**: Set up `OpenHome3D.UI`, `MainWindow.axaml`, wire up `MainViewModel`, implement `OpenFileCommand`.
- [ ] **3.2 SkiaSharp 2D Plan View Control**: Create `PlanViewControl`, override `OnRender` to draw grid, walls, rooms. Bind to `HomeViewModel`.
- [ ] **3.3 2D Viewport Interactions**: Implement `PointerPressed/Moved/Released` for pan, zoom, and basic selection logic.
- [ ] **3.4 Drag-and-Drop & Wall Creation**: Implement "Wall Creation" mode and drag-and-drop furniture from catalog to 2D view.

### Phase 4: 3D Rendering Engine
*Goal: Replace Java 3D with a modern, hardware-accelerated 3D view using Godot.*
- [ ] **4.1 Godot Scaffolding & IPC**: Create Godot C# project, define JSON message protocol (`RenderingCommand`, `SceneState`), write serialization tests.
- [ ] **4.2 Procedural Mesh Generation**: Implement `WallMeshGenerator` and `FloorMeshGenerator` in Core. Create `SceneBuilder.cs` in Godot.
- [ ] **4.3 Furniture Instantiation**: Implement logic to map `ModelName` to `.gltf`/`.obj`, create `FurnitureLoader.cs` in Godot, apply transformations.
- [ ] **4.4 Camera Controls & Lighting**: Add `Camera3D`/`DirectionalLight3D`, implement `OrbitCameraController.cs`, write constraint tests.
- [ ] **4.5 Visual Regression Testing**: Create standardized test scene, implement headless Godot screenshot script, compare SHA256 hash in xUnit.

### Phase 5: Advanced Features & Export
- [ ] Implement PDF/SVG export (QuestPDF / Svg.Skia).
- [ ] Port video creation workflow (`FFMpegCore`).
- [ ] Defer advanced ray-traced photo rendering to post-V1.

### Phase 6: Packaging & Distribution
- [ ] Configure GitHub Actions for CI/CD.
- [ ] Package for Windows (MSIX/NSIS), macOS (Universal `.dmg`), Linux (AppImage/Flatpak).

---

## 7. Testing Strategy
- **Unit Testing**: xUnit for all pure C# logic. Target >85% coverage on `Core`.
- **Roundtrip Testing**: Parameterized tests (Load -> Save -> Load) asserting deep equality of core properties to guarantee 100% data fidelity.
- **UI Testing**: Avalonia.Headless for automated testing of view models and basic control interactions.
- **Visual Regression**: Screenshot hashing for 3D/2D renderers to catch graphical regressions.

## 8. Technical Debt Tracking
| ID | Description | Reason Incurred | Anticipated Complications | Status |
| :--- | :--- | :--- | :--- | :--- |
| *None currently logged.* | | | | |

## 9. Definition of Done (DoD)
A task is only marked as completed (`- [x]`) when **ALL** of the following criteria are met:
1. Code compiles without errors or unintended warnings (`dotnet build`).
2. All automated tests (new and existing) pass successfully (`dotnet test`).
3. No linter, formatter, or static analysis violations are introduced.
4. Relevant documentation (`README.md`, code comments) has been updated.
5. The change has been successfully committed to version control with a descriptive message.
6. The corresponding checklist item in this document is marked as complete.

## 10. Session Log & Next Steps
- **Current Session**: Plan restructured to align with `docs/agents/execution-tracking-process.md`. Added atomic task checklists, DoD, and technical debt tracking.
- **Tasks Completed**: Plan standardization, Phase 1 marked complete.
- **Next Task**: Execute **Phase 2, Iteration 2.1: 2D Geometry Primitives**.
  1. Create `Point2D.cs`, `Segment2D.cs`, `GeometryUtils.cs`.
  2. Implement distance and intersection logic.
  3. Write xUnit tests.
  4. Validate with `dotnet test` and update `README.md`.