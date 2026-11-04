using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OWCE.Themes;
using OWCE.Views;
namespace OWCE.Pages
{
    public partial class AppSettingsPage : BaseContentPage
    {
        public bool MetricDisplay { get; set; }
        public bool AutoRideRecording { get; set; }

        // Themes are previewed as soon as one is picked. If the page is left without saving, the old theme comes back.
        readonly string _originalThemeId = ThemeManager.CurrentId;
        bool _saved = false;
        bool _isLoadingThemes = true;

        public AppSettingsPage()
        {
            InitializeComponent();

            MetricDisplay = App.Current.MetricDisplay;
            AutoRideRecording = Preferences.Get("auto_ride_recording", false);

            ThemePicker.ItemsSource = ThemeManager.Themes.Select(x => x.Name).ToList();
            ThemePicker.SelectedIndex = ThemeManager.Themes.ToList().FindIndex(x => x.Id == _originalThemeId);
            ThemeDescriptionLabel.Text = ThemeManager.Find(_originalThemeId).Description;
            _isLoadingThemes = false;

            CustomToolbarItems.Add(new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Left,
                Text = "Cancel",
                Command = new AsyncRelayCommand(async () =>
                {
                    // Leaving the page puts the old theme back (see OnDisappearing).
                    await Navigation.PopModalAsync();
                }),
            });


            CustomToolbarItems.Add(new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Right,
                Text = "Save",
                Command = new AsyncRelayCommand(async () =>
                {
                    App.Current.MetricDisplay = MetricDisplay;

                    Preferences.Set("metric_display", MetricDisplay);
                    Preferences.Set("auto_ride_recording", AutoRideRecording);

                    var selectedIndex = ThemePicker.SelectedIndex;
                    if (selectedIndex >= 0)
                    {
                        ThemeManager.Save(ThemeManager.Themes[selectedIndex].Id);
                    }
                    _saved = true;

                    WeakReferenceMessenger.Default.Send(new UnitDisplayUpdatedMessage());

                    await Navigation.PopModalAsync();
                }),
            });

            BindingContext = this;

        }

        void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingThemes || ThemePicker.SelectedIndex < 0)
            {
                return;
            }

            var theme = ThemeManager.Themes[ThemePicker.SelectedIndex];
            ThemeDescriptionLabel.Text = theme.Description;
            ThemeManager.Apply(theme.Id);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_saved == false && ThemeManager.CurrentId != _originalThemeId)
            {
                ThemeManager.Apply(_originalThemeId);
            }
        }
    }
}

