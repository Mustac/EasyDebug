using Godot;
using System;
using System.Linq.Expressions;
using System.Reflection; // Keep this if you use it elsewhere, though not strictly needed for this file's current logic

// Ensure this namespace matches your other addon scripts
namespace MonsterHunt.addons.easydebug
{
    public static class EasyDebugExtensions
    {
        /// <summary>
        /// Tracks properties of a Node using a full TrackOptions object for detailed configuration.
        /// </summary>
        public static void Track<T>(
            this T target,
            TrackOptions options,
            System.Linq.Expressions.Expression<Func<T, object>> propertiesSelector) where T : Node
        {
            if (EasyDebug.Instance == null) return;
            if (target == null) { GD.PrintErr("EasyDebugExtensions.Track(): Target node cannot be null."); return; }
            if (options == null) { GD.PrintErr("EasyDebugExtensions.Track(): TrackOptions cannot be null."); return; }
            if (propertiesSelector == null) { GD.PrintErr("EasyDebugExtensions.Track(): Properties selector cannot be null."); return; }

            System.Linq.Expressions.Expression body = propertiesSelector.Body;
            if (body is System.Linq.Expressions.UnaryExpression unaryExpression &&
                unaryExpression.NodeType == System.Linq.Expressions.ExpressionType.Convert)
            {
                body = unaryExpression.Operand;
            }

            if (body is System.Linq.Expressions.NewExpression newExpression)
            {
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var argument = newExpression.Arguments[i];
                    string displayName = newExpression.Members[i].Name;

                    var valueLambda = System.Linq.Expressions.Expression.Lambda<Func<T, object>>(
                        System.Linq.Expressions.Expression.Convert(argument, typeof(object)),
                        propertiesSelector.Parameters
                    );

                    var compiledAccessor = valueLambda.Compile();
                    Func<object> valueAccessor = () => compiledAccessor(target);

                    var trackedProp = new TrackedProperty(target, options, displayName, valueAccessor);
                    EasyDebug.Instance.AddTrackedProperty(trackedProp);
                }
            }
            else
            {
                GD.PrintErr($"EasyDebug.Track() for category '{options.Category}' on node '{target.Name}' requires an anonymous object selector (e.g., x => new {{ x.MyProp }}).");
            }
        }

        /// <summary>
        /// Tracks properties of a Node using a specified category string.
        /// Default styling options from TrackOptions will be used.
        /// </summary>
        public static void Track<T>(
            this T target,
            string category,
            System.Linq.Expressions.Expression<Func<T, object>> propertiesSelector) where T : Node
        {
            if (target == null) { GD.PrintErr("EasyDebugExtensions.Track(): Target node cannot be null."); return; }

            var options = new TrackOptions();
            if (!string.IsNullOrEmpty(category))
            {
                options.Category = category;
            }

            target.Track(options, propertiesSelector);
        }

        /// <summary>
        /// Tracks properties of a Node using default TrackOptions.
        /// The category will default to "Default" or as configured in TrackOptions.
        /// </summary>
        public static void Track<T>(
            this T target,
            System.Linq.Expressions.Expression<Func<T, object>> propertiesSelector) where T : Node
        {
            if (target == null) { GD.PrintErr("EasyDebugExtensions.Track(): Target node cannot be null."); return; }

            // Create a new TrackOptions instance, which will use all its default values.
            var options = new TrackOptions();

            // Call the main Track method with the default options
            target.Track(options, propertiesSelector);
        }
    }
}