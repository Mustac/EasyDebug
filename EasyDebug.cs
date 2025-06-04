using Godot;
using System;
using System.Collections.Generic;

// Ensure this namespace matches your other addon scripts
namespace MonsterHunt.addons.easydebug
{
    public partial class EasyDebug : Node
    {
        public static EasyDebug Instance { get; private set; }

        private readonly List<TrackedProperty> _trackedProperties = new List<TrackedProperty>();

        private Window _window; // This will be our debug window
        private Tree _propertyTree;
        private PanelContainer _panelContainer;
        private MarginContainer _marginContainer;
        private Control _treeParentControl;

        private readonly Dictionary<string, TreeItem> _categoryTreeItems = new Dictionary<string, TreeItem>();

        // ProjectSettings keys for window persistence
        private const string BaseSettingPath = "addons/monsterhunt_easydebug/window/";
        private const string SizeXKey = BaseSettingPath + "size_x";
        private const string SizeYKey = BaseSettingPath + "size_y";
        private const string WindowSideKey = BaseSettingPath + "side"; // "left" or "right"
        private const string SettingsSavedKey = BaseSettingPath + "settings_saved"; // To track if initial settings (like side) were saved

        private const int WindowEdgeOffset = 10; // Pixels between main window and debug window

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                GD.PrintErr("EasyDebug: Another instance detected. Destroying this one.");
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("EasyDebug Singleton Initialized.");

            // --- Window Setup ---
            _window = new Window();
            _window.Name = "EasyDebugWindow";
            AddChild(_window);
            GD.Print("EasyDebug: Created debug Window programmatically.");
            // --- End of Window Setup ---

            // Common window setup
            _window.Title = "EasyDebug";
            // Set an initial size. This will be overridden by loaded settings.
            _window.Size = new Vector2I(450, 700);
            _window.Visible = true;
            _window.AlwaysOnTop = true;
            _window.InitialPosition = Window.WindowInitialPosition.Absolute; // Ensure we can set position manually
            _window.Transient = true; // Makes it a separate OS window

            // Connect signals for saving only the size, and for closing the window.
            _window.Connect(Window.SignalName.SizeChanged, Callable.From(OnWindowSizeChanged));
            _window.Connect(Window.SignalName.CloseRequested, Callable.From(OnWindowCloseRequested));

            // UI Hierarchy Setup (No changes here, remains the same)
            _panelContainer = new PanelContainer();
            _panelContainer.Name = "ED_PanelContainer";
            _panelContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _window.AddChild(_panelContainer);

            _marginContainer = new MarginContainer();
            _marginContainer.Name = "ED_MarginContainer";
            _marginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _marginContainer.AddThemeConstantOverride("margin_left", 5);
            _marginContainer.AddThemeConstantOverride("margin_top", 5);
            _marginContainer.AddThemeConstantOverride("margin_right", 5);
            _marginContainer.AddThemeConstantOverride("margin_bottom", 5);
            _panelContainer.AddChild(_marginContainer);

            _treeParentControl = new Control();
            _treeParentControl.Name = "ED_TreeParentControl";
            _treeParentControl.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _marginContainer.AddChild(_treeParentControl);

            _propertyTree = new Tree();
            _propertyTree.Name = "PropertyTree";
            _propertyTree.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _propertyTree.Columns = 2;
            _propertyTree.SetColumnTitle(0, "Property");
            _propertyTree.SetColumnTitle(1, "Value");
            _propertyTree.SetColumnExpandRatio(0, 2);
            _propertyTree.SetColumnExpandRatio(1, 3);
            _propertyTree.HideRoot = true;
            _treeParentControl.AddChild(_propertyTree);

            LoadWindowSettingsAndSetInitialPosition();
        }

        public override void _ExitTree()
        {
            // Disconnect signals
            if (IsInstanceValid(_window))
            {
                if (_window.IsConnected(Window.SignalName.SizeChanged, Callable.From(OnWindowSizeChanged)))
                    _window.Disconnect(Window.SignalName.SizeChanged, Callable.From(OnWindowSizeChanged));
                if (_window.IsConnected(Window.SignalName.CloseRequested, Callable.From(OnWindowCloseRequested)))
                    _window.Disconnect(Window.SignalName.CloseRequested, Callable.From(OnWindowCloseRequested));

                // Save settings when the autoload exits (e.g., game quits or plugin is disabled)
                SaveWindowSettings();
            }
            if (Instance == this) Instance = null;
            GD.Print("EasyDebug Singleton Exiting Tree.");
        }

        private void LoadWindowSettingsAndSetInitialPosition()
        {
            // Load saved size, if any
            int sizeX = (int)ProjectSettings.GetSetting(SizeXKey, _window.Size.X);
            int sizeY = (int)ProjectSettings.GetSetting(SizeYKey, _window.Size.Y);
            _window.Size = new Vector2I(sizeX, sizeY);
            GD.Print($"EasyDebug: Loaded window size: {_window.Size}");

            // Load saved side preference (default to "left")
            string side = (string)ProjectSettings.GetSetting(WindowSideKey, "left");
            if (side.ToLower() != "left" && side.ToLower() != "right")
            {
                GD.Print($"EasyDebug: Invalid side '{side}', defaulting to 'left'.");
                side = "left";
            }

            // Get main window properties
            Vector2I mainGameWindowPosition = DisplayServer.WindowGetPosition();
            Vector2I mainGameWindowSize = DisplayServer.WindowGetSize();

            int targetX;
            // Calculate target X based on the chosen side
            if (side.ToLower() == "right")
            {
                targetX = mainGameWindowPosition.X + mainGameWindowSize.X + WindowEdgeOffset;
            }
            else // Default to left
            {
                targetX = mainGameWindowPosition.X - _window.Size.X - WindowEdgeOffset;
            }

            // Calculate target Y to align vertical centers
            int targetY = mainGameWindowPosition.Y + (mainGameWindowSize.Y / 2) - (_window.Size.Y / 2);

            _window.Position = new Vector2I(targetX, targetY);
            GD.Print($"EasyDebug: Window positioned to the {side} of the main window, centered vertically.");

            // Always save the current size and side preference after setting the initial position.
            // This ensures that the size is saved on the first run, and the side preference is consistent.
            SaveWindowSettings(side);
        }

        private void SaveWindowSettings(string sideToSave = null)
        {
            if (!IsInstanceValid(_window)) return;

            ProjectSettings.SetSetting(SizeXKey, _window.Size.X);
            ProjectSettings.SetSetting(SizeYKey, _window.Size.Y);

            if (sideToSave != null)
            {
                ProjectSettings.SetSetting(WindowSideKey, sideToSave);
            }
            ProjectSettings.SetSetting(SettingsSavedKey, true);
            ProjectSettings.Save();
        }

        private void OnWindowSizeChanged()
        {
            SaveWindowSettings();
        }

        private void OnWindowCloseRequested()
        {
            SaveWindowSettings();
            if (IsInstanceValid(_window)) _window.Visible = false;
            GD.Print("EasyDebug: Debug window closed, settings saved.");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!EasyDebugPlugin.DebugEnable)
            {
                if (_window != null && IsInstanceValid(_window) && _window.Visible) _window.Visible = false;
                if (_trackedProperties.Count > 0)
                {
                    _trackedProperties.Clear();
                    if (IsInstanceValid(_propertyTree)) _propertyTree.Clear();
                    _categoryTreeItems.Clear();
                }
                return;
            }
            else
            {
                if (_window != null && IsInstanceValid(_window) && !_window.Visible)
                {
                    _window.Visible = true;
                    LoadWindowSettingsAndSetInitialPosition();
                }
            }

            if (_window == null || !IsInstanceValid(_window) || !_window.Visible) return;

            // Update tracked properties
            for (int i = _trackedProperties.Count - 1; i >= 0; i--)
            {
                var prop = _trackedProperties[i];
                if (prop.NodeInstance.TryGetTarget(out Node targetNode) && IsInstanceValid(targetNode))
                {
                    if (prop.UiItem == null || !IsInstanceValid(prop.UiItem))
                    {
                        AddTrackedPropertyToUi(prop);
                        if (prop.UiItem == null || !IsInstanceValid(prop.UiItem)) continue;
                    }
                    try
                    {
                        object propValue = prop.ValueAccessor.Invoke();
                        string valueText;

                        if (prop.Options.RoundingDigits.HasValue)
                        {
                            string format = $"F{prop.Options.RoundingDigits.Value}";
                            if (propValue is float fVal) valueText = fVal.ToString(format);
                            else if (propValue is double dVal) valueText = dVal.ToString(format);
                            else if (propValue is Vector2 v2Val) valueText = $"X: {v2Val.X.ToString(format)}, Y: {v2Val.Y.ToString(format)}";
                            else if (propValue is Vector3 v3Val) valueText = $"X: {v3Val.X.ToString(format)}, Y: {v3Val.Y.ToString(format)}, Z: {v3Val.Z.ToString(format)}";
                            else valueText = propValue?.ToString() ?? "null";
                        }
                        else
                        {
                            valueText = propValue?.ToString() ?? "null";
                        }
                        prop.UiItem.SetText(1, valueText);

                        if (prop.Options.TextColor.HasValue)
                        {
                            prop.UiItem.SetCustomColor(1, prop.Options.TextColor.Value);
                        }
                        else { prop.UiItem.ClearCustomColor(1); }

                        if (prop.Options.BackgroundColor.HasValue)
                        {
                            prop.UiItem.SetCustomBgColor(0, prop.Options.BackgroundColor.Value);
                            prop.UiItem.SetCustomBgColor(1, prop.Options.BackgroundColor.Value);
                        }
                        else { prop.UiItem.ClearCustomBgColor(0); prop.UiItem.ClearCustomBgColor(1); }
                    }
                    catch (Exception e)
                    {
                        if (prop.UiItem != null && IsInstanceValid(prop.UiItem))
                        {
                            prop.UiItem.SetText(1, "ERROR");
                            prop.UiItem.SetCustomColor(1, Colors.Red);
                        }
                        GD.PrintErr($"[EasyDebug] Error for {prop.Options.Category} | {targetNode.Name}.{prop.PropertyName}: {e.Message}");
                    }
                }
                else
                {
                    // Clean up UI for removed/invalidated nodes
                    if (prop.UiItem != null && IsInstanceValid(prop.UiItem))
                    {
                        TreeItem parentItem = prop.UiItem.GetParent();
                        parentItem?.RemoveChild(prop.UiItem);
                        prop.UiItem.Free();

                        if (parentItem != null && IsInstanceValid(parentItem) && parentItem.GetChildCount() == 0)
                        {
                            string categoryKey = parentItem.GetText(0);
                            if (_categoryTreeItems.ContainsKey(categoryKey))
                            {
                                _categoryTreeItems.Remove(categoryKey);
                                parentItem.GetParent()?.RemoveChild(parentItem);
                                parentItem.Free();
                            }
                        }
                    }
                    _trackedProperties.RemoveAt(i);
                }
            }
        }

        public void AddTrackedProperty(TrackedProperty trackedProp)
        {
            if (trackedProp != null)
            {
                _trackedProperties.Add(trackedProp);
                if (IsInstanceValid(_propertyTree)) AddTrackedPropertyToUi(trackedProp);
            }
        }

        private void AddTrackedPropertyToUi(TrackedProperty prop)
        {
            if (!prop.NodeInstance.TryGetTarget(out Node targetNode) || !IsInstanceValid(targetNode)) return;
            TreeItem root = _propertyTree.GetRoot() ?? _propertyTree.CreateItem();
            if (!_categoryTreeItems.TryGetValue(prop.Options.Category, out TreeItem categoryItem) || !IsInstanceValid(categoryItem))
            {
                categoryItem = _propertyTree.CreateItem(root);
                categoryItem.SetText(0, prop.Options.Category);
                categoryItem.SetSelectable(0, false); categoryItem.SetSelectable(1, false);
                if (prop.Options.CategoryTextColor.HasValue) categoryItem.SetCustomColor(0, prop.Options.CategoryTextColor.Value);
                if (prop.Options.CategoryBackgroundColor.HasValue) categoryItem.SetCustomBgColor(0, prop.Options.CategoryBackgroundColor.Value);
                _categoryTreeItems[prop.Options.Category] = categoryItem;
            }
            // --- CHANGE HERE ---
            string fullPropertyName = prop.PropertyName; // Display just the property name
            // --- END CHANGE ---

            bool uiExists = false; TreeItem propertyUiItemToUse = null;
            if (prop.UiItem != null && IsInstanceValid(prop.UiItem)) { uiExists = true; propertyUiItemToUse = prop.UiItem; }
            else { for (int idx = 0; idx < categoryItem.GetChildCount(); idx++) { var child = categoryItem.GetChild(idx); if (child.GetText(0) == fullPropertyName) { propertyUiItemToUse = child; uiExists = true; break; } } }
            if (!uiExists) { propertyUiItemToUse = _propertyTree.CreateItem(categoryItem); propertyUiItemToUse.SetText(0, fullPropertyName); propertyUiItemToUse.SetText(1, "Loading..."); }
            if (prop.Options.TextColor.HasValue) propertyUiItemToUse.SetCustomColor(0, prop.Options.TextColor.Value); else propertyUiItemToUse.ClearCustomColor(0);
            if (prop.Options.BackgroundColor.HasValue) propertyUiItemToUse.SetCustomBgColor(0, prop.Options.BackgroundColor.Value); else propertyUiItemToUse.ClearCustomBgColor(0);
            prop.UiItem = propertyUiItemToUse;
        }
    }
}