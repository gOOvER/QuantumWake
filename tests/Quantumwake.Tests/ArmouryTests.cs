using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The Armoury's arithmetic on the install's own figures for a gun.
/// </summary>
/// <remarks>
/// The fixtures are this install's numbers, read through <c>--armoury</c> on
/// 2026-09-16: the P4-AR at 12 physical, 810 a minute, 40 rounds; the
/// Gallant's three-round burst at 900 with a quarter-second cooldown; the
/// Zenith's 3.5-second charge for twice the hit and three rounds; the
/// Quartz's 225-a-second beam drawing 7.5 rounds a second from 45; the
/// Tripledown's three barrels of three pellets; the Salvo's charge that turns
/// one 45-point slug into seven quarter-strength fragments. None of the
/// derived figures is the game's - the rule is stated on the page - so the
/// tests pin the rule.
/// </remarks>
public class ArmouryTests
{
    private static PersonalWeapon Gun(string cls, DamageKinds hit, int magazine, params FireMode[] modes) =>
        new(cls, cls, "Rifle", "Medium", 2, "Behring", hit, 550, 2, 40, 0.05, 10, magazine, cls + "_mag", 2.65, modes);

    private static readonly FireMode Auto810 = new("AUTO", "Auto", 810);
    private static readonly PersonalWeapon P4 = Gun("behr_rifle_ballistic_01", new DamageKinds(Physical: 12), 40, Auto810, new FireMode("SEMI", "Semi", 810));

    [Fact]
    public void Automatic_fire_is_the_hit_times_the_cyclic_rate()
    {
        Assert.Equal(810, Armoury.SustainedRoundsPerMinute(Auto810));
        Assert.Equal(162, Armoury.DamagePerSecond(P4, Auto810));
        Assert.Equal(480, Armoury.DamagePerMagazine(P4, Auto810));
        Assert.Equal(2.96, Math.Round(Armoury.SecondsToEmpty(P4, Auto810), 2));
    }

    [Fact]
    public void A_burst_sustains_its_shots_over_the_burst_and_the_gap()
    {
        // Three at 900 is two gaps of 66.7 ms, then a quarter second: 3 shots in 0.383 s.
        var burst = new FireMode("BURST", "Burst", 900, BurstShots: 3, BurstCooldown: 0.25);
        Assert.Equal(470, Math.Round(Armoury.SustainedRoundsPerMinute(burst)));

        // With no cooldown of its own the next burst waits one cyclic gap.
        var free = new FireMode("BURST", "Burst", 600, BurstShots: 2);
        Assert.Equal(600, Math.Round(Armoury.SustainedRoundsPerMinute(free)));
    }

    [Fact]
    public void A_charge_is_one_full_charge_after_another_and_costs_its_rounds()
    {
        var zenith = Gun("volt_sniper_energy_01", new DamageKinds(Energy: 42.5), 22,
            new FireMode("CHARGE", "Charge", 325, ChargeSeconds: 3.5, ChargeDamageMultiplier: 2, ChargeAmmoMultiplier: 3));
        var charge = zenith.Modes[0];

        Assert.Equal(85, Armoury.DamagePerShot(zenith, charge));
        // 60 / (3.5 + 60/325) = 16.3 a minute; 22 rounds at three a shot is seven shots.
        Assert.Equal(16.3, Math.Round(Armoury.SustainedRoundsPerMinute(charge), 1));
        Assert.Equal(595, Armoury.DamagePerMagazine(zenith, charge));
        Assert.Equal(25.8, Math.Round(Armoury.SecondsToEmpty(zenith, charge), 1));
    }

    [Fact]
    public void A_charge_that_fragments_the_round_counts_the_fragments()
    {
        var salvo = Gun("hdgw_pistol_ballistic_01", new DamageKinds(Physical: 45), 8,
            new FireMode("CHARGE", "Charge", 170, ChargeSeconds: 1, ChargeDamageMultiplier: 0.25, ChargePellets: 7));

        Assert.Equal(78.75, Armoury.DamagePerShot(salvo, salvo.Modes[0]));
    }

    [Fact]
    public void A_beam_is_its_own_figure_a_second_and_drains_the_magazine_by_the_second()
    {
        var quartz = Gun("volt_smg_energy_01", new DamageKinds(Energy: 5), 45,
            new FireMode("BEAM", "Beam", 0, 1, 0, BeamDamagePerSecond: new DamageKinds(Energy: 225, Distortion: 50), BeamFullRange: 10, BeamZeroRange: 25, BeamAmmoPerSecond: 7.5));
        var beam = quartz.Modes[0];

        Assert.Equal(0, Armoury.SustainedRoundsPerMinute(beam));
        Assert.Equal(275, Armoury.DamagePerSecond(quartz, beam));
        Assert.Equal(6, Armoury.SecondsToEmpty(quartz, beam));
        Assert.Equal(1650, Armoury.DamagePerMagazine(quartz, beam));
    }

    [Fact]
    public void Parallel_barrels_are_pellets_and_rounds_added_up()
    {
        var tripledown = Gun("none_pistol_ballistic_01", new DamageKinds(Physical: 12), 12, new FireMode("SEMI", "Semi", 60, Pellets: 9, AmmoPerShot: 3));
        var semi = tripledown.Modes[0];

        Assert.Equal(108, Armoury.DamagePerShot(tripledown, semi));
        Assert.Equal(432, Armoury.DamagePerMagazine(tripledown, semi));
        Assert.Equal(4, Armoury.SecondsToEmpty(tripledown, semi));
    }

    [Fact]
    public void A_mode_on_the_second_load_uses_that_loads_hit()
    {
        var arlington = Gun("hdgw_rifle_ballistic_01", new DamageKinds(Physical: 80), 20,
            new FireMode("SEMI slug", "Semi", 85),
            new FireMode("SEMI", "Semi", 85, Pellets: 8, SecondaryAmmo: true, Hit: new DamageKinds(Physical: 12.5)));

        Assert.Equal(80, Armoury.DamagePerShot(arlington, arlington.Modes[0]));
        Assert.Equal(100, Armoury.DamagePerShot(arlington, arlington.Modes[1]));
    }

    [Fact]
    public void The_drop_is_a_straight_line_from_the_start_to_the_floor()
    {
        Assert.Equal(12, Armoury.DamageAt(P4, 30));
        Assert.Equal(12, Armoury.DamageAt(P4, 40));
        Assert.Equal(11, Armoury.DamageAt(P4, 60));
        Assert.Equal(10, Armoury.DamageAt(P4, 500));
        Assert.Equal(80, Armoury.FloorAt(P4));

        var flat = P4 with { DropStart = 0, DropPerMetre = 0 };
        Assert.Equal(12, Armoury.DamageAt(flat, 1000));
        Assert.Equal(0, Armoury.FloorAt(flat));
    }

    [Fact]
    public void A_magazine_the_files_leave_empty_derives_nothing()
    {
        var animus = Gun("apar_special_ballistic_02", DamageKinds.None, 0, new FireMode("SEMI", "Semi", 300));
        Assert.Equal(0, Armoury.DamagePerMagazine(animus, animus.Modes[0]));
        Assert.Equal(0, Armoury.SecondsToEmpty(animus, animus.Modes[0]));
    }

    [Fact]
    public void Damage_is_named_by_the_kind_most_of_it_is()
    {
        Assert.Equal("physical", new DamageKinds(Physical: 12).Dominant);
        Assert.Equal("energy", new DamageKinds(Energy: 32.5, Distortion: 7.5, Stun: 1).Dominant);
        Assert.Equal("mixed", new DamageKinds(Physical: 20000, Energy: 21000).Dominant);
        Assert.Equal("", DamageKinds.None.Dominant);
    }

    [Fact]
    public void A_set_is_the_words_before_the_slot_word()
    {
        Assert.Equal("Testudo", GameArmoury.Family("Testudo Core Deathblow"));
        Assert.Equal("Aves Starchaser", GameArmoury.Family("Aves Starchaser Core"));
        Assert.Equal("MacFlex \"Rucksack\"", GameArmoury.Family("MacFlex \"Rucksack\" Backpack"));
        Assert.Equal("Xanthule", GameArmoury.Family("Xanthule Flight Suit"));
        Assert.Equal("Geist", GameArmoury.Family("Geist Armor Arms"));
        Assert.Equal("Carnifex", GameArmoury.Family("Carnifex Armor"));
        Assert.Equal("Stoneskin Agra", GameArmoury.Family("Stoneskin Agra Undersuit"));
        // No slot word: the name is its own set, and "Helmets" is not "Helmet".
        Assert.Equal("Exo-8 Helmets", GameArmoury.Family("Exo-8 Helmets"));
    }

    [Fact]
    public void A_finish_is_the_plain_name_without_the_quoted_word()
    {
        Assert.Equal("Yubarev Pistol", GameArmoury.Unquoted("Yubarev \"Deadeye\" Pistol"));
        Assert.Equal("Atzkav Sniper Rifle", GameArmoury.Unquoted("Atzkav \"Deadeye\" Sniper Rifle"));
        Assert.Equal("P4-AR Rifle", GameArmoury.Unquoted("P4-AR Rifle"));
    }
}
