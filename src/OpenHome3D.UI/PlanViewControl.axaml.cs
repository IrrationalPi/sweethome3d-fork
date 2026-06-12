using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using OpenHome3D.Core.Geometry;
using OpenHome3D.Core.Models;
using OpenHome3D.Core.ViewModels;

namespace OpenHome3D.UI;

public class PlanViewControl : Control
{
    private bool _isPanning;
    private bool _isSpacePressed;
    private Point _lastPointerPosition;
    
    // Wall creation state
    private bool _isCreatingWall;
    private Point2D? _wallCreationStartPoint;
    private Point2D? _wallCreationCurrentPoint;

    // Furniture placement preview
    private Point2D? _furniturePlacementPreviewPoint;

    public PlanViewControl()
    {
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _isSpacePressed = true;
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true; // Prevent spacebar from triggering button clicks
        }
        else if (e.Key == Key.Delete)
        {
            if (DataContext is MainViewModel { CurrentHome: { } viewModel })
            {
                viewModel.DeleteSelectedCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _isSpacePressed = false;
            if (!_isPanning)
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if (_isCreatingWall)
            {
                _isCreatingWall = false;
                _wallCreationStartPoint = null;
                _wallCreationCurrentPoint = null;
                if (DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.CurrentTool = "Select";
                }
                InvalidateVisual();
            }
            else if (DataContext is MainViewModel mvm && mvm.SelectedCatalogItem != null)
            {
                mvm.SelectedCatalogItem = null;
                _furniturePlacementPreviewPoint = null;
                InvalidateVisual();
            }
            e.Handled = true;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            AttachToCurrentHome(mainViewModel.CurrentHome);
        }
    }

    private void OnMainViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentHome) && DataContext is MainViewModel mainViewModel)
        {
            AttachToCurrentHome(mainViewModel.CurrentHome);
        }
    }

    private void AttachToCurrentHome(HomeViewModel? homeViewModel)
    {
        if (homeViewModel != null)
        {
            homeViewModel.Walls.CollectionChanged += OnWallsCollectionChanged;
            homeViewModel.Rooms.CollectionChanged += OnRoomsCollectionChanged;
            homeViewModel.Furniture.CollectionChanged += OnFurnitureCollectionChanged;
            InvalidateVisual();
        }
    }

    private void OnFurnitureCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnWallsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnRoomsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.Focus(NavigationMethod.Pointer);

        if (DataContext is not MainViewModel mainViewModel || mainViewModel.CurrentHome is not { } viewModel)
            return;

        // Reset wall creation if tool changed
        if (mainViewModel.CurrentTool != "CreateWall" && _isCreatingWall)
        {
            _isCreatingWall = false;
            _wallCreationStartPoint = null;
            _wallCreationCurrentPoint = null;
            InvalidateVisual();
        }

        var point = e.GetPosition(this);
        var worldPoint = ScreenToWorld(point, viewModel);

        var isMiddleButton = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;
        var isLeftButton = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

        if (isMiddleButton || (_isSpacePressed && isLeftButton))
        {
            _isPanning = true;
            _lastPointerPosition = point;
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(this);
        }
        else if (isLeftButton)
        {
            if (mainViewModel.CurrentTool == "CreateWall")
            {
                var snappedPoint = SnapPoint(worldPoint, viewModel);
                var snappedPoint2D = new Point2D((float)snappedPoint.X, (float)snappedPoint.Y);

                if (!_isCreatingWall)
                {
                    _isCreatingWall = true;
                    _wallCreationStartPoint = snappedPoint2D;
                    _wallCreationCurrentPoint = snappedPoint2D;
                }
                else
                {
                    if (_wallCreationStartPoint.HasValue)
                    {
                        var newWall = new Wall(
                            Guid.NewGuid().ToString(),
                            _wallCreationStartPoint.Value.X,
                            _wallCreationStartPoint.Value.Y,
                            snappedPoint2D.X,
                            snappedPoint2D.Y,
                            10,
                            250
                        );
                        viewModel.Walls.Add(new WallViewModel(newWall));
                        
                        // Continuous drawing: new start point is the end point
                        _wallCreationStartPoint = snappedPoint2D;
                        _wallCreationCurrentPoint = snappedPoint2D;
                    }
                }
                InvalidateVisual();
            }
            else if (mainViewModel.SelectedCatalogItem != null)
            {
                // Place furniture from catalog at the clicked (snapped) position
                var snappedPoint = SnapPoint(worldPoint, viewModel);
                var newFurniture = new HomePieceOfFurniture(
                    Guid.NewGuid().ToString(),
                    (float)snappedPoint.X,
                    (float)snappedPoint.Y,
                    0, 0, 100, 100, 100,
                    mainViewModel.SelectedCatalogItem
                );
                viewModel.AddFurniture(newFurniture);
                // Keep catalog item selected so user can place multiple copies
                InvalidateVisual();
            }
            else
            {
                HandleSelection(point, viewModel);
            }
            _lastPointerPosition = point;
            e.Pointer.Capture(this);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not MainViewModel mainViewModel || mainViewModel.CurrentHome is not { } viewModel)
            return;

        var point = e.GetPosition(this);
        var worldPoint = ScreenToWorld(point, viewModel);

        if (_isPanning)
        {
            var delta = point - _lastPointerPosition;
            viewModel.PanX += delta.X;
            viewModel.PanY += delta.Y;
            _lastPointerPosition = point;
            InvalidateVisual();
        }
        else if (_isCreatingWall)
        {
            var snappedPoint = SnapPoint(worldPoint, viewModel);
            _wallCreationCurrentPoint = new Point2D((float)snappedPoint.X, (float)snappedPoint.Y);
            InvalidateVisual();
        }
        else if (mainViewModel.SelectedCatalogItem != null)
        {
            var snappedPoint = SnapPoint(worldPoint, viewModel);
            _furniturePlacementPreviewPoint = new Point2D((float)snappedPoint.X, (float)snappedPoint.Y);
            InvalidateVisual();
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            if (!_isSpacePressed)
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }
        e.Pointer.Capture(null);
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not MainViewModel mainViewModel || mainViewModel.CurrentHome is not { } viewModel)
            return;

        var point = e.GetPosition(this);
        
        var worldX = (point.X - viewModel.PanX) / viewModel.Zoom;
        var worldY = (point.Y - viewModel.PanY) / viewModel.Zoom;

        var zoomDelta = e.Delta.Y * 0.1;
        var newZoom = Math.Max(0.1, Math.Min(10.0, viewModel.Zoom + zoomDelta));

        var newPanX = point.X - worldX * newZoom;
        var newPanY = point.Y - worldY * newZoom;

        viewModel.Zoom = newZoom;
        viewModel.PanX = newPanX;
        viewModel.PanY = newPanY;
        
        InvalidateVisual();
        e.Handled = true;
    }

    private void HandleSelection(Point screenPoint, HomeViewModel viewModel)
    {
        var worldPoint = ScreenToWorld(screenPoint, viewModel);
        var currentPoint = new Point2D((float)worldPoint.X, (float)worldPoint.Y);

        viewModel.SelectedWall = null;
        viewModel.SelectedRoom = null;
        viewModel.SelectedFurniture = null;

        // 1. Check Furniture first (top layer)
        foreach (var furnVm in viewModel.Furniture)
        {
            var dist = GeometryUtils.Distance(currentPoint, new Point2D(furnVm.X, furnVm.Y));
            var hitRadius = Math.Max(furnVm.Width, furnVm.Depth) / 2f + 5f;
            if (dist <= hitRadius)
            {
                viewModel.SelectedFurniture = furnVm;
                InvalidateVisual();
                return;
            }
        }

        // 2. Check Walls
        foreach (var wallVm in viewModel.Walls)
        {
            var wall = new Wall(wallVm.Id, wallVm.XStart, wallVm.YStart, wallVm.XEnd, wallVm.YEnd, wallVm.Thickness, wallVm.Height);
            var corners = WallGeometryUtils.GetCornerPoints(wall);
            
            if (IsPointInPolygon(worldPoint, corners))
            {
                viewModel.SelectedWall = wallVm;
                InvalidateVisual();
                return;
            }
        }

        // 3. Check Rooms
        foreach (var roomVm in viewModel.Rooms)
        {
            var room = new Room(roomVm.Id, roomVm.Points, roomVm.Name);
            if (room.ContainsPoint(currentPoint))
            {
                viewModel.SelectedRoom = roomVm;
                InvalidateVisual();
                return;
            }
        }
        
        InvalidateVisual();
    }

    private Point ScreenToWorld(Point screenPoint, HomeViewModel viewModel)
    {
        return new Point(
            (screenPoint.X - viewModel.PanX) / viewModel.Zoom,
            (screenPoint.Y - viewModel.PanY) / viewModel.Zoom
        );
    }

    private bool IsPointInPolygon(Point point, Point2D[] polygon)
    {
        if (polygon.Length < 3) return false;

        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            float xi = polygon[i].X, yi = polygon[i].Y;
            float xj = polygon[j].X, yj = polygon[j].Y;

            bool intersect = ((yi > point.Y) != (yj > point.Y))
                && (point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi);
            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private Point SnapPoint(Point worldPoint, HomeViewModel viewModel)
    {
        float snapMargin = 15f;
        float gridSize = 20f;

        var currentPoint = new Point2D((float)worldPoint.X, (float)worldPoint.Y);

        // 1. Check wall endpoints first (higher priority)
        foreach (var wallVm in viewModel.Walls)
        {
            var start = new Point2D(wallVm.XStart, wallVm.YStart);
            var end = new Point2D(wallVm.XEnd, wallVm.YEnd);

            if (GeometryUtils.Distance(currentPoint, start) <= snapMargin)
            {
                return new Point(start.X, start.Y);
            }
            if (GeometryUtils.Distance(currentPoint, end) <= snapMargin)
            {
                return new Point(end.X, end.Y);
            }
        }

        // 2. Snap to grid
        float snappedX = (float)Math.Round(worldPoint.X / gridSize) * gridSize;
        float snappedY = (float)Math.Round(worldPoint.Y / gridSize) * gridSize;

        return new Point(snappedX, snappedY);
    }

    private void OnDragOver(object? sender, Avalonia.Input.DragEventArgs e)
    {
        if (e.DataTransfer.Formats.Contains(DataFormat.Text))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDrop(object? sender, Avalonia.Input.DragEventArgs e)
    {
        if (DataContext is not MainViewModel mainViewModel || mainViewModel.CurrentHome is not { } viewModel)
            return;

        if (e.DataTransfer.Formats.Contains(DataFormat.Text))
        {
            var furnitureId = e.DataTransfer.TryGetText();
            if (!string.IsNullOrEmpty(furnitureId))
            {
                var point = e.GetPosition(this);
                var worldPoint = ScreenToWorld(point, viewModel);

                var newFurniture = new HomePieceOfFurniture(
                    Guid.NewGuid().ToString(),
                    (float)worldPoint.X,
                    (float)worldPoint.Y,
                    0, // Z
                    0, // Angle
                    100, // Width
                    100, // Depth
                    100, // Height
                    furnitureId
                );

                viewModel.AddFurniture(newFurniture);
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (DataContext is not MainViewModel mainViewModel || mainViewModel.CurrentHome is not { } viewModel)
            return;

        // Clear furniture preview if no catalog item is selected
        if (mainViewModel.SelectedCatalogItem == null)
            _furniturePlacementPreviewPoint = null;

        var bounds = Bounds;
        var width = bounds.Width;
        var height = bounds.Height;

        context.FillRectangle(Brushes.White, new Rect(0, 0, width, height));

        using (context.PushTransform(new Matrix(viewModel.Zoom, 0, 0, viewModel.Zoom, viewModel.PanX, viewModel.PanY)))
        {
            DrawGrid(context, width, height);

            foreach (var wallVm in viewModel.Walls)
            {
                DrawWall(context, wallVm, viewModel.SelectedWall == wallVm);
            }

            foreach (var roomVm in viewModel.Rooms)
            {
                DrawRoom(context, roomVm, viewModel.SelectedRoom == roomVm);
            }

            foreach (var furnVm in viewModel.Furniture)
            {
                DrawFurniture(context, furnVm, viewModel.SelectedFurniture == furnVm);
            }

            // Draw furniture placement preview
            if (mainViewModel.SelectedCatalogItem != null && _furniturePlacementPreviewPoint.HasValue)
            {
                const double previewSize = 100.0;
                var px = _furniturePlacementPreviewPoint.Value.X - previewSize / 2;
                var py = _furniturePlacementPreviewPoint.Value.Y - previewSize / 2;
                context.DrawRectangle(
                    new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Avalonia.Media.Colors.SaddleBrown, 0.5),
                    new Pen(Brushes.DarkRed, 1.5, dashStyle: DashStyle.Dash),
                    new Rect(px, py, previewSize, previewSize));
            }

            // Draw wall creation preview
            if (_isCreatingWall && _wallCreationStartPoint.HasValue && _wallCreationCurrentPoint.HasValue)
            {
                var previewWall = new Wall(
                    "preview",
                    _wallCreationStartPoint.Value.X,
                    _wallCreationStartPoint.Value.Y,
                    _wallCreationCurrentPoint.Value.X,
                    _wallCreationCurrentPoint.Value.Y,
                    10,
                    250
                );
                DrawWall(context, new WallViewModel(previewWall), false, isPreview: true);

                // Draw snap indicator
                var snapRadius = 5.0;
                var snapGeometry = new Avalonia.Media.EllipseGeometry(new Rect(
                    _wallCreationCurrentPoint.Value.X - snapRadius,
                    _wallCreationCurrentPoint.Value.Y - snapRadius,
                    snapRadius * 2,
                    snapRadius * 2
                ));
                context.DrawGeometry(Brushes.Yellow, new Pen(Brushes.Black, 1.0), snapGeometry);
            }
        }
    }

    private void DrawGrid(DrawingContext context, double width, double height)
    {
        const double gridSize = 50.0;
        var pen = new Pen(Brushes.LightGray, 1.0, dashStyle: DashStyle.Dash);

        for (double x = -1000; x < 1000; x += gridSize)
        {
            context.DrawLine(pen, new Point(x, -1000), new Point(x, 1000));
        }

        for (double y = -1000; y < 1000; y += gridSize)
        {
            context.DrawLine(pen, new Point(-1000, y), new Point(1000, y));
        }
    }

    private void DrawWall(DrawingContext context, WallViewModel wallVm, bool isSelected, bool isPreview = false)
    {
        var wall = new Wall(wallVm.Id, wallVm.XStart, wallVm.YStart, wallVm.XEnd, wallVm.YEnd, wallVm.Thickness, wallVm.Height);
        var corners = WallGeometryUtils.GetCornerPoints(wall);

        if (corners.Length < 4)
            return;

        var points = new global::Avalonia.Point[]
        {
            new(corners[0].X, corners[0].Y),
            new(corners[1].X, corners[1].Y),
            new(corners[2].X, corners[2].Y),
            new(corners[3].X, corners[3].Y)
        };

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(points[0], true);
        ctx.LineTo(points[1]);
        ctx.LineTo(points[2]);
        ctx.LineTo(points[3]);
        ctx.EndFigure(true);

        IBrush fillBrush = isPreview ? Brushes.LightGray : (isSelected ? Brushes.LightBlue : Brushes.DarkGray);
        IBrush strokeBrush = isPreview ? Brushes.Gray : (isSelected ? Brushes.Blue : Brushes.Black);
        double strokeWidth = isPreview ? 1.0 : 1.0;
        IDashStyle? dashStyle = isPreview ? DashStyle.Dash : null;

        context.DrawGeometry(fillBrush, new Pen(strokeBrush, strokeWidth, dashStyle: dashStyle), geometry);
    }

    private void DrawRoom(DrawingContext context, RoomViewModel roomVm, bool isSelected)
    {
        var room = new Room(roomVm.Id, roomVm.Points, roomVm.Name);
        if (room.Points.Count < 3)
            return;

        var points = room.Points.Select(p => new global::Avalonia.Point(p.X, p.Y)).ToArray();

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(points[0], true);
        foreach (var pt in points.Skip(1))
        {
            ctx.LineTo(pt);
        }
        ctx.EndFigure(true);

        IBrush fillBrush = isSelected ? Brushes.LightGreen : Brushes.Transparent;
        IBrush strokeBrush = isSelected ? Brushes.Green : Brushes.DarkGreen;
        double strokeWidth = 2.0;

        context.DrawGeometry(fillBrush, new Pen(strokeBrush, strokeWidth), geometry);
    }

    private void DrawFurniture(DrawingContext context, FurnitureViewModel furnVm, bool isSelected)
    {
        double width = furnVm.Width > 0 ? furnVm.Width : 20.0;
        double depth = furnVm.Depth > 0 ? furnVm.Depth : 20.0;
        
        var rect = new Rect(furnVm.X - width / 2, furnVm.Y - depth / 2, width, depth);
        
        IBrush fillBrush = isSelected ? Brushes.LightCoral : Brushes.SaddleBrown;
        IBrush strokeBrush = isSelected ? Brushes.Red : Brushes.Black;
        double strokeWidth = isSelected ? 2.0 : 1.0;
        
        context.DrawRectangle(fillBrush, new Pen(strokeBrush, strokeWidth), rect);
    }
}
