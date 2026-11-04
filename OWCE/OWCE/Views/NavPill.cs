using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    /// <summary>
    /// A bottom navigation bar shaped like a pill. It holds a few big items (an icon above a label); the selected one sits
    /// in a tinted pill of its own. Made to be easy to hit while riding.
    /// </summary>
    public class NavPill : Border
    {
        sealed class Item
        {
            public Border Highlight;
            public BoxView Tint;
            public Microsoft.Maui.Controls.Shapes.Path Icon;
            public Label Label;
        }

        readonly Grid _grid;
        readonly List<Item> _items = new List<Item>();
        int _selectedIndex = -1;

        /// <summary>Raised when an item is tapped, with its index. The bar does not change the selection by itself: set SelectedIndex.</summary>
        public event Action<int> ItemTapped;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                for (var i = 0; i < _items.Count; ++i)
                {
                    ApplyState(_items[i], i == value);
                }
            }
        }

        public NavPill()
        {
            HeightRequest = 76;
            StrokeThickness = 1.5;
            Padding = new Thickness(6);
            StrokeShape = new RoundRectangle() { CornerRadius = 38 };
            this.SetDynamicResource(BackgroundColorProperty, "SurfaceColor");
            this.SetDynamicResource(StrokeProperty, "DividerBrush");

            _grid = new Grid() { ColumnSpacing = 4 };
            Content = _grid;
        }

        /// <param name="iconPath">A vector icon, see Icons.</param>
        public void AddItem(string text, string iconPath)
        {
            var index = _items.Count;

            var item = new Item();
            // The tint is a layer of the accent colour at a low strength, so it follows the theme.
            item.Tint = new BoxView() { Opacity = 0 };
            item.Tint.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");
            item.Highlight = new Border()
            {
                StrokeThickness = 0,
                Padding = new Thickness(0),
                StrokeShape = new RoundRectangle() { CornerRadius = 32 },
                BackgroundColor = Colors.Transparent,
                Content = item.Tint,
            };
            item.Highlight.SetDynamicResource(Border.StrokeProperty, "AccentBrush");
            item.Icon = Icons.Create(iconPath, "MutedTextBrush", 26);
            item.Label = new Label()
            {
                Text = text,
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            var stack = new VerticalStackLayout()
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            stack.Children.Add(item.Icon);
            stack.Children.Add(item.Label);

            var cell = new Grid() { BackgroundColor = Colors.Transparent };
            cell.Children.Add(item.Highlight);
            cell.Children.Add(stack);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (sender, e) =>
            {
                try
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                }
                catch (Exception)
                {
                    // Not every device can do this.
                }
                ItemTapped?.Invoke(index);
            };
            cell.GestureRecognizers.Add(tap);

            _items.Add(item);
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _grid.Add(cell, index, 0);

            ApplyState(item, index == _selectedIndex);
        }

        static void ApplyState(Item item, bool selected)
        {
            if (selected)
            {
                item.Tint.Opacity = 0.18;
                item.Highlight.StrokeThickness = 1.5;
                item.Icon.SetDynamicResource(Microsoft.Maui.Controls.Shapes.Shape.FillProperty, "AccentBrush");
                item.Label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");
                item.Label.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                item.Tint.Opacity = 0;
                item.Highlight.StrokeThickness = 0;
                item.Icon.SetDynamicResource(Microsoft.Maui.Controls.Shapes.Shape.FillProperty, "MutedTextBrush");
                item.Label.SetDynamicResource(Label.TextColorProperty, "MutedTextColor");
                item.Label.FontAttributes = FontAttributes.None;
            }
        }
    }
}
