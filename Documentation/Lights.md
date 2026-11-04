# Lights

The **Lights** panel on the board screen turns the board's lights on or off: tap the panel to toggle.

## Protocol

`LightMode` (`E659F30C-EA98-11E3-AC10-0800200C9A66`): `0` is off and `1` is the default lights. The app writes the mode
and trusts the write: it does not read the mode back (the board can be slow to report a new value, which made
successful changes look like failures). If the board rejects the write, nothing happens and nothing is shown.

These values come from community documentation ([pOnewheel](https://github.com/ponewheel/android-ponewheel)).

## Not supported

Stock Onewheel lights are white and red only, so there is no RGB colour. A white/red mixer and fade effects were
tried and removed because they did not work reliably. The front and back colour characteristics
(`E659F30D` and `E659F30E`, two bytes `[white, red]`, each `0..0x75`, used when the light mode is `2`) are
documented by pOnewheel but were never verified on a board.
