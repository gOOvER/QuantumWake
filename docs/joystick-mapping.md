# Joystick mapping: what the files hold, and what 0.15 could do with it

Research, 2026-09-17, for a Controls page: the game's own keybinding screen
is hard to use with a HOTAS, bindings break when sticks get re-enumerated,
and there is no backup. Everything below is read from this install (Alpha
4.10, six joystick devices) and from a Windows API probe on this machine;
the numbers are what to check against after a patch.

## Where the bindings live

**`user\client\0\Profiles\default\actionmaps.xml`** is the live profile,
plain XML, rewritten by the game when a binding changes (17.8 KB here,
last written 2026-09-16 23:56). Three parts:

- `<deviceoptions name="Throttle - HOTAS Warthog  {0404044F-…}">` - per
  device, dead zones per axis.
- `<options type="joystick" instance="2" Product="Throttle - HOTAS Warthog
  {0404044F-0000-0000-0000-504944564944}">` - **which product holds which
  instance number**, with per-action curves (`flight_move_pitch
  exponent="1.5"`, `flight_strafe_longitudinal invert="1"`). Eight
  joystick instances here, six with a product: F16 MFD 2 (js1), Warthog
  throttle (js2), T-Pendular rudder (js3), Warthog stick (js4), an Arduino
  Micro (js5), F16 MFD 1 (js6).
- `<actionmap name="seat_general"><action name="v_eject"><rebind
  input="js6_button7"/>` - the bindings themselves: 160 rebinds across the
  six devices (js1 25, js2 46, js3 5, js4 26, js5 41, js6 17), by
  **instance number**, never by product.

That last line is the swap problem in one sentence. A binding says `js2`,
and `js2` is whichever device the game enumerated second at start. Unplug
the throttle, plug it into another port, and the pedals may be `js2`;
every throttle binding now points at the pedals. The `<options>` block is
the only record of what `js2` was meant to be.

**`user\client\0\controls\mappings\<name>.xml`** is the export format
(`pp_rebindkeys` / the keybinding screen's export): the same document
under a `<CustomisationUIHeader>` naming the devices and categories.
`layout_nick_exported.xml` here (2026-05-10, 149 rebinds) is an older
export of the same profile - already 11 bindings behind the live file.

**`Data\Libs\Config\defaultProfile.xml`** in `Data.p4k` is the catalogue:
**50 action maps, 1,106 actions**, 1,032 with a `UILabel` (`@ui_CIEject`)
and a `UIDescription`, grouped by `UICategory` (`@ui_CCSpaceFlight`), each
with its default keyboard, mouse, gamepad and joystick input and its
activation mode (`tap`, `hold`, `double_tap`, `delayed_press_long`… - 20
modes, defined in the same file). It is **CryXmlB**, the engine's binary
XML, not text: an 8-byte magic, node/attribute/child tables and a string
table. A reader is sixty lines and was written for this probe (1,531
nodes, 8,386 attributes decode cleanly); nothing in the app reads CryXmlB
yet.

**Labels resolve.** Of 837 distinct `UILabel` keys, 770 have a string in
`global.ini` (`ui_CIYawView=Look left / right`, `ui_CGSpaceFlightView=
Vehicles - View`), which the app already loads for item names; the 67
without one fall back to the action id.

**`Data\Libs\Config\keybinding_localization.xml`** names every key and
button per device (`@input_key_keyboard_a`…), 472 nodes, CryXmlB too.

**`Data\Libs\Config\Mappings\`** ships **17 reference layouts**: Warthog
(with and without pedals), X52, X52 Pro, X55, X56, G940, T16000M (single,
dual, with TWCS), T.Flight HOTAS X, VKB SCG and GNX premium dual,
GameGlass, plus `layout_all_blank`. Each is the export format with CIG's
own bindings for that hardware, keyed by Product GUID - the Warthog one
has 73 rebinds. That is the "what should be mapped" answer for anyone with
one of those sticks: CIG's default for exactly this device.

## Knowing the device

The game's `Product` GUID is not opaque: `{0404044F-0000-0000-0000-
504944564944}` is **PID 0404, VID 044F** (Thrustmaster) and the ASCII
"PIDVID". Windows lists the same six as HID game controllers
(`HID\VID_044F&PID_0404`, `…B351`, `…B352`, `…B68F`, `…0402`,
`HID\VID_2341&PID_8037` for the Arduino).

**Windows.Gaming.Input reads them live, without DirectInput.** A probe on
`net10.0-windows10.0.19041.0` - the tray app's target, so nothing new to
add - enumerated all six through `RawGameController` with their counts and
ids, and composed the game's GUID from them exactly:

| Device | VID:PID | Buttons | Axes | Hats | GUID composed |
| --- | --- | --- | --- | --- | --- |
| Warthog stick | 044F:0402 | 19 | 2 | 1 | `{0402044F-…}` ✓ |
| Warthog throttle | 044F:0404 | 32 | 5 | 1 | `{0404044F-…}` ✓ |
| T-Pendular rudder | 044F:B68F | 0 | 3 | 0 | `{B68F044F-…}` ✓ |
| F16 MFD 1 / 2 | 044F:B351 / B352 | 28 | 0 | 0 | ✓ |
| Arduino Micro | 2341:8037 | 32 | 0 | 0 | `{80372341-…}` ✓ |

`GetCurrentReading` returns the button, hat and axis state on demand, so
"press a button and see what it does" is a poll loop, not a driver. Two
things the probe did not settle, to test first thing: whether readings
change while the game holds the device (DirectInput non-exclusive should
allow it), and that the API keeps answering from a tray process with no
foreground window (it enumerated from a console; button presses were not
exercised, nobody was at the stick).

`DisplayName` is the generic "HID-compliant game controller"; the product
name the game shows comes from the HID product string, readable through
`hid.dll` (`HidD_GetProductString`) or the `MediaProperties\…\Joystick\OEM`
registry key - or, for any device the game has already seen, from the
`<options Product=…>` line in the profile.

## What is doable

**A Controls page under Settings**, from `actionmaps.xml` and the
catalogue alone - no device access needed, so it works in the plain
server too:

- Every device the profile knows, by product name and instance, with the
  button-by-button table: button 7 → *Eject* (Seat, hold), button 12 →
  *Cycle operator mode back*; the unbound buttons listed as such. Axes
  the same, with dead zone and curve from `<deviceoptions>`/`<options>`.
- The other way round: every action by category, with its binding on each
  device and the keyboard default, labelled in the game's words, and the
  ones bound nowhere on any stick - the "sea of buttons" read as a list
  with gaps.
- Search across both.
- For a stick with a shipped layout, CIG's reference beside the pilot's:
  what the Warthog layout puts on button 7, what this profile does.

**Backups, automatically.** A watcher on `actionmaps.xml` (the log watcher
already does this for `Game.log`): each distinct version kept under
`%LOCALAPPDATA%\Quantumwake\controls\` with its time, a history list, a
diff between any two ("since Tuesday: js2_button11 moved from *Guns mode*
to *Cycle operator mode forward*, 3 bindings gone"). Restore writes a
file into `controls\mappings\` for the game to import, never over the
live profile while the game runs - the game rewrites that file itself,
and a write under it is lost or worse.

**The swap fix.** When a device's instance changes - the throttle that
was js2 is now js3 - the profile's `<options>` block says what each
instance was, the live enumeration says what it is, and rewriting the
export with every `js2_` for that product turned into `js3_` is string
work. Offered as "retarget this export to the devices as they are now",
imported in-game. The one thing not readable is the game's own current
numbering (DirectInput enumeration order) without DirectInput; the page
can show the mismatch only once the game has written a new profile, or
ask the pilot which stick is which.

**Live: press it, see it.** In the tray app, a poll over
`RawGameController` pushed on the live stream; the page highlights the
button on the device and shows what it is bound to. The plain server (no
Windows API) says so instead.

## What is not

- **A picture of the stick from the install.** `Data.p4k` has none (the
  UI icon folders were listed for the Armoury work). The community has a
  library, though - next section - so the fallback drawing from counts
  (19 buttons, 2 axes, a hat, as a labelled grid that lights up) is for a
  device nobody has drawn yet, not the main case.
- **Editing bindings in place.** The app can write a layout for import;
  it should not be the game's keybinding screen. The game validates
  conflicts and activation modes on import; a file the app wrote wrong
  fails there, visibly, rather than silently in play.
- **The game's live instance order**, as above.

## Pictures of sticks: the community library

Checked 2026-09-17 after the first draft said there were none.
**[Joystick Diagrams](https://github.com/Rexeh/joystick-diagrams)** (Rexeh,
GPL-2.0, Python) does for DCS, MSFS and Star Citizen roughly what this
page would do: read the game's bindings, print them on a picture of the
stick. Its value here is the **template library**: 84 devices on
[joystick-diagrams.com/templates](https://www.joystick-diagrams.com/templates/)
- 45 in the repository's `templates/` folder, 39 more shared by users on
its Discord. Cloned and counted: 48 SVGs in the repo - Thrustmaster
Warthog stick and throttle, T.16000M (left, right, throttle), Saitek X52
and X56, VKB Gladiator NXT L/R, Virpil Alpha/Alpha Prime/WarBRD/VFX/MT-50
stick and throttle and a control panel, CH Fighterstick and Pro Throttle,
Total Controls MFD and button box, 22 WinWing panels and grips.

**The format is trivial, which is the point.** Each template is a draw.io
SVG: a photograph of the device embedded as PNG (the Warthog stick's is
1.5 MB), and over it plain `<text>` elements whose content is a
placeholder - `Button_1` … `Button_19`, `POV_1_U/D/L/R`, `AXIS_X`. The
tool's whole rendering step is a regex over those (`\bBUTTON_\d+\b`,
`\bAXIS_[a-zA-Z]+_?\d?\b`, case-insensitive) replacing each with the
binding's label. The Warthog templates were checked: stick 19 buttons and
one hat, throttle 32 buttons and one hat - the same counts
Windows.Gaming.Input reports for the real devices above, and the same
numbering the game's `js4_button7` uses. So a browser can do the
rendering natively: load the SVG, find the text node for `Button_7`,
write *Eject* into it, and for a live press draw a ring at that node's
`x`/`y`. Nothing needs Python.

Its Star Citizen plugin
([joystick-diagrams-star-citizen-plugin](https://github.com/Rexeh/joystick-diagrams-star-citizen-plugin))
parses the same profile the same way - `<options instance= Product=>`,
the GUID's last 36 characters, `jsN_buttonM` / `hatM_up` / axis names -
and labels actions from a hand-written table of ~200 names, since it
does not read the CryXmlB catalogue. This app would label all 1,106 from
the game's own strings.

**How to use it without inheriting the licence question.** The code is
GPL-2.0 and the templates ship under the repository; the photographs
inside them are of unstated provenance - several look like maker product
shots. So: not bundled. Fetched at runtime, on the same opt-in switch as
every other feed (see `feeds-stay-optional`), from the repository's raw
URLs by device, cached under `community\joystick-templates\`, credited
on the page with a link to the project - the way the Garage takes a
cooler's photo from the wiki. And two doors beside it: point the app at
a Joystick Diagrams install or any folder of its templates (the 39
Discord ones live nowhere else), and drop any SVG that follows the
`Button_N` convention, which is also what a pilot would draw over their
own photo. Matching a device to a template is by product GUID in a small
table this app keeps (Warthog stick `0402044F` → "Thrustmaster Warthog -
Joystick.svg"), with the pilot's choice overriding it.

Makers publish sheets too - Virpil's button-mapping worksheets on its
support site, Thrustmaster's Warthog template - as PDFs for the owner's
use, which the pilot can bring in through the same door; they are not
redistributable and the app should not try.

An older alternative, [hotasmap](https://github.com/RudolfCardinal/hotasmap),
covers the Warthog and MFG Crosswind only.

## Cost, roughly

A CryXmlB reader in Core (small, testable against the two files); the
template fetch and the SVG renderer (a text substitution and a ring at a
node's coordinates, in the browser); a
profile reader and a catalogue reader with label resolution; a
`ControlsStore` for backups; three endpoints; one page with two views and
a diff; a `RawGameController` service in the tray host with a stub in the
server. The reading half is a few days; the live half a day once the two
open questions are answered; the pilot-taught picture is its own line of
work later.

## Built: 0.15.0

The reading half, as planned above, on branch `controls`:

- `CryXml` (Core/GameData) decodes CryXmlB; `ControlInput`,
  `ControlProfile`, `ControlCatalogue`, `GameControls` and
  `ControlsExport` (Core/Controls) read and write the documents.
  `GameCommodities.Controls` carries the catalogue and the 17 layouts in the
  game-data cache. One thing the first read missed: a third of the action
  labels exist in `global.ini` only under a `,P` line
  (`ui_CIMissileMode,P=Toggle Missile Operator Mode`); the catalogue reads
  that as the fallback, which takes the labelled count from 753 to over a
  thousand.
- `ControlsStore` (Data) keeps every distinct `actionmaps.xml` under
  `controls\backups\`, named by time and content hash;
  `ControlsWatchService` (Server) looks every five seconds. `ControlsDiff`
  says what changed; `ControlsExport` writes the import form, retargeted.
- `JoystickTemplates` (Data) is the opt-in feed: the repository's tree from
  the GitHub API for the index, each SVG from raw.githubusercontent.com,
  kept under `controls\templates\`; a GUID table for the sticks this
  install and the shipped layouts name; a folder of the pilot's own.
- `GET /api/controls` and the rest in `ControlsEndpoints`; the page in
  `web/`, four panes under Settings → Controls. The SVG is fetched as
  text and the placeholders rewritten in the browser
  (`controlsDrawSvg`), long names cut to fit the template's boxes with
  the table holding them whole.
- Checked on this install 2026-09-17: 6 sticks, 101 real joystick
  bindings (59 more rebinds are `jsN_ `, defaults taken away), 45
  templates indexed, the Warthog stick and throttle matched and drawn.

Not built yet: the live half (`RawGameController` in the tray host, the
pressed button lit on the picture), and the pilot-taught picture.
