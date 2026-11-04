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

        public AppSettingsPage()
        {
            InitializeComponent();

            MetricDisplay = App.Current.MetricDisplay;
            AutoRideRecording = Preferences.Get("auto_ride_recording", false);

            ThemePicker.Select(_originalThemeId);
            ThemeDescriptionLabel.Text = ThemeManager.Find(_originalThemeId).Description;
            ThemePicker.ThemeSelected += ThemePicker_ThemeSelected;

            CustomToolbarItems.Add(new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Left,
                IconPathData = Icons.ArrowBack,
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

                    if (ThemePicker.SelectedThemeId != null)
                    {
                        ThemeManager.Save(ThemePicker.SelectedThemeId);
                    }
                    _saved = true;

                    WeakReferenceMessenger.Default.Send(new UnitDisplayUpdatedMessage());

                    await Navigation.PopModalAsync();
                }),
            });

            BindingContext = this;

        }

        void ThemePicker_ThemeSelected(string themeId)
        {
            ThemeDescriptionLabel.Text = ThemeManager.Find(themeId).Description;
            ThemeManager.Apply(themeId);
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

