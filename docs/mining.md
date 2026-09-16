# Mining: can it be cracked?

The 0.14 line. What the install says about mining, and a calculator on the
Mining page that runs the community's rule on it. Everything here is read
from this install (Alpha 4.10, `Game2.dcb`) on 2026-09-15 with
`dotnet run --project src\Quantumwake.Cli -c Release -- --mining`; run that
again after a patch and the tables below are what to compare against.

## What prompted it

scminer.rocks has a "Can I Crack It?" calculator: rock mass, resistance,
instability and mineral in; laser, modules and gadget in; a verdict out. The
question was whether this install holds enough to do the same. It holds all
of it and a little more. None of it was being read: the Garage's community
part digest carries a mining laser's mass and health and nothing it does to a
rock, and the logs record no mining at all - no scan, no fracture, no
extraction - which is why the Mining page's "mine" figure is ore sold that
was never bought, an inference, and stays so.

## What the files hold

Structs that name themselves, all under `libs/foundry/records/mining/`:

- **`MiningGlobalParams`** - the constants of the rock model:
  `powerCapacityPerMass 10`, `decayPerMass 0.2`, `optimalWindowSize 0.1`
  (`optimalWindowMaxSize 0.5`, `optimalWindowFactor 0.75`),
  `resistanceCurveFactor 0.6`, `optimalWindowThinnessCurveFactor 0.7`,
  `controlledBreakingFillRate 0.5`/s (`dangerBreakingFillRate 0.3`),
  `absorbableVolumeThreshold 330`, `cSCUPerVolume 3`. A ground-vehicle set
  beside it with `powerCapacityPerMass 4`.
- **`MineableElement`** × 42 (31 ship, the rest hand and ROC): per mineral
  `elementResistance` (-1 to 1), `elementInstability`, the optimal window's
  midpoint, randomness and thinness, an explosion multiplier and a cluster
  factor. Quantainium: resistance 0.95, instability 1000, blast ×260. Iron:
  -0.40, 50, ×20. The `resourceType` reference names it through the
  commodity table the app already reads.
- **`MineableComposition`** × 213 presets (templates and tests dropped): what
  a deposit is made of, element by element with min/max share and
  probability. `Asteroid_PType_Copper` is copper 30-70% always, laranite
  30-60% at 0.8, gold 20-50% at 0.1, quantainium 20-50% at 0.02. The HUD name
  (`@hud_mining_asteroid_name_1` → "Asteroid (P-Type)") is shared by a dozen
  presets, so the class is the key.
- **Lasers** × 18 (`EntityClassDefinition.Mining_Laser_*`, templates and
  tests dropped): the fracture beam's `damagePerSecond.DamageEnergy` is the
  power, the second fire action is the extraction beam, and
  `SEntityComponentMiningLaserParams` carries the head's own modifiers and
  filter. Names from the item catalogue.
- **Modules** × 29 (`Mining_Modules_Passive_*`, `_Active_*`): two weapon
  modifiers - the first on the fracture beam, the second (`fireActionIndex=1`)
  on extraction - a mining modifier, a filter modifier; actives carry
  `charges` and a `modifierLifetime`.
- **Gadgets** × 6 (`Mining_Gadget_*`): one rock modifier each.

The nested structs are laid inline on some records and behind a pointer on
others under the same field name (`damagePerSecond`, `miningLaserModifiers`,
`weaponModifier`), and the composition parts are a class array rather than a
pointer array; `GameMining.Nested` reads either shape, which is what the
first dump - 0 lasers, empty modifiers, 0 compositions - taught.

## Checked against scminer.rocks

Their calculator reads the same files. Every wattage they print is the
fracture beam's `DamageEnergy` on this install - Arbor MH1 2,340, Helix I
3,900, Hofstede-S1 2,600, Impact I 2,600, Klein-S1 3,120, Lancet MH1 3,120,
Pitman 3,900 - and every module effect they list is the file's: Rieger-C3
×1.25 power, Surge ×1.5, Brandt ×1.35, Forel +15.5% resistance, Sabir -50%
resistance / +50% window / +15% instability, BoreMax +10% resistance / -70%
instability. Where a figure of theirs and a figure of ours disagree, the
files are the referee, and so far they have not.

## The rule, and whose it is

The game does not publish how mass, resistance and power meet. What the
community settled on, read out of scminer's calculator bundle on
2026-09-15: **a rock needs 0.36 W per kilogram at zero resistance**; the
HUD's resistance is scaled by the fit's resistance points
(`hud × (1 + Σ points / 100)`) and taken off the delivered power; delivered
over required at 115% or more breaks solo, 70% or more breaks with a gadget,
less needs more heads. Their green-zone width (18%) and crack time (14 s)
are theirs too and are **not** used: the game's own `optimalWindowSize`
gives the window, and the crack time is the time to reach the window, which
is the pilot's throttle hand.

`RockCracking.RequiredWattsPerKg`, `SoloRatio` and `GadgetRatio` are the
whole of the borrowed rule, named so a measured one replaces them in one
place. Beside the verdict the page quotes what the game's constants say about
the same rock - it holds `mass × 10` energy and sheds `mass × 0.2` a second -
which is a different model of the same event and, until measured against,
neither confirms nor contradicts the line. The page calls the verdict an
estimate and says whose rule it is.

**What would settle it:** a handful of scan HUD screenshots (mass,
resistance, instability, composition are all printed) paired with what
happened - broke at what throttle, or would not charge. The screen reader
already reads that HUD's family; a `Mining` frame kind reading those four
figures is the natural next step, and it would feed this calculator without
a form.

## Module slots

The head's own item ports (`SItemPortContainerComponentParams.Ports`, a
class array) name the slots: `BONE_ItemPort_Consumable_1`, `_2`, `_3`
take a `MiningModifier`; the `VEN` port beside them takes a weapon
attachment and is not one. On this install: Arbor MH1, Hofstede-S1, Lancet
MH1 and Klein-S2 one; Helix I, Impact I, Pitman, Arbor MH2, Hofstede-S2 and
Lancet MH2 two; Helix II and Impact II three; Klein-S1 none. The page shows
that many selects per head and says "no module slots" for the Klein-S1.

## The deposit on the page

Picking a deposit shows the preset's mix - each mineral's share when it is
present and its chance of being present - with UEX's best sell for the
refined mineral a SCU beside it, the way the places table values rock (the
element's "(Raw)"/"Ore" stripped to reach the UEX name). The likeliest
mineral goes into the notes. A rough worth of a SCU of the mix is given -
middle share × chance × refined price, summed - and called rough: the game
normalises the shares of the minerals that turn up and this does not.

**Per rock is not given, and says so.** The HUD's mass is kilograms; the
game's `cSCUPerVolume 3` is cargo per unit of *volume*, and nothing read
carries a density to get from one to the other. scminer's "Is It Worth
Mining?" takes the extracted cSCU as input for the same reason. The
extraction beam's power is read and unused until that gap is closed.

## Not read yet

- The scan HUD. It prints the four inputs - mass, resistance, instability,
  and the composition with percentages - and a `Mining` frame kind for the
  screen reader would fill the form from a screenshot. Every reader so far
  was written from a frame already in the log, and there is no mining frame
  on this install: the first one a pilot takes is what it gets written from.

## The dump

`--mining` on this install, 2026-09-15:

```
18 lasers
  S0 Arbor MHV Mining Laser       power      1  extraction     0  slots 0  filter   0%  throttle min 0.20  inst -40%  [Mining_Laser_GRIN_Arbor_S0]
  S0 Lawson Mining Laser          power      1  extraction     0  slots 0  filter   0%  throttle min 0.20  res -40% inst +30% window +40%  [Mining_Laser_SHIN_Klein_S0]
  S0 Mining Laser SHIN Hofstede S0 power      1  extraction     0  slots 0  filter   0%  throttle min 0.20  res -40% inst +30% window +40% rate +20%  [Mining_Laser_SHIN_Hofstede_S0]
  S0 Mining Laser THCN Helix S0   power      1  extraction     0  slots 0  filter   0%  throttle min 0.15  window -40% rate +20%  [Mining_Laser_THCN_Helix_S0]
  S1 Arbor MH1 Mining Laser       power   1850  extraction  1000  slots 1  filter   0%  throttle min 0.00    [Mining_Laser_MPUV_Arm]
  S1 Arbor MH1 Mining Laser       power   2340  extraction  1850  slots 1  filter  30%  throttle min 0.05  res +25% inst -35% window +40%  [Mining_Laser_GRIN_Arbor_S1]
  S1 Helix I Mining Laser         power   3900  extraction  1850  slots 2  filter  30%  throttle min 0.20  res -30% window -40%  [Mining_Laser_THCN_Helix_S1]
  S1 Hofstede-S1 Mining Laser     power   2600  extraction  1295  slots 1  filter  30%  throttle min 0.05  res -30% inst +10% window +60% rate +20%  [Mining_Laser_SHIN_Hofstede_S1]
  S1 Impact I Mining Laser        power   2600  extraction  2775  slots 2  filter  30%  throttle min 0.20  res +10% inst -10% window +20% rate -40%  [Mining_Laser_THCN_Impact_S1]
  S1 Klein-S1 Mining Laser        power   3120  extraction  2220  slots 0  filter  30%  throttle min 0.15  res -45% inst +35% window +20%  [Mining_Laser_SHIN_Klein_S1]
  S1 Lancet MH1 Mining Laser      power   3120  extraction  1850  slots 1  filter  30%  throttle min 0.20  inst -10% window -60% rate +40%  [Mining_Laser_GRIN_Lancet_S1]
  S1 Pitman Mining Laser          power   3900  extraction  1295  slots 2  filter  40%  throttle min 0.20  res +25% inst +35% window +40% rate -40%  [Mining_Laser_DRAK_Golem_S1]
  S2 Arbor MH2 Mining Laser       power   2900  extraction  2590  slots 2  filter  40%  throttle min 0.05  res +25% inst -35% window +40%  [Mining_Laser_GRIN_Arbor_S2]
  S2 Helix II Mining Laser        power   4930  extraction  2590  slots 3  filter  40%  throttle min 0.30  res -30% window -40%  [Mining_Laser_THCN_Helix_S2]
  S2 Hofstede-S2 Mining Laser     power   4060  extraction  1295  slots 2  filter  40%  throttle min 0.10  res -30% inst +10% window +60% rate +20%  [Mining_Laser_SHIN_Hofstede_S2]
  S2 Impact II Mining Laser       power   4060  extraction  3145  slots 3  filter  40%  throttle min 0.30  res +10% inst -10% window +20% rate -40%  [Mining_Laser_THCN_Impact_S2]
  S2 Klein-S2 Mining Laser        power   4350  extraction  2775  slots 1  filter  40%  throttle min 0.20  res -45% inst +35% window +20%  [Mining_Laser_SHIN_Klein_S2]
  S2 Lancet MH2 Mining Laser      power   4350  extraction  2590  slots 2  filter  40%  throttle min 0.30  inst -10% window -60% rate +40%  [Mining_Laser_GRIN_Lancet_S2]

29 modules
  passive Deluge Module          power ×1.15  extraction ×0.85  filter    0%  res -15.5%  [Mining_Modules_Passive_Deluge]
  passive FLTR Module            power ×1.00  extraction ×0.85  filter   20%    [Mining_Modules_Passive_FLTR_MK1]
  passive FLTR-L Module          power ×1.00  extraction ×0.90  filter   23%    [Mining_Modules_Passive_FLTR_MK2]
  passive FLTR-XL Module         power ×1.00  extraction ×0.95  filter   24%    [Mining_Modules_Passive_FLTR_MK3]
  passive Focus II Module        power ×0.90  extraction ×1.00  filter    0%  window +37%  [Mining_Modules_Passive_Focus_MK2]
  passive Focus III Module       power ×0.95  extraction ×1.00  filter    0%  window +40%  [Mining_Modules_Passive_Focus_MK3]
  passive Focus Module           power ×0.85  extraction ×1.00  filter    0%  window +30%  [Mining_Modules_Passive_Focus_MK1]
  passive Overrun Module         power ×1.02  extraction ×0.85  filter    0%  res -24.8%  [Mining_Modules_Passive_Overrun]
  passive Rieger Module          power ×1.15  extraction ×1.00  filter    0%  window -10%  [Mining_Modules_Passive_Rieger_MK1]
  passive Rieger-C2 Module       power ×1.20  extraction ×1.00  filter    0%  window -3%  [Mining_Modules_Passive_Rieger_MK2]
  passive Rieger-C3 Module       power ×1.25  extraction ×1.00  filter    0%  window -1%  [Mining_Modules_Passive_Rieger_MK3]
  passive Torrent II Module      power ×1.00  extraction ×1.00  filter    0%  window -3% rate +35%  [Mining_Modules_Passive_Torrent_MK2]
  passive Torrent III Module     power ×1.00  extraction ×1.00  filter    0%  window -1% rate +45%  [Mining_Modules_Passive_Torrent_MK3]
  passive Torrent Module         power ×1.00  extraction ×1.00  filter    0%  window -10% rate +30%  [Mining_Modules_Passive_Torrent_MK1]
  passive Vaux Module            power ×1.00  extraction ×1.15  filter    0%  rate -20%  [Mining_Modules_Passive_Vaux_MK1]
  passive Vaux-C2 Module         power ×1.00  extraction ×1.20  filter    0%  rate -15%  [Mining_Modules_Passive_Vaux_MK2]
  passive Vaux-C3 Module         power ×1.00  extraction ×1.25  filter    0%  rate -5%  [Mining_Modules_Passive_Vaux_MK3]
  passive XTR Module             power ×1.00  extraction ×0.85  filter    5%  window +15%  [Mining_Modules_Passive_XTR_MK1]
  passive XTR-L Module           power ×1.00  extraction ×0.90  filter  5.8%  window +22%  [Mining_Modules_Passive_XTR_MK2]
  passive XTR-XL Module          power ×1.00  extraction ×0.95  filter    6%  window +25%  [Mining_Modules_Passive_XTR_MK3]
  active  Brandt Module          power ×1.35  extraction ×1.00  filter    0%  60s × 5  res +15.5% shatter -30%  [Mining_Modules_Active_Brandt]
  active  Clearcut Module        power ×1.15  extraction ×1.00  filter    0%  30s × 6  window +30%  [Mining_Modules_Active_Clearcut]
  active  Forel Module           power ×1.00  extraction ×1.50  filter    0%  60s × 6  res +15.5% overcharge -60%  [Mining_Modules_Active_Forel]
  active  Lifeline Module        power ×1.00  extraction ×1.00  filter    0%  15s × 3  res -15.5% inst -20% overcharge +60%  [Mining_Modules_Active_Lifeline]
  active  Optimum Module         power ×0.85  extraction ×1.00  filter    0%  60s × 5  inst -10% overcharge -80%  [Mining_Modules_Active_Optimum]
  active  Rime Module            power ×0.85  extraction ×1.00  filter    0%  20s × 10  res -24.8% shatter -10%  [Mining_Modules_Active_Rime]
  active  Stampede Module        power ×1.35  extraction ×0.85  filter    0%  30s × 6  inst -10% shatter -10%  [Mining_Modules_Active_Stampede]
  active  Surge Module           power ×1.50  extraction ×1.00  filter    0%  15s × 7  res -15.5% inst +10%  [Mining_Modules_Active_Surge]
  active  Torpid Module          power ×1.00  extraction ×1.00  filter    0%  60s × 5  rate +60% overcharge -60% shatter +40%  [Mining_Modules_Active_Torpid]

6 gadgets
  BoreMax      res +10% inst -70% cluster +30%  [Mining_Gadget_THCN_BoreMax]
  Okunis       window +50% rate +100% cluster -20%  [Mining_Gadget_SHIN_Okunis]
  OptiMax      res -25% window -30% cluster +60%  [Mining_Gadget_GRIN_OptiMax]
  Sabir        res -50% inst +15% window +50%  [Mining_Gadget_SHIN_Sabir]
  Stalwart     inst -35% window -30% rate +50% cluster +30%  [Mining_Gadget_THCN_Stalwart]
  Waveshift    inst -35% window +100% rate -30%  [Mining_Gadget_GRIN_WaveShift]
```
