namespace OpenHome3D.Core.Models;

public record UserPreferences(
    string Unit = "Centimeter",
    bool MagnetismEnabled = true,
    float NewWallThickness = 10f,
    float NewWallHeight = 250f
);