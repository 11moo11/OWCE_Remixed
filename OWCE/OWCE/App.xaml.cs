using Microsoft.Extensions.DependencyInjection;
using OWCE.DependencyInterfaces;
using OWCE.Pages;
using OWCE.Themes;

namespace OWCE
{
    public partial class App : Application
    {
        public static new App Current => Application.Current as App;

        IOWBLE _owble;

        // Resolved lazily because on Android the BLE implementation needs an Activity,
        // which does not exist yet while the Application (and this class) is being constructed.
        public IOWBLE OWBLE => _owble ??= CreateOWBLE();

#if DEBUG
        public const string OWCEApiServer = "api.dev.owce.app";
#else
        public const string OWCEApiServer = "api.owce.app";
#endif

        public static readonly BindableProperty MetricDisplayProperty = BindableProperty.Create(
            nameof(MetricDisplay),
            typeof(bool),
            typeof(App),
            false);

        public bool MetricDisplay
        {
            get { return (bool)GetValue(MetricDisplayProperty); }
            set { SetValue(MetricDisplayProperty, value); }
        }

        public string LogsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "past_rides");

        public string UserAgent => $"OWCE/{AppInfo.Current.VersionString} Build/{AppInfo.Current.BuildString} ({DeviceInfo.Current.Platform}; {DeviceInfo.Current.VersionString})";

        /// <summary>The page currently at the root of the (first) window.</summary>
        public Page RootPage => Windows.Count > 0 ? Windows[0].Page : null;

        /// <summary>Resolves a service registered in <see cref="MauiProgram"/>.</summary>
        public static T GetService<T>() => IPlatformApplication.Current.Services.GetRequiredService<T>();

        public App()
        {
            MetricDisplay = Preferences.Get("metric_display", System.Globalization.RegionInfo.CurrentRegion.IsMetric);

            if (Directory.Exists(LogsDirectory) == false)
            {
                Directory.CreateDirectory(LogsDirectory);
            }

            Database.Init();

            InitializeComponent();

            // The colours and panel/button styles come from the theme, so it has to be applied before any page is created.
            ThemeManager.ApplySaved();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = new Window(new NavigationPage(new BoardListPage()));

            // The launcher icon follows the theme, but only changes once the app is out of sight.
            window.Stopped += (sender, e) => AppIconCatalog.ApplyForCurrentTheme();
            return window;
        }

        IOWBLE CreateOWBLE()
        {
#if DEBUG
            // If simulator or emulator use MockOWBLE.
            if (DeviceInfo.Current.DeviceType == DeviceType.Virtual)
            {
                return new MockOWBLE();
            }
#endif
            return GetService<IOWBLE>();
        }

        internal async Task<OWBoard> ConnectToBoard(OWBaseBoard baseBoard, CancellationToken token)
        {
            var didConnect = await OWBLE.Connect(baseBoard, token);
            if (didConnect)
            {
                return new OWBoard(OWBLE, baseBoard);
            }

            return null;
        }
    }
}
