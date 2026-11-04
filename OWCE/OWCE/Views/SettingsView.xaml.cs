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
