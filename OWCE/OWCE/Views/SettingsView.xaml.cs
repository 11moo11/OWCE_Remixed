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

            CornersControl.SetItems("Very round", "Round", "Sharp");
            DepthControl.SetItems("None", "Drop shadow", "Edge light");
            CornersControl.SelectionChanged += index => Change(spec => spec.Corners = (CornerStyle)index);
            DepthControl.SelectionChanged += index => Change(spec => spec.Depth = (DepthStyle)index);

            BuildFontChips();
            BuildMaterialOptions();
            ThemePicker.ThemeSelected += ThemePicker_ThemeSelected;
            LoadFromCurrent();

            _loading = false;
            BindingContext = this;
        }

        /// <summary>The first sections spring up into place one after another.</summary>
        public void PlayEntrance() => Motion.StaggerIn(ContentStack.Children.OfType<VisualElement>().Take(8), 55, 30);

        // ---- Theme builder ----

        readonly List<Border> _seedSwatches = new List<Border>();
        readonly List<(Border Chip, Label Label, string Family)> _fontChips = new List<(Border, Label, string)>();
        DateTime _lastHueApply = DateTime.MinValue;

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
            ThemeDescriptionLabel.Text = ThemeManager.IsUserStyle(spec.Id) ? $"Your style, based on {ThemeManager.Find(spec.BaseId).Name}." : ThemeManager.Find(spec.Id).Description;

            CornersControl.SelectedIndex = (int)spec.Corners;
            DepthControl.SelectedIndex = (int)spec.Depth;
            MarkSelectedFont();

            MaterialDarkSwitch.IsToggled = spec.Dark;
            HueSlider.Value = Color.FromArgb(spec.Seed).GetHue() * 360;
            MarkSelectedSwatch();

            ShowMaterialOptions(spec.BaseId == "material");
            StyleActions.IsVisible = ThemeManager.IsUserStyle(spec.Id);

            _loading = wasLoading;
        }

        void ShowMaterialOptions(bool show)
        {
            if (show == MaterialOptions.IsVisible)
            {
                return;
            }

            if (show)
            {
                MaterialOptions.Opacity = 0;
                MaterialOptions.IsVisible = true;
                _ = MaterialOptions.FadeToAsync(1, 300, Easing.CubicOut);
            }
            else
            {
                MaterialOptions.IsVisible = false;
            }
        }

        void BuildFontChips()
        {
            foreach (var font in FontCatalog.Fonts)
            {
                var family = font.Family;

                // Each name is written in its own font.
                var label = new Label()
                {
                    Text = font.Name,
                    FontFamily = family,
                    FontSize = 16,
                    InputTransparent = true,
                    VerticalOptions = LayoutOptions.Center,
                };
                var chip = new Border()
                {
                    Margin = new Thickness(0, 0, 8, 8),
                    Padding = new Thickness(16, 10),
                    StrokeThickness = 1.5,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 20 },
                    Content = label,
                };
                chip.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(chip, 0.9);
                        Change(spec => spec.Font = family);
                    }),
                });

                _fontChips.Add((chip, label, family));
                FontChips.Children.Add(chip);
            }
        }

        void MarkSelectedFont()
        {
            var selected = ThemeManager.Current.Font ?? string.Empty;
            foreach (var (chip, label, family) in _fontChips)
            {
                var on = family == selected;
                chip.RemoveDynamicResource(VisualElement.BackgroundColorProperty);
                chip.RemoveDynamicResource(Border.StrokeProperty);
                if (on)
                {
                    chip.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");
                    chip.SetDynamicResource(Border.StrokeProperty, "AccentBrush");
                    label.SetDynamicResource(Label.TextColorProperty, "OnAccentColor");
                }
                else
                {
                    chip.BackgroundColor = Colors.Transparent;
                    chip.SetDynamicResource(Border.StrokeProperty, "DividerBrush");
                    label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");
                }
            }
        }

        void BuildMaterialOptions()
        {
            // The rainbow behind the hue slider.
            var rainbow = new LinearGradientBrush() { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
            for (var i = 0; i <= 12; ++i)
            {
                rainbow.GradientStops.Add(new GradientStop(Color.FromHsla(i / 12f, 0.75f, 0.5f), i / 12f));
            }
            HueBar.Background = rainbow;

            foreach (var hex in MaterialPalette.SeedChoices)
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
                swatch.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Motion.Tick();
                        _ = Motion.SquishAsync(swatch, 0.8);
                        Change(spec => spec.Seed = color.ToRgbaHex());
                    }),
                });
                _seedSwatches.Add(swatch);
                SeedSwatches.Children.Add(swatch);
            }
        }

        void MarkSelectedSwatch()
        {
            var seed = Color.FromArgb(ThemeManager.Current.Seed).ToRgbaHex();
            for (var i = 0; i < _seedSwatches.Count; ++i)
            {
                var selected = Color.FromArgb(MaterialPalette.SeedChoices[i]).ToRgbaHex() == seed;
                _seedSwatches[i].RemoveDynamicResource(Border.StrokeProperty);
                if (selected)
                {
                    _seedSwatches[i].SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
                }
                else
                {
                    _seedSwatches[i].Stroke = Brush.Transparent;
                }
            }
        }

        void HueSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            // The whole app is restyled for each step, so not on every single movement.
            if ((DateTime.UtcNow - _lastHueApply).TotalMilliseconds > 120)
            {
                _lastHueApply = DateTime.UtcNow;
                var spec = ThemeManager.Current.Clone();
                spec.Seed = Color.FromHsla((float)(e.NewValue / 360), 0.6f, 0.5f).ToRgbaHex();
                ThemeManager.Apply(spec);
            }
        }

        void HueSlider_DragCompleted(object sender, EventArgs e)
        {
            Change(spec => spec.Seed = Color.FromHsla((float)(HueSlider.Value / 360), 0.6f, 0.5f).ToRgbaHex());
        }

        void MaterialDarkSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            Change(spec => spec.Dark = e.Value);
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
