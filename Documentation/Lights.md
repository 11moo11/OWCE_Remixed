# Lights

The **Lights** panel on the board screen:

* Tap the panel to turn the board's lights on (default lights) or off.
* Tap the arrow to open **custom colours**, **effects** and **Reset to default lights** (puts the board back in its
  default light mode).

## Colours

Stock Onewheel lights are **white and red only** (white at the front, red at the back by default). There is no RGB
hardware, so "custom colours" mixes the white and red LEDs separately for the front and the back (0-100%).
Aftermarket RGB lights use their own controllers and are not supported.

## Effects

All effects only use white and red, and are slow fades (never a strobe): **Breathe** (normal colours pulse),
**Fade** (both ends fade white to red and back together), **Swap** (front and back fade between white and red in
opposite phase). Speed and brightness can be changed while an effect runs. Tapping the running effect again stops it
and puts the lights back how they were. Updates go out about 5 times per second because Bluetooth writes are slow.

## Protocol

* `LightMode` (`E659F30C`): `0` off, `1` default lights, `2` manual. Levels are only used in manual mode.
  The app does not read the mode back after writing it (the board can be slow to report a new value, which made
  successful changes look like failures). If the board rejects a write, nothing happens and nothing is shown.
* `LightsFront` (`E659F30D`) / `LightsBack` (`E659F30E`): 2 bytes `[white, red]`, each `0..0x75` (117).
  The app's BLE layer reverses bytes for us, see [Lights.cs](../OWCE/OWCE/Lights.cs).
  These characteristics may only accept writes *without* a response, so the BLE layer picks the write type from what
  each characteristic supports.

These come from community documentation ([pOnewheel](https://github.com/ponewheel/android-ponewheel), which shows
`gatttool` examples for the three characteristics). **Not verified on every board or firmware**; if front white lights
red (or the reverse) on your board, the byte order is the first thing to check.
