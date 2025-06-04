#if TOOLS
using Godot;
using System;

namespace MonsterHunt.addons.easydebug
{
    [Tool]
    public partial class EasyDebugPlugin : EditorPlugin
    {
        public static bool DebugEnable = true;

        private const string AutoloadName = "EasyDebug";
        private const string AutoloadSceneName = "EasyDebug.cs";
        private const string SettingPathDebugEnable = "addons/monsterhunt_easydebug/debugging_enabled";
        private const string SettingPathDockPosX = "addons/monsterhunt_easydebug/dock_pos_x";
        private const string SettingPathDockPosY = "addons/monsterhunt_easydebug/dock_pos_y";
        private const string SettingPathDockSizeX = "addons/monsterhunt_easydebug/dock_size_x";
        private const string SettingPathDockSizeY = "addons/monsterhunt_easydebug/dock_size_y";


        private VBoxContainer _dock;
        private CheckBox _checkbox;

        public override void _EnterTree()
        {
            // --- Load DebugEnable setting ---
            if (ProjectSettings.HasSetting(SettingPathDebugEnable))
            {
                DebugEnable = (bool)ProjectSettings.GetSetting(SettingPathDebugEnable);
            }
            else
            {
                ProjectSettings.SetSetting(SettingPathDebugEnable, DebugEnable);
                ProjectSettings.Save();
            }

            // --- Create UI Programmatically ---
            _dock = new VBoxContainer();
            _dock.Name = "EasyDebug";
            _dock.CustomMinimumSize = new Vector2(200, 100); // Give it a sensible minimum size

            _checkbox = new CheckBox();
            _checkbox.Text = "Enable Debugging";
            _checkbox.ButtonPressed = DebugEnable;
            _checkbox.Toggled += OnEnableDebuggingToggled;

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 4);
            margin.AddThemeConstantOverride("margin_top", 4);
            margin.AddThemeConstantOverride("margin_right", 4);
            margin.AddThemeConstantOverride("margin_bottom", 4);

            _dock.AddChild(margin);
            margin.AddChild(_checkbox);

            AddControlToDock(DockSlot.LeftUl, _dock); // Add the dock to the editor

            // --- Attempt to load and set dock position/size if it's not automatically handled ---
            // NOTE: Godot usually handles this automatically for controls added to docks.
            // This manual loading is generally not needed for editor docks.
            if (ProjectSettings.HasSetting(SettingPathDockPosX) && ProjectSettings.HasSetting(SettingPathDockPosY))
            {
                float x = (float)ProjectSettings.GetSetting(SettingPathDockPosX);
                float y = (float)ProjectSettings.GetSetting(SettingPathDockPosY);
                _dock.Position = new Vector2(x, y);
            }
            if (ProjectSettings.HasSetting(SettingPathDockSizeX) && ProjectSettings.HasSetting(SettingPathDockSizeY))
            {
                float sx = (float)ProjectSettings.GetSetting(SettingPathDockSizeX);
                float sy = (float)ProjectSettings.GetSetting(SettingPathDockSizeY);
                _dock.Size = new Vector2(sx, sy);
            }

            // --- Connect to size and position changed signals to save them ---
            // NOTE: These signals are not directly available on Control for editor dock position/size changes.
            // EditorPlugin doesn't expose a direct signal for dock position/size changes.
            // If manual saving was necessary, you'd save these in _ExitTree or on editor shutdown.
            // For now, we'll rely on Godot's built-in dock layout saving.


            UpdateAutoloadState();
            GD.Print("EasyDebug Addon: Plugin Enabled. Debugging is " + (DebugEnable ? "ON." : "OFF."));
        }

        public override void _ExitTree()
        {
            if (_dock != null)
            {
                // --- Save dock position and size here if Godot wasn't handling it automatically ---
                // This approach ensures the last known position/size is saved when the plugin is disabled or editor closes.
                // Again, Godot should handle this for dock controls, but this is how you'd do it manually.
                ProjectSettings.SetSetting(SettingPathDockPosX, _dock.Position.X);
                ProjectSettings.SetSetting(SettingPathDockPosY, _dock.Position.Y);
                ProjectSettings.SetSetting(SettingPathDockSizeX, _dock.Size.X);
                ProjectSettings.SetSetting(SettingPathDockSizeY, _dock.Size.Y);
                ProjectSettings.Save();


                if (IsInstanceValid(_checkbox))
                {
                    _checkbox.Toggled -= OnEnableDebuggingToggled;
                }
                RemoveControlFromDocks(_dock);
                _dock.Free();
                _dock = null;
                _checkbox = null;
            }
            RemoveAutoloadSingleton(AutoloadName);
            GD.Print("EasyDebug Addon: Plugin Disabled, autoload removed.");
        }

        private void OnEnableDebuggingToggled(bool isPressed)
        {
            DebugEnable = isPressed;
            UpdateAutoloadState();

            ProjectSettings.SetSetting(SettingPathDebugEnable, DebugEnable);
            ProjectSettings.Save();
            GD.Print($"EasyDebug setting changed. Debugging is now {(DebugEnable ? "ON." : "OFF.")}");
        }

        private void UpdateAutoloadState()
        {
            Script currentPluginScript = GetScript().As<Script>();
            if (currentPluginScript == null)
            {
                GD.PrintErr("EasyDebugPlugin: Could not get own script resource. Autoload state not changed.");
                return;
            }

            string addonPath = currentPluginScript.ResourcePath.GetBaseDir();
            string pathToAutoloadScene = addonPath.PathJoin(AutoloadSceneName);

            bool autoloadIsCurrentlyRegistered = GetTree().Root.GetNodeOrNull(AutoloadName) != null;

            if (DebugEnable)
            {
                if (!autoloadIsCurrentlyRegistered)
                {
                    AddAutoloadSingleton(AutoloadName, pathToAutoloadScene);
                    GD.Print($"EasyDebug: Autoload '{AutoloadName}' added from '{pathToAutoloadScene}'.");
                }
            }
            else
            {
                if (autoloadIsCurrentlyRegistered)
                {
                    RemoveAutoloadSingleton(AutoloadName);
                    GD.Print($"EasyDebug: Autoload '{AutoloadName}' removed.");
                }
            }
        }
    }
}
#endif