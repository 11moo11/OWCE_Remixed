using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    /// <summary>A row of big options, one of them selected (the selected one is filled with the accent colour).</summary>
    public class SegmentedControl : Border
    {
        sealed class Segment
        {
            public BoxView Fill;
            public Label Label;
        }

        readonly Grid _grid = new Grid() { ColumnSpacing = 0 };
        readonly List<Segment> _segments = new List<Segment>();
        int _selectedIndex = -1;

        /// <summary>Raised when the user taps an option, with its index.</summary>
        public event Action<int> SelectionChanged;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                for (var i = 0; i < _segments.Count; ++i)
                {
                    Apply(_segments[i], i == value);
                }
            }
        }

        public SegmentedControl()
        {
            HeightRequest = 48;
            Padding = new Thickness(3);
            StrokeThickness = 1.5;
            StrokeShape = new RoundRectangle() { CornerRadius = 24 };
            BackgroundColor = Colors.Transparent;
            this.SetDynamicResource(StrokeProperty, "DividerBrush");
            Content = _grid;
        }

        public void SetItems(params string[] labels)
        {
            _grid.Children.Clear();
            _grid.ColumnDefinitions.Clear();
            _segments.Clear();

            for (var i = 0; i < labels.Length; ++i)
            {
                var index = i;
                var fill = new BoxView() { CornerRadius = 21, InputTransparent = true };
                fill.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");

                var label = new Label()
                {
                    Text = labels[i],
                    FontSize = 15,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    InputTransparent = true,
                };

                var cell = new Grid() { BackgroundColor = Colors.Transparent, Children = { fill, label } };
                cell.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(cell, 0.94);
                        SelectedIndex = index;
                        SelectionChanged?.Invoke(index);
                    }),
                });

                var segment = new Segment() { Fill = fill, Label = label };
                _segments.Add(segment);
                _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _grid.Add(cell, i, 0);
                Apply(segment, i == _selectedIndex);
            }
        }

        static void Apply(Segment segment, bool selected)
        {
            segment.Fill.Opacity = selected ? 1 : 0;
            segment.Label.FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
            segment.Label.SetDynamicResource(Label.TextColorProperty, selected ? "OnAccentColor" : "OnSurfaceColor");
        }
    }
}
