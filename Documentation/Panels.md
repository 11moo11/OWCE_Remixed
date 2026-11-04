# Moving and resizing panels

The board screen is made of panels: Speed, Battery and current, Temperatures, Ride mode and Lights. Battery and current
are one panel (battery on the left, current on the right, tap it for the cell voltages).

## Editing

* **Hold a panel and drag it** onto another one to move it there, or choose **Edit panels** in the side menu.
* While editing each panel is dimmed and shows buttons: **up** and **down** (move one place), **FULL / HALF** (width) and
  **S / M / L** (size). A bar at the top has **Reset** (back to the default layout) and **Done**.
* The list scrolls by itself when you drag a panel near the top or bottom of the screen.
* The layout is saved and comes back the next time you open the app.

## How the layout works

The screen is a two column grid. A **full** panel takes a whole row. Two **half** panels that are next to each other in the
order share a row, and a half panel with no partner takes the left side of its row.

**Size** scales everything in the panel (text, bars, spacing), so a small panel is a compact version of the same panel and
a large one is easier to read at a glance. Turning a panel to half width starts it at S, because a medium panel is
designed for the full width of the screen.

## Code

* [PanelLayout.cs](../OWCE/OWCE/PanelLayout.cs): order, widths, sizes, row packing, saving and loading (no UI). A saved
  layout that is damaged or out of date is repaired, so a panel can never go missing.
* [PanelBoard](../OWCE/OWCE/Views/PanelBoard.cs): the grid, drag and drop, the edit bar.
* [PanelHost](../OWCE/OWCE/Views/PanelHost.cs): one panel plus its edit buttons.
* [PanelScaleBox](../OWCE/OWCE/Views/PanelScaleBox.cs): lays a panel out at a different width and scales it to fit.
* To add a panel: create the view, declare it in the `PanelSource` in `BoardPage.xaml`, and add it to `SetupPanels` in
  `BoardPage.xaml.cs` and to `PanelLayout.DefaultOrder`.

## Not done

Panels are resized with buttons, not by dragging a corner like an Android widget. Drag handles inside a scrolling list
fight with the scrolling and I could not test that.
