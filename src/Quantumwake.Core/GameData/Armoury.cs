namespace Quantumwake.Core.GameData;

/// <summary>
/// The arithmetic the Armoury page runs on a gun: what a mode does a second,
/// a magazine, and how long the magazine lasts. Pure functions over the
/// records <see cref="GameArmoury"/> reads, so the numbers can be asserted.
/// </summary>
/// <remarks>
/// None of these is a figure the game publishes; each is the files' own
/// numbers combined the obvious way, and the page calls them derived. A
/// burst's sustained rate assumes the trigger is pulled again the moment the
/// gun allows it, a charge's assumes every shot is held to full charge, and
/// the drop curve is the game's straight line - so many per metre past a
/// start, down to a floor.
/// </remarks>
public static class Armoury
{
    /// <summary>
    /// Rounds a minute with the trigger held: the cyclic rate for automatic
    /// fire, the cap for semi, and for a burst its shots over the burst and
    /// the gap after it. A charge is one shot per charge; a beam has no rounds.
    /// </summary>
    public static double SustainedRoundsPerMinute(FireMode mode)
    {
        if (mode.Kind == "Beam") return 0;

        if (mode.Kind == "Charge")
            return mode.ChargeSeconds > 0
                ? 60 / (mode.ChargeSeconds + (mode.RoundsPerMinute > 0 ? 60 / mode.RoundsPerMinute : 0))
                : mode.RoundsPerMinute;

        if (mode.BurstShots > 1 && mode.RoundsPerMinute > 0)
        {
            var cyclic = 60 / mode.RoundsPerMinute;
            // A burst with no cooldown of its own still cannot start the next
            // burst before the cyclic gap the last round would have had.
            var burst = (mode.BurstShots - 1) * cyclic + (mode.BurstCooldown > 0 ? mode.BurstCooldown : cyclic);
            return mode.BurstShots * 60 / burst;
        }

        return mode.RoundsPerMinute;
    }

    /// <summary>One projectile's damage in this mode: the second load's where the mode fires that, the gun's otherwise.</summary>
    public static DamageKinds Hit(PersonalWeapon weapon, FireMode mode) => mode.Hit ?? weapon.Damage;

    /// <summary>One pull's damage at the muzzle: the hit, every pellet, and a full charge where there is one.</summary>
    public static double DamagePerShot(PersonalWeapon weapon, FireMode mode)
    {
        var pellets = mode.Kind == "Charge" && mode.ChargePellets > 0 ? mode.ChargePellets : Math.Max(1, mode.Pellets);
        return Hit(weapon, mode).Total * pellets * (mode.Kind == "Charge" ? mode.ChargeDamageMultiplier : 1);
    }

    /// <summary>Damage a second, holding the trigger, at the muzzle.</summary>
    public static double DamagePerSecond(PersonalWeapon weapon, FireMode mode) =>
        mode.Kind == "Beam"
            ? mode.BeamDamagePerSecond?.Total ?? 0
            : DamagePerShot(weapon, mode) * SustainedRoundsPerMinute(mode) / 60;

    /// <summary>Rounds one pull takes: the launcher's cost, times a charge's multiplier.</summary>
    private static double RoundsPerShot(FireMode mode) =>
        Math.Max(1, mode.AmmoPerShot) * (mode.Kind == "Charge" ? Math.Max(1, mode.ChargeAmmoMultiplier) : 1);

    /// <summary>What a whole magazine does, at the muzzle.</summary>
    public static double DamagePerMagazine(PersonalWeapon weapon, FireMode mode)
    {
        if (weapon.Magazine <= 0) return 0;
        if (mode.Kind == "Beam")
            return mode.BeamAmmoPerSecond > 0 ? (mode.BeamDamagePerSecond?.Total ?? 0) * weapon.Magazine / mode.BeamAmmoPerSecond : 0;
        return DamagePerShot(weapon, mode) * Math.Floor(weapon.Magazine / RoundsPerShot(mode));
    }

    /// <summary>Seconds of held trigger a magazine lasts.</summary>
    public static double SecondsToEmpty(PersonalWeapon weapon, FireMode mode)
    {
        if (weapon.Magazine <= 0) return 0;
        if (mode.Kind == "Beam") return mode.BeamAmmoPerSecond > 0 ? weapon.Magazine / mode.BeamAmmoPerSecond : 0;
        var rpm = SustainedRoundsPerMinute(mode);
        return rpm > 0 ? Math.Floor(weapon.Magazine / RoundsPerShot(mode)) * 60 / rpm : 0;
    }

    /// <summary>One projectile's damage at a distance, after the drop.</summary>
    public static double DamageAt(PersonalWeapon weapon, double metres)
    {
        var full = weapon.Damage.Total;
        if (weapon.DropStart <= 0 || weapon.DropPerMetre <= 0 || metres <= weapon.DropStart) return full;
        return Math.Max(weapon.DropFloor, full - (metres - weapon.DropStart) * weapon.DropPerMetre);
    }

    /// <summary>Metres past which a projectile does its least, or 0 when it never drops.</summary>
    public static double FloorAt(PersonalWeapon weapon)
    {
        if (weapon.DropStart <= 0 || weapon.DropPerMetre <= 0) return 0;
        var fall = weapon.Damage.Total - weapon.DropFloor;
        return fall <= 0 ? weapon.DropStart : weapon.DropStart + fall / weapon.DropPerMetre;
    }
}
