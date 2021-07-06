using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Nonogram.WPF.Controls
{
    public class UniformGrid : System.Windows.Controls.Primitives.UniformGrid
    {
        public Orientation Orientation { get; set; }
        private static readonly FieldInfo _rowsField = typeof(System.Windows.Controls.Primitives.UniformGrid).GetField("_rows", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;
        private static readonly FieldInfo _columnsField = typeof(System.Windows.Controls.Primitives.UniformGrid).GetField("_columns", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;


        /// <inheritdoc/>
        /// <remarks>Adapted from the base class at <a href="https://source.dot.net/PresentationFramework/System/Windows/Controls/Primitives/UniformGrid.cs.html#40ad07eee5c6d874">source.dot.net</a></remarks>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Orientation is Orientation.Vertical)
                return base.ArrangeOverride(arrangeSize);

            var childBounds = new Rect(0, 0, arrangeSize.Width / (int)_columnsField.GetValue(this)!, arrangeSize.Height / (int)_rowsField.GetValue(this)!);
            var yStep = childBounds.Height;
            var yBound = arrangeSize.Height - 1.0;

            childBounds.Y += childBounds.Height * FirstColumn;

            // Arrange and Position each child to the same cell size
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(childBounds);

                // only advance to the next grid cell if the child was not collapsed
                if (child.Visibility != Visibility.Collapsed)
                {
                    childBounds.Y += yStep;
                    if (childBounds.Y >= yBound)
                    {
                        childBounds.X += childBounds.Width;
                        childBounds.Y = 0;
                    }
                }
            }

            return arrangeSize;
        }
    }
}
