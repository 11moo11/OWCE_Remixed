using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OWCE.Views;
namespace OWCE.Pages
{
    public partial class AppSettingsPage : BaseContentPage
    {
        public bool MetricDisplay { get; set; }
        public bool AutoRideRecording { get; set; }

        public AppSettingsPage()
        {
            InitializeComponent();

            MetricDisplay = App.Current.MetricDisplay;
            AutoRideRecording = Preferences.Get("auto_ride_recording", false);

            CustomToolbarItems.Add(new CustomToolbarItem()
            {
                Position = CustomToolbarItemPosition.Left,
                Text = "Cancel",
                Command = new AsyncRelayCommand(async () =>
                {
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

                    WeakReferenceMessenger.Default.Send(new UnitDisplayUpdatedMessage());

                    await Navigation.PopModalAsync();
                }),
            });

            BindingContext = this;

        }
    }
}

