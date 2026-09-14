using Quantumwake.Core.GameData;
using Quantumwake.Data;
using Xunit.Abstractions;

namespace Quantumwake.Tests;

/// <summary>
/// The garage sheet against the dataset's own totals.
/// </summary>
/// <remarks>
/// <para>
/// The fixture is seven ships cut from the community dump - a light fighter,
/// a stealth Hornet, a Cutlass with a crewed turret, a Constellation with
/// two, an Aurora, a Starlancer - with their <c>Emission</c>,
/// <c>Power</c>, <c>ShieldHp</c> and <c>Weaponry</c> blocks kept. The sheet
/// never reads those blocks; it recomputes them from the parts. Agreement is
/// the evidence that the model is the dump's model and not a resemblance.
/// </para>
/// <para>
/// The same comparison runs over the whole dump through the CLI
/// (<c>--garage-check</c>), which is where the 269-of-269 in docs/garage.md
/// comes from; this is the part of it that runs on every build.
/// </para>
/// </remarks>
public class GarageSheetTests(ITestOutputHelper output)
{
    private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static (Dictionary<string, ShipBase> Ships, Dictionary<string, PartStats> Parts) Load()
    {
        var parts = CommunityData.DigestPartStats(File.ReadAllText(Path.Combine(Root, "garage-ship-items.json")));
        return (CommunityData.DigestShipStats(File.ReadAllText(Path.Combine(Root, "garage-ships.json")), parts), parts);
    }

    /// <summary>
    /// The seven hulls whose remote-turret guns the published dump counts
    /// differently from its own name rule. Listed in docs/garage.md.
    /// </summary>
    private static readonly HashSet<string> RemoteTurretDisputed = new(StringComparer.Ordinal)
    {
        "ANVL_Asgard", "ANVL_Asgard_Collector_Military", "DRAK_Cutlass_Steel",
        "MISC_Starlancer_Max", "MISC_Starlancer_Max_Collector_Indust",
        "MISC_Starlancer_TAC", "MISC_Starlancer_TAC_Collector_Military",
    };

    private static bool Within(double got, double want, double tolerance) =>
        want == 0 ? got == 0 : Math.Abs(got - want) / want <= tolerance;

    [Fact]
    public void The_digest_keeps_what_the_sheet_needs()
    {
        var (ships, parts) = Load();

        Assert.Equal(7, ships.Count);
        Assert.Equal(374, parts.Count);

        var gladius = ships["AEGS_Gladius"];
        Assert.Equal("Aegis Gladius", gladius.Name);
        Assert.Equal(2, gladius.PowerPools["Shield"]);
        Assert.Equal(6923, gladius.CrossSection.X);
        Assert.Equal(600, gladius.QuantumFuel);

        var bracer = parts["COOL_AEGS_S01_Bracer_SCItem"];
        Assert.Equal("Cooler", bracer.Type);
        Assert.Equal(1490, bracer.Em);
        Assert.Equal(7260, bracer.Ir);
        Assert.True(bracer.CoolantGen > 0);

        var panther = parts["KLWE_LaserRepeater_S3"];
        Assert.NotNull(panther.Weapon);
        Assert.Equal(545.6, panther.Weapon.Dps, 1);
    }

    /// <summary>
    /// EM, IR and the power segments must agree on every ship, because the
    /// model was taken from the dump's generator. A miss here is a port error,
    /// not a data quirk.
    /// </summary>
    [Fact]
    public void Signatures_and_power_match_the_dataset_on_every_ship()
    {
        var (ships, parts) = Load();
        var misses = new List<string>();

        foreach (var ship in ships.Values.Where(s => s.IsSpaceship && s.Dataset.EmShields > 0))
        {
            var sheet = ShipSheet.Compute(ship, parts);
            var d = ship.Dataset;

            output.WriteLine($"{ship.Name,-40} EM {sheet.Shields.Em,7:0} / {d.EmShields,7:0}   IR {sheet.Shields.Ir,6:0} / {d.IrShields,6:0}   power {sheet.Power.Available,3} / {d.PowerSegments,3}   EMq {sheet.Quantum.Em,7:0} / {d.EmQuantum,7:0}");

            if (!Within(sheet.Shields.Em, d.EmShields, 0.01)) misses.Add($"{ship.Name}: EM {sheet.Shields.Em} vs {d.EmShields}");
            if (!Within(sheet.Shields.Ir, d.IrShields, 0.01)) misses.Add($"{ship.Name}: IR {sheet.Shields.Ir} vs {d.IrShields}");
            if (!Within(sheet.Quantum.Em, d.EmQuantum, 0.01)) misses.Add($"{ship.Name}: EM quantum {sheet.Quantum.Em} vs {d.EmQuantum}");
            if (!Within(sheet.Quantum.Ir, d.IrQuantum, 0.01)) misses.Add($"{ship.Name}: IR quantum {sheet.Quantum.Ir} vs {d.IrQuantum}");
            if (sheet.Power.Available != d.PowerSegments) misses.Add($"{ship.Name}: power {sheet.Power.Available} vs {d.PowerSegments}");
            if (!Within(sheet.Cooling.Generated, d.CoolingSegments, 0.01)) misses.Add($"{ship.Name}: cooling {sheet.Cooling.Generated} vs {d.CoolingSegments}");
        }

        Assert.Empty(misses);
    }

    [Fact]
    public void Shields_dps_range_and_mass_match_the_dataset()
    {
        var (ships, parts) = Load();
        var misses = new List<string>();

        foreach (var ship in ships.Values.Where(s => s.IsSpaceship))
        {
            var sheet = ShipSheet.Compute(ship, parts);
            var d = ship.Dataset;

            output.WriteLine($"{ship.Name,-40} shield {sheet.Shield.Hp,6:0} / {d.ShieldHp,6:0}   dps {sheet.Weapons.FixedDps,7:0.0} / {d.FixedDps,7:0.0}   range {sheet.QuantumDrive?.Range,14:0} / {d.QuantumRange,14:0}   mass {sheet.Mass,7:0} / {d.MassTotal,7:0}");

            if (!Within(sheet.Shield.Hp, d.ShieldHp, 0.01)) misses.Add($"{ship.Name}: shield {sheet.Shield.Hp} vs {d.ShieldHp}");
            // Which row a remote turret lands in is the one thing the dump does not
            // settle - see ShipSheet.IsTurret - and the Starlancer here is one of
            // the seven it lands differently. Its guns are still all on the sheet.
            if (!Within(sheet.Weapons.FixedDps, d.FixedDps, 0.02) && !RemoteTurretDisputed.Contains(ship.Class))
                misses.Add($"{ship.Name}: dps {sheet.Weapons.FixedDps} vs {d.FixedDps}");
            if (sheet.QuantumDrive is { } q && !Within(q.Range, d.QuantumRange, 0.01)) misses.Add($"{ship.Name}: range {q.Range} vs {d.QuantumRange}");
            if (!Within(sheet.Mass, d.MassTotal, 0.02)) misses.Add($"{ship.Name}: mass {sheet.Mass} vs {d.MassTotal}");
        }

        Assert.Empty(misses);
    }

    /// <summary>
    /// The stealth question, by hand. Two Bracers off, two Glaciers on: the
    /// Glacier's own IR is higher but it makes more coolant, so the cooling
    /// load falls and the ship's IR with it. The sheet must show the parts it
    /// changed and leave every other port as stock.
    /// </summary>
    [Fact]
    public void Swapping_the_coolers_moves_ir_the_way_the_model_says()
    {
        var (ships, parts) = Load();
        var gladius = ships["AEGS_Gladius"];
        var stock = ShipSheet.Compute(gladius, parts);

        var glacier = parts.Values.Single(p => p.Type == "Cooler" && p.Name == "Glacier");
        var coolerPorts = gladius.Loadout.Where(p => p.Type == "Cooler").Select(p => p.PortId).ToList();
        Assert.Equal(2, coolerPorts.Count);

        var swapped = ShipSheet.Compute(gladius, parts,
            coolerPorts.ToDictionary(id => id, _ => (string?)glacier.Class));

        // The expected figure, worked from the model rather than from the code
        // under test: IR is the part sum times the armour times the load.
        var stockCoolerIr = parts["COOL_AEGS_S01_Bracer_SCItem"].Ir;
        var otherIr = stock.Shields.Ir / (stock.Cooling.LoadShields * stock.ArmorSignals.Ir) - 2 * stockCoolerIr;
        var newGeneration = stock.Cooling.Generated - 2 * parts["COOL_AEGS_S01_Bracer_SCItem"].CoolantGen + 2 * glacier.CoolantGen;
        var newLoad = stock.Cooling.UsedShields / newGeneration;
        var expectedIr = (otherIr + 2 * glacier.Ir) * stock.ArmorSignals.Ir * newLoad;

        output.WriteLine($"stock IR {stock.Shields.Ir}, swapped IR {swapped.Shields.Ir}, expected {expectedIr:0}");

        // Within two: the hand figure runs through the sheet's rounded load.
        Assert.InRange(swapped.Shields.Ir, expectedIr - 2, expectedIr + 2);
        Assert.True(swapped.Shields.Ir < stock.Shields.Ir, "a better cooler should lower the ship's IR");
        Assert.Equal(2, swapped.Parts.Count(p => p.Changed));
        Assert.All(swapped.Parts.Where(p => p.Changed), p => Assert.Equal(glacier.Class, p.Class));
        Assert.Equal(stock.Weapons.FixedDps, swapped.Weapons.FixedDps);
        Assert.Equal(stock.Shield.Hp, swapped.Shield.Hp);
    }

    /// <summary>A third shield on a two-pool ship never comes online, in the fight or in the signature.</summary>
    [Fact]
    public void A_shield_beyond_the_pool_counts_for_nothing()
    {
        var (ships, parts) = Load();
        var gladius = ships["AEGS_Gladius"];
        var stock = ShipSheet.Compute(gladius, parts);

        Assert.Equal(2, stock.Shield.Generators);
        Assert.Equal(2, stock.Shield.PoolLimit);
        Assert.Equal(2 * parts["SHLD_GODI_S01_AllStop_SCItem"].Shield!.Hp, stock.Shield.Hp);
    }

    [Fact]
    public void A_fitted_part_the_reference_cannot_describe_is_noted_not_invented()
    {
        var (ships, parts) = Load();
        var gladius = ships["AEGS_Gladius"];
        var plant = gladius.Loadout.Single(p => p.Type == "PowerPlant").PortId;

        var sheet = ShipSheet.Compute(gladius, parts, new Dictionary<string, string?> { [plant] = "POWR_FROM_THE_FUTURE" });

        Assert.Contains(sheet.Notes, n => n.Contains("POWR_FROM_THE_FUTURE"));
        Assert.Equal(0, sheet.Power.Available);
    }
}
