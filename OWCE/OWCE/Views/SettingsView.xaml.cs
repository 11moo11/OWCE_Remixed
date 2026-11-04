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

            var themeId = ThemeManager.CurrentId;
            ThemePicker.Select(themeId);
            ThemeDescriptionLabel.Text = ThemeManager.Find(themeId).Description;
            ThemePicker.ThemeSelected += ThemePicker_ThemeSelected;

            BuildMaterialOptions();
            UpdateMaterialOptions(false);

            _loading = false;
            BindingContext = this;
        }

        /// <summary>The first sections spring up into place one after another.</summary>
        public void PlayEntrance() => Motion.StaggerIn(ContentStack.Children.OfType<VisualElement>().Take(8), 55, 30);

        void ThemePicker_ThemeSelected(string themeId)
        {
            ThemeDescriptionLabel.Text = ThemeManager.Find(themeId).Description;
            ThemeManager.Apply(themeId);
            ThemeManager.Save(themeId);
            UpdateMaterialOptions(true);
        }

        // ---- Material colours ----

        readonly List<Border> _seedSwatches = new List<Border>();
        DateTime _lastHueApply = DateTime.MinValue;

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
                        SetSeed(color, true);
                    }),
                });
                _seedSwatches.Add(swatch);
                SeedSwatches.Children.Add(swatch);
            }
        }

        // Shows the options only for the Material theme, and loads the saved choices into the controls.
        void UpdateMaterialOptions(bool animate)
        {
            var show = ThemePicker.SelectedThemeId == "material";

            _loading = true;
            MaterialDarkSwitch.IsToggled = MaterialTheme.IsDark;
            HueSlider.Value = MaterialTheme.Seed.GetHue() * 360;
            _loading = false;
            MarkSelectedSwatch();

            if (show == MaterialOptions.IsVisible)
            {
                return;
            }

            if (show && animate)
            {
                MaterialOptions.Opacity = 0;
                MaterialOptions.IsVisible = true;
                _ = MaterialOptions.FadeToAsync(1, 300, Easing.CubicOut);
            }
            else
            {
                MaterialOptions.Opacity = 1;
                MaterialOptions.IsVisible = show;
            }
        }

        void MarkSelectedSwatch()
        {
            var seed = MaterialTheme.Seed.ToRgbaHex();
            for (var i = 0; i < _seedSwatches.Count; ++i)
            {
                var selected = Color.FromArgb(MaterialPalette.SeedChoices[i]).ToRgbaHex() == seed;
                if (selected)
                {
                    _seedSwatches[i].SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
                }
                else
                {
                    _seedSwatches[i].RemoveDynamicResource(Border.StrokeProperty);
                    _seedSwatches[i].Stroke = Brush.Transparent;
                }
            }
        }

        // Saves the colour and re-applies the Material theme.
        void SetSeed(Color seed, bool updateSlider)
        {
            MaterialTheme.Seed = seed;

            if (updateSlider)
            {
                _loading = true;
                HueSlider.Value = seed.GetHue() * 360;
                _loading = false;
            }

            ApplyMaterial();
            MarkSelectedSwatch();
        }

        void ApplyMaterial()
        {
            ThemeManager.Apply("material");
            ThemePicker.Rebuild();
        }

        void HueSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            MaterialTheme.Seed = Color.FromHsla((float)(e.NewValue / 360), 0.6f, 0.5f);

            // The whole app is restyled for each step, so not on every single movement.
            if ((DateTime.UtcNow - _lastHueApply).TotalMilliseconds > 120)
            {
                _lastHueApply = DateTime.UtcNow;
                ThemeManager.Apply("material");
            }
        }

        void HueSlider_DragCompleted(object sender, EventArgs e)
        {
            ApplyMaterial();
            MarkSelectedSwatch();
        }

        void MaterialDarkSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            MaterialTheme.IsDark = e.Value;
            ApplyMaterial();
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
