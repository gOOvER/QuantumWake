# Garage: a ship's numbers, and what a part would do to them

Plan for 0.13.0, written 2026-09-14 before any code. The question was whether
the app can show every stat for one of your ships, let you try a different
component and see what moves - DPS, shield, EM, IR, cross-section - and then
put the parts you settled on into a shopping list. It can - including EM and
IR, once the dataset's own signature model had been read rather than guessed.

## What the app already has

- **Ports per ship**, with what fits where: `digest-ship-slots.json`, read from
  the community dump's `Loadout`, already drives the per-ship *Upgrades* panel
  on the Fleet page (`/api/fleet/upgrades`): every editable port, its kind and
  size, the fitted part, and every part that would fit, with prices and shops
  from UEX.
- **Item identity** for 12,296 items straight out of the install's DataCore -
  type, size, grade, manufacturer - and the `flightReady` tag.
- **Shopping lists** (`JobStore`, kind `list`, with a destination) that the Now
  page and the overlay already carry into the seat.

What it does not have is a single number about performance. The Upgrades
panel can say a Glacier is a size 2 grade C cooler sold at Dumper's Depot; it
cannot say what it would do to the ship.

## Where the numbers are

The community dump the app already downloads (`ships.json`, 41 MB;
`ship-items.json`, 14 MB) carries them, and the digest step throws them away.
Checked on 2026-09-14 against the current files:

**Per component** (`ship-items.json`, 5,394 items, `stdItem`):

| Kind | Count | What it carries |
| --- | --- | --- |
| WeaponGun | 203 | DPS, sustained DPS, alpha, split by damage type; rate of fire; range; capacitor; ammo speed |
| Shield | 73 | max HP, regen, downed and damaged delays, absorption and resistance per damage type |
| PowerPlant | 88 | power generation rate (segments), EM signature |
| Cooler | 81 | coolant rate for power drawn, EM and IR signature |
| QuantumDrive | 63 | drive speed, spool and cooldown, fuel per distance, disconnect range |
| Missile / MissileLauncher | 68 / 144 | damage, speed, lock time and tracking signal, rack size |
| Turret | 317 | the mount; the guns on it are WeaponGuns |
| Radar | 77 | detection ranges |
| every kind | | mass, durability (HP, resistances), EM and IR emission, power draw |

**Per ship** (`ships.json`, 318 vehicles):

`CrossSection` (X, Y, Z), `Emission` (EM and IR totals for the *shields* and
*quantum* power states, broken down by component group), `Power` and `Cooling`
(generation and use in segments, by group), `ShieldsTotal`, `Weaponry`
(fixed-gun DPS with each weapon named), `Agility`, `FlightCharacteristics`
(SCM, boost, max), `QuantumTravel` (speed, range, spool, a reference trip),
`Health` and part HP, `Mass` / `MassLoadout` / `MassTotal`, insurance times, and
the full `Loadout` tree with the class of every fitted part.

So a stat sheet is a projection of data already on disk, and a swap is a
recomputation over a small set of it.

## What can be recomputed - all of it, once the model was found

The test that matters is the one the commodity work taught: recompute the
stock loadout's totals from the parts and compare with the dataset's own
figures, which the recomputation never read.

The first attempt summed each part's EM and got 4 of 269 ships right. The
per-group figures were the parts' EM times a factor that changed from ship to
ship - 1.00, 1.10, 1.13, 1.30, 0.6 - and the power plant's figure was
nothing like the plant's own number. That looked like the derivation trap
`datacore.md` records, and the plan said "estimate" for a day.

Then the dataset's own generator was read -
[`EmissionAggregator.php`](https://github.com/octfx/ScDataDumper/blob/master/src/Services/Vehicle/EmissionAggregator.php)
in octfx's ScDataDumper, credited in `credits.md` - and the model is small:

- **The factor is the ship's armour.** Every armour item carries
  `Armor.SignalMultipliers.Electromagnetic` and `.Infrared`; the Gladius's is
  1.13, the Golem's 1.10. Every group's EM is multiplied by it. (Armour also
  carries a `CrossSection` multiplier, 1.0 on everything looked at.)
- **The power plant's EM is per segment drawn**, not the plant's maximum:
  `Σ plant EM ÷ available segments × segments the fit draws`. Available
  segments for `n` plants is `Σ round(gen ÷ n) + (n − 1) × Σ size`.
- **Shields count only up to the ship's shield pool**; **weapon EM scales
  down** when the guns draw more than the weapon pool allows; thrusters are
  left out; the *shields* scenario drops the quantum drive and the *quantum*
  scenario drops the shields.
- **IR is the sum of every part's IR, times the armour, times the cooling
  load** - coolant used plus power drawn, over coolant generated. That is why
  a better cooler lowers IR: it raises the denominator.

Re-implemented from that description over the same `ship-items.json` and
`ships.json` and run on every spaceship with a stock fit:

| Figure | Recompute from parts | Stock fit matches the dataset (`--garage-check`, 2026-09-14) |
| --- | --- | --- |
| EM, shields-up and quantum | the model above | **269 of 269** within 1% |
| IR, shields-up and quantum | the model above | **269 of 269** within 1% |
| Power segments available, cooling generated | the plant rule above; coolers summed | **269 of 269** |
| Shield HP / regen | sum over fitted Shields, capped at the pool | **269 of 269** |
| Quantum range | tank ÷ the drive's fuel rate | **259 of 259** with a drive |
| Mass | the dump's stock mass, moved by the swap | **269 of 269** by construction - a parts sum undershoots every large hull, a Carrack by a fifth, because the dump has no block for much of what a big ship carries |
| Pilot-fired DPS | sum over guns not under a crewed or remote turret | **231 of 238** within 2% - see below |
| Cross-section | ship geometry × armour multiplier | parts do not move it; shown as a fact |

**The seven.** Whether a remote turret's guns are the pilot's is decided by
controller tags in the vehicle XML, which `ships.json` does not carry. The
sheet uses the port name - `TurretBase` is crewed, a `Turret` port named
"remote" or "pdc" is remote, everything else is the pilot's - and the
published dump agrees on 231 of 238 armed ships. (The dumper's newer rule,
which also reads the fitted class, agrees on 206: the file was not generated
by it.) The seven the file counts differently: Asgard and its Wikelo variant,
Cutlass Steel, Starlancer MAX and TAC and their Wikelo variants - all remote
turrets whose class says "Remote" and whose port does not. On those the
Starlancer's four turret repeaters sit in the pilot row where the file has
them in the turret row; the guns are on the sheet either way, and the total is
the same. Not chased further: the answer is in a file the dataset does not
publish.

So a stealth fit is a real answer, not an estimate: swap the Bracers for a
Glacier and the IR moves by exactly what the game's own numbers say, with the
armour and the cooling load accounted for. The page still says which scenario
it is showing - shields up, everything drawing its maximum - because that is
the one the dataset (and erkul.games) publishes, and a pilot who has pulled
power off weapons is quieter than it says.

That is also how erkul does it: the same game files, read by their own
extractor, the same component sums with the armour multipliers, and sliders
that vary the segments the power model is fed. The one thing they show that
the shipped scenario does not is the power triangle. A later version can add
the slider, because the model here takes segments as an input already.

Anything the page cannot do is said in the same sentence as the number - not
in a footnote.

## What it looks like

A **Garage** page under Gear (Loadout, Stash, Loot, **Garage**), and the Fleet
card's *Upgrades* button opens the same page on that ship rather than the
current panel.

1. **Pick a ship.** Your fleet first - the ships the logs have seen you fly,
   most flown first - then any ship in the reference. The class name is the
   key throughout, as it is everywhere else.
2. **The sheet.** Everything the dataset knows, grouped the way a pilot thinks:
   *Hull* (HP, mass, cross-section, crew, cargo), *Flight* (SCM, boost, max,
   pitch/yaw/roll), *Weapons* (fixed DPS with each gun, turret DPS, missiles),
   *Defence* (shield HP, regen, face type, resistances), *Signature* (EM, IR
   by group, both power states), *Systems* (power and cooling budget),
   *Quantum* (speed, range, spool, fuel). Every group names its source.
3. **The bench.** The ship's editable ports, each with its fitted part. Pick a
   port, and the parts that fit are listed with the figure that matters for
   that kind - DPS for a gun, HP and regen for a shield, coolant for a cooler,
   speed for a drive - plus price and where it is sold. Choosing one changes
   the sheet: each figure that moved shows *was → now* with the delta
   coloured, and a "power over budget" or "cooling over budget" line appears
   when the draw exceeds generation.
4. **Save the build.** A named plan - "Gladius, stealth fit" - kept as authored
   data (`builds.json`, `IStamped`, in the backup like the rest). Reopen it,
   compare two builds side by side, revert to stock.
5. **Shop for it.** *Add to shopping list* turns the changed parts into a job
   of kind `list`, one line per part, and proposes the destination that sells
   the most of them (or the cheapest total when one shop has them all). From
   there it is the existing flow: Now page, overlay, MFD List page.

## Storage, API, digest

- **Digest**: two new files from the same download - `digest-ship-stats.json`
  (per ship: the figures above, the loadout tree with editable flags) and
  `digest-part-stats.json` (per part class: the stat block reduced to what
  the sheet uses). Estimated 3-4 MB together; the 5.8 MB of digests today.
  Regenerating needs the ~55 MB re-download, which is the existing *Refresh
  community data* path. A digest written before these existed reads as
  "reference data predates the garage; refresh it", the sentence the Upgrades
  panel already has for slots.
- **Computation** lives in `Quantumwake.Core` as pure functions over the
  digest shapes - `ShipSheet.From(ship, fitted)` and `ShipSheet.Delta(a, b)` -
  so the WebTests can assert numbers and the server only serialises.
- **API**: `GET /api/garage/{class}` (sheet + ports + options),
  `POST /api/garage/{class}/sheet` (a proposed fit → the sheet and deltas),
  `GET/PUT/DELETE /api/garage/builds`, `POST /api/garage/builds/{id}/shop`
  (→ a job).
- **Nothing stored in a session changes.** No parser, no `PayloadVersion`.

## Verification, before it is believed

1. The stock-fit comparison above becomes a test in `Quantumwake.Tests` that
   runs against the digest and prints the match table - EM, IR and power
   segments must stay at 100%, and any figure whose rule does not reach 95%
   on stock fits is not shown on the page.
2. Take the Gladius, swap the two Bracers for Glaciers, and check the new IR
   by hand: `Σ IR × 1.13 × cooling load`, with the load recomputed from the
   Glacier's coolant generation.
3. Screenshot the sheet and the bench for one fighter and one hauler against
   the real install, and read them.

## Deliberately out of scope

- **Cross-section changes** - none exist; parts do not change geometry. (Some
  hulls - the Starlancer MAX among them - have no cross-section in the dataset
  at all; the sheet says so rather than showing zeros.)
- **Pictures of parts from the game files.** There are none: of 3,137
  `displayIcon`s in the DataCore, every one is a ship silhouette or an FPS
  loadout preset, and not one cooler, shield, plant, drive or gun has an icon.
  UEX's item records carry a `screenshot` field and it is empty for every
  component checked (coolers, plants, drives, guns: 0 of 359, and 0 of 2,244
  across every item category on 15 Sep 2026). What the app shows comes from
  the Star Citizen Wiki, fetched once and cached under
  `community/part-pictures/`, two ways in order:
  1. **By uuid**, through the wiki's item API (`/api/v2/items/{uuid}`), which
     files every item under the game's id and lists the pictures it has
     gathered from its own uploads, the German star-citizen.wiki and
     cstone.space's item finder. Across the bench kinds it has one for 485
     of 1,306 items (141/173 guns, 71/78 plants, 64/74 coolers, 58/67
     shields, 55/57 drives, 39/139 missile racks, 30/65 missiles, 11/20
     mining heads, 8/241 turrets, 2/63 radars, 0/7 EMPs, 0/5 jump drives).
  2. **By page title** (the lead image of the part's wiki page, by display
     name), which is what 0.13.5 did alone - 316 of the 633 bench part names
     had one on 14 Sep 2026 - and still finds pages the item record does not
     list a picture for.

  Measured on this install's miss list from the title-only lookup: the uuid
  lookup filled 48 of its 78 blanks (coolers 14/15, plants 10/10, shields
  10/11, drives 8/8, guns 5/13, radars 0/20), checked end to end through
  `GET /api/garage/picture/{uuid}` against a copy of the data - 3.5 MB for
  the 48, the largest a 369 KB PNG from cstone. The wiki's own thumbnails
  are taken before cstone's originals for that reason. What is left without
  a picture is mostly radars, missile racks and turrets, which nobody has
  photographed anywhere public; those keep the maker's mark. **cstone's file
  store is not guessed at**: its `uifimages/<uuid>.png` answers 200 with an
  HTML page for ids it lacks as often as it 404s, so only the urls the wiki
  names are fetched.
- **Reading UEX's player marketplace as prices.** It is not a price list:
  the feed is the newest 500 advertisements (about two days' worth, 465 sells,
  186 naming an item UEX has a uuid for), each one person's ask. The Garage
  shows them as *asking prices, not market prices*, beside the terminal price
  and never in its place, and the optimiser's *Terminal aUEC only* ignores
  them. The feed is its own switch under UEX in Settings - enabling UEX does
  not turn it on - and the join runs through UEX's `items` table read a
  category at a time (`marketplace-items.json`), because the price feed the
  app already holds resolved only 26 of the 441 listed item ids: what players
  advertise is what no shop stocks. **Only the id-to-uuid pair is kept from
  that table.** UEX's name, section and category for an item are dropped at
  the digest, and its `categories` endpoint is not called at all: the install
  says what a uuid is - name, kind, size, grade, maker - and the page reads
  those from the part card, so nothing the app can read itself is taken from
  UEX. A listing UEX cannot put a uuid to keeps only the seller's own title.
  On this install's Hermes: 17 of the 52 component listings fit a port at
  its size.
- **Makers' marks, on the other hand, are in the game files.** The Fankit
  covers 15 of the 60 makers on the bench (the hull makers), but every
  `SCItemManufacturer` record carries a `Logo` naming a 256-square texture
  under `UI/SharedAssets/ManufacturerLogos/`, and the 4.10 archive holds it
  for 127 of the 211 non-paint maker records (`--maker-logos` in the CLI
  prints the table) - 57 of the 59 makers on the bench; the 84 without are
  shops and stations wearing the same record type. 115 are plain 256-square
  BC3; nine ship as split mip chains (a 464-byte `.dds` plus `.dds.1`–`.4`,
  the top level alone in the highest part); two (RAMP, Gyson) and one
  4096-square (Broad & Rabiee) are uncompressed 32-bit. All decode through
  the same path as the vehicle icons. `GET /api/garage/maker/{code}` serves
  the game's mark first, the wiki's manufacturer page for a maker the install
  lacks (Associated Sciences), and 404 - the monogram - for a part with no
  maker.
  has the numbers, the page has no honest way to combine them.
- **Armour and hull damage models** beyond HP and the resistance table.
- **Sharing builds** between installs - `ExportDocument` has no class for it;
  a later version can add one with a `ContentVersion` bump.
- **Reading your actual current fit from the logs.** `Game.log` names the
  ship, not its parts (see *Loadout* in `untapped-signals.md`: the attachment
  lines are the pilot's armour and weapons, not the vehicle's). The sheet starts from the stock
  fit and says so; the pilot sets the bench to what they actually fly.

## Build order

1. ~~Digest: the two new files, the version check, the refresh sentence.~~ Done: `CommunityData.Garage.cs`, `HasGarage`.
2. ~~`ShipSheet` in Core, with the stock-fit verification as a test that prints
   the match table.~~ Done: `Core/GameData/Garage.cs`, `GarageSheetTests`, `--garage-check`.
3. ~~`GET /api/garage/{class}` and the sheet page - read-only, every group
   labelled.~~ Done, 0.13.1; `POST .../sheet` exists for step 4.
4. ~~The bench and the delta: `POST .../sheet`, the was → now rendering, the
   budget lines.~~ Done, 0.13.2, plus `GET .../options` and folded rows.
5. ~~Builds: store, backup, restore, the Fleet card's button re-pointed.~~ Done, 0.13.3; compare-against too.
6. ~~Shopping: the job, the destination proposal.~~ Done, 0.13.4; `GarageShopping.Propose`, `POST .../shop`.
7. Screenshots, docs, release notes under `### 0.13.0`.
