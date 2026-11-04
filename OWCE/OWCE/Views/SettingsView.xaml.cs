using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OWCE.Themes;

namespace OWCE.Views
{
    /// <summary>
    /// The settings, with the About section at the bottom. Every change is saved as soon as it is made.
    /// Used as a tab on the board page, and by the settings page opened from the board list.
    /// </summary>
    public partial class SettingsView : ContentView
    {
        public string VersionString => $"{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})";

        /// <summary>The big "Settings" heading. Hidden where the page already shows one.</summary>
        public bool ShowTitle
        {
            get => TitleLabel.IsVisible;
            set => TitleLabel.IsVisible = value;
        }

        IAsyncRelayCommand _faqCommand;
        public IAsyncRelayCommand FAQCommand => _faqCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://owce.app/faq/"));

        IAsyncRelayCommand _sourceCodeCommand;
        public IAsyncRelayCommand SourceCodeCommand => _sourceCodeCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://github.com/OnewheelCommunityEdition/OWCE_App"));

        IAsyncRelayCommand _reportProblemCommand;
        public IAsyncRelayCommand ReportProblemCommand => _reportProblemCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://github.com/OnewheelCommunityEdition/OWCE_App/issues/new/choose"));

        IAsyncRelayCommand _requestFeatureCommand;
        public IAsyncRelayCommand RequestFeatureCommand => _requestFeatureCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://github.com/OnewheelCommunityEdition/OWCE_App/issues/new/choose"));

        IAsyncRelayCommand _redditCommand;
        public IAsyncRelayCommand RedditCommand => _redditCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://reddit.com/r/OWCE"));

        IAsyncRelayCommand _twitterCommand;
        public IAsyncRelayCommand TwitterCommand => _twitterCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://twitter.com/owceapp"));

        IAsyncRelayCommand _facebookPageCommand;
        public IAsyncRelayCommand FacebookPageCommand => _facebookPageCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://www.facebook.com/owceapp"));

        IAsyncRelayCommand _facebookGroupCommand;
        public IAsyncRelayCommand FacebookGroupCommand => _facebookGroupCommand ??= new AsyncRelayCommand(() => OpenUrlAsync("https://www.facebook.com/groups/owceappgroup"));

        bool _loading = true;

        public SettingsView()
        {
            InitializeComponent();

            ImperialSwitch.IsToggled = App.Current.MetricDisplay == false;
            AutoRecordSwitch.IsToggled = Preferences.Get("auto_ride_recording", false);

            ModeControl.SetItems("Light", "Dark");
            CornersControl.SetItems("Very round", "Round", "Sharp");
            DepthControl.SetItems("None", "Drop shadow", "Edge light");
            ModeControl.SelectionChanged += index => Change(spec => spec.Dark = index == 1);
            CornersControl.SelectionChanged += index => Change(spec => spec.Corners = (CornerStyle)index);
            DepthControl.SelectionChanged += index => Change(spec => spec.Depth = (DepthStyle)index);

            BuildFontChips();
            BuildRoleChips();
            BuildHueBar();
            ThemePicker.ThemeSelected += ThemePicker_ThemeSelected;
            LoadFromCurrent();

            _loading = false;
            BindingContext = this;
        }

        /// <summary>The first sections spring up into place one after another.</summary>
        public void PlayEntrance() => Motion.StaggerIn(ContentStack.Children.OfType<VisualElement>().Take(8), 55, 30);

        // ---- Theme builder ----

        static readonly string[] RoleNames = { "Primary", "Secondary", "Accent" };
        static readonly string[] RoleHints =
        {
            "Primary: highlights, selected items, and the speed and battery panels.",
            "Secondary: cards, buttons and the lights panel.",
            "Accent: the power and temperature panels.",
        };

        readonly List<Border> _seedSwatches = new List<Border>();
        readonly List<string> _seedHexes = new List<string>();
        readonly List<Border> _roleCircles = new List<Border>();
        readonly List<Border> _roleChips = new List<Border>();
        int _role = 0;
        DateTime _lastColourApply = DateTime.MinValue;

        static string GetRole(ThemeSpec spec, int role) => role == 0 ? spec.Primary : role == 1 ? spec.Secondary : spec.Accent;

        static void SetRole(ThemeSpec spec, int role, string hex)
        {
            if (role == 0)
            {
                spec.Primary = hex;
            }
            else if (role == 1)
            {
                spec.Secondary = hex;
            }
            else
            {
                spec.Accent = hex;
            }
        }

        // Changes the theme in use (keeping the rest of it as it is) and applies it.
        void Change(Action<ThemeSpec> edit)
        {
            if (_loading)
            {
                return;
            }

            var spec = ThemeManager.Current.Clone();
            edit(spec);
            ThemeManager.Apply(spec);
            ThemePicker.Rebuild();
            LoadFromCurrent();
        }

        void ThemePicker_ThemeSelected(string themeId)
        {
            ThemeManager.Apply(themeId);
            LoadFromCurrent();
        }

        // Shows the theme in use in all the controls.
        void LoadFromCurrent()
        {
            var spec = ThemeManager.Current;
            var wasLoading = _loading;
            _loading = true;

            ThemePicker.Select(spec.Id);

            ModeControl.SelectedIndex = spec.Dark ? 1 : 0;
            CornersControl.SelectedIndex = (int)spec.Corners;
            DepthControl.SelectedIndex = (int)spec.Depth;
            MarkSelectedFont();

            for (var i = 0; i < _roleCircles.Count; ++i)
            {
                _roleCircles[i].BackgroundColor = Color.FromArgb(GetRole(spec, i));
            }
            MarkRole();
            RebuildSwatches();

            var colour = Color.FromArgb(GetRole(spec, _role));
            HueSlider.Value = colour.GetHue() * 360;
            ShadeSlider.Value = Math.Clamp(colour.GetSaturation(), 0.15f, 0.95f);
            ResetColoursButton.IsVisible = ThemeManager.HasCustomColors(spec);

            StyleActions.IsVisible = ThemeManager.IsUserStyle(spec.Id);

            _loading = wasLoading;
        }

        readonly List<(BoxView Fill, Label Label, string Family)> _fontChips = new List<(BoxView, Label, string)>();

        void BuildFontChips()
        {
            foreach (var font in FontCatalog.Fonts)
            {
                var family = font.Family;

                // The chip is filled by its own layer (like the other options), shown when the font is selected.
                var fill = new BoxView() { CornerRadius = 20, InputTransparent = true };
                fill.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");

                // Each name is written in its own font.
                var label = new Label()
                {
                    Text = font.Name,
                    FontFamily = family,
                    FontSize = 16,
                    Margin = new Thickness(16, 10),
                    InputTransparent = true,
                    VerticalOptions = LayoutOptions.Center,
                };
                var chip = new Border()
                {
                    Margin = new Thickness(0, 0, 8, 8),
                    Padding = new Thickness(0),
                    StrokeThickness = 1.5,
                    BackgroundColor = Colors.Transparent,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 20 },
                    Content = new Grid() { Children = { fill, label } },
                };
                chip.SetDynamicResource(Border.StrokeProperty, "DividerBrush");
                chip.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(chip, 0.9);
                        Change(spec => spec.Font = family);
                    }),
                });

                _fontChips.Add((fill, label, family));
                FontChips.Children.Add(chip);
            }
        }

        void MarkSelectedFont()
        {
            var selected = ThemeManager.Current.Font ?? string.Empty;
            foreach (var (fill, label, family) in _fontChips)
            {
                var on = family == selected;
                fill.Opacity = on ? 1 : 0;
                label.SetDynamicResource(Label.TextColorProperty, on ? "OnAccentColor" : "OnSurfaceColor");
            }
        }

        // Three buttons, each with a circle of its current colour. The tapped one is the colour the swatches and sliders edit.
        void BuildRoleChips()
        {
            for (var i = 0; i < 3; ++i)
            {
                var role = i;

                var circle = new Border()
                {
                    WidthRequest = 32,
                    HeightRequest = 32,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    HorizontalOptions = LayoutOptions.Center,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 16 },
                    InputTransparent = true,
                };
                var label = new Label() { Text = RoleNames[i], FontSize = 14, HorizontalOptions = LayoutOptions.Center, InputTransparent = true };
                label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");

                var chip = new Border()
                {
                    Padding = new Thickness(6, 10),
                    StrokeThickness = 1.5,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 18 },
                    Content = new VerticalStackLayout() { Spacing = 6, Children = { circle, label }, InputTransparent = true },
                };
                chip.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(chip, 0.94);
                        _role = role;
                        LoadFromCurrent();
                    }),
                });

                _roleCircles.Add(circle);
                _roleChips.Add(chip);
                RoleRow.Add(chip, i, 0);
            }
        }

        void MarkRole()
        {
            RoleHintLabel.Text = RoleHints[_role];
            for (var i = 0; i < _roleChips.Count; ++i)
            {
                _roleChips[i].RemoveDynamicResource(Border.StrokeProperty);
                if (i == _role)
                {
                    _roleChips[i].SetDynamicResource(Border.StrokeProperty, "AccentBrush");
                }
                else
                {
                    _roleChips[i].SetDynamicResource(Border.StrokeProperty, "DividerBrush");
                }
            }
        }

        // The rainbow behind the hue slider.
        void BuildHueBar()
        {
            var rainbow = new LinearGradientBrush() { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
            for (var i = 0; i <= 12; ++i)
            {
                rainbow.GradientStops.Add(new GradientStop(Color.FromHsla(i / 12f, 0.75f, 0.5f), i / 12f));
            }
            HueBar.Background = rainbow;
        }

        // Ready made colours for the colour being edited. The first one is the colour the theme started with.
        void RebuildSwatches()
        {
            SeedSwatches.Children.Clear();
            _seedSwatches.Clear();
            _seedHexes.Clear();

            var preset = ThemeManager.Find(ThemeManager.Current.BaseId);
            var start = _role == 0 ? preset.Primary : _role == 1 ? preset.Secondary : preset.Accent;
            var choices = new[] { start }.Concat(MaterialPalette.SeedChoices).ToList();
            var current = Color.FromArgb(GetRole(ThemeManager.Current, _role)).ToRgbaHex();

            foreach (var hex in choices)
            {
                var color = Color.FromArgb(hex);
                var swatch = new Border()
                {
                    WidthRequest = 44,
                    HeightRequest = 44,
                    Margin = new Thickness(0, 0, 10, 10),
                    BackgroundColor = color,
                    StrokeThickness = 3,
                    Stroke = Brush.Transparent,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 22 },
                };
                if (color.ToRgbaHex() == current)
                {
                    swatch.SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
                }

                var role = _role;
                swatch.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(swatch, 0.8);
                        Change(spec => SetRole(spec, role, color.ToRgbaHex()));
                    }),
                });
                _seedSwatches.Add(swatch);
                SeedSwatches.Children.Add(swatch);
            }
        }

        Color SliderColour() => Color.FromHsla((float)(HueSlider.Value / 360), (float)ShadeSlider.Value, 0.5f);

        void ColourSliderChanged()
        {
            if (_loading)
            {
                return;
            }

            // The whole app is restyled for each step, so not on every single movement.
            if ((DateTime.UtcNow - _lastColourApply).TotalMilliseconds > 120)
            {
                _lastColourApply = DateTime.UtcNow;
                var spec = ThemeManager.Current.Clone();
                SetRole(spec, _role, SliderColour().ToRgbaHex());
                ThemeManager.Apply(spec);
                _roleCircles[_role].BackgroundColor = SliderColour();
            }
        }

        void ColourSliderFinished()
        {
            var role = _role;
            var colour = SliderColour().ToRgbaHex();
            Change(spec => SetRole(spec, role, colour));
        }

        void HueSlider_ValueChanged(object sender, ValueChangedEventArgs e) => ColourSliderChanged();

        void HueSlider_DragCompleted(object sender, EventArgs e) => ColourSliderFinished();

        void ShadeSlider_ValueChanged(object sender, ValueChangedEventArgs e) => ColourSliderChanged();

        void ShadeSlider_DragCompleted(object sender, EventArgs e) => ColourSliderFinished();

        void ResetColours_Clicked(object sender, EventArgs e)
        {
            Change(spec =>
            {
                var preset = ThemeManager.Find(spec.BaseId);
                spec.Dark = preset.Dark;
                spec.Primary = preset.Primary;
                spec.Secondary = preset.Secondary;
                spec.Accent = preset.Accent;
            });
        }

        static Page CurrentPage => App.Current.RootPage;

        async void SaveStyle_Tapped(object sender, EventArgs e)
        {
            var name = await CurrentPage.DisplayPromptAsync("Save style", "Give your style a name.", "Save", "Cancel", maxLength: 24, placeholder: "My style");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            ThemeManager.SaveAsNewStyle(name.Trim());
            ThemePicker.Rebuild();
            LoadFromCurrent();
        }

        void UpdateStyle_Tapped(object sender, EventArgs e)
        {
            ThemeManager.UpdateCurrentStyle();
            ThemePicker.Rebuild();
        }

        async void DeleteStyle_Tapped(object sender, EventArgs e)
        {
            var spec = ThemeManager.Current;
            if (await CurrentPage.DisplayAlertAsync("Delete style", $"Delete \"{spec.Name}\"?", "Delete", "Cancel"))
            {
                ThemeManager.DeleteStyle(spec.Id);
                ThemePicker.Rebuild();
                LoadFromCurrent();
            }
        }

        void ImperialSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            App.Current.MetricDisplay = !e.Value;
            Preferences.Set("metric_display", !e.Value);

            WeakReferenceMessenger.Default.Send(new UnitDisplayUpdatedMessage());
        }

        void AutoRecordSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            Preferences.Set("auto_ride_recording", e.Value);
        }

        static async Task OpenUrlAsync(string url)
        {
            // Try launch browser, but if we can't launch internal browser.
            if (await Launcher.TryOpenAsync(url) == false)
            {
                await Browser.OpenAsync(url);
            }
        }
    }
}
