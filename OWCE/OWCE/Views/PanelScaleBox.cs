namespace OWCE.Views
{
    /// <summary>
    /// Shows a panel at a fraction (or multiple) of its normal size. The panel is laid out as if the screen were wider or
    /// narrower, then scaled to fit, so all of its text, bars and spacing grow or shrink together. (MAUI has no Viewbox.)
    /// At the normal size (1) it does nothing at all.
    /// </summary>
    public class PanelScaleBox : ContentView
    {
        double _scale = 1;
        double _lastWidth;

        public double PanelScale
        {
            get => _scale;
            set
            {
                if (Math.Abs(_scale - value) < 0.001)
                {
                    return;
                }

                _scale = value;
                Apply();
            }
        }

        public View Panel
        {
            get => Content;
            set
            {
                if (Content != null)
                {
                    Content.SizeChanged -= Panel_SizeChanged;
                }

                Content = value;

                if (value != null)
                {
                    value.SizeChanged += Panel_SizeChanged;
                }

                Apply();
            }
        }

        bool IsNormalSize => Math.Abs(_scale - 1) < 0.001;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width > 0 && Math.Abs(width - _lastWidth) > 0.5)
            {
                _lastWidth = width;
                Apply();
            }
        }

        // The panel can change height by itself (an expander opening), and this box has to follow.
        void Panel_SizeChanged(object sender, EventArgs e)
        {
            UpdateHeight();
        }

        void Apply()
        {
            var panel = Content;
            if (panel == null)
            {
                return;
            }

            if (IsNormalSize)
            {
                panel.Scale = 1;
                panel.AnchorX = 0.5;
                panel.AnchorY = 0.5;
                panel.WidthRequest = -1;
                panel.HorizontalOptions = LayoutOptions.Fill;
                panel.VerticalOptions = LayoutOptions.Fill;
                HeightRequest = -1;
                return;
            }

            if (Width <= 0)
            {
                // Waiting for the first layout. Apply is called again once the width is known.
                return;
            }

            // Lay the panel out at 1/scale of the width, then scale it up or down from its top left corner to fill this box.
            panel.AnchorX = 0;
            panel.AnchorY = 0;
            panel.HorizontalOptions = LayoutOptions.Start;
            panel.VerticalOptions = LayoutOptions.Start;
            panel.WidthRequest = Width / _scale;
            panel.Scale = _scale;

            UpdateHeight();
        }

        void UpdateHeight()
        {
            var panel = Content;
            if (panel == null || IsNormalSize)
            {
                return;
            }

            if (panel.Height > 0)
            {
                HeightRequest = panel.Height * _scale;
            }
        }
    }
}
