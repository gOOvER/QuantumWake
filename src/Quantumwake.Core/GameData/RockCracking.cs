namespace Quantumwake.Core.GameData;

/// <summary>One laser as mounted: the head, the passives in its slots, and the actives switched on.</summary>
public sealed record LaserFit(MiningLaser Laser, IReadOnlyList<MiningModule> Modules);

/// <summary>What the rock was scanned as: the figures the HUD prints.</summary>
/// <param name="MassKg">The rock's mass, in kilograms.</param>
/// <param name="ResistancePercent">The HUD's resistance, 0 to 100.</param>
/// <param name="InstabilityPercent">The HUD's instability, 0 to 100.</param>
public sealed record RockScan(double MassKg, double ResistancePercent, double InstabilityPercent);

/// <summary>Whether a fit breaks a rock, and by how much.</summary>
/// <param name="PowerDelivered">Laser power after modules and the rock's effective resistance.</param>
/// <param name="PowerRequired">What the rock needs, by the community rule below.</param>
/// <param name="Ratio">Delivered over required; 1 is the line.</param>
/// <param name="Verdict"><c>solo</c>, <c>gadget</c> or <c>crew</c>.</param>
/// <param name="EffectiveResistancePercent">The HUD's resistance after the fit's modifiers.</param>
/// <param name="EffectiveInstabilityPercent">The HUD's instability after them.</param>
/// <param name="WindowPercent">The optimal charge window's width, as a share of the gauge, after them.</param>
/// <param name="MaxCrackableMassKg">The heaviest rock this fit breaks at this resistance.</param>
/// <param name="EnergyCapacity">The rock's charge gauge at full, by the game's own constant.</param>
/// <param name="EnergyDecayPerSecond">What the rock sheds each second, by the game's own constant.</param>
public sealed record CrackVerdict(
    double PowerDelivered,
    double PowerRequired,
    double Ratio,
    string Verdict,
    double EffectiveResistancePercent,
    double EffectiveInstabilityPercent,
    double WindowPercent,
    double MaxCrackableMassKg,
    double EnergyCapacity,
    double EnergyDecayPerSecond,
    MiningModifiers Modifiers,
    IReadOnlyList<string> Notes);

/// <summary>
/// The rock-cracking calculator: the community's rule for whether a fit
/// breaks a rock, run on the game's own figures for the fit.
/// </summary>
/// <remarks>
/// <para>
/// What is the game's, read from the install: every laser's power and
/// modifiers, every module's multipliers and modifiers, every gadget's, and
/// the constants of the rock model - the gauge is <c>powerCapacityPerMass</c>
/// energy per kilogram and sheds <c>decayPerMass</c> per kilogram a second,
/// the optimal window is <c>optimalWindowSize</c> of it. Those are quoted on
/// the page as what they are.
/// </para>
/// <para>
/// What is not the game's is the line itself. The game does not publish how
/// resistance, mass and power meet; the community tools settled on
/// <b>0.36 W per kilogram at zero resistance</b>, with the HUD's resistance
/// scaled by the fit's resistance modifiers and taken off the power
/// delivered, and verdict lines at 115% (breaks solo) and 70% (breaks with a
/// gadget). That rule is scminer.rocks's, read from its calculator on
/// 2026-09-15, and it is used here unchanged so that the two agree on the
/// same rock - a different answer with no measurement behind it would be a
/// worse one. It is named <see cref="RequiredWattsPerKg"/> so that a
/// measured rule, when the logs or the HUD give one, replaces it in one
/// place. The page calls the result an estimate and says whose.
/// </para>
/// <para>
/// Not estimated: how long the crack takes. The game fills the fracture at
/// <c>controlledBreakingFillRate</c> a second once the charge sits in the
/// window - two seconds - so the time is the time to reach the window, which
/// is the pilot's throttle hand and not a figure this can produce.
/// </para>
/// </remarks>
public static class RockCracking
{
    /// <summary>The community line: watts a rock needs per kilogram at zero resistance. scminer.rocks, 2026-09-15.</summary>
    public const double RequiredWattsPerKg = 0.36;

    /// <summary>Delivered over required from which the rock breaks on the fit alone.</summary>
    public const double SoloRatio = 1.15;

    /// <summary>From which a gadget on the rock is expected to make up the difference.</summary>
    public const double GadgetRatio = 0.70;

    /// <summary>What a fit does to a rock as scanned.</summary>
    /// <param name="constants">The game's rock constants; null when the install has not been read, and the game-side figures come out as zero.</param>
    public static CrackVerdict Assess(MiningConstants? constants, RockScan rock, IReadOnlyList<LaserFit> fit, MiningGadget? gadget)
    {
        var notes = new List<string>();
        var heads = fit.Where(f => f.Laser.Power > 0).ToList();

        // Power: every head's fracture beam, each multiplied by its own
        // modules. The modifiers in percent points are averaged across the
        // heads - a MOLE's three Helixes are one -30% resistance, not three -
        // and the modules' and the gadget's are added on top, which is how
        // the community tools stack them.
        var power = 0.0;
        var modifiers = MiningModifiers.None;
        foreach (var head in heads)
        {
            var multiplier = head.Modules.Aggregate(1.0, (m, module) => m * module.PowerMultiplier);
            power += head.Laser.Power * multiplier;
            foreach (var module in head.Modules) modifiers = modifiers.Plus(module.Modifiers);
        }
        if (heads.Count > 0)
        {
            var averaged = heads.Aggregate(MiningModifiers.None, (m, h) => m.Plus(h.Laser.Modifiers));
            modifiers = modifiers.Plus(new MiningModifiers(
                averaged.Instability / heads.Count, averaged.WindowSize / heads.Count, averaged.Resistance / heads.Count,
                averaged.ShatterDamage / heads.Count, averaged.ClusterFactor / heads.Count,
                averaged.WindowRate / heads.Count, averaged.CatastrophicRate / heads.Count));
        }
        if (gadget is not null) modifiers = modifiers.Plus(gadget.Modifiers);

        var resistance = Math.Clamp(rock.ResistancePercent * (1 + modifiers.Resistance / 100), 0, 98);
        var instability = Math.Clamp(rock.InstabilityPercent * (1 + modifiers.Instability / 100), 0, 100);

        var required = RequiredWattsPerKg * rock.MassKg;
        var delivered = power * (1 - resistance / 100);
        var ratio = required > 0 ? delivered / required : 0;
        var verdict = heads.Count == 0 ? "none" : ratio >= SoloRatio ? "solo" : ratio >= GadgetRatio ? "gadget" : "crew";

        // The game's own window: its base share of the gauge, widened or
        // narrowed by the fit, and never past the widest the game allows.
        var baseWindow = constants?.OptimalWindowSize ?? 0;
        var maxWindow = constants?.OptimalWindowMaxSize ?? 1;
        var window = Math.Clamp(baseWindow * (1 + modifiers.WindowSize / 100), 0, maxWindow) * 100;

        var capacity = (constants?.PowerCapacityPerMass ?? 0) * rock.MassKg;
        var decay = (constants?.DecayPerMass ?? 0) * rock.MassKg;

        if (heads.Count == 0) notes.Add("No laser on the fit.");
        if (verdict == "gadget") notes.Add($"Short by {(1 - ratio) * 100:0}%: a gadget that cuts resistance, or an active module that raises power, is expected to make it up.");
        if (verdict == "crew") notes.Add($"Delivers {delivered:N0} of {required:N0} needed: more heads on the rock, or a lighter rock.");
        if (instability > 50) notes.Add($"Instability {instability:0}% after the fit: the charge will wander; a module or gadget that damps it helps more than power here.");
        if (constants is not null && rock.MassKg > 0)
            notes.Add($"By the game's constants the rock holds {capacity:N0} energy and sheds {decay:N0} a second; the window is {window:0}% of the gauge.");

        return new CrackVerdict(
            Math.Round(delivered), Math.Round(required), Math.Round(ratio, 3), verdict,
            Math.Round(resistance, 1), Math.Round(instability, 1), Math.Round(window, 1),
            Math.Round(power * (1 - resistance / 100) / RequiredWattsPerKg),
            Math.Round(capacity), Math.Round(decay), modifiers, notes);
    }
}
