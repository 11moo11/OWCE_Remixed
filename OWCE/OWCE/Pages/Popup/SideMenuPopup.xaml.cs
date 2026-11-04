using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mopups.Pages;
using Mopups.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace OWCE.Pages.Popup
{
    public partial class SideMenuPopup : PopupPage
    {
        static SideMenuPopup _instance;
        public static SideMenuPopup Instance => _instance ??= new SideMenuPopup();

        IAsyncRelayCommand _closeCommand;
        public IAsyncRelayCommand CloseCommand => _closeCommand ??= new AsyncRelayCommand(CloseCommand_Clicked);

        IAsyncRelayCommand<View> _aboutCommand;
        public IAsyncRelayCommand<View> AboutCommand => _aboutCommand ??= new AsyncRelayCommand<View>(async (sender) => await AboutCommand_Clicked(sender));

        IAsyncRelayCommand<View> _settingsCommand;
        public IAsyncRelayCommand<View> SettingsCommand => _settingsCommand ??= new AsyncRelayCommand<View>(async (sender) => await SettingsCommand_Clicked(sender));

        View _pageSpecificSideMenu = null;
        public View PageSpecificSideMenu {
            get
            {
                return _pageSpecificSideMenu;
            }
            set
            {
                if (_pageSpecificSideMenu != null)
                {
                    MainGrid.Children.Remove(_pageSpecificSideMenu);
                    _pageSpecificSideMenu = null;
                }

                if (value != null)
                {
                    _pageSpecificSideMenu = value;
                    Grid.SetRow(_pageSpecificSideMenu, 1);
                    MainGrid.Children.Add(_pageSpecificSideMenu);
                }
            }
        }

        private SideMenuPopup()
        {
            InitializeComponent();

            BindingContext = this;
        }

        internal async Task CloseCommand_Clicked()
        {
            await MopupService.Instance.PopAsync(true);
        }

        async Task AboutCommand_Clicked(View sender)
        {
            sender.Opacity = 0.6f;

            await Task.WhenAll(
                sender.FadeToAsync(1f),
                App.Current.RootPage.Navigation.PushModalAsync(new NavigationPage(new AboutPage())),
                MopupService.Instance.RemovePageAsync(this)
            );
        }

        async Task SettingsCommand_Clicked(View sender)
        {
            sender.Opacity = 0.6f;

            await Task.WhenAll(
                sender.FadeToAsync(1f),
                App.Current.RootPage.Navigation.PushModalAsync(new NavigationPage(new AppSettingsPage())),
                MopupService.Instance.RemovePageAsync(this)
            );
        }

    }
}
