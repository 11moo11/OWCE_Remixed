using CommunityToolkit.Mvvm.Input;
using OWCE.Extensions;
using OWCE.Pages;

namespace OWCE.Views
{
    /// <summary>The list of recorded rides. A tab on the board list screen.</summary>
    public partial class PastRidesView : ContentView
    {
        public ObservableRangeCollection<Ride> Rides { get; } = new ObservableRangeCollection<Ride>();

        AsyncRelayCommand<Ride> _rideSelectedCommand;
        public AsyncRelayCommand<Ride> RideSelectedCommand => _rideSelectedCommand ??= new AsyncRelayCommand<Ride>(RideSelected);

        AsyncRelayCommand<Ride> _deleteRideCommand;
        public AsyncRelayCommand<Ride> DeleteRideCommand => _deleteRideCommand ??= new AsyncRelayCommand<Ride>(DeleteRide);

        AsyncRelayCommand<Ride> _renameRideCommand;
        public AsyncRelayCommand<Ride> RenameRideCommand => _renameRideCommand ??= new AsyncRelayCommand<Ride>(RenameRide);

        public PastRidesView()
        {
            InitializeComponent();
            BindingContext = this;
            Refresh();
        }

        /// <summary>Loads the rides again (a ride may have been recorded since).</summary>
        public void Refresh()
        {
            var rides = Database.Connection.Table<Ride>().OrderByDescending((r) => r.StartTime).ToList();
            Rides.Clear();
            Rides.AddRange(rides);
        }

        static Page CurrentPage => App.Current.RootPage;

        async Task RideSelected(Ride ride)
        {
            var submit = await CurrentPage.DisplayAlertAsync("Coming Soon", "Viewing of recorded rides is not available yet. We are working hard to bring it to you soon.\n\nIn the meantime if you would like to assist with OWCE development (specifically if you have aftermarket batteries) you can submit your ride data for research purposes.", "Learn More", "Cancel");
            if (submit)
            {
                await CurrentPage.Navigation.PushModalAsync(new NavigationPage(new SubmitRidePage(ride)));
            }
        }

        async Task DeleteRide(Ride ride)
        {
            var delete = await CurrentPage.DisplayAlertAsync("Delete Ride", $"Are you sure you want to delete the ride \"{ride.Name}\"?", "Delete", "Cancel");
            if (delete)
            {
                Database.Connection.Delete(ride);
                Rides.Remove(ride);
            }
        }

        async Task RenameRide(Ride ride)
        {
            var name = await CurrentPage.DisplayPromptAsync("Rename Ride", "What would like to rename this ride to?", "Save", initialValue: ride.Name);
            if (string.IsNullOrWhiteSpace(name) == false)
            {
                ride.Name = name;
                ride.Save();
            }
            else if (name == null)
            {
                // Cancelled.
            }
            else
            {
                await CurrentPage.DisplayAlertAsync("Error", $"\"{name}\" was an invalid name for a ride.", "OKAY");
            }
        }
    }
}
