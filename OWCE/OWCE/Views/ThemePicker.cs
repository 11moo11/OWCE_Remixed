using Microsoft.Maui.Controls.Shapes;
using OWCE.Themes;

namespace OWCE.Views
{
    /// <summary>
    /// Shows every theme as a small preview (its background and a few panel colours) with its name. Tap one to select it.
    /// </summary>
    public class ThemePicker : ContentView
    {
        const int Columns = 3;

        readonly Dictionary<string, Border> _previews = new Dictionary<string, Border>();

        /// <summary>The id of the selected theme (see ThemeManager).</summary>
        public string SelectedThemeId { get; private set; }

        /// <summary>Raised when the user taps a theme.</summary>
        public event Action<string> ThemeSelected;

        public ThemePicker()
        {
            Build();
        }

        /// <summary>Draws the previews again (a theme such as Material can change its colours).</summary>
        public void Rebuild()
        {
            Build();
            if (SelectedThemeId != null)
            {
                Select(SelectedThemeId);
            }
        }

        void Build()
        {
            _previews.Clear();
            var grid = new Grid() { RowSpacing = 16, ColumnSpacing = 8 };
            for (var i = 0; i < Columns; ++i)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }

            var themes = ThemeManager.Themes;
            for (var i = 0; i < themes.Count; ++i)
            {
                if (i % Columns == 0)
                {
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }

                var swatch = BuildSwatch(themes[i]);
                grid.Add(swatch, i % Columns, i / Columns);
            }

            Content = grid;
        }

        /// <summary>Marks a theme as selected. Does not raise <see cref="ThemeSelected"/>.</summary>
        public void Select(string id)
        {
            SelectedThemeId = id;

            foreach (var item in _previews)
            {
                var selected = item.Key == id;
                if (selected)
                {
                    item.Value.SetDynamicResource(Border.StrokeProperty, "AccentBrush");
                }
                else
                {
                    item.Value.RemoveDynamicResource(Border.StrokeProperty);
                    item.Value.Stroke = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        // The tapped preview shrinks and springs back.
        static async Task PopAsync(View view)
        {
            try
            {
                view.CancelAnimations();
                view.Scale = 0.86;
                await view.ScaleToAsync(1, 500, Easing.SpringOut);
            }
            catch (Exception)
            {
            }
        }

        View BuildSwatch(ThemeInfo theme)
        {
            // Read the theme's colours without applying it.
            var colors = theme.Create();
            Color Colour(string key) => colors.TryGetValue(key, out var value) && value is Color color ? color : Colors.Gray;

            Border Block(string key) => new Border()
            {
                BackgroundColor = Colour(key),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle() { CornerRadius = 6 },
            };

            // A tiny picture of the dashboard: four panels on the theme's background.
            var panels = new Grid()
            {
                RowDefinitions = new RowDefinitionCollection() { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Star) },
                ColumnDefinitions = new ColumnDefinitionCollection() { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                RowSpacing = 4,
                ColumnSpacing = 4,
                Padding = new Thickness(8),
            };
            panels.Add(Block("SpeedPanel"), 0, 0);
            panels.Add(Block("BatteryHeader"), 1, 0);
            panels.Add(Block("TempPanel"), 0, 1);
            panels.Add(Block("LightsPanel"), 1, 1);

            var preview = new Border()
            {
                Content = panels,
                WidthRequest = 88,
                HeightRequest = 72,
                BackgroundColor = Colour("BackgroundGradientStart"),
                StrokeThickness = 3,
                Stroke = new SolidColorBrush(Colors.Transparent),
                StrokeShape = new RoundRectangle() { CornerRadius = 20 },
                HorizontalOptions = LayoutOptions.Center,
            };
            _previews[theme.Id] = preview;

            var name = new Label()
            {
                Text = theme.Name,
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            name.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");

            var swatch = new VerticalStackLayout() { Spacing = 8, Children = { preview, name } };
            swatch.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    Motion.Tick();
                    _ = PopAsync(preview);
                    Select(theme.Id);
                    ThemeSelected?.Invoke(theme.Id);
                }),
            });
            return swatch;
        }
    }
}
