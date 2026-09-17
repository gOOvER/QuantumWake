# Salvage: the equipment and the rules, not the wreck

Started 2026-09-16, the fifth item on the list in `IDEAS.md` and the one
whose probe came back the other way. The question was "what is this hull
worth scraped, and is a Reclaimer trip worth the fuel"; the answer from the
files is that they do not say, and this records why so nobody spends the
day again, and what was built instead. Run `dotnet run --project
src\Quantumwake.Cli -c Release -- --salvage` after a patch.

## The negative result

**The per-hull yield is not in `Game2.dcb`.** The salvage system is fully
described there - and none of it is per hull:

- `SGlobalSalvageRepairBeamParams.materialParams`: `hullThicknessMeters`
  0.009 and `ammoToMaterialFactor` 100, with the resource `RMC`. A beam
  scrapes nine millimetres of hull; what it gets is that depth times the
  *area it covers*.
- `SCItemSalvageControllerParams` on each salvage hull's controller:
  `disintegrationSCUPerCubicMetre` and the resource it makes. What a hull
  gives up when disintegrated is that rate times the *volume taken apart*.
- Nothing on a vehicle record carries its surface area or volume. The two
  `SurfaceArea` fields in the whole struct table are on coolers.

The area and volume are geometry - the model files, not the DataCore - and
the vehicle definition files that might summarise them are the encrypted
ones `datacore.md` already records as closed. UEX's `vehicles` endpoint
carries no salvage figure (checked 2026-09-16: `scu`, `mass`, dimensions
and a set of `is_*` flags, `is_salvage` being whether the hull salvages,
not what it yields). The logs record no salvage - `untapped-signals.md`
counts 592 lines under the word, all `SetSalvageRepairAmmoCount_NoTarget`
warnings and damage-map paths. So "a Cutlass gives 22 SCU of RMC" is a
figure only a measuring community has, and the page does not show one.

## What the files do say, and is shown

A **Salvage** pane on the Mining page (the heading already said "Mining &
salvage"), `GameSalvage.Read` in Core, `GET /api/salvage/model`:

**The hulls** - each salvage ship's controller, several controllers merged
to the hull (the Reclaimer's arm disintegrates and its turret scrapes; the
MOTH has four):

| Hull | Heads | Scrapes to | Disintegrates, SCU per m³ | Into |
| --- | --- | --- | --- | --- |
| Fortune | 1 | RMC | 0.0039 | Construction Rubble |
| Salvation | 2 | RMC | 0.00175 | Construction Rubble |
| Vulture | 2 | RMC | 0.0027 | Construction Rubble |
| MOTH | 3 | RMC | 0.0055 | Construction Pieces |
| Reclaimer | 1 | RMC | 0.00525 | Construction Salvage |

The resource names are the game's own, through the commodity table
(`ResourceType.ConstructionMaterialPowder` is *Construction Rubble*). UEX
prices RMC as Recycled Material Composite (7,700 aUEC a SCU at Patch City
on 16 Sep 2026) and the three construction resources as one commodity,
Construction Materials (13,000 at ARC-L3), which the pane says. Beside each
hull is its hold from the community dataset and **what a full hold of each
fetches at that price** - the Vulture's 12 SCU is 92,400 of RMC; the
Reclaimer's 420 is 3.2 million - called a ceiling on a trip, before the fuel
and before the finding, which is what it is.

**The scraper modules**, from `EntityComponentAttachableModifierParams ›
ItemWeaponModifiersParams › weaponModifier.weaponStats.salvageModifier`:

| Module | Speed | Radius | Efficiency |
| --- | --- | --- | --- |
| Trawler | 0.05 | 6 m | 60 % |
| Abrade | 0.15 | 3.5 m | 90 % |
| Cinch | 0.6 | 1.5 m | 100 % |
| Comer (Salvation) | 0.7 | 1.5 m | 80 % |
| Hart (Salvation) | 0.8 | 1 m | 100 % |

The same three figures each module's own description text states ("Extraction
Speed: 0.15, Radius: 3.5m, Extraction Efficiency: 90%" on the Abrade), which
is the check the reader never uses. A tractor module leaves all three at 1
and is not listed. Priced by UEX the way the mining heads are: Abrade
20,188, Trawler 29,593, Cinch 11,150; the Salvation's two have no terminal
recorded.

**The heads**: the Baler with two module slots, the Salvation head with one
(`SItemPortContainerComponentParams` ports that take a `SalvageModifier`).

## Not done, and why

- The per-hull yield, as above. If a future dump or UEX ever publishes hull
  surface area or volume, the rule to apply is already read.
- "Is a Reclaimer trip worth the fuel": the fuel side is one UEX feed away
  (`fuel_prices_all` is already an optional feed) and the yield side is the
  missing half, so the arithmetic would be a full hold against a tank, which
  the page can do in the reader's head from the two figures it shows.
