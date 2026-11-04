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
            ModeControl.SelectionChanged += index => Change(spec =>
            {
                // Light and dark are the usual page colours for the primary colour.
                var (background, text) = ColorScheme.DefaultNeutrals(Color.FromArgb(spec.Primary), index == 1);
                spec.Background = background.ToRgbaHex();
                spec.Text = text.ToRgbaHex();
            });
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

        // The five colours, in the order Realtime Colors lists them.
        static readonly string[] RoleNames = { "Text", "Background", "Primary", "Secondary", "Accent" };
        static readonly string[] RoleHints =
        {
            "Text: the colour of text and icons on the page.",
            "Background: the page behind everything.",
            "Primary: the main colour: the speed and battery panels, cards and selected buttons.",
            "Secondary: a quieter colour: the lights and ride mode panels and soft buttons.",
            "Accent: highlights: the selected tab, sliders, headings and links, and the power and temperature panels.",
        };

        readonly List<Border> _roleCircles = new List<Border>();
        readonly List<Border> _roleChips = new List<Border>();
        readonly List<Border> _swatchViews = new List<Border>();
        readonly bool[] _customOpen = new bool[5];
        int _role = 2;
        DateTime _lastColourApply = DateTime.MinValue;

        static string GetRole(ThemeSpec spec, int role) => role switch
        {
            0 => spec.Text,
            1 => spec.Background,
            2 => spec.Primary,
            3 => spec.Secondary,
            _ => spec.Accent,
        };

        static void SetRole(ThemeSpec spec, int role, string hex)
        {
            switch (role)
            {
                case 0: spec.Text = hex; break;
                case 1: spec.Background = hex; break;
                case 2: spec.Primary = hex; break;
                case 3: spec.Secondary = hex; break;
                default: spec.Accent = hex; break;
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
            for (var i = 0; i < 5; ++i)
            {
                var role = i;

                var circle = new Border()
                {
                    WidthRequest = 28,
                    HeightRequest = 28,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    HorizontalOptions = LayoutOptions.Center,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 14 },
                    InputTransparent = true,
                };
                var label = new Label() { Text = RoleNames[i], FontSize = 11, LineBreakMode = LineBreakMode.NoWrap, HorizontalOptions = LayoutOptions.Center, InputTransparent = true };
                label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");

                var chip = new Border()
                {
                    Padding = new Thickness(2, 10),
                    StrokeThickness = 1.5,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 16 },
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

        // The swatches for the colour being edited: a custom one (a plus), the colour the theme started with, and the ready made ones.
        // They are laid out six to a row across the whole width.
        void RebuildSwatches()
        {
            SwatchGrid.Children.Clear();
            SwatchGrid.RowDefinitions.Clear();
            _swatchViews.Clear();

            var preset = ThemeManager.Find(ThemeManager.Current.BaseId);
            var start = _role switch { 0 => preset.Text, 1 => preset.Background, 2 => preset.Primary, 3 => preset.Secondary, _ => preset.Accent };
            var choices = new[] { start }.Concat(ColorScheme.SeedChoices).Select(x => Color.FromArgb(x).ToRgbaHex()).Distinct().ToList();
            var current = Color.FromArgb(GetRole(ThemeManager.Current, _role));

            // Custom is selected when it was tapped, or when the colour is not one of the ready made ones.
            var customSelected = _customOpen[_role] || choices.Contains(current.ToRgbaHex()) == false;
            CustomControls.IsVisible = customSelected;

            var role = _role;
            var index = 0;

            Border MakeCircle(Color fill)
            {
                var circle = new Border()
                {
                    StrokeThickness = 3,
                    Stroke = Brush.Transparent,
                    BackgroundColor = fill,
                    HorizontalOptions = LayoutOptions.Center,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 100 },
                };
                _swatchViews.Add(circle);

                var row = index / 6;
                if (SwatchGrid.RowDefinitions.Count <= row)
                {
                    SwatchGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }
                SwatchGrid.Add(circle, index % 6, row);
                index++;
                return circle;
            }

            // The custom swatch: an empty circle with a plus, which fills with the custom colour once it is selected.
            var custom = MakeCircle(customSelected ? current : Colors.Transparent);
            if (customSelected)
            {
                custom.SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
            }
            else
            {
                custom.SetDynamicResource(Border.StrokeProperty, "MutedTextBrush");
                var plus = new Label() { Text = "+", FontSize = 26, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
                plus.SetDynamicResource(Label.TextColorProperty, "MutedTextColor");
                custom.Content = plus;
            }
            custom.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    Motion.Tick();
                    _ = Motion.SquishAsync(custom, 0.8);
                    _customOpen[role] = true;
                    LoadFromCurrent();
                }),
            });

            foreach (var hex in choices)
            {
                var color = Color.FromArgb(hex);
                var swatch = MakeCircle(color);
                if (customSelected == false && hex == current.ToRgbaHex())
                {
                    swatch.SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
                }
                else
                {
                    // A thin edge so black or white swatches can be seen against a page of the same colour.
                    swatch.SetDynamicResource(Border.StrokeProperty, "DividerBrush");
                }

                swatch.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(swatch, 0.8);
                        _customOpen[role] = false;
                        Change(spec => SetRole(spec, role, color.ToRgbaHex()));
                    }),
                });
            }

            ResizeSwatches();
        }

        void SwatchGrid_SizeChanged(object sender, EventArgs e) => ResizeSwatches();

        // Makes the circles as big as the width allows, so every row fills the panel.
        void ResizeSwatches()
        {
            if (SwatchGrid.Width <= 0)
            {
                return;
            }

            var size = Math.Max(24, SwatchGrid.Width / 6 - 8);
            foreach (var circle in _swatchViews)
            {
                circle.WidthRequest = size;
                circle.HeightRequest = size;
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
            Array.Clear(_customOpen);
            Change(spec =>
            {
                var preset = ThemeManager.Find(spec.BaseId);
                spec.Background = preset.Background;
                spec.Text = preset.Text;
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
