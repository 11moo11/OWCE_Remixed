namespace OWCE.Views
{
    /// <summary>
    /// Holds a few views on top of each other and shows one. Changing tab slides the whole view sideways, in the direction of the
    /// tab (a tab further right slides in from the right), so the tabs feel like parts of one wide screen.
    /// </summary>
    public class TabPager : Grid
    {
        const uint SlideMilliseconds = 280;

        int _index = -1;
        int _token = 0;

        public int SelectedIndex => _index;

        public TabPager()
        {
            IsClippedToBounds = true;
        }

        /// <summary>Shows a tab. Slides if it is not the first one shown (and the pager has been laid out).</summary>
        public async Task SelectAsync(int index)
        {
            if (index == _index || index < 0 || index >= Children.Count)
            {
                return;
            }

            var token = ++_token;
            var from = _index >= 0 ? (View)Children[_index] : null;
            var to = (View)Children[index];
            var direction = index > _index ? 1 : -1;
            _index = index;

            // Stop whatever was still sliding, and put away every page that is not part of this slide.
            foreach (View page in Children)
            {
                page.CancelAnimations();
                if (page != from && page != to)
                {
                    page.IsVisible = false;
                    page.TranslationX = 0;
                }
            }

            var width = Width;
            if (from == null || width <= 0)
            {
                to.TranslationX = 0;
                to.IsVisible = true;
                return;
            }

            from.TranslationX = 0;
            to.TranslationX = direction * width;
            to.IsVisible = true;

            await Task.WhenAll(
                from.TranslateToAsync(-direction * width, 0, SlideMilliseconds, Easing.CubicOut),
                to.TranslateToAsync(0, 0, SlideMilliseconds, Easing.CubicOut));

            if (token == _token)
            {
                from.IsVisible = false;
                from.TranslationX = 0;
            }
        }

        /// <summary>Shows a tab without sliding (used for the first one).</summary>
        public void Select(int index)
        {
            _ = SelectAsync(index);
        }
    }
}
