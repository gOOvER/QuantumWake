namespace Quantumwake.Core.GameData;

/// <summary>A scraper module in a salvage head's slot, as the install rates it.</summary>
/// <param name="Speed">The extraction speed the beam runs at - the Abrade's 0.15, the file's own unit.</param>
/// <param name="Radius">The beam's radius on the hull, metres.</param>
/// <param name="Efficiency">Share of what is scraped that lands in the hold, 0 to 1.</param>
public sealed record SalvageModule(string Class, string Name, double Speed, double Radius, double Efficiency, string Manufacturer);

/// <summary>A salvage head - the Baler - and how many modules it takes.</summary>
public sealed record SalvageHead(string Class, string Name, int Slots, string Manufacturer);

/// <summary>
/// What one hull's salvage controller says about how it salvages: which
/// commodity its scraping makes, how much of which commodity its
/// disintegration makes per cubic metre of hull, and how many heads it runs.
/// </summary>
/// <param name="Ship">The vehicle class the controller names - <c>DRAK_Vulture</c>.</param>
/// <param name="ScrapesTo">The resource the hull-scraping beam produces, or null for a hull that only disintegrates.</param>
/// <param name="ScuPerCubicMetre">SCU of the disintegration resource per cubic metre of hull taken apart.</param>
/// <param name="DisintegratesTo">The resource disintegration produces - chunks, powder or scrap, which sell differently.</param>
/// <param name="Heads">Salvage heads the controller runs; 0 on an arm that only disintegrates.</param>
/// <param name="BoxSecondsPerScu">Seconds the filler takes to pack one SCU.</param>
public sealed record SalvageShip(
    string Ship,
    string? ScrapesTo,
    double ScuPerCubicMetre,
    string? DisintegratesTo,
    int Heads,
    double BoxSecondsPerScu);

/// <summary>The constants the game scrapes a hull by.</summary>
/// <param name="HullThicknessMetres">How deep the beam takes material from a hull - 9 mm on this install.</param>
/// <param name="AmmoToMaterialFactor">The factor from the beam's ammunition to material made.</param>
public sealed record SalvageConstants(double HullThicknessMetres, double AmmoToMaterialFactor);

public sealed record GameSalvageData(SalvageConstants? Constants, List<SalvageModule> Modules, List<SalvageHead> Heads, List<SalvageShip> Ships)
{
    public static readonly GameSalvageData Empty = new(null, [], [], []);
}

/// <summary>
/// Reads what the install says about salvage.
/// </summary>
/// <remarks>
/// <para>
/// Not what a hull is worth. The question the community calculators answer
/// - so many SCU of RMC from a Cutlass - needs the hull's surface area and
/// volume, and those are geometry the DataCore does not hold: it carries
/// the rule (<c>hullThicknessMeters</c> 0.009 and a material factor on
/// <c>SGlobalSalvageRepairBeamParams</c>, a per-ship SCU-per-cubic-metre on
/// each salvage controller) and never the area or the volume the rule is
/// applied to. UEX's vehicle records carry no salvage figure either, and
/// the logs record no salvage (see <c>untapped-signals.md</c>). So no
/// per-hull yield is shown, and the page says why.
/// </para>
/// <para>
/// What is here is the equipment and the rules: every scraper module's
/// speed, radius and efficiency (the same three figures its own description
/// text states, which is the check), every head and its slots, and each
/// salvage hull's controller - what its scraping makes (Recycled Material
/// Composite), what its disintegration makes and at what rate (the
/// Reclaimer 0.00525 SCU of Construction Salvage a cubic metre, the Vulture
/// 0.0027 of Construction Rubble, the MOTH 0.0055 of Construction Pieces),
/// and how many heads it runs.
/// </para>
/// </remarks>
public static class GameSalvage
{
    public static GameSalvageData Read(DataCore core, IReadOnlyDictionary<string, GameItem> facts, IReadOnlyDictionary<string, string> resourceNames)
    {
        var data = new GameSalvageData(null, [], [], []);
        var controller = core.StructIndexOf("SCItemSalvageControllerParams");
        var modifier = core.StructIndexOf("EntityComponentAttachableModifierParams");
        var ports = core.StructIndexOf("SItemPortContainerComponentParams");

        var byId = new Dictionary<Guid, DataRecord>();
        foreach (var record in core.Records()) byId.TryAdd(record.Hash, record);

        foreach (var record in core.Records())
        {
            if (record.Name.Equals("SGlobalSalvageRepairBeamParams.SGlobalSalvageRepairBeamParams", StringComparison.OrdinalIgnoreCase))
            {
                var at = core.InstanceAt(record, record.VariantIndex);
                if (GameMining.Nested(core, at, record.StructIndex, "materialParams") is { } m)
                    data = data with { Constants = new SalvageConstants(core.SingleAt(m.At, m.StructIndex, "hullThicknessMeters") ?? 0, core.SingleAt(m.At, m.StructIndex, "ammoToMaterialFactor") ?? 0) };
                continue;
            }

            if (!record.Name.StartsWith("EntityClassDefinition.", StringComparison.OrdinalIgnoreCase)) continue;
            var cls = record.Name["EntityClassDefinition.".Length..];
            if (cls.Contains("template", StringComparison.OrdinalIgnoreCase)) continue;

            if (cls.StartsWith("Controller_Salvage_", StringComparison.OrdinalIgnoreCase))
            {
                if (Ship(core, record, cls, byId, resourceNames, controller) is { } ship) data.Ships.Add(ship);
            }
            else if (cls.StartsWith("Salvage_Modifier_", StringComparison.OrdinalIgnoreCase))
            {
                if (Module(core, record, cls, facts, modifier) is { } module) data.Modules.Add(module);
            }
            else if (cls.StartsWith("Salvage_Head_", StringComparison.OrdinalIgnoreCase))
            {
                if (Head(core, record, cls, facts, ports) is { } head) data.Heads.Add(head);
            }
        }

        data.Modules.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        data.Heads.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        // A hull runs several controllers - the Reclaimer's arm disintegrates
        // and its turrets scrape, the MOTH has four - and the page wants the
        // hull: what any of them scrapes to, the disintegration rate of the
        // one that has it, the heads added up.
        var merged = data.Ships
            .GroupBy(s => s.Ship, StringComparer.OrdinalIgnoreCase)
            .Select(g => new SalvageShip(
                g.Key,
                g.Select(s => s.ScrapesTo).FirstOrDefault(r => r is not null),
                g.Max(s => s.ScuPerCubicMetre),
                g.OrderByDescending(s => s.ScuPerCubicMetre).Select(s => s.DisintegratesTo).FirstOrDefault(r => r is not null),
                g.Sum(s => s.Heads),
                g.Where(s => s.Heads > 0).Select(s => s.BoxSecondsPerScu).DefaultIfEmpty(g.Max(s => s.BoxSecondsPerScu)).Max()))
            .OrderBy(s => s.Ship, StringComparer.OrdinalIgnoreCase)
            .ToList();
        data.Ships.Clear();
        data.Ships.AddRange(merged);
        return data;
    }

    private static SalvageShip? Ship(DataCore core, DataRecord record, string cls, IReadOnlyDictionary<Guid, DataRecord> byId, IReadOnlyDictionary<string, string> resourceNames, int controller)
    {
        // The bare controller, the hand tools and the C1's tractor-only rig say
        // nothing about a hull taking one apart.
        var ship = cls["Controller_Salvage_".Length..];
        if (ship.Length == 0 || ship.Contains("TractorBeamOnly", StringComparison.OrdinalIgnoreCase)) return null;
        foreach (var suffix in new[] { "_ArmOperator", "_ToolArm", "_Turret", "_FrontCab", "_Left", "_Right", "_Secondary" })
            if (ship.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) ship = ship[..^suffix.Length];

        foreach (var component in core.PointerArray(record, "Components"))
        {
            if (component.StructIndex != controller) continue;
            var at = core.InstanceAt(component);
            var s = controller;

            // The game's own name for the resource - Recycled Material Composite,
            // Construction Rubble - as the commodity table names it, the record's
            // class when it does not.
            string? Resource(long from, int structIndex, string field) =>
                core.ReferenceAt(from, structIndex, field) is { } id
                    ? resourceNames.TryGetValue(id.ToString(), out var named) ? named : byId.TryGetValue(id, out var r) ? r.Name["ResourceType.".Length..] : null
                    : null;

            string? scrapes = null;
            if (core.PointerAt(at, s, "scrapingParams") is { } sp) scrapes = Resource(core.InstanceAt(sp), sp.StructIndex, "scrapingResourceType");

            double rate = 0;
            string? disintegrates = null;
            if (core.PointerAt(at, s, "structuralParams") is { } st)
            {
                var sat = core.InstanceAt(st);
                rate = core.SingleAt(sat, st.StructIndex, "disintegrationSCUPerCubicMetre") ?? 0;
                disintegrates = Resource(sat, st.StructIndex, "disintegrationResourceType");
            }

            double boxSeconds = 0;
            if (core.PointerAt(at, s, "cargoParams") is { } cp) boxSeconds = core.SingleAt(core.InstanceAt(cp), cp.StructIndex, "boxFillingTimePerSCU") ?? 0;

            if (scrapes is null && rate <= 0) return null;
            return new SalvageShip(ship, scrapes, Math.Round(rate, 6), disintegrates, core.Int32At(at, s, "numSupportedSalvageHeads") ?? 0, boxSeconds);
        }
        return null;
    }

    private static SalvageModule? Module(DataCore core, DataRecord record, string cls, IReadOnlyDictionary<string, GameItem> facts, int modifier)
    {
        foreach (var component in core.PointerArray(record, "Components"))
        {
            if (component.StructIndex != modifier) continue;
            var at = core.InstanceAt(component);
            foreach (var pointer in core.PointerArrayAt(at, modifier, "modifiers"))
            {
                if (core.StructName(pointer.StructIndex) != "ItemWeaponModifiersParams") continue;
                var mat = core.InstanceAt(pointer);
                if (GameMining.Nested(core, mat, pointer.StructIndex, "weaponModifier") is not { } weapon) continue;
                if (GameMining.Nested(core, weapon.At, weapon.StructIndex, "weaponStats") is not { } stats) continue;
                if (GameMining.Nested(core, stats.At, stats.StructIndex, "salvageModifier") is not { } salvage) continue;
                var speed = core.SingleAt(salvage.At, salvage.StructIndex, "salvageSpeedMultiplier") ?? 1;
                var radius = core.SingleAt(salvage.At, salvage.StructIndex, "radiusMultiplier") ?? 1;
                var efficiency = core.SingleAt(salvage.At, salvage.StructIndex, "extractionEfficiency") ?? 1;
                // A tractor module leaves the three at 1 and is not a scraper.
                if (speed == 1 && radius == 1 && efficiency == 1) return null;
                facts.TryGetValue(cls, out var item);
                if (item is null || item.Name == cls) return null;
                return new SalvageModule(cls, item.Name, Math.Round(speed, 4), Math.Round(radius, 3), Math.Round(efficiency, 4), item.Manufacturer);
            }
        }
        return null;
    }

    private static SalvageHead? Head(DataCore core, DataRecord record, string cls, IReadOnlyDictionary<string, GameItem> facts, int ports)
    {
        facts.TryGetValue(cls, out var item);
        if (item is null || item.Name == cls) return null;
        var slots = 0;
        foreach (var component in core.PointerArray(record, "Components"))
        {
            if (component.StructIndex != ports) continue;
            var at = core.InstanceAt(component);
            foreach (var port in core.ClassArrayAt(at, ports, "Ports"))
            {
                var pat = core.InstanceAt(port);
                var types = core.ClassArrayAt(pat, port.StructIndex, "Types");
                if (types.Any(t => string.Equals(core.EnumAt(core.InstanceAt(t), t.StructIndex, "Type"), "SalvageModifier", StringComparison.Ordinal)))
                    slots++;
            }
        }
        return new SalvageHead(cls, item.Name, slots, item.Manufacturer);
    }
}
