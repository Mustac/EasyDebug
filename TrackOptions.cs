using Godot;
using System; // Required for ArgumentNullException if you choose to throw it

// Ensure this namespace matches your other addon scripts
namespace MonsterHunt.addons.easydebug;

/// <summary>
/// Options for configuring how a property is tracked and displayed.
/// </summary>
public class TrackOptions
{
    // Ensure each property is defined only ONCE
    public string Category { get; set; } = "Default"; // Default value if not set otherwise
    public int? RoundingDigits { get; set; } = null;
    public Color? TextColor { get; set; } = null;
    public Color? BackgroundColor { get; set; } = null;
    public Color? CategoryTextColor { get; set; } = null;
    public Color? CategoryBackgroundColor { get; set; } = null;
    public bool IsCategoryHeaderBold { get; set; } = false;
    public bool IsPropertyNameBold { get; set; } = false;
    public bool IsValueBold { get; set; } = false;

    // Parameterless constructor
    public TrackOptions()
    {
    }
}
