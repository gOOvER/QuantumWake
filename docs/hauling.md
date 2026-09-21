# Hauling: where the cargo goes

A hauler with five contracts open sees five cards in the mobiGlas, each naming
its own destination and nothing else. Where the cargo is collected is in each
card's detail panel, one card at a time, and the logs never print it. This is
what the app can and cannot know about a hauling run, with the count behind
each claim, and how the plan on the Shopping page is put together from it.

Everything below was measured on this install's 209 backups on 21 September
2026 (`dotnet run --project src\Quantumwake.Cli -- --hauling`), except where a
frame is named.

---

## What the logs say

Two lines carry anything about a hauling contract's route, and they land
within 20 ms of each other with the same mission id.

**The acceptance toast** carries the contract's *displayed* title:

```
<SHUDEvent_OnNotification> Added notification "Contract Accepted:  Junior | Stellar Small Haul | to Stanton Gateway <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>: " [41] to queue. New queue size: 1, MissionId: [19fac03f-…], ObjectiveId: []
```

With the StarStrings text mod installed the title names a place. Across 1,160
hauling acceptances here:

| Title shape | Acceptances | What it gives |
|---|---:|---|
| `… \| to X` | 235 | the delivery |
| `… \| from X` | 273 | the pickup |
| `… \| A > B` | 138 | both ends |
| vanilla `Junior Rank - Direct Small Cargo Haul` | 285 | nothing |
| `… \| to Destinationname` | 11 | nothing — the mod's template, unfilled |

Folded to contracts (the toast repeats), that is 139 hauling contracts, every
one with its title, 54 naming a delivery, 41 a pickup, 14 both and 58 neither.
Seventeen distinct places are named; Port Tressler 209 times, Ruin Station 99.

**The objective marker** carries the archetype id:

```
<SMarkerHandler_Base::CreateMissionObjectiveMarker> … contract [RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle]
```

It spells the *shape* — `AToB`, `SingleToMulti2`, `Multi3ToSingle` — and
usually the cargo (123 of 139 here; the interstellar bulk ids abbreviate it,
`ShipAmm_Hydro_Med`, and are left unread). It never names a place. The 49
direct, 40 single-to-multi and 43 multi-to-single contracts here are how the
plan knows a `to X` title has three pickups still to find.

**The objective ids** name their end. `ObjectiveUpserted` ids read
`pickup_<uuid>_1` and `dropoff_<uuid>_0` — 250 and 207 of them — so "two of
three pickups done" is readable from the logs even though which place a step
was is not. The objective *text* is not: `CMissionLogEntry::UpdateActiveObjective`
carries a `uiDisplay[Text=…]` for 4,867 lines here and every one is either a
template (`Go to ~mission(Location)`) or a combat line; no hauling step ever
appears with text. Checked so nobody looks again.

### The join that was missing

Until 0.15.24 the toast's title went into the timeline and nowhere else. Every
contract record was named from the archetype — "Red Wind · Recover Cargo ·
Easy" — and three things read a title off that name that never carried one:
the rep and blueprint chips on the Contracts page, the screenshot check that
compares the Contracts app's cards to the logs' open contracts, and anything
wanting a destination. The two lines share the mission id and arrive in either
order, so the builder now keeps the title by mission id and both sides look
the other up. Contracts are keyed by mission id as well: keyed by archetype,
two Aluminium runs to the same station in one session were one record.

---

## What a screenshot of the card says

The Contracts app's detail panel prints `PRIMARY OBJECTIVES` for the selected
card only. Three shapes are measured, and the indent is what pairs a pickup
with its drop-off:

| Shape | Parent line | Indented under it | Frame |
|---|---|---|---|
| direct | `Collect Agricultural Supplies from Port Tressler.` | `Deliver 0/13 SCU to NB Int. Spaceport.` — no cargo named | Star Docs hauling guide, read by eye |
| multi-pickup | `Deliver 0/18 SCU of Aluminum to Stanton Gateway.` | `Collect Aluminum from Fallow Field.` ×3 — no share per source | this install, 8 Sep 2026 |
| multi-drop | `Collect Waste from CRU-L5 Beautiful Glen Station.` ×4 | `Deliver 0/2 SCU of Waste to Seraphim Station above Crusader.` | timesaver.gg 4.10 guide, read by eye |

A long line wraps — `… to Seraphim Station above` / `Crusader.` — and comes
back from the engine as two lines, the second without a diamond; it is joined
back when it sits within two line heights and no further left than the text.
`above Crusader` and `on Pyro IV` come off the place as the body.

The contract's own text adds the one thing the objectives never say: which
body a pickup is on. `PICK UP LOCATIONS (ANY ORDER)` lists `Freight elevator
at Fallow Field on Pyro IV`, and the 8 Sep frame read all three, `Pyro Ill`
for III included — kept as read, since a body is a hint for grouping stops and
not a claim the map has to honour.

What one frame does **not** give: the other cards' objectives. Five contracts
means five screenshots, one per card selected, and the plan says how many are
still to take.

---

## The plan

`GET /api/haul/plan`, drawn on the Shopping page under the session's
contracts, and `POST /api/haul/plan/trip` to write it into a tracked flight
plan with a load or unload action at every stop.

Each open hauling contract gets its legs from the best source it has, and
says which:

- **screenshot** — a Contracts-app frame showing this card selected, taken
  after the contract was accepted. Titles repeat — three of the five cards on
  the 8 Sep frame read *to Stanton Gateway* — so the archetype's cargo and its
  count of sources break the tie, and a frame already given to one contract
  goes to a second only when nothing else fits, flagged when it does.
- **title** — one leg with whichever end the title named and the other
  missing, with the archetype's count of the missing end in the note: *3
  pickups, places unknown until this card is photographed*.
- **none** — a vanilla title; only a screenshot can place it.

The stops are every pickup before any delivery, same-body stops kept
together, a delivery whose contract has no pickup on the plan placed after
every pickup the plan can see (the cargo has to come from somewhere), and a
place that is one contract's source and another's destination visited for the
load first and again to deliver once its cargo is aboard. Nothing here knows a
distance — the atlas has no coordinates for a station — so it is a starting
order for the pilot to rewrite, not an answer.

The SCU total is a floor and is called one: it counts the contracts whose card
was read. Against it the plan prints the ship's hold from the community dump
when the live feed knows the ship, and says in words when the floor is already
more than the hold.

Places resolve to the map the way price terminals do — by the place whose name
the text contains — and a place the logs have never seen visited keeps its
name and is listed as *not on the map yet* rather than dropped.

---

## Not done, and why

- **Which pickup is done.** The objective ids carry an index (`_0`, `_1`) that
  might follow the card's order. Nothing here checks it, so the plan shows
  *1 of 3 pickups done* on the contract and ticks no stop on its own.
- **Distances between stops.** The atlas is places, not coordinates. A
  `/showlocation` pin has coordinates, and one day the stops could be ordered
  by them; today the order is by body and by what there is to do.
- **The Offers tab.** The reader anchors on `ACCEPTED (n/m)`, so a frame of a
  contract not yet taken reads as nothing. Deliberate: a plan is for contracts
  the pilot holds.
