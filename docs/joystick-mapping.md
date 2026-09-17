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

- **A picture of the stick.** The install has none: no image of any
  joystick anywhere in `Data.p4k` (the UI icon folders were listed for
  the Armoury work). Maker pictures are copyrighted. What can be drawn is
  the device from its counts - 19 buttons, 2 axes, a hat - as a labelled
  grid that lights up, which is honest and enough to answer "which button
  is 7". A pilot-taught picture is the step after: drop a photo of the
  stick, click where each button is, stored per product GUID; the same
  idea as the inventory-tile portfolio.
- **Editing bindings in place.** The app can write a layout for import;
  it should not be the game's keybinding screen. The game validates
  conflicts and activation modes on import; a file the app wrote wrong
  fails there, visibly, rather than silently in play.
- **The game's live instance order**, as above.

## Cost, roughly

A CryXmlB reader in Core (small, testable against the two files); a
profile reader and a catalogue reader with label resolution; a
`ControlsStore` for backups; three endpoints; one page with two views and
a diff; a `RawGameController` service in the tray host with a stub in the
server. The reading half is a few days; the live half a day once the two
open questions are answered; the pilot-taught picture is its own line of
work later.
