# Phase 3: 2D UI & Plan View - Detailed Implementation & Verification Plan

## 1. Executive Summary
This plan details the incremental implementation of the 2D floor plan editor to achieve feature parity with Sweet Home 3D. The current state has a functioning UI shell, but lacks core interactions (wall creation, panning, zooming). This plan breaks the remaining work into highly granular, testable iterations. 

**Crucial Workflow Rule**: Implementation will proceed **one iteration at a time**. After completing an iteration, I will prompt you to verify the functionality. I will not proceed to the next iteration until you confirm the current one works as expected.

## 2. Current State Assessment
- **Completed**: Avalonia UI shell opens (Iteration 3.1 baseline).
- **Missing/Incomplete**: SkiaSharp rendering of geometry, viewport navigation (pan/zoom), wall creation tools, selection, and drag-and-drop.

## 3. Iterative Implementation & Verification Plan

### Iteration 3.1: Robust 2D Plan View Rendering (SkiaSharp)
- **Goal**: Ensure the `PlanViewControl` correctly renders the current `Home` model (walls, rooms, furniture) using SkiaSharp, with proper coordinate mapping (world to screen).
- **Tasks**:
  1. Verify `PlanViewControl` exists and is wired to `HomeViewModel`.
  2. Implement `OnRender` (or `ICustomDrawOperation`) to draw:
     - A background grid (configurable spacing, e.g., 20cm or 1m).
     - Walls as filled polygons based on `Wall` corner points (utilizing Phase 2 geometry utilities).
     - Rooms as outlined polygons.
  3. Implement a `CameraTransform` (Scale, OffsetX, OffsetY) in the ViewModel/Control to serve as the foundation for pan/zoom.
- **User Verification Step**:
  - **Action**: Run `dotnet run --project src/OpenHome3D.UI`. Load a test `.sh3d` file containing walls and rooms.
  - **Expected Result**: The 2D view displays a background grid, and the walls/rooms are drawn accurately and centered on the screen.
  - **Prompt to User**: *"Please run the UI, load a test file, and confirm you can see the walls and rooms drawn on a grid. Reply 'PASS' or describe exactly what is missing or incorrect."*

### Iteration 3.2: Viewport Interactions (Pan & Zoom)
- **Goal**: Implement mouse and keyboard interactions for navigating the 2D plan view, matching Sweet Home 3D's UX.
- **Tasks**:
  1. Add `PointerPressed`, `PointerMoved`, and `PointerReleased` event handlers to `PlanViewControl`.
  2. Implement **Pan**: Middle mouse button drag (or Spacebar + Left drag) updates the `CameraTransform.Offset`.
  3. Implement **Zoom**: Mouse wheel updates the `CameraTransform.Scale` (clamped between min/max values, e.g., 0.1x to 10x), zooming towards the current mouse cursor position.
  4. Ensure the SkiaSharp render loop reacts to `CameraTransform` property changes and invalidates/ redraws the visual state.
- **User Verification Step**:
  - **Action**: Run the UI. Use the middle mouse button to pan around the view. Use the mouse wheel to zoom in and out.
  - **Expected Result**: The view pans smoothly without lag. Zooming scales the grid and geometry correctly, centered around the mouse pointer.
  - **Prompt to User**: *"Please test panning and zooming. Does it feel smooth and accurate? Reply 'PASS' or describe any issues (e.g., zooming off-center, input lag, or incorrect bounds)."*

### Iteration 3.3: Wall Creation Tool
- **Goal**: Implement the core "Add Wall" interaction, matching Sweet Home 3D's UX.
- **Tasks**:
  1. Add a "Wall Mode" state to `HomeViewModel` (e.g., `bool IsWallCreationMode`).
  2. In `PlanViewControl`, when in Wall Mode and Left Mouse is clicked:
     - **First click**: Sets the `WallStartPoint` (snapped to grid or existing wall endpoints using Phase 2 snapping logic). Draws a temporary preview line following the mouse.
     - **Second click**: Sets the `WallEndPoint`, creates a new `Wall` object, adds it to `HomeViewModel.Walls`, and either exits Wall Mode or stays in mode for continuous drawing (Sweet Home 3D style).
  3. Implement visual feedback: Preview line is dashed or a distinct color. Snapping highlights the target point visually.
- **User Verification Step**:
  - **Action**: Run the UI. Activate "Wall Mode" (e.g., via a toolbar button). Click to start a wall, move the mouse, and click to end it. Try snapping to an existing wall's endpoint or the grid.
  - **Expected Result**: A new wall is created and persists in the view. The preview line follows the mouse. Snapping works (cursor jumps to grid/wall endpoints).
  - **Prompt to User**: *"Please test creating a new wall. Does the preview line appear? Does it snap to the grid/existing walls? Does the wall save correctly to the model? Reply 'PASS' or describe the behavior."*

### Iteration 3.4: Selection & Basic Manipulation
- **Goal**: Allow users to select existing walls/furniture and delete them.
- **Tasks**:
  1. Implement hit-testing in `PlanViewControl`: On Left Click (when NOT in Wall Mode), check if the click intersects any `Wall` or `Room` bounding box/polygon (accounting for the current `CameraTransform`).
  2. Update `HomeViewModel.SelectedItem` to the clicked object.
  3. Render selected objects with a distinct visual style (e.g., highlighted border, different fill color, or selection handles).
  4. Implement a `DeleteCommand` bound to the `Delete` key, which removes `SelectedItem` from the respective collection in the ViewModel.
- **User Verification Step**:
  - **Action**: Run the UI. Click on an existing wall. Press the `Delete` key on your keyboard.
  - **Expected Result**: The wall highlights upon clicking. Pressing `Delete` removes it from the view and the underlying model immediately.
  - **Prompt to User**: *"Please test selecting and deleting a wall. Does it highlight clearly? Does the Delete key remove it? Reply 'PASS' or describe any issues."*

### Iteration 3.5: Drag-and-Drop Furniture Placement
- **Goal**: Implement dragging furniture from a catalog sidebar into the 2D plan view.
- **Tasks**:
  1. Create a basic `CatalogView` (e.g., a `ListBox`) bound to a mock `FurnitureCatalog` in the ViewModel.
  2. Enable Avalonia Drag-and-Drop: Set `DragDrop.SetAllowDrop` to `true` on `PlanViewControl`.
  3. Handle the `Drop` event: Extract the furniture ID from the drag data, calculate the world coordinates from the screen drop point (accounting for current Pan/Zoom `CameraTransform`), and add a new `HomePieceOfFurniture` to `HomeViewModel.Furniture`.
- **User Verification Step**:
  - **Action**: Run the UI. Drag an item from the catalog sidebar and drop it onto the 2D plan view.
  - **Expected Result**: The furniture item appears at the exact drop location in the 2D view, correctly scaled and oriented.
  - **Prompt to User**: *"Please test dragging and dropping a furniture item. Does it appear at the correct location relative to your mouse? Reply 'PASS' or describe any offset/positioning issues."*

## 4. Overall Completion Criteria
This Phase 3 plan is considered **complete** only when:
1. You have replied "PASS" to all 5 verification steps above.
2. The 2D view demonstrably supports:
   - Rendering walls, rooms, and furniture on a configurable grid.
   - Smooth, intuitive panning and zooming.
   - Interactive wall creation with grid/wall snapping.
   - Selection and deletion of objects.
   - Drag-and-drop furniture placement from a catalog.
3. All new code is covered by `Avalonia.Headless` unit tests where applicable, and `dotnet test` passes without regressions.

## 5. Next Steps
If you agree with this phased, verification-driven approach, I will begin implementing **Iteration 3.1**. I will pause after completing it and wait for your explicit feedback before moving to Iteration 3.2.
