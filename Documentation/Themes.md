# Themes

Pick one in **Settings > Appearance**. A theme is applied and saved as soon as you tap it.

| Theme | Idea |
|---|---|
| Default | The original OWCE look. |
| Material | Material Design 3 baseline: tonal purple surfaces, large rounded corners, soft shadows. |
| Devil Trigger | Devil May Cry: black, crimson and bone, hard edges, red glow. |
| Codec | Metal Gear Solid codec screens: green phosphor on black, thin outlines, no rounding. |
| Underground | Need for Speed Underground: night city, hot pink, cyan and amber neon. |
| Nanotech | Ratchet & Clank: deep sci-fi blue with bright orange, yellow and cyan, very round. |
| Master Thief | Sly Cooper: rooftop-at-night indigo with gold, a teal accent and a gold outline. |
| Keyblade | Kingdom Hearts II: command-menu navy and sky blue with a gold highlight and a blue glow. |
| City Escape | Sonic Adventure 2: black and yellow with hero blue and rival red. |

The PS2-inspired palettes are interpretations of each game's general look, not copies of their UIs. The sources I could
find confirm structure (for example [Devil May Cry's orb and vitality gauge HUD](https://devilmaycry.fandom.com/wiki/Orbs),
[the codec as a green-tinted dialogue screen](https://knowyourmeme.com/memes/codec-screen)) but not exact colours or
fonts, so the colours are my own. All themes use the system font.

## How it works

A theme is a `ResourceDictionary` in [OWCE/OWCE/Themes](../OWCE/OWCE/Themes). Every theme defines the **same keys**:

* page colours: `BackgroundGradientStart`, `BackgroundGradientEnd`, `OnBackgroundColor`
* surfaces: `SurfaceColor`, `OnSurfaceColor`, `MutedTextColor`, `DividerColor`, `ScrimColor`, `CardColor`, `OnCardColor`
* buttons and sliders: `ButtonColor`, `OnButtonColor`, `ButtonBorderColor`, `SelectedButtonColor`,
  `OnSelectedButtonColor`, `SliderActiveColor`, `SliderInactiveColor`
* each panel: `SpeedPanel`, `BatteryPanel`/`BatteryHeader`/`BatteryBar`, `PowerPanel`/`PowerBar`,
  `TempPanel`/`TempTrack`/`TempBar`, `RideModePanel`, `LightsPanel`, each with an `On...` text colour
* the settings style look: `CardSurfaceColor` (a row's background), `AccentColor` (headings, buttons, selection) and `OnAccentColor`
* brush versions of some colours, for properties that need a brush: `DividerBrush`, `OnBackgroundBrush`, `ButtonBorderBrush`, `MutedTextBrush`, `AccentBrush`
* styles: `BoardDetailBlockBaseStyle` (the panels' shape, outline and shadow), `RoundButtonStyle`, `SelectedRoundButtonStyle`

The rest of the app refers to those keys with `{DynamicResource ...}`, so applying a theme restyles what is already on
screen. `ThemeManager` swaps the dictionary at runtime. The speed dial and battery cells are drawn in code and listen for
a theme change. Names starting with `On` are text drawn on the matching background.

## Adding a theme

1. Copy an existing theme (`.xaml` and `.xaml.cs`), change the class name in both and the `x:Class`, and edit the values.
2. Add it to the list in `ThemeManager.cs`.
3. Check that every text colour is readable on the colour it sits on (aim for a contrast ratio of 4.5:1 or more).

Colours that come from a theme must use `DynamicResource`. A `StaticResource` or hardcoded colour will not change with the theme.

Icons are black images. To make one follow the theme, tint it with `ThemedIconTint` (not a plain
`IconTintColorBehavior` with a `DynamicResource`, which does not update when the theme changes):

```xml
<Image Source="menu_settings">
    <Image.Behaviors>
        <views:ThemedIconTint ThemeColorKey="OnSurfaceColor" />
    </Image.Behaviors>
</Image>
```

## Not themed

The unused angle and footpad views, and system pop-ups such as the list that opens from the theme picker, keep their own
colours. The battery cell grid fades from the theme's battery bar colour towards white instead of magenta, with black or
white text depending on which is easier to read.

## The settings style look

The Settings, About, Past rides, Submit ride and Raw data screens use a Material Design 3 style layout:

* A **large title** under a small bar with a back arrow (`UseLargeTitle="True"` on the page). Actions on the right, like
  Save, are accent coloured text.
* Content in **rounded groups**: put `SettingsItem`s inside a `SettingsGroup`. Only the group's outer corners are very
  round, the corners between items are only slightly round, and there is a small gap between items.
* Small accent coloured headings above each group (`Style="{StaticResource SectionHeaderStyle}"`).
* The theme picker shows a small picture of each theme.
* Dialogs are neutral rounded surfaces with a text button (`DialogButtonStyle`). The bottom navigation bar (Actions, Dashboard, Settings) is a rounded pill with a tinted, outlined selected item.

```xml
<views:SettingsGroup>
    <views:SettingsItem Title="Imperial units" Subtitle="Show speed in mph">
        <Switch IsToggled="{Binding Imperial}" />
    </views:SettingsItem>
    <views:SettingsItem Title="Version" ShowChevron="True" Command="{Binding OpenCommand}" />
</views:SettingsGroup>
```

## The dashboard panels

The board screen's panels keep their bold colours and use the same system font as the rest of the app, but are laid
out the same modern way:

* a small spaced-out caption (`PanelCaptionStyle`), then the number, left aligned;
* slim rounded progress bars (battery, current and the three temperatures) instead of filling the whole row;
* the speed is a semicircle gauge with a rounded track and an arc that grows with the speed;
* the Lights panel has a switch-style pill that shows on or off;
* the panels' corner radius, outline and shadow come from the theme (`BoardDetailBlockBaseStyle`): Default and Material
  are round and soft, Codec and Devil Trigger are sharp.


## The five colours

Every theme, preset or your own, is made from five colours, the way [Realtime Colors](https://www.realtimecolors.com/docs/selecting-colors)
describes a palette: neutral **text** and **background** colours, one **primary** colour, a less highlighted **secondary**
colour, and an **accent** colour "used for highlights, checkmarks, boxes, links, etc." (Realtime Colors' docs only define the
roles at that level; they do not publish how shades are made, so the shades below are my own.)

| Colour | Used for |
|---|---|
| Background | The page, and (a touch lighter or darker) the cards, bars and dividers on it. |
| Text | Text and icons on the page. Quiet text and dividers are mixes of text and background. |
| Primary | The main colour: the speed and battery panels, the board cards, selected buttons. |
| Secondary | The quieter colour: the lights and ride mode panels and soft buttons. |
| Accent | Highlights: the selected tab, sliders, switches, headings, links, the edge light glow, and the power and temperature panels. |

[ColorScheme](../OWCE/OWCE/Themes/ColorScheme.cs) turns the five into all the colours the app uses. The picked colours are used
as they are (a panel is exactly the colour you picked, with a slightly darker or lighter shade of it for bars and tracks).
Only text is adjusted: text on a panel is black or white, whichever reads better, and the page text, quiet text and the
accent are nudged lighter or darker where needed so they stay readable on the page (4.5 : 1). I checked this with every preset
and about 2,000 random sets of five colours whose text and background contrast reasonably (3,000 pairs, none below 4.5 : 1).
If you pick a text colour that is close to the background, readability can't be guaranteed, since you chose it.

**Light** and **Dark** are shortcuts: they set the background and text to the usual dark or light page colours, lightly tinted
with the primary colour (so a dark theme from a green is dark green, not grey). You can then change either colour.
Each preset is just a starting set of five colours (plus corners, effect and font), and **Reset colours** goes back to them.

## The theme builder

Every theme is a `ThemeSpec` ([ThemeSpec.cs](../OWCE/OWCE/Themes/ThemeSpec.cs)): a colour palette (one of the presets above)
plus a few simple settings:

* **Panel corners:** Very round, Round or Sharp (28, 14 or 2 units).
* **Panel effect:** None, Drop shadow, or Edge light (a thin accent outline with a glow).
* **Font:** the system font or one of the bundled Google Fonts, see [Fonts.md](Fonts.md).
* **The five colours** (see above), with Light and Dark as shortcuts.

The presets are specs too (each with its own five colours, default corners and effect; all use the system font). Because every colour is generated from the five, the presets are no longer hand tuned palettes: they are tonal and flat compared with the old neon versions. Change any setting under
**Settings > Appearance** and it applies straight away. **Save as a new style** keeps your version, which then appears
next to the presets; **Save changes** updates it and **Delete** removes it. Saved styles and the theme in use are kept
in preferences (`theme_user_styles`, `theme_current`). A preset whose corners and effect are left alone keeps its hand
made panel style; changing either builds the panel style from the settings.

Corners and effect apply to the dashboard panels. The settings cards, buttons and the bottom bar keep their own shapes.

## The app icon

On Android the launcher icon follows the theme. There are 15 icons (the original yellow plus 14 colours, made from the same artwork by
`Resources/AppIcon/appicon_*.png`), and the one closest to the theme's primary colour is used ([AppIconCatalog](../OWCE/OWCE/AppIconCatalog.cs)).
Android cannot draw an icon at run time, so a custom colour gets the nearest of the 15. Mechanically, the manifest has one launcher
entry (an `activity-alias`) per icon, and [AppIconSwitcher](../OWCE/OWCE/Platforms/Android/AppIconSwitcher.cs) turns one on and the
others off. This happens when the app goes to the background, because some launchers close the app when its icon changes, and the
launcher can take a few seconds to show the new icon. The splash screen colour is fixed (it is part of the app's theme) and iOS keeps
its single icon.
