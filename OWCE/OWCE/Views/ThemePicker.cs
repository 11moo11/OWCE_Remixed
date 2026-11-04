using Microsoft.Maui.Controls.Shapes;
using OWCE.Themes;

namespace OWCE.Views
{
    /// <summary>
    /// Shows every theme as a small preview (its background and a few panel colours) with its name. Tap one to select it.
    /// </summary>
    public class ThemePicker : ContentView
    {
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
            var scrollX = _scroll?.ScrollX ?? 0;
            Build();
            if (scrollX > 0)
            {
                Dispatcher.Dispatch(async () => { try { await _scroll.ScrollToAsync(scrollX, 0, false); } catch (Exception) { } });
            }
            if (SelectedThemeId != null)
            {
                Select(SelectedThemeId);
            }
        }

        readonly Dictionary<string, View> _swatches = new Dictionary<string, View>();
        ScrollView _scroll;

        void Build()
        {
            _previews.Clear();
            _swatches.Clear();

            // A carousel: the themes are in one row that scrolls sideways.
            var row = new HorizontalStackLayout() { Spacing = 14, Padding = new Thickness(12, 4) };
            foreach (var theme in ThemeManager.AllSpecs)
            {
                var swatch = BuildSwatch(theme);
                _swatches[theme.Id] = swatch;
                row.Children.Add(swatch);
            }

            _scroll = new ScrollView()
            {
                Orientation = ScrollOrientation.Horizontal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                Content = row,
            };
            Content = _scroll;
        }

        /// <summary>Marks a theme as selected. Does not raise <see cref="ThemeSelected"/>.</summary>
        public void Select(string id)
        {
            SelectedThemeId = id;
            ScrollToSelected();

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

        // Slides the carousel so the selected theme is in view.
        void ScrollToSelected()
        {
            if (SelectedThemeId == null || _scroll == null || _swatches.TryGetValue(SelectedThemeId, out var swatch) == false)
            {
                return;
            }

            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await Task.Delay(50);
                    await _scroll.ScrollToAsync(swatch, ScrollToPosition.Center, true);
                }
                catch (Exception)
                {
                }
            });
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

        View BuildSwatch(ThemeSpec theme)
        {
            // Read the theme's colours without applying it.
            // The theme in use is drawn with the changes made to it so far.
            var shown = theme.Id == ThemeManager.Current.Id ? ThemeManager.Current : theme;
            var colors = ThemeManager.CreateDictionary(shown);
            Color Colour(string key) => colors.TryGetValue(key, out var value) && value is Color color ? color : Colors.Gray;

            Border Block(string key) => new Border()
            {
                BackgroundColor = Colour(key),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle() { CornerRadius = Math.Min(6, ThemeManager.CornerRadiusFor(shown.Corners) / 4) },
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
                WidthRequest = 112,
                HeightRequest = 88,
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
