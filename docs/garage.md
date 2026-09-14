# Garage: a ship's numbers, and what a part would do to them

Plan for 0.13.0, written 2026-09-14 before any code. The question was whether
the app can show every stat for one of your ships, let you try a different
component and see what moves - DPS, shield, EM, IR, cross-section - and then
put the parts you settled on into a shopping list. It can, with two honest
limits set out below.

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

## What can be recomputed, and what cannot

The test that matters is the one the commodity work taught: recompute the
stock loadout's totals from the parts and compare with the dataset's own
figures, which the recomputation never read.

| Figure | Recompute from parts | Stock loadout matches dataset within 2% | Verdict |
| --- | --- | --- | --- |
| Fixed-gun DPS | sum of `Weapon.Damage.DpsTotal` over fixed WeaponGuns | 177 of 238 | **exact where the classification agrees**; the misses are turret-mounted guns counted as fixed, to be settled per ship in the build |
| Shield HP / regen | sum over fitted Shields | 213 of 269 | **exact**; misses to be looked at (likely duplicate generators in the walk) |
| Quantum speed, spool, fuel per jump | the fitted drive plus the ship's tank | - | **exact**, single part |
| Power and cooling budget | generation vs draw in segments | - | **exact**, the dataset stores the same segments |
| Mass | ship + parts | - | **exact** |
| Cross-section | - | - | **ship geometry; parts do not move it**. Shown as a fact, never with a delta |
| EM / IR ship total | sum of part emissions | **4 of 269** | **not recomputable**. A size 1 power plant whose own EM is 7,920 shows as 13,488 in the ship's total: the dataset runs a power-segment model that scales each part by its operating point. Re-implementing it is the derivation trap `datacore.md` records |

EM and IR therefore work like this, and the page says so:

- the **ship's** stock totals come from the dataset and are labelled as its
  figures, by group;
- a **part's** own EM and IR are exact and are what the swap panel compares -
  "this cooler: 1,490 EM / 7,920 IR; the fitted one: 2,100 / 9,020";
- the **new ship total** is an *estimate*: the dataset's figure for that
  group scaled by the ratio of the new part's emission to the old one's. It is
  shown with the word estimate on it, the way inferred locations carry a
  confidence.

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

1. Recompute the stock loadout for all 318 vehicles and compare with the
   dataset's `Weaponry.FixedWeapons.DpsTotal`, `ShieldHp`, `QuantumTravel.Speed`
   and `Power.GenerationSegments`; write the match rates into this document
   and refuse to show any figure whose rule does not reach 95% on stock fits.
2. Take the Gladius, swap the stock cooler for a Glacier, and check the
   component EM/IR numbers against the item records by hand.
3. Screenshot the sheet and the bench for one fighter and one hauler against
   the real install, and read them.

## Deliberately out of scope

- **Cross-section changes** - none exist; parts do not change geometry.
- **Turret-mounted gun ballistics** (gimbal spread, convergence) - the dump
  has the numbers, the page has no honest way to combine them.
- **Armour and hull damage models** beyond HP and the resistance table.
- **Sharing builds** between installs - `ExportDocument` has no class for it;
  a later version can add one with a `ContentVersion` bump.
- **Reading your actual current fit from the logs.** `Game.log` names the
  ship, not its parts (see *Loadout* in `untapped-signals.md`: the attachment
  lines are the pilot's armour and weapons, not the vehicle's). The sheet starts from the stock
  fit and says so; the pilot sets the bench to what they actually fly.

## Build order

1. Digest: the two new files, the version check, the refresh sentence.
2. `ShipSheet` in Core, with the stock-fit verification as a test that prints
   the match table.
3. `GET /api/garage/{class}` and the sheet page - read-only, every group
   labelled.
4. The bench and the delta: `POST .../sheet`, the was → now rendering, the
   budget lines.
5. Builds: store, backup, restore, the Fleet card's button re-pointed.
6. Shopping: the job, the destination proposal.
7. Screenshots, docs, release notes under `### 0.13.0`.
