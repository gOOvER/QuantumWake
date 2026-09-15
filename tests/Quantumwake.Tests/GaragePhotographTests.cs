using Quantumwake.Core.GameData;
using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// A loadout screenshot, turned into a bench the pilot can start from.
/// </summary>
/// <remarks>
/// The screen labels ports "Cooler 2" and the dump calls them
/// hardpoint_cooler_right; only order joins them. What matters here is that
/// a port applies only when the reading settles on one class, that "Empty" is
/// a reading and empties the port, and that every port not applied says why -
/// because a bench quietly half-filled from a photograph is worse than stock.
/// </remarks>
public class GaragePhotographTests
{
    private static PartStats Part(string cls, string type, int size, string name) =>
        new(cls, type, size, 2, name, "Aegis Dynamics", null, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null, null, null, null);

    private static readonly Dictionary<string, PartStats> Parts = new(StringComparer.Ordinal)
    {
        ["COOL_Bracer"] = Part("COOL_Bracer", "Cooler", 1, "Bracer"),
        ["COOL_Glacier"] = Part("COOL_Glacier", "Cooler", 1, "Glacier"),
        ["COOL_Frost"] = Part("COOL_Frost", "Cooler", 2, "Frost-Star EX"),
        ["POWR_Regulus"] = Part("POWR_Regulus", "PowerPlant", 1, "Regulus"),
        ["POWR_Genoa"] = Part("POWR_Genoa", "PowerPlant", 2, "Genoa"),
        ["SHLD_Allstop"] = Part("SHLD_Allstop", "Shield", 1, "AllStop"),
        ["GUN_M6A"] = Part("GUN_M6A", "WeaponGun", 3, "M6A Laser Cannon"),
        ["GUN_Panther"] = Part("GUN_Panther", "WeaponGun", 3, "CF-337 Panther Repeater"),
    };

    private static FitPort Port(string id, string hardpoint, string? cls, string type, int size, bool editable = true, params FitPort[] children) =>
        new(id, hardpoint, cls, type, editable, size, size, [type], children);

    /// <summary>A Gladius-shaped ship: two coolers, a plant, a shield, three fixed guns, and a turret with a gun in it.</summary>
    private static ShipBase Gladius() => new(
        "AEGS_Gladius", "Aegis Gladius", "Aegis Dynamics", "Light Fighter", "Combat", 2, 1, true,
        48552, 0, 6110, new Vec3(1, 1, 1), new FlightStats(226, 520, 1193, 68, 52, 200), 600, 1600, 0,
        new Dictionary<string, int>(), new DatasetTotals(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
        [
            Port("p-cool-l", "hardpoint_cooler_left", "COOL_Bracer", "Cooler", 1),
            Port("p-cool-r", "hardpoint_cooler_right", "COOL_Bracer", "Cooler", 1),
            Port("p-plant", "hardpoint_power_plant", "POWR_Regulus", "PowerPlant", 1),
            Port("p-shield", "hardpoint_shield_generator", "SHLD_Allstop", "Shield", 1),
            Port("p-gun-1", "hardpoint_weapon_left", "GUN_Panther", "WeaponGun", 3),
            Port("p-gun-2", "hardpoint_weapon_right", "GUN_Panther", "WeaponGun", 3),
            Port("p-turret", "hardpoint_turret", "TUR_Ball", "Turret", 2, editable: false,
                Port("p-turret-gun", "hardpoint_class_2", "GUN_Panther", "WeaponGun", 3)),
        ]);

    private static readonly Dictionary<string, ShipInfo> Ships = new(StringComparer.Ordinal)
    {
        ["AEGS_Gladius"] = new("Aegis Gladius", "Combat", "Light Fighter", 1, true, null, null, null),
    };

    private static ScreenFitting Read(string slot, string cls, string name) =>
        new(slot, name, name, cls, "Exact", [], []);

    private static ScreenSighting Frame(string ship, params ScreenFitting[] fittings) =>
        new("ScreenShot-2026-09-12_21-55-29.jpg", new DateTimeOffset(2026, 9, 12, 21, 55, 29, TimeSpan.Zero),
            ScreenKind.Loadout, "a loadout", [], null, new LoadoutReading(ship.ToUpperInvariant(), ship, [], null, fittings),
            null, null, [], 0);

    [Fact]
    public void Ports_are_matched_by_kind_and_order_and_only_a_settled_reading_is_applied()
    {
        var fit = GaragePhotograph.Match(Gladius(), Frame("Aegis Gladius",
            Read("Cooler 1", "COOL_Bracer", "Bracer"),
            Read("Cooler 2", "COOL_Glacier", "Glacier"),
            Read("Power Plant 1", "POWR_Regulus", "Regulus"),
            new ScreenFitting("Shield Generator 1", null, null, null, "Empty", [], []),
            new ScreenFitting("Weapon 1", "MBA Cannon", null, null, "None", [], []),
            new ScreenFitting("Weapon 2", null, null, null, "None", [], [])), Parts, Ships);

        Assert.NotNull(fit);
        Assert.Equal("Aegis Gladius", fit.Ship);
        Assert.Equal(new DateTimeOffset(2026, 9, 12, 21, 55, 29, TimeSpan.Zero), fit.ShotAt);

        // The second cooler is the right-hand hardpoint; the shield is emptied; the guns wait.
        Assert.Equal(new Dictionary<string, string?> { ["p-cool-l"] = "COOL_Bracer", ["p-cool-r"] = "COOL_Glacier", ["p-plant"] = "POWR_Regulus", ["p-shield"] = null }, fit.Swaps);
        Assert.Equal(4, fit.Applied);
        Assert.Equal(2, fit.Changed);

        var gun1 = Assert.Single(fit.Ports, p => p.Slot == "Weapon 1");
        Assert.False(gun1.Applied);
        Assert.Equal("p-gun-1", gun1.PortId);
        Assert.Contains("read “MBA Cannon”, which names no one part", gun1.Why);

        var gun2 = Assert.Single(fit.Ports, p => p.Slot == "Weapon 2");
        Assert.Equal("nothing read under it", gun2.Why);
    }

    /// <summary>
    /// The Hermes lists shield_generator_02 before _01 in the dump; the screen
    /// numbers them by name. The number in the hardpoint wins when the kind has
    /// one on every port; left and right keep the dump's order.
    /// </summary>
    [Fact]
    public void A_numbered_hardpoint_is_the_port_the_screen_numbers_the_same()
    {
        var ship = Gladius() with
        {
            Loadout =
            [
                Port("s2", "hardpoint_shield_generator_02_hermes", "SHLD_Allstop", "Shield", 1),
                Port("s1", "hardpoint_shield_generator_01_hermes", "SHLD_Allstop", "Shield", 1),
                Port("c-l", "hardpoint_cooler_left", "COOL_Bracer", "Cooler", 1),
                Port("c-r", "hardpoint_cooler_right", "COOL_Bracer", "Cooler", 1),
            ],
        };

        var fit = GaragePhotograph.Match(ship, Frame("Aegis Gladius",
            new ScreenFitting("Shield Generator 1", null, null, null, "Empty", [], []),
            Read("Cooler 2", "COOL_Glacier", "Glacier")), Parts, Ships);

        Assert.NotNull(fit);
        Assert.Equal(new Dictionary<string, string?> { ["s1"] = null, ["c-r"] = "COOL_Glacier" }, fit.Swaps);
    }

    /// <summary>
    /// The Fleet Manager's loadout estimate is a table of parts by type, not a
    /// tree of ports: <c>Cooler ×2</c> is two ports carrying the same cooler,
    /// and the estimate says nothing about which is which, so the first two of
    /// the kind take it. Its type column pluralises (<c>Quantum Drives</c>) and
    /// the engine reads that Q as an O on the Hermes' estimate of 8 September -
    /// both are still the quantum drive.
    /// </summary>
    [Fact]
    public void The_estimates_counted_types_fill_that_many_ports_of_the_kind()
    {
        var parts = new Dictionary<string, PartStats>(Parts, StringComparer.Ordinal)
        {
            ["QDRV_Hemera"] = Part("QDRV_Hemera", "QuantumDrive", 1, "Hemera"),
        };
        var ship = Gladius() with
        {
            Loadout =
            [
                .. Gladius().Loadout,
                Port("p-qd", "hardpoint_quantum_drive", "QDRV_Atlas", "QuantumDrive", 1),
            ],
        };

        var fit = GaragePhotograph.Match(ship, Frame("Aegis Gladius",
            Read("Cooler ×2", "COOL_Glacier", "Glacier"),
            Read("Ouantum Drives", "QDRV_Hemera", "Hemera"),
            Read("Power Plant", "POWR_Regulus", "Regulus")), parts, Ships);

        Assert.NotNull(fit);
        Assert.Equal(new Dictionary<string, string?>
        {
            ["p-cool-l"] = "COOL_Glacier", ["p-cool-r"] = "COOL_Glacier", ["p-qd"] = "QDRV_Hemera", ["p-plant"] = "POWR_Regulus",
        }, fit.Swaps);
        Assert.Equal(4, fit.Applied);
        Assert.Equal(3, fit.Changed);
        Assert.Equal(2, fit.Ports.Count(p => p.Slot == "Cooler ×2"));
    }

    /// <summary>The M6A that reads as the M8A too: a name without a class is a tie, and a tie is not fitted.</summary>
    [Fact]
    public void A_reading_two_classes_answer_to_is_not_applied()
    {
        var fit = GaragePhotograph.Match(Gladius(), Frame("Aegis Gladius",
            new ScreenFitting("Weapon 1", "M6A Laser Cannon", "M6A Laser Cannon", null, "Confusable", [], [])), Parts, Ships);

        Assert.NotNull(fit);
        Assert.Empty(fit.Swaps);
        Assert.Contains("more than one class", Assert.Single(fit.Ports).Why);
    }

    [Fact]
    public void A_part_that_does_not_fit_the_port_is_refused_and_says_so()
    {
        var fit = GaragePhotograph.Match(Gladius(), Frame("Aegis Gladius",
            Read("Cooler 1", "COOL_Frost", "Frost-Star EX"),
            Read("Power Plant 1", "POWR_Genoa", "Genoa"),
            Read("Cooler 3", "COOL_Glacier", "Glacier"),
            Read("Turret Weapon Slot 1", "GUN_M6A", "M6A Laser Cannon")), Parts, Ships);

        Assert.NotNull(fit);
        Assert.Empty(fit.Swaps);
        Assert.Equal(
            ["Frost-Star EX (S2 Cooler) does not fit that port", "Genoa (S2 PowerPlant) does not fit that port",
             "the ship has no cooler 3 the bench can change", "not a kind the bench changes"],
            fit.Ports.Select(p => p.Why));
    }

    /// <summary>A resemblance is not a reading: the frame that read DUKE CORSAIR names nobody's bench.</summary>
    [Fact]
    public void A_frame_of_another_ship_or_of_a_ship_only_resembled_is_not_this_ships_fit()
    {
        Assert.Null(GaragePhotograph.Match(Gladius(), Frame("Drake Corsair", Read("Cooler 1", "COOL_Glacier", "Glacier")), Parts, Ships));

        var resembled = new ScreenSighting("s.jpg", DateTimeOffset.UtcNow, ScreenKind.Loadout, "", [], null,
            new LoadoutReading("AEGIS GLADIUS", null, ["Aegis Gladius"], null, [Read("Cooler 1", "COOL_Glacier", "Glacier")]),
            null, null, [], 0);
        Assert.Null(GaragePhotograph.Match(Gladius(), resembled, Parts, Ships));
    }

    [Fact]
    public void The_newest_reading_of_the_ship_wins()
    {
        var older = Frame("Aegis Gladius", Read("Cooler 1", "COOL_Bracer", "Bracer")) with { ShotAt = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero), Shot = "old.jpg" };
        var newer = Frame("Aegis Gladius", Read("Cooler 1", "COOL_Glacier", "Glacier"));

        var fit = GaragePhotograph.Latest(Gladius(), [older, newer, Frame("Drake Corsair")], Parts, Ships);

        Assert.NotNull(fit);
        Assert.Equal("ScreenShot-2026-09-12_21-55-29.jpg", fit.Shot);
        Assert.Equal("COOL_Glacier", fit.Swaps["p-cool-l"]);
    }
}
