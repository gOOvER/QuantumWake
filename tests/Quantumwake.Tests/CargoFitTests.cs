using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The crate packer on the dump's own grids.
/// </summary>
/// <remarks>
/// The grids are the community dump's for four hulls on 2026-09-16 - the
/// Hermes' two 5 × 22.5 × 2.5 m holds, the Corsair's one 5 × 11.25 × 2.5
/// that takes a box no longer than 7.5 m, the Freelancer's 2.5 × 11.25 ×
/// 3.75 rear and two 2.5 × 1.25 × 3.75 mids, the Cutlass Black's 5 × 6.25
/// × 2.5 main that takes only a 2 SCU box - and the crates are the
/// install's. The questions are the ones a hauler asks.
/// </remarks>
public class CargoFitTests
{
    private static readonly Vec3 Cell = new(1.25, 1.25, 1.25);
    private static readonly Vec3 Any = new(0, 0, 0);

    private static CargoGrid Grid(string cls, double x, double y, double z, double scu, Vec3? max = null, Vec3? min = null) =>
        new(cls, null, x, y, z, scu, min ?? Cell, max ?? Any, false);

    // The one-cell rule on a 144 SCU hold is the dump's placeholder.
    private static readonly IReadOnlyList<CargoGrid> Hermes =
    [
        Grid("RSI_Hermes_CargoInventory_Main", 5, 22.5, 2.5, 144, Cell),
        Grid("RSI_Hermes_CargoInventory_Main", 5, 22.5, 2.5, 144, Cell),
    ];

    private static readonly IReadOnlyList<CargoGrid> Corsair =
        [Grid("DRAK_Corsair_CargoGrid", 5, 11.25, 2.5, 72, new Vec3(5, 7.5, 2.5))];

    private static readonly IReadOnlyList<CargoGrid> Freelancer =
    [
        Grid("MISC_Freelancer_CargoGrid_Rear", 2.5, 11.25, 3.75, 54, new Vec3(2.5, 10, 2.5)),
        Grid("MISC_Freelancer_CargoGrid_Mid", 2.5, 1.25, 3.75, 6, new Vec3(2.5, 1.25, 1.25)),
        Grid("MISC_Freelancer_CargoGrid_Mid", 2.5, 1.25, 3.75, 6, new Vec3(2.5, 1.25, 1.25)),
    ];

    private static readonly IReadOnlyList<CargoGrid> Cutlass =
    [
        Grid("DRAK_Cutlass_CargoGrid_Main", 5, 6.25, 2.5, 40, new Vec3(2.5, 1.25, 1.25)),
        Grid("DRAK_Cutlass_CargoGrid_Rear", 1.25, 3.75, 2.5, 6, Cell),
    ];

    private static CargoLoad Load(params (int Scu, int Count)[] crates) =>
        new(crates.ToDictionary(c => c.Scu, c => c.Count));

    [Fact]
    public void Four_32s_and_two_16s_fit_the_hermes_and_the_drawing_says_where()
    {
        var fit = CargoFit.Pack(Hermes, Load((32, 4), (16, 2)));

        Assert.True(fit.Fits);
        Assert.Equal(288, fit.CapacityScu);
        Assert.Equal(160, fit.LoadScu);
        Assert.Equal(160, fit.PlacedScu);
        Assert.Empty(fit.Left);
        Assert.Empty(fit.Reasons);

        // Each hold is 4 across, 18 along, 2 up; the 32s lie along the first,
        // two lanes of two, and a 16 lies across the 2 cells of length that
        // leaves - the crate turned, not the hold - with the other in the
        // second hold.
        Assert.Equal((4, 18, 2), fit.Grids[0].Cells);
        var thirtyTwos = fit.Grids[0].Placed.Where(p => p.Scu == 32).ToList();
        Assert.Equal(4, thirtyTwos.Count);
        Assert.All(thirtyTwos, p => Assert.Equal((2, 8, 2), (p.DX, p.DY, p.DZ)));
        var across = Assert.Single(fit.Grids[0].Placed.Where(p => p.Scu == 16));
        Assert.Equal((4, 2, 2), (across.DX, across.DY, across.DZ));
        Assert.Equal(16, across.Y);
        Assert.Single(fit.Grids[1].Placed.Where(p => p.Scu == 16));
        Assert.True(fit.Grids[0].RuleIgnored);
    }

    [Fact]
    public void The_corsair_refuses_a_32_by_the_grids_own_rule_and_says_so()
    {
        var fit = CargoFit.Pack(Corsair, Load((32, 1)));

        Assert.False(fit.Fits);
        Assert.Equal(1, fit.Left[32]);
        Assert.Contains(fit.Reasons, r => r.Contains("32 SCU crate is bigger than any box") && r.Contains("5 × 7.5 × 2.5 m"));
        Assert.False(fit.Grids[0].RuleIgnored);

        // Two 24s and two 8s do: two lanes of a 6-long crate, an 8 behind each.
        var ok = CargoFit.Pack(Corsair, Load((24, 2), (8, 2)));
        Assert.True(ok.Fits);
        Assert.Equal(64, ok.PlacedScu);
    }

    [Fact]
    public void The_freelancer_takes_one_32_and_not_two_because_the_rear_is_one_lane_wide()
    {
        Assert.True(CargoFit.Pack(Freelancer, Load((32, 1))).Fits);

        var two = CargoFit.Pack(Freelancer, Load((32, 2)));
        Assert.False(two.Fits);
        Assert.Equal(1, two.Left[32]);
        // Room, not a rule: 64 of 66 SCU would fit by volume.
        Assert.Empty(two.Reasons);
        Assert.Equal(66, two.CapacityScu);
    }

    [Fact]
    public void Small_crates_stack_on_big_ones_but_never_float()
    {
        // The rear is three cells high: a 32 lies two high along it, a layer of
        // 4s (2 × 2 × 1) goes on top of it - four along the 8 cells it covers -
        // and a fifth has nowhere. The one cell of length past the 32 would
        // take it stood on its side, and the install says a crate keeps its
        // top up; on top it would hang past the crate under it.
        var fit = CargoFit.Pack([Freelancer[0]], Load((32, 1), (4, 5)));

        Assert.False(fit.Fits);
        Assert.Equal(1, fit.Left[4]);
        var fours = fit.Grids[0].Placed.Where(p => p.Scu == 4).ToList();
        Assert.Equal(4, fours.Count);
        Assert.All(fours, p => Assert.Equal(2, p.Z));
        Assert.All(fours, p => Assert.Equal(1, p.DZ));

        // Told it may lie on its side, the packer would stand the fifth there.
        var loose = CargoFit.StandardCrates.Select(c => c with { UprightOnly = false }).ToList();
        Assert.True(CargoFit.Pack([Freelancer[0]], Load((32, 1), (4, 5)), loose).Fits);
    }

    [Fact]
    public void The_cutlass_main_takes_2s_only_and_the_rear_1s_only()
    {
        var fit = CargoFit.Pack(Cutlass, Load((4, 1), (2, 3), (1, 2)));

        Assert.False(fit.Fits);
        Assert.Equal(1, fit.Left[4]);
        Assert.Contains(fit.Reasons, r => r.StartsWith("A 4 SCU crate is bigger"));
        // The 2s went to the main, the 1s to the rear - the largest grid first.
        Assert.Equal(3, fit.Grids[0].Placed.Count(p => p.Scu == 2));
        Assert.Equal(2, fit.Grids.SelectMany(g => g.Placed).Count(p => p.Scu == 1));
    }

    [Fact]
    public void An_unknown_size_and_an_empty_load_are_said_rather_than_fitted()
    {
        var odd = CargoFit.Pack(Corsair, Load((12, 1)));
        Assert.False(odd.Fits);
        Assert.Contains(odd.Reasons, r => r.StartsWith("No crate of 12 SCU"));

        var nothing = CargoFit.Pack(Corsair, Load());
        Assert.False(nothing.Fits);
        Assert.Equal(0, nothing.LoadScu);
    }

    [Fact]
    public void The_crates_are_the_boxes_the_install_gives_them()
    {
        // The 32 is one lane wide and eight cells long, which is what decides
        // whether a grid takes it.
        var c32 = CargoFit.StandardCrates.Single(c => c.Scu == 32);
        Assert.Equal((2.5, 10, 2.5), (c32.X, c32.Y, c32.Z));
        Assert.Equal(32, Math.Round(c32.X * c32.Y * c32.Z / Math.Pow(CargoFit.CellMetres, 3)));
        Assert.All(CargoFit.StandardCrates, c => Assert.Equal(c.Scu, Math.Round(c.X * c.Y * c.Z / Math.Pow(CargoFit.CellMetres, 3))));
    }
}
