# Armoury: guns and armour, read from the install

The personal-armoury line, started 2026-09-16 after the 0.14 mining work.
What the install says about the guns a pilot carries and the armour they
wear, an Armoury page under Gear that compares them, and UEX's price beside
each. Everything here is read from this install (Alpha 4.10, `Game2.dcb`)
with `dotnet run --project src\Quantumwake.Cli -c Release -- --armoury`;
run that again after a patch and the tables below are what to compare
against. `--armoury=all` lists every finish under its gun and every colour
under its set.

## What prompted it

"Which rifle, what armour, where to buy it" is a question the community
answers on SPViewer and the wiki's weapon tables. The item catalogue the app
reads already names all 9,553 attachables, the Garage already prices a part
by the game's own id, and the Kits page already keeps loadouts by name. What
was missing was any number about what a gun does.

## Where the numbers are

**A gun's damage is not on the gun.** `SCItemWeaponComponentParams` names
the magazine the gun ships with (`ammoContainerRecord`); the magazine's
`SAmmoContainerComponentParams` names an `AmmoParams` record
(`ammoParamsRecord`, and `secondaryAmmoParamsRecord` for a gun with two
loads); and that record carries the projectile - `speed`, `lifetime`, a
`DamageInfo` per hit under `projectileParams.damage` in six kinds (physical,
energy, distortion, thermal, biochemical, stun), a `damageDropParams` block
(the metres before the fall starts, the loss per metre, the floor), and for a
launcher a `detonationParams.explosionParams` block with the blast's damage
and radii. The gun keeps the fire modes.

First read of this install, 2026-09-16 - the P4-AR's line is the one the
community tables print, which is what says the chain is being followed
rather than guessed:

| Gun | Read |
| --- | --- |
| P4-AR | 12 physical, 810 rpm auto and semi, 40 rounds, drop from 40 m at 0.05/m to a floor of 10 |
| P6-LR | 100 physical, 55 rpm, 8 rounds, drop from 550 m |
| S-38 | 22.5 physical, 450 rpm, 20 rounds |
| Gallant | 21 energy, 3-round burst at 900 with 0.25 s between, 45 rounds, no drop |
| Karna | 17.5 energy, 600 rpm auto; a charge ×6 after 2 s costing 3 rounds |
| Quartz | a beam, 225 energy + 50 distortion a second to 10 m and none past 25, 7.5 rounds a second from 45 |

The magazine count has a check the reader never uses: the magazine's own
description text, written by hand - "Capacity: 40 ... holds fourty 5.56mm
cartridges" on `behr_rifle_ballistic_01_mag` - agrees with `maxAmmoCount`.

**The fire modes are a small zoo of structs.** Rapid (auto), Single (semi),
Burst (`shotCount`, `fireRate`, `cooldownTime`), Charged (`chargeTime` and a
`maxChargeModifier` with `damageMultiplier`, `ammoCostMultiplier` and
`pellets`) and Beam (`damagePerSecond`, `fullDamageRange`, `zeroDamageRange`,
`maxEnergyDraw` a second) are the leaves. Three wrappers sit over them:

- **Sequence**, with `sequenceEntries` each holding a leaf, a `delay` in
  seconds or RPM and `repetitions`, and a `mode`: *Looping* cycles them while
  the trigger is held (the Killshot's two barrels alternating at 535 a
  minute, the Lumin V's three-round bursts with a 625-rpm pause between),
  *Automatically* sends every entry on one pull (the Scalpel's two barrels
  together - a two-round burst), *Individually* one entry a pull (the
  Scalpel's semi at 60).
- **Parallel** fires every `weaponActions` entry at once: the Tripledown's
  three barrels of three pellets are one nine-pellet shot costing three
  rounds.
- **DynamicCondition** picks by state: the Parallax and the Prism turn into a
  beam and a slug when `SWeaponConditionHeatLevel` reads 40% or more.

`GameArmoury.Modes` flattens each wrapper to what the trigger does, keeping
the leaf's cyclic rate and folding a loop's delay into the burst's cooldown,
so the page compares a rifle to a rifle without knowing the tree. A mode
whose launcher says `projectileType: Secondary` fires the magazine's second
load and carries that load's damage - the Arlington's eight-pellet spread is
12.5 a pellet where its slug is 80.

**The HUD's word for a mode** is `localisedName` - `@FireMode_Rapid` is
`[AUTO]` - and the files' own `name` says more where the HUD would not:
the Prism's slug and its spread are both SEMI on the HUD and *SEMI slug* and
*SEMI* here.

**Finishes.** 327 personal weapons on this install are 46 guns and 281
finishes of them: `behr_rifle_ballistic_01_black01` is the P4-AR in black,
same figures, its own price. A finish is the gun whose class it extends, and
where the class does not extend it - the Yubarev "Deadeye" is
`lbco_pistol_energy_cen01` beside `lbco_pistol_energy_01` - the name without
its quoted word and the same figures say so. UEX files a gun under whichever
of its classes it met first (its P4-AR is
`behr_rifle_ballistic_01_contestedzonereward`), so the plain gun's price is
the cheapest across every class the game names the same.

**What is left out**, by the sub-type the catalogue already carries: knives
(`Knife`, melee damage on `SMeleeWeaponComponentParams`, not read), grenades
(`Grenade`, a primeable explosion, not read), the gadgets (multi-tool,
medical gun, tractor beam, extinguisher - tools, not weapons), and anything
under `/dev/`, a template, a test rig or a toy. The Animus missile launcher
is kept with what the files give it: a blast and a magazine that holds zero,
which the page prints as a dash rather than a number.

## Armour

**Resistance is by class, not by piece.** Every `SCItemSuitArmorParams`
points at one of twelve `DamageResistanceMacro` records, and every piece of
a class points at the same one:

| Macro | Pieces | Physical / energy / distortion / thermal / biochemical | Stun | Impact |
| --- | --- | --- | --- | --- |
| LightArmor | 769 | ×0.8 | ×0.7 | ×0.9 |
| MediumArmor | 559 | ×0.7 | ×0.55 | ×0.6925 |
| HeavyArmor | 558 | ×0.6 | ×0.4 | ×0.65 |
| UndersuitArmor | 235 | ×0.95 | ×0.9 | ×1 |
| CombatFlightsuitArmor | 40 | ×0.85 | ×0.8 | ×0.9 |
| HeavyArmorUtility | 48 | ×0.75 | ×0.4 | ×0.65 |
| SuperHeavyArmor | 7 | ×0.125 / ×0.225 | ×0.125 | ×0.125 |

So a column of resistances would be a column of identical figures, and the
page says the rule once above the table instead. What differs between two
pieces of a class is the rest of `SCItemClothingParams` and the container:

- `TemperatureResistance.MinResistance` / `MaxResistance` - the Testudo core
  is -50 to 80 °C, the ADP -75 to 105, the Carnifex -5 to 38.
- `RadiationResistance.MaximumRadiationCapacity` and
  `RadiationDissipationRate` - 26,400 and 145.8/s on a medium core, 15,200
  and 81/s on an undersuit.
- `Flight.gForceResistance` - +0.9 on an undersuit, -0.5 on a heavy core.
- `SCItemInventoryContainerComponentParams.containerParams` →
  `InventoryContainer.*.inventoryType.capacity.microSCU` - a heavy core's
  pockets hold 8,000 to 12,000 microSCU, a Novikov backpack 180,000.
- `signatureParams` - electromagnetic 2 on a light piece, 6 on a medium, 20
  to 30 on a heavy; a helmet adds infrared.
- `restrictedMoveViewPenalty` → `MoveViewRestrictionPenalty.*` - 0.5 on
  medium, 1 on heavy, 0 on light. The game's own figure; what puts a piece
  into its restricted state is not read, and the page says so.
- `SEntityPhysicsControllerParams.PhysType.Mass`.

**Sets.** 2,349 named pieces are 384 sets-and-figures: a set is the words
before the slot word ("Testudo Core Deathblow" is Testudo), and the row is
one set, one slot, one set of figures, with the colours under it and the
cheapest of their prices beside it. A set whose figures changed between two
editions - the Morozov-SH at -90 and at -95 - is two rows, which is the
truth.

**Left out**: invisible stand-ins (`nodraw_`, `volume_`), placeholders
("TEST STRING NAME"), the NPC-only resistance macro (`_ai_exclusive`), and
anything the game has not named, which is what a shop cannot sell.

## Derived figures, and whose they are

None of these is the game's; each is the files' numbers combined the
obvious way, in `Armoury` (Core) so the tests can pin the rule:

- **Sustained rate**: the cyclic rate for auto; the cap for semi; for a
  burst, its shots over the burst's gaps and the cooldown after it (the
  Gallant's three at 900 with 0.25 s between is 470 a minute); for a charge,
  one full charge after another.
- **Damage a second**: one pull's damage - the hit, every pellet, a full
  charge's multiplier - times the sustained rate; a beam's own figure.
- **Per magazine** and **empties in**: the rounds a pull costs, a charge's
  multiplier included, into the magazine.
- **Drop**: a straight line from the start to the floor.

The page says so in the sentence under the table, and the CLI prints the
same figures so a drift shows.

## Prices

The way the Mining page prices a head: the game's own id for the class
(`GameCommodities.ItemUuid`) into UEX's item feed, cheapest terminal and
where. On 16 Sep 2026 UEX prices 20 of the 46 guns and 126 of the 384 sets.
A blank is "no terminal recorded", not free; with UEX off the column says
what it needs.

## Storage, API

Read in the same pass as the rest of the game data (`GameArmoury.Read`,
cached in `commodities.json` under `Armoury`; `CacheVersion` moved).
`GET /api/armoury` returns the plain guns with their modes and derived
figures, their finishes with prices, and the armour rows. Nothing stored in
a session changes: no parser, no `PayloadVersion`.

The resolver behind "cheapest at" is taken once per request: `LogLibrary.
Terminals` rebuilds from the atlas on every read, and three thousand pieces
each asking for it took the first version of this endpoint thirty seconds.

## Next

- Knives and grenades, from `SMeleeWeaponComponentParams` and the primeable
  explosion.
- Attachments: the catalogue has 118 barrels, 57 sights and 12 underbarrel
  pieces; `SWeaponStats` modifiers on each would let the page show what a
  suppressor or a scope does to the row.
- Kits: a gun or a set from this page into a kit on the Loadout page, and a
  kit's missing pieces onto a shopping list with the destination that sells
  the most of them, as the Garage does for parts.
