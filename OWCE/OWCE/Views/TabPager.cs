namespace OWCE.Views
{
    /// <summary>
    /// Holds a few views side by side as one wide strip and shows a window onto it, so changing tab slides the whole view sideways
    /// in the direction of the tab. The position is a fractional tab number (1.5 is half way between tab 1 and tab 2), which
    /// lets a finger drag it, and lets it spring past its target and settle. Pages in the distance fade and shrink a little.
    /// </summary>
    public class TabPager : Grid
    {
        int _selected = -1;
        double _position;
        TaskCompletionSource _running;

        /// <summary>The tab that is (or is heading to be) shown.</summary>
        public int SelectedIndex => _selected;

        /// <summary>The fractional tab number the strip is currently at.</summary>
        public double Position => _position;


        /// <summary>Raised whenever the strip moves, with the new position.</summary>
        public event Action<double> PositionChanged;

        /// <summary>Raised when a tab is chosen, before the slide starts. The flag says the user dragged to it.</summary>
        public event Action<int, bool> TabSelected;

        public TabPager()
        {
            IsClippedToBounds = true;
            SizeChanged += (sender, e) => Apply(_position);
        }

        /// <summary>Moves the strip to a position straight away (used while dragging). Stops any slide in progress.</summary>
        public void SetPosition(double position)
        {
            this.AbortAnimation("TabPager");
            Apply(position);
        }

        void Apply(double position)
        {
            _position = position;
            var width = Width > 0 ? Width : 0;

            for (var i = 0; i < Children.Count; ++i)
            {
                var page = (View)Children[i];
                var distance = i - position;
                var near = Math.Abs(distance) < 1;

                page.IsVisible = near;
                if (near)
                {
                    var amount = Math.Abs(distance);
                    page.TranslationX = distance * width;
                    page.Opacity = 1 - 0.7 * amount;
                    page.Scale = 1 - 0.06 * amount;
                }
            }

            PositionChanged?.Invoke(position);
        }

        /// <summary>Shows a tab straight away, with no animation (used for the first one).</summary>
        public void Select(int index)
        {
            _selected = index;
            SetPosition(index);
        }

        /// <summary>Slides to a tab. It springs a little past it and settles.</summary>
        public Task SelectAsync(int index, bool fromDrag = false)
        {
            if (index < 0 || index >= Children.Count)
            {
                return Task.CompletedTask;
            }

            if (index == _selected && Math.Abs(_position - index) < 0.001)
            {
                return Task.CompletedTask;
            }

            var changed = index != _selected;
            _selected = index;
            if (changed)
            {
                TabSelected?.Invoke(index, fromDrag);
            }

            if (Width <= 0)
            {
                SetPosition(index);
                return Task.CompletedTask;
            }

            this.AbortAnimation("TabPager");
            _running?.TrySetResult();
            var finished = new TaskCompletionSource();
            _running = finished;

            var distance = Math.Abs(index - _position);
            var length = (uint)Math.Clamp(380 + 140 * distance, 380, 700);

            var animation = new Animation(value => Apply(value), _position, index, Easing.SpringOut);
            animation.Commit(this, "TabPager", 16, length, finished: (value, cancelled) =>
            {
                if (!cancelled)
                {
                    Apply(index);
                }
                finished.TrySetResult();
            });

            return finished.Task;
        }
    }
}
