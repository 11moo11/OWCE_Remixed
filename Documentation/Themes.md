# Themes

Pick one in **Settings > Theme**. Choosing a theme previews it straight away; **Save** keeps it, **Cancel** (or leaving
the page) puts the old one back.

| Theme | Idea |
|---|---|
| Default | The original OWCE look. |
| Material | Material Design 3 baseline: tonal purple surfaces, large rounded corners, soft shadows. |
| Devil Trigger | Devil May Cry: black, crimson and bone, hard edges, red glow. |
| Codec | Metal Gear Solid codec screens: green phosphor on black, thin outlines, no rounding. |
| Underground | Need for Speed Underground: night city, hot pink, cyan and amber neon. |
| Nanotech | Ratchet & Clank: deep sci-fi blue with bright orange, yellow and cyan, very round. |

The PS2-inspired palettes are interpretations of each game's general look, not copies of their UIs. The sources I could
find confirm structure (for example [Devil May Cry's orb and vitality gauge HUD](https://devilmaycry.fandom.com/wiki/Orbs),
[the codec as a green-tinted dialogue screen](https://knowyourmeme.com/memes/codec-screen)) but not exact colours or
fonts, so the colours are my own. All themes use the same font (Saira Extra Condensed).

## How it works

A theme is a `ResourceDictionary` in [OWCE/OWCE/Themes](../OWCE/OWCE/Themes). Every theme defines the **same keys**:

* page colours: `BackgroundGradientStart`, `BackgroundGradientEnd`, `OnBackgroundColor`
* surfaces: `SurfaceColor`, `OnSurfaceColor`, `MutedTextColor`, `DividerColor`, `ScrimColor`, `CardColor`, `OnCardColor`
* buttons and sliders: `ButtonColor`, `OnButtonColor`, `ButtonBorderColor`, `SelectedButtonColor`,
  `OnSelectedButtonColor`, `SliderActiveColor`, `SliderInactiveColor`
* each panel: `SpeedPanel`, `BatteryPanel`/`BatteryHeader`/`BatteryBar`, `PowerPanel`/`PowerBar`,
  `TempPanel`/`TempTrack`/`TempBar`, `RideModePanel`, `LightsPanel`, each with an `On...` text colour
* styles: `BoardDetailBlockBaseStyle` (the panels' shape, outline and shadow), `RoundButtonStyle`, `SelectedRoundButtonStyle`

The rest of the app refers to those keys with `{DynamicResource ...}`, so applying a theme restyles what is already on
screen. `ThemeManager` swaps the dictionary at runtime. The speed dial and battery cells are drawn in code and listen for
a theme change. Names starting with `On` are text drawn on the matching background.

## Adding a theme

1. Copy an existing theme (`.xaml` and `.xaml.cs`), change the class name in both and the `x:Class`, and edit the values.
2. Add it to the list in `ThemeManager.cs`.
3. Check that every text colour is readable on the colour it sits on (aim for a contrast ratio of 4.5:1 or more).

Colours that come from a theme must use `DynamicResource`. A `StaticResource` or hardcoded colour will not change with the theme.

## Not themed

The unused angle and footpad views, and system pop-ups such as the list that opens from the theme picker, keep their own
colours. The battery cell grid fades from the theme's battery bar colour towards white instead of magenta, with black or
white text depending on which is easier to read.
