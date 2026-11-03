# Custom Shaping

When a board is in **Custom** ride mode, the ride mode panel shows three sliders:

| Slider | Shown as | Raw board value |
|---|---|---|
| Aggressiveness | 0 - 10 | -80 .. 127 |
| Turn compensation | 0 - 10 | -100 .. 100 |
| Angle offset | -3.0 .. 1.5 degrees (positive = nose up) | `degrees / 0.05`, i.e. -60 .. 30 |

Supported boards: Plus, XR, GT, and **Rewheel patched** Pints. A stock Pint falls back to another ride mode when
Custom is selected, so Custom shaping on a Pint needs the Rewheel patch. Pint X and V1 are not supported.

## How it works

* The board exposes the settings ("ride traits") on characteristic `E659F31E-EA98-11E3-AC10-0800200C9A66`.
* Each setting is a 2 byte frame: `[value, trait]`. `value` is a signed byte (two's complement).
  Traits: `0` angle offset, `1` turn compensation, `2` aggressiveness (`3` is Simple Stop, not used here).
* When the app subscribes to the characteristic the board reports each trait as a notification. The app uses this to
  start the sliders at the board's real settings, then unsubscribes (Android limits simultaneous notifications).
  **Reload from board** subscribes again.
* Changes are written when you let go of a slider, not while dragging.

Code: [CustomShaping.cs](../OWCE/OWCE/CustomShaping.cs) (conversions and frame format),
[OWBoard.cs](../OWCE/OWCE/OWBoard.cs) (state and writes), [RideModeView](../OWCE/OWCE/Views/RideModeView.xaml.cs) (UI).

## Safety guards

* Values are clamped to the ranges above before anything is sent.
* Nothing is written unless the board is in Custom ride mode. Changes are allowed while riding.
* A one time notice is shown before the first change.

## Provenance and what is not verified

The characteristic, frame layout, trait ids and value ranges come from community work on OWCE: upstream pull request
[#85](https://github.com/OnewheelCommunityEdition/OWCE_App/pull/85) ("Simple Stop + Custom Shaping") as merged in
[sirmammingtonham/OWCE](https://github.com/sirmammingtonham/OWCE). Both are GPL-3.0, like this project. This code is
an independent implementation, not a copy. The angle offset sign is flipped compared with that code so that a positive
value tilts the nose up (confirmed by riding a Rewheel patched Pint).

**This has not been checked against official Onewheel app traffic**, and the app does not read a value back after
writing it: the sliders show what was sent. Treat the ranges as the community's best knowledge, test with small
changes, and do not expect it to work on firmware that Future Motion has locked down.
