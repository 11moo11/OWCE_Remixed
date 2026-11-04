namespace OWCE.Views
{
    /// <summary>
    /// A stack of <see cref="SettingsItem"/>s with a small gap between them. Only the group's outer corners are very round
    /// (the first item's top and the last item's bottom); the corners between items are only slightly round.
    /// </summary>
    public class SettingsGroup : VerticalStackLayout
    {
        public SettingsGroup()
        {
            Spacing = 4;
            ChildAdded += (sender, e) => UpdateCorners();
            ChildRemoved += (sender, e) => UpdateCorners();
        }

        void UpdateCorners()
        {
            var items = Children.OfType<SettingsItem>().Where(x => x.IsVisible).ToList();

            for (var i = 0; i < items.Count; ++i)
            {
                var top = i == 0 ? SettingsItem.OuterRadius : SettingsItem.InnerRadius;
                var bottom = i == items.Count - 1 ? SettingsItem.OuterRadius : SettingsItem.InnerRadius;
                items[i].SetCorners(top, bottom);
            }
        }
    }
}
