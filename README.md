# EasyDebug for Godot (C#)

**EasyDebug** is a powerful and flexible C# debugging addon for Godot 4.x projects. It allows developers to easily track and visualize object properties and values at runtime through a dedicated, customizable UI window.

## Features

* **Runtime Property Tracking:** Monitor variables, properties (including nested ones), and method return values from your C# scripts in real-time.
* **Categorized Display:** Organize tracked properties into logical categories within a clean Tree UI.
* **Customizable Appearance:**
    * Control text color and background color for individual tracked properties and category headers.
    * Specify rounding for floating-point numbers.
    * (Future/Planned: Bold styling for text).
* **Fluent API:** Uses C# extension methods for an intuitive `this.Track(...)` syntax.
* **Efficient Property Access:** Leverages C# Expression Trees to create highly optimized delegates for property access, minimizing performance overhead during runtime.
* **Editor Integration:**
    * Adds a dock to the Godot editor ("EasyDebug" dock).
    * Enable or disable the entire debugging system with a checkbox.
    * Choose the initial side (left/right of the main game window) for the debug window.
* **Persistent Settings:**
    * The debug window's size and initial side preference are saved in your `project.godot` file.
    * The enabled/disabled state of the debugger is also persisted.
* **Minimal Performance Impact When Disabled:** If debugging is turned off via the editor dock, the addon has virtually zero performance cost on your game.
* **Automatic Cleanup:** Tracked objects are held with `WeakReference`s, and the system automatically cleans up entries for destroyed nodes.

## Installation

1.  **Download/Clone:** Obtain the addon files.
2.  **Place in Project:** Copy the entire addon folder (e.g., `easydebug` or `monsterhunt_easydebug`, ensure the name matches the `plugin.cfg` reference) into the `addons/` directory of your Godot C# project.
    * Your project structure should look like:
        ```
        MyGodotProject/
        ├── addons/
        │   └── easydebug/  <-- Or your chosen addon folder name
        │       ├── plugin.cfg
        │       ├── EasyDebugPlugin.cs
        │       ├── EasyDebug.cs         (This script is autoloaded)
        │       ├── EasyDebugExtensions.cs
        │       ├── TrackOptions.cs      (if in a separate file, or part of EasyDebug.cs)
        │       └── TrackedProperty.cs   (if in a separate file, or part of EasyDebug.cs)
        └── project.godot
        ```
3.  **Enable Plugin:**
    * Open your Godot project.
    * Go to **Project > Project Settings**.
    * Navigate to the **Plugins** tab.
    * Find "Easy C# Debugger UI" (or the name specified in your `plugin.cfg`) in the list and check the **Enable** box.

## Usage

1.  **Enable Debugging in Editor:**
    * After enabling the plugin, an "EasyDebug" dock should appear in the Godot editor (typically on the left).
    * Ensure the "Enable Debugging" checkbox within this dock is checked.
    * You can also select the initial side (left/right) for the debug window when the game starts.

2.  **Tracking Properties in Your C# Scripts:**
    * In any C# script attached to a `Node`, you can start tracking its properties.
    * Make sure to include the namespace of the addon: `using MonsterHunt.addons.easydebug;` (adjust if your namespace is different).

    **Basic Tracking (Category only):**
    ```csharp
    // In your Player.cs _Ready() method, for example
    public override void _Ready()
    {
        this.Track("PlayerInfo", p => new { 
            NodeName = p.Name,
            p.Position 
        });
    }
    ```

    **Tracking with `TrackOptions` for more control:**
    ```csharp
    // In your Player.cs _Ready() method
    public override void _Ready()
    {
        var movementOptions = new TrackOptions("PlayerMovement") 
        {
            RoundingDigits = 2,
            TextColor = Colors.LightBlue,
            BackgroundColor = new Color(0.1f, 0.1f, 0.2f) // Dark blueish
        };
        this.Track(movementOptions, p => new { 
            p.Velocity,
            GlobalPos = p.GlobalPosition 
        });

        var statsOptions = new TrackOptions("PlayerStats")
        {
            CategoryTextColor = Colors.Gold,
            IsPropertyNameBold = true // (Requires bold font setup in EasyDebug.cs)
        };
        this.Track(statsOptions, p => new { p.Health, p.Mana });
    }
    ```
    * The first argument to `Track()` is either a `string` for the category or a `TrackOptions` object.
    * The second argument is a lambda expression `p => new { ... }` where `p` is the node instance (`this`).
    * Inside the `new { ... }`, list the properties you want to track. You can assign custom display names (e.g., `GlobalPos = p.GlobalPosition`).
    * Nested properties are supported (e.g., `p.StateMachine.CurrentStateName`).

3.  **Running the Game:**
    * When you run your game, if debugging is enabled, the EasyDebug window will appear (created programmatically by `EasyDebug.cs`).
    * It will display the tracked properties, organized by category, and update their values in real-time.
    * The window's size can be changed, and this size will be remembered for the next session.
    * The window will attempt to stay on top of your game window.

## Configuration

* **Enable/Disable:** Via the "Enable Debugging" checkbox in the "EasyDebug" editor dock. This setting is saved in `project.godot` under `addons/monsterhunt_easydebug/debugging_enabled`.
* **Debug Window Side:** Set the initial side (left/right of the main game window) via the OptionButton in the "EasyDebug" editor dock. Saved in `project.godot` under `addons/monsterhunt_easydebug/window/side`.
* **Debug Window Size:** Resizing the debug window during runtime will save its new size to `project.godot` under `addons/monsterhunt_easydebug/window/size_x` and `size_y`.
* **Styling via `TrackOptions`:**
    * `Category`: (string) The group name for these properties.
    * `RoundingDigits`: (int?) Number of decimal places for floats/doubles.
    * `TextColor`: (Color?) Color for the property name and value text.
    * `BackgroundColor`: (Color?) Background color for the property row.
    * `CategoryTextColor`: (Color?) Color for the category header text.
    * `CategoryBackgroundColor`: (Color?) Background color for the category header.
    * `IsCategoryHeaderBold`, `IsPropertyNameBold`, `IsValueBold`: (bool) Flags for bold text (requires font setup).

## Key Addon Files

* `plugin.cfg`: Addon manifest.
* `EasyDebugPlugin.cs`: Manages editor integration (dock, settings persistence, autoload control).
* `EasyDebug.cs`: Core runtime logic; this script is autoloaded. It creates the debug UI window and updates tracked values. It may also contain `TrackOptions` and `TrackedProperty` class definitions, or these can be in separate files.
* `EasyDebugExtensions.cs`: Provides the convenient `this.Track(...)` extension methods for `Node`.
* `TrackOptions.cs` (Optional): If you separate the `TrackOptions` class.
* `TrackedProperty.cs` (Optional): If you separate the `TrackedProperty` class.

## Dependencies

* Godot 4.x project with C# support enabled.

---

This README should provide a good starting point for users of your addon!
