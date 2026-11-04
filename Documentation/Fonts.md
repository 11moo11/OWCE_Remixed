# Fonts

The theme builder (Settings > Appearance) can set the font for the whole app. The choices are free
[Google Fonts](https://fonts.google.com/) bundled in
[OWCE/OWCE/Resources/Fonts](../OWCE/OWCE/Resources/Fonts), all under the
[SIL Open Font License 1.1](https://openfontlicense.org/):

Poppins, Space Grotesk, Oswald, Bebas Neue, Rajdhani, Exo 2, Chakra Petch, Orbitron, Michroma, Audiowide, Russo One,
Righteous, Share Tech Mono and Press Start 2P (plus the system font). The files come from the
[google/fonts](https://github.com/google/fonts) repository; copyright belongs to each font's authors (see the name table
inside each file).

They are bundled rather than downloaded at run time so they work offline and need no extra permissions. To add one:
put the `.ttf` in `Resources/Fonts` (the file name, without `.ttf`, is the family name), and add a line to
`FontCatalog` in [ThemeSpec.cs](../OWCE/OWCE/Themes/ThemeSpec.cs).

Some of these are *variable* fonts (Orbitron, Exo 2, Oswald, Space Grotesk). Only their regular weight is used, and bold
text is the regular weight made bolder by Android, so it is a little less refined than a true bold.
