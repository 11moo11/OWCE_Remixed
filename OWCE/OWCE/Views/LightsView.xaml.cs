namespace OWCE.Views
{
    public partial class LightsView : ContentView
    {
        bool _isBusy = false;

        public LightsView()
        {
            InitializeComponent();
        }

        async void OnTapped(object sender, TappedEventArgs e)
        {
            // Ignore taps while a change is still being sent to the board.
            if (_isBusy || BindingContext is not OWBoard board)
            {
                return;
            }

            _isBusy = true;
            Opacity = 0.6;

            try
            {
                // If the board does not take it, nothing changes and nothing is shown.
                await board.SetLightsAsync(!board.LightMode);
            }
            finally
            {
                Opacity = 1.0;
                _isBusy = false;
            }
        }
    }
}
