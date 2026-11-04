namespace OWCE.Views
{
    /// <summary>Small reusable animations used around the app.</summary>
    public static class Motion
    {
        /// <summary>Makes each view fade and spring up into place, one after another.</summary>
        public static void StaggerIn(IEnumerable<VisualElement> views, int stepMilliseconds = 55, double rise = 28)
        {
            var index = 0;
            foreach (var view in views.Where(x => x.IsVisible).ToList())
            {
                view.CancelAnimations();
                view.Opacity = 0;
                view.TranslationY = rise;
                view.Scale = 0.96;
                _ = RunStaggerAsync(view, index++ * stepMilliseconds);
            }
        }

        static async Task RunStaggerAsync(VisualElement view, int delay)
        {
            try
            {
                await Task.Delay(delay);
                await Task.WhenAll(
                    view.FadeToAsync(1, 240, Easing.CubicOut),
                    view.TranslateToAsync(0, 0, 450, Easing.SpringOut),
                    view.ScaleToAsync(1, 450, Easing.SpringOut));
            }
            catch (Exception)
            {
                // The view went away while animating. Nothing to do.
            }
        }

        /// <summary>A quick press: shrinks, then springs back with a little overshoot.</summary>
        public static async Task SquishAsync(VisualElement view, double to = 0.94)
        {
            try
            {
                view.CancelAnimations();
                await view.ScaleToAsync(to, 70, Easing.CubicOut);
                await view.ScaleToAsync(1, 380, Easing.SpringOut);
            }
            catch (Exception)
            {
            }
        }

        public static void Tick(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            try
            {
                HapticFeedback.Default.Perform(type);
            }
            catch (Exception)
            {
                // Not every device can do this.
            }
        }
    }
}
