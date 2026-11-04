using System.ComponentModel;
using OWCE.Extensions;

namespace OWCE.Views
{
    /// <summary>Battery and current in one panel. Its BindingContext is the board.</summary>
    public partial class BatteryPowerView : ContentView
    {
        OWBoard _board;

        public BatteryPowerView()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (_board != null)
            {
                _board.PropertyChanged -= Board_PropertyChanged;
            }

            _board = BindingContext as OWBoard;

            if (_board != null)
            {
                _board.PropertyChanged += Board_PropertyChanged;
            }

            UpdateBars();
        }

        void Board_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OWBoard.BatteryPercent) || e.PropertyName == nameof(OWBoard.CurrentAmps))
            {
                MainThread.BeginInvokeOnMainThread(UpdateBars);
            }
        }

        // The bars are only as wide as their half of the panel, so they have to be set again when the panel changes size.
        void Halves_SizeChanged(object sender, EventArgs e)
        {
            UpdateBars();
        }

        void UpdateBars()
        {
            if (_board == null)
            {
                return;
            }

            // Battery: filled to the battery percentage.
            BatteryBar.AnimateWidthPercent(Math.Clamp(_board.BatteryPercent, 0, 100) * 0.01);

            // Current: filled to the current out of 20A. Regen (negative) fills the same way.
            if (PowerHalf.Width > 0)
            {
                PowerBar.WidthRequest = Math.Clamp(Math.Abs(_board.CurrentAmps) / 20.0, 0, 1) * PowerHalf.Width;
            }
        }

        void ExpanderView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CommunityToolkit.Maui.Views.Expander.IsExpandedProperty.PropertyName.Equals(e.PropertyName))
            {
                if (ExpanderView.IsExpanded)
                {
                    ExpanderArrow.RotateToAsync(180, 200, Easing.CubicIn);
                }
                else
                {
                    ExpanderArrow.RotateToAsync(0, 200, Easing.CubicOut);
                }
            }
        }
    }
}
