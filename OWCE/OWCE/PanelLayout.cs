using System.Text.Json;

namespace OWCE
{
    /// <summary>One panel on the board screen: where it is in the order, how wide it is, and how big its contents are.</summary>
    public class PanelLayoutItem
    {
        public string Id { get; set; }

        /// <summary>Half width (two panels can share a row) or full width.</summary>
        public bool Half { get; set; }

        /// <summary>0 small, 1 medium (normal) or 2 large.</summary>
        public int Size { get; set; } = PanelLayout.MediumSize;
    }

    /// <summary>Where a panel goes in the 2 column grid.</summary>
    public readonly record struct PanelCell(string Id, int Row, int Column, int ColumnSpan);

    /// <summary>
    /// The order, width and size of the panels on the board screen, with saving and loading. No UI code.
    /// </summary>
    public class PanelLayout
    {
        public const int SmallSize = 0;
        public const int MediumSize = 1;
        public const int LargeSize = 2;

        /// <summary>The panels and the order they start in.</summary>
        public static readonly string[] DefaultOrder = { "speed", "battery", "temperature", "ride-mode", "lights" };

        public List<PanelLayoutItem> Items { get; set; } = new List<PanelLayoutItem>();

        public static PanelLayout CreateDefault()
        {
            return new PanelLayout { Items = DefaultOrder.Select(id => new PanelLayoutItem { Id = id }).ToList() };
        }

        /// <summary>How much a panel's contents are scaled for each size.</summary>
        public static double ScaleFor(int size) => size switch
        {
            SmallSize => 0.7,
            LargeSize => 1.3,
            _ => 1.0,
        };

        public static string SizeName(int size) => size switch
        {
            SmallSize => "S",
            LargeSize => "L",
            _ => "M",
        };

        public int IndexOf(string id) => Items.FindIndex(x => x.Id == id);

        public PanelLayoutItem Find(string id) => Items.FirstOrDefault(x => x.Id == id);

        /// <summary>Moves a panel so it ends up at <paramref name="newIndex"/> in the order.</summary>
        public void MoveTo(string id, int newIndex)
        {
            var oldIndex = IndexOf(id);
            if (oldIndex < 0)
            {
                return;
            }

            newIndex = Math.Clamp(newIndex, 0, Items.Count - 1);
            if (newIndex == oldIndex)
            {
                return;
            }

            var item = Items[oldIndex];
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, item);
        }

        public void MoveBy(string id, int delta)
        {
            var index = IndexOf(id);
            if (index >= 0)
            {
                MoveTo(id, index + delta);
            }
        }

        public void SetHalf(string id, bool half)
        {
            var item = Find(id);
            if (item == null || item.Half == half)
            {
                return;
            }

            item.Half = half;

            // Normal sized contents are too wide for half a screen, so start half width panels small.
            if (half && item.Size == MediumSize)
            {
                item.Size = SmallSize;
            }
        }

        public void SetSize(string id, int size)
        {
            var item = Find(id);
            if (item != null)
            {
                item.Size = Math.Clamp(size, SmallSize, LargeSize);
            }
        }

        /// <summary>
        /// Works out each panel's row and column. Two half width panels that are next to each other in the order share a row;
        /// a half width panel with no partner takes the left side of its row.
        /// </summary>
        public List<PanelCell> Pack()
        {
            var cells = new List<PanelCell>();
            var row = -1;
            var waitingForPartner = false;

            foreach (var item in Items)
            {
                if (item.Half)
                {
                    if (waitingForPartner)
                    {
                        cells.Add(new PanelCell(item.Id, row, 1, 1));
                        waitingForPartner = false;
                    }
                    else
                    {
                        ++row;
                        cells.Add(new PanelCell(item.Id, row, 0, 1));
                        waitingForPartner = true;
                    }
                }
                else
                {
                    ++row;
                    cells.Add(new PanelCell(item.Id, row, 0, 2));
                    waitingForPartner = false;
                }
            }

            return cells;
        }

        public string ToJson() => JsonSerializer.Serialize(Items);

        /// <summary>
        /// Loads a saved layout. Anything unexpected (bad data, panels that no longer exist, repeats, missing panels) is
        /// repaired: unknown panels are dropped and missing ones are added at the end, so a bad save can never hide a panel.
        /// </summary>
        public static PanelLayout FromJson(string json, IEnumerable<string> knownIds)
        {
            var known = knownIds.ToList();
            var layout = new PanelLayout();

            try
            {
                var saved = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<List<PanelLayoutItem>>(json);
                if (saved != null)
                {
                    foreach (var item in saved)
                    {
                        if (item?.Id != null && known.Contains(item.Id) && layout.Find(item.Id) == null)
                        {
                            layout.Items.Add(new PanelLayoutItem
                            {
                                Id = item.Id,
                                Half = item.Half,
                                Size = Math.Clamp(item.Size, SmallSize, LargeSize),
                            });
                        }
                    }
                }
            }
            catch (JsonException)
            {
                layout.Items.Clear();
            }
            catch (NotSupportedException)
            {
                layout.Items.Clear();
            }

            foreach (var id in known)
            {
                if (layout.Find(id) == null)
                {
                    layout.Items.Add(new PanelLayoutItem { Id = id });
                }
            }

            return layout;
        }
    }
}
