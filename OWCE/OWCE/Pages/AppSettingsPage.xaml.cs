using CommunityToolkit.Mvvm.Input;
using OWCE.Views;

namespace OWCE.Pages
{
    /// <summary>The settings as a page of their own (opened from the board list). On the board page they are a tab instead.</summary>
    public partial class AppSettingsPage : BaseContentPage
    {
        public AppSettingsPage()
        {
            InitializeComponent();

            CustomToolbarItems.Add(new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Left,
                IconPathData = Icons.ArrowBack,
                Command = new AsyncRelayCommand(async () =>
                {
                    await Navigation.PopModalAsync();
                }),
            });
        }
    }
}
