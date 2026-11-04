using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using OWCE.Pages.Popup;
using Mopups.Services;
using System.Linq;
using OWCE.Views;
using OWCE.DependencyInterfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OWCE.Pages
{
    public partial class BoardPage : BaseContentPage
    {
        ConnectingAlert _reconnectingAlert;


        public OWBoard Board { get; private set; }
        /*
        public string SpeedHeader
        {
            get
            {
                var unit = App.Current.MetricDisplay ? "km/h" : "mph";
                return unit; // return $"Speed ({unit})";
            }
        }*/

        private bool _initialSubscirbe = false;
        Grid _sideMenuItem = null;

        IAsyncRelayCommand _startRecordRideCommand = null;
        public IAsyncRelayCommand StartRecordRideCommand => _startRecordRideCommand ??= new AsyncRelayCommand(StartRecordingAsync);

        IAsyncRelayCommand _editPanelsCommand = null;
        public IAsyncRelayCommand EditPanelsCommand => _editPanelsCommand ??= new AsyncRelayCommand(EditPanelsAsync);

        IAsyncRelayCommand _stopRecordRideCommand = null;
        public IAsyncRelayCommand StopRecordRideCommand => _stopRecordRideCommand ??= new AsyncRelayCommand(StopRecordingAsync);



        public BoardPage(OWBoard board) : base()
        {
            Board = board;

            InitializeComponent();
            BindingContext = board;

            SetupPanels();

            AppVersionLabel.Text = $"{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})";

            // TODO: Fix ImperialSwitch.IsToggled = !App.Current.MetricDisplay;


            Board.Init();
            // I really don't like this.
            _ = Board.SubscribeToBLE();

            App.Current.OWBLE.BoardDisconnected += OWBLE_BoardDisconnected;
            App.Current.OWBLE.BoardReconnecting += OWBLE_BoardReconnecting;
            App.Current.OWBLE.BoardReconnected += OWBLE_BoardReconnected;

            // Shift title to the right.
            var titleLabel = GetTitleLabel();
            titleLabel.HorizontalOptions = LayoutOptions.End;
            titleLabel.Padding = new Thickness(0, 0, 16, 0);


            var sideMenuItem = new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Left,
                IconColorKey = "OnMenuButtonColor",
                IconBackgroundKey = "MenuButtonColor",
                IconImageSource = "hamburger",
                Command = new AsyncRelayCommand(async () =>
                {
                    await MopupService.Instance.PushAsync(Popup.SideMenuPopup.Instance);
                }),
            };
            CustomToolbarItems.Add(sideMenuItem);

        }

        private void OWBLE_BoardDisconnected()
        {
            System.Diagnostics.Debug.WriteLine("OWBLE_BoardDisconnected");
        }

        private void OWBLE_BoardReconnecting()
        {
            System.Diagnostics.Debug.WriteLine("OWBLE_BoardReconnecting");
            
            _reconnectingAlert = new ConnectingAlert(Board.Name, new Command(() =>
            {
                // TODO Disconnect.
                MopupService.Instance.RemovePageAsync(_reconnectingAlert);
                _reconnectingAlert = null;
            }), "Reconnecting...");
            

            if (MopupService.Instance.PopupStack.Contains(_reconnectingAlert) == false)
            {
                MopupService.Instance.PushAsync(_reconnectingAlert, true);
            }

        }


        private void OWBLE_BoardReconnected()
        {
            System.Diagnostics.Debug.WriteLine("OWBLE_BoardReconnected");

            if (MopupService.Instance.PopupStack.Contains(_reconnectingAlert))
            {
                MopupService.Instance.RemovePageAsync(_reconnectingAlert);
                _reconnectingAlert = null;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Popup.SideMenuPopup.Instance.Title = "OWCE";

            if (_sideMenuItem == null)
            {
                var dataTemplate = (DataTemplate)Resources["SideMenu"];
                _sideMenuItem = dataTemplate.CreateContent() as Grid;
                _sideMenuItem.BindingContext = this;
            }
            Popup.SideMenuPopup.Instance.PageSpecificSideMenu = _sideMenuItem;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            /*
            if (GaugeAbsolueLayout.WidthRequest.AlmostEqualTo(width) == false)
            {
                GaugeAbsolueLayout.WidthRequest = width;
                GaugeAbsolueLayout.HeightRequest = width;
                GaugeAbsolueLayout.MinimumWidthRequest = width;
                GaugeAbsolueLayout.MinimumHeightRequest = width;
            }
            */
        }

        protected override bool OnBackButtonPressed()
        {
            Disconnect_Tapped(null, EventArgs.Empty);
            //DisconnectAndPop();
            return true;
        }

        async void Disconnect_Tapped(System.Object sender, System.EventArgs e)
        {
            var result = await DisplayActionSheetAsync("Are you sure you want to disconnect?", "Cancel", "Disconnect");

            if (result == "Disconnect")
            {
                if (MopupService.Instance.PopupStack.Any())
                {
                    await MopupService.Instance.PopAllAsync();
                }
                await DisconnectAndPop();
            }
        }

        public async Task DisconnectAndPop()
        {
            await App.Current.OWBLE.Disconnect();

            Board.StopLogging();

            await Navigation.PopModalAsync();

            IWatch watchService = App.GetService<IWatch>();

            watchService.StopListeningForWatchMessages();
        }


        void ImperialSwitch_IsToggledChanged(object sender, bool isToggled)
        {
            App.Current.MetricDisplay = !isToggled;
            Preferences.Set("metric_display", !isToggled);

            WeakReferenceMessenger.Default.Send(new UnitDisplayUpdatedMessage());
        }

        // Hands the panels declared in the XAML to the PanelBoard, which arranges them and saves how the user arranged them.
        void SetupPanels()
        {
            var panels = new (string Id, string Title, View Panel)[]
            {
                ("speed", "Speed", SpeedRangeDistanceView),
                ("battery", "Battery and current", BatteryPowerPanel),
                ("temperature", "Temperatures", TemperatureView),
                ("ride-mode", "Ride mode", RideModePanel),
                ("lights", "Lights", LightsPanel),
            };

            foreach (var panel in panels)
            {
                PanelSource.Children.Remove(panel.Panel);
            }

            Panels.Configure(panels.Select(x => new PanelDefinition(x.Id, x.Title, x.Panel)));
        }

        async Task EditPanelsAsync()
        {
            await Popup.SideMenuPopup.Instance.CloseCommand_Clicked();
            Panels.StartEditing();
        }

        async Task StartRecordingAsync()
        {
            await Popup.SideMenuPopup.Instance.CloseCommand_Clicked();
            Board.StartLogging();
        }

        async Task StopRecordingAsync()
        {
            await Popup.SideMenuPopup.Instance.CloseCommand_Clicked();
            Board.StopLogging();
        }
    }
}
