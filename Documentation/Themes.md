# Themes

Pick one in **Settings > Appearance**. A theme is applied and saved as soon as you tap it.

| Theme | Idea |
|---|---|
| Default | The original OWCE look. |
| Material | Material Design 3: tonal surfaces, large rounded corners, soft shadows. Pick any colour (swatches or a hue slider) and switch on dark mode; the whole scheme is built from your colour. |
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


## Material colours and dark mode

When Material is selected, Settings shows a colour picker and a **Dark mode** switch. The colour is a *seed*:
[MaterialPalette](../OWCE/OWCE/Themes/MaterialPalette.cs) builds the whole scheme from it (tones of the seed, a muted
version, a shifted one and tinted greys), in light or dark. It is an HSL approximation of Material's tonal palettes,
not the exact colour science. Every text colour is checked against what it sits on and adjusted until it is readable
(4.5 : 1). The default purple in light mode is the hand written baseline in `MaterialTheme.xaml`. The choices are
saved as `material_seed` and `material_dark`. `MaterialTheme.xaml.cs` is hand written (not generated) because it applies
the palette, so keep it if you regenerate the other themes.
