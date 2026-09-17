using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The rock-cracking rule on the game's figures for the fit.
/// </summary>
/// <remarks>
/// The fixtures are the install's own numbers, read through <c>--mining</c>
/// on 2026-09-15: Helix I 3,900 at -30% resistance, Arbor MH1 2,340 at +25%,
/// Surge ×1.5 for 15 s, Sabir -50% resistance. The line - 0.36 W a kilogram,
/// 115% and 70% - is the community's, and the first test pins the numbers
/// scminer.rocks prints for the same rock so a drift on either side shows.
/// </remarks>
public class RockCrackingTests
{
    private static readonly MiningConstants Constants = new(10, 0.2, 0.1, 0.75, 0.5, 0.6, 0.7, 0.5, 0.5, 0.3, 0.1, 330, 3);

    private static readonly MiningLaser Helix1 = new("Mining_Laser_THCN_Helix_S1", "Helix I Mining Laser", 1, 3900, 1850, 30, 0.2,
        new MiningModifiers(WindowSize: -40, Resistance: -30), "Thermyte Concern");

    private static readonly MiningLaser Arbor1 = new("Mining_Laser_GRIN_Arbor_S1", "Arbor MH1 Mining Laser", 1, 2340, 1850, 30, 0.05,
        new MiningModifiers(Instability: -35, WindowSize: 40, Resistance: 25), "Greycat Industrial");

    private static readonly MiningModule Surge = new("Mining_Modules_Active_Surge", "Surge Module", true, 15, 7, 1.5, 1.0, 0,
        new MiningModifiers(Instability: 10, Resistance: -15.5), "Thermyte Concern");

    private static readonly MiningModule Focus = new("Mining_Modules_Passive_Focus_MK1", "Focus Module", false, 0, 0, 0.85, 1.0, 0,
        new MiningModifiers(WindowSize: 30), "Thermyte Concern");

    private static readonly MiningGadget Sabir = new("Mining_Gadget_SHIN_Sabir", "Sabir",
        new MiningModifiers(Instability: 15, WindowSize: 50, Resistance: -50), "Shubin Interstellar");

    /// <summary>scminer.rocks's default rock: 5,000 kg, clean, on a Helix I - 3,900 delivered, 1,800 required, 217%, 10,833 kg at most.</summary>
    [Fact]
    public void A_clean_rock_on_a_helix_reads_as_the_community_tool_prints_it()
    {
        var verdict = RockCracking.Assess(Constants, new RockScan(5000, 0, 15), [new LaserFit(Helix1, [])], null);

        Assert.Equal(3900, verdict.PowerDelivered);
        Assert.Equal(1800, verdict.PowerRequired);
        Assert.Equal(2.167, verdict.Ratio);
        Assert.Equal("solo", verdict.Verdict);
        Assert.Equal(10833, verdict.MaxCrackableMassKg);

        // The game's side, said beside it: 50,000 energy, 1,000 shed a second,
        // a window 6% of the gauge after the Helix's -40%.
        Assert.Equal(50000, verdict.EnergyCapacity);
        Assert.Equal(1000, verdict.EnergyDecayPerSecond);
        Assert.Equal(6, verdict.WindowPercent);
    }

    /// <summary>The HUD's resistance is scaled by the fit's modifiers, then taken off the power.</summary>
    [Fact]
    public void Resistance_is_scaled_by_the_fit_and_taken_off_the_power()
    {
        var helix = RockCracking.Assess(Constants, new RockScan(10000, 50, 20), [new LaserFit(Helix1, [])], null);

        // 50% × (1 - 0.30) = 35%; 3,900 × 0.65 = 2,535 against 3,600.
        Assert.Equal(35, helix.EffectiveResistancePercent);
        Assert.Equal(2535, helix.PowerDelivered);
        Assert.Equal(3600, helix.PowerRequired);
        Assert.Equal("gadget", helix.Verdict);

        // Sabir on the rock: -30 - 50 = -80% → 10%; 3,510 delivered, and it breaks.
        var withSabir = RockCracking.Assess(Constants, new RockScan(10000, 50, 20), [new LaserFit(Helix1, [])], Sabir);
        Assert.Equal(10, withSabir.EffectiveResistancePercent);
        Assert.Equal(3510, withSabir.PowerDelivered);
        Assert.Equal("gadget", withSabir.Verdict);
        Assert.Equal(23, withSabir.EffectiveInstabilityPercent);

        // The Arbor raises resistance instead: 50% × 1.25 = 62.5%, and 2,340 × 0.375 = 878.
        var arbor = RockCracking.Assess(Constants, new RockScan(10000, 50, 20), [new LaserFit(Arbor1, [])], null);
        Assert.Equal(62.5, arbor.EffectiveResistancePercent);
        Assert.Equal(878, arbor.PowerDelivered);
        Assert.Equal("crew", arbor.Verdict);
    }

    /// <summary>A module multiplies its own head's power and adds its points; three heads average their own.</summary>
    [Fact]
    public void Modules_multiply_their_head_and_several_heads_average_their_modifiers()
    {
        var surged = RockCracking.Assess(Constants, new RockScan(12000, 0, 10), [new LaserFit(Helix1, [Surge, Focus])], null);

        // 3,900 × 1.5 × 0.85 = 4,972.5 against 4,320.
        Assert.Equal(4972, surged.PowerDelivered);   // 4,972.5 rounds to even
        Assert.Equal("solo", surged.Verdict);
        // Window: -40 (Helix) +30 (Focus) = -10% → 9% of the gauge.
        Assert.Equal(9, surged.WindowPercent);

        var mole = RockCracking.Assess(Constants, new RockScan(30000, 40, 10),
            [new LaserFit(Helix1, []), new LaserFit(Helix1, []), new LaserFit(Arbor1, [])], null);

        // Resistance points averaged: (-30 - 30 + 25) / 3 = -11.7% → 40% × 0.883 = 35.3%.
        Assert.Equal(35.3, mole.EffectiveResistancePercent);
        // (3,900 + 3,900 + 2,340) × (1 - 0.3533) = 6,557 against 10,800: not this fit.
        Assert.Equal(6557, mole.PowerDelivered);
        Assert.Equal("crew", mole.Verdict);
    }

    [Fact]
    public void No_laser_is_said_rather_than_computed()
    {
        var verdict = RockCracking.Assess(null, new RockScan(5000, 0, 0), [], null);

        Assert.Equal("none", verdict.Verdict);
        Assert.Equal(0, verdict.PowerDelivered);
        Assert.Contains("No laser on the fit.", verdict.Notes);
        Assert.Equal(0, verdict.EnergyCapacity);
    }
}
