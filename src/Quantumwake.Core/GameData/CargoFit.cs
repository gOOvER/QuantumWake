namespace Quantumwake.Core.GameData;

/// <summary>
/// One cargo grid as the dump places it on a hull: its inside in metres,
/// and the smallest and largest box it accepts, per axis.
/// </summary>
/// <param name="Class">The grid's record - <c>DRAK_Corsair_CargoGrid</c>; a hull can place one class twice.</param>
/// <param name="X">Width, metres; the game's cargo unit is 1.25 m, so 5 is four boxes across.</param>
/// <param name="Y">Length.</param>
/// <param name="Z">Height.</param>
/// <param name="Scu">What the dump says the grid holds; the metres agree with it on every hull checked.</param>
/// <param name="MaxBox">The largest box the grid takes, per axis - the Corsair's 5×7.5×2.5 takes a 24 and not a 32.</param>
/// <param name="External">A grid on the outside of the hull, like the Hull C's spindle.</param>
public sealed record CargoGrid(
    string Class,
    string? Uuid,
    double X,
    double Y,
    double Z,
    double Scu,
    Vec3 MinBox,
    Vec3 MaxBox,
    bool External);

/// <summary>A standard cargo crate as the install describes it: the size on the label and the box it actually is.</summary>
/// <param name="Scu">1, 2, 4, 8, 16, 24 or 32.</param>
/// <param name="X">Metres. The 32 is 2.5 × 10 × 2.5 - a long box, not a square one.</param>
/// <param name="Mass">Kilograms, full of the commodity the record was read from.</param>
/// <param name="UprightOnly">Only its top may face up - <c>CargoGridOccupantProperties</c> on every crate read - so it turns on the spot and never stands on its side.</param>
public sealed record CargoCrate(int Scu, double X, double Y, double Z, double Mass, bool UprightOnly = true);

/// <summary>How many of each crate a load is.</summary>
public sealed record CargoLoad(IReadOnlyDictionary<int, int> Crates)
{
    public int TotalScu => Crates.Sum(c => c.Key * c.Value);
    public int Count => Crates.Sum(c => c.Value);
    public bool IsEmpty => Count == 0;
}

/// <summary>Where one crate went: its corner in cells and its extent in cells, in the grid's own frame.</summary>
public sealed record CratePlacement(int Scu, int X, int Y, int Z, int DX, int DY, int DZ);

/// <summary>One grid after packing: what went in and how much room is left.</summary>
/// <param name="Cells">The grid in cells - width, length, height.</param>
/// <param name="UsedScu">Cells filled, which is SCU.</param>
/// <param name="RuleIgnored">The dump gave this grid a largest box of one cell - a placeholder on a hold of 16 SCU or more - and the packer went by the geometry instead.</param>
public sealed record GridFit(CargoGrid Grid, (int W, int L, int H) Cells, IReadOnlyList<CratePlacement> Placed, int UsedScu, bool RuleIgnored);

/// <summary>The answer: whether every crate found a place, and where.</summary>
/// <param name="Fits">Every crate placed.</param>
/// <param name="Left">Crates that found no place, by size.</param>
/// <param name="Reasons">Why a crate could not go anywhere, when the reason is a rule rather than room.</param>
public sealed record CargoFitResult(
    bool Fits,
    IReadOnlyList<GridFit> Grids,
    IReadOnlyDictionary<int, int> Left,
    IReadOnlyList<string> Reasons,
    int CapacityScu,
    int LoadScu)
{
    public int PlacedScu => Grids.Sum(g => g.UsedScu);
}

/// <summary>
/// Does a load of crates fit a hull's grids? A first-fit packer on the
/// game's 1.25 m lattice, crates largest first, each grid's own
/// largest-box rule respected.
/// </summary>
/// <remarks>
/// <para>
/// The rules are the dump's and the install's: a grid is so many cells
/// across, along and up (its metres over 1.25, which divides every grid
/// checked exactly), it takes a box no larger per axis than its
/// <c>MaxSize</c>, and a crate is the box the install gives it. The packing
/// itself is this project's: every crate is tried in every axis-aligned
/// orientation the grid's rule allows, at the lowest, then nearest, then
/// leftmost free corner, and it must stand on the floor or on other crates
/// - every cell under it filled. Largest crates go first, into the grid
/// with the most room first.
/// </para>
/// <para>
/// So "fits" is a packing found, and is real; "does not fit" is this
/// packer finding none, which for a load whose volume is within the grid
/// is not proof. The page says which of the two it is. First-fit-decreasing
/// on these shapes - every crate is two cells wide or one, and stacks in
/// twos - finds the packing whenever the crates tile the grid, which is
/// the case a hauler asks about; the failure it can produce is a load of
/// odd-length crates that would need interleaving, and it says so.
/// </para>
/// </remarks>
public static class CargoFit
{
    /// <summary>The game's cargo unit: a 1 SCU crate is 1.25 m on a side.</summary>
    public const double CellMetres = 1.25;

    /// <summary>
    /// The crates as read from this install on 2026-09-16, for a caller with
    /// no install to read - the tests, and an app whose game data is not in
    /// yet. The page says which it used.
    /// </summary>
    public static readonly IReadOnlyList<CargoCrate> StandardCrates =
    [
        new(1, 1.25, 1.25, 1.25, 938),
        new(2, 1.25, 2.5, 1.25, 1875),
        new(4, 2.5, 2.5, 1.25, 3750),
        new(8, 2.5, 2.5, 2.5, 7500),
        new(16, 2.5, 5, 2.5, 15000),
        new(24, 2.5, 7.5, 2.5, 26250),
        new(32, 2.5, 10, 2.5, 30000),
    ];

    private static int Cells(double metres) => (int)Math.Round(metres / CellMetres);

    /// <summary>Whether a grid's metres sit on the lattice - every one in the dump does, but a later dump is not promised to.</summary>
    public static bool OnLattice(CargoGrid grid) =>
        Math.Abs(grid.X / CellMetres - Math.Round(grid.X / CellMetres)) < 0.01
        && Math.Abs(grid.Y / CellMetres - Math.Round(grid.Y / CellMetres)) < 0.01
        && Math.Abs(grid.Z / CellMetres - Math.Round(grid.Z / CellMetres)) < 0.01;

    public static CargoFitResult Pack(IReadOnlyList<CargoGrid> grids, CargoLoad load, IReadOnlyList<CargoCrate>? crates = null)
    {
        crates ??= StandardCrates;
        var bySize = crates.ToDictionary(c => c.Scu);

        var bins = grids.Where(OnLattice).Select(g => new Bin(g)).OrderByDescending(b => b.Capacity).ToList();
        var capacity = bins.Sum(b => b.Capacity);

        // Largest first, and within a size in one run, so a grid fills from
        // its big pieces down and the small ones take what is left.
        var queue = load.Crates.Where(c => c.Value > 0).OrderByDescending(c => c.Key)
            .SelectMany(c => Enumerable.Repeat(c.Key, c.Value)).ToList();

        var left = new Dictionary<int, int>();
        var reasons = new List<string>();
        var unknown = new HashSet<int>();
        var tooBig = new HashSet<int>();

        foreach (var size in queue)
        {
            if (!bySize.TryGetValue(size, out var crate))
            {
                left[size] = left.GetValueOrDefault(size) + 1;
                unknown.Add(size);
                continue;
            }

            var dims = (Cells(crate.X), Cells(crate.Y), Cells(crate.Z));
            var placed = false;
            var allowedSomewhere = false;
            foreach (var bin in bins)
            {
                var orientations = bin.Orientations(dims, crate.UprightOnly);
                if (orientations.Count == 0) continue;
                allowedSomewhere = true;
                if (bin.Place(size, orientations)) { placed = true; break; }
            }

            if (!placed)
            {
                left[size] = left.GetValueOrDefault(size) + 1;
                if (!allowedSomewhere && bins.Count > 0) tooBig.Add(size);
            }
        }

        foreach (var size in unknown.OrderBy(s => s))
            reasons.Add($"No crate of {size} SCU is known; the install describes 1, 2, 4, 8, 16, 24 and 32.");
        foreach (var size in tooBig.OrderBy(s => s))
        {
            reasons.Add($"A {size} SCU crate is bigger than any box this hull's grids accept - the largest they take is {Describe(bins.MaxBy(b => b.Grid.MaxBox.X * b.Grid.MaxBox.Y * b.Grid.MaxBox.Z)!.Grid.MaxBox)}.");
        }
        if (bins.Count == 0 && grids.Count > 0)
            reasons.Add("The grids' metres do not sit on the 1.25 m lattice, so nothing was packed.");

        var fits = left.Count == 0 && !load.IsEmpty;
        return new CargoFitResult(fits, bins.Select(b => b.Result()).ToList(), left, reasons, capacity, load.TotalScu);
    }

    private static string Describe(Vec3 box) => $"{box.X:0.##} × {box.Y:0.##} × {box.Z:0.##} m";

    /// <summary>One grid's cells, being filled.</summary>
    private sealed class Bin
    {
        public CargoGrid Grid { get; }
        private readonly int _w, _l, _h;
        private readonly bool[,,] _taken;
        private readonly List<CratePlacement> _placed = [];
        private readonly (int X, int Y, int Z) _max;

        public Bin(CargoGrid grid)
        {
            Grid = grid;
            _w = Cells(grid.X);
            _l = Cells(grid.Y);
            _h = Cells(grid.Z);
            _taken = new bool[_w, _l, _h];
            // The dump's largest-box rule, except where it is plainly unfilled:
            // the Ironclad's 720 SCU hold and the Hermes' 144 read one cell,
            // as the Cutlass's 6 SCU side grids do - and those really take
            // only 1 SCU crates. Sixteen SCU is the line: below it the rule is
            // believed, at or above it a one-cell rule is treated as unstated
            // and the row says so.
            var mx = Cells(grid.MaxBox.X);
            var my = Cells(grid.MaxBox.Y);
            var mz = Cells(grid.MaxBox.Z);
            var unstated = mx <= 0 || my <= 0 || mz <= 0;
            RuleIgnored = !unstated && mx == 1 && my == 1 && mz == 1 && Capacity >= 16;
            _max = unstated || RuleIgnored ? (_w, _l, _h) : (mx, my, mz);
        }

        public int Capacity => _w * _l * _h;
        public bool RuleIgnored { get; }

        /// <summary>
        /// The axis-aligned orientations of a crate this grid's rule allows,
        /// the one lying along the grid's long axis first - that is how a
        /// hauler stacks them, and it is the arrangement that tiles.
        /// </summary>
        public List<(int DX, int DY, int DZ)> Orientations((int A, int B, int C) dims, bool uprightOnly)
        {
            var (a, b, c) = dims;
            var all = uprightOnly ? new[] { (a, b, c), (b, a, c) } : new[] { (a, b, c), (b, a, c), (a, c, b), (c, a, b), (b, c, a), (c, b, a) };
            var seen = new HashSet<(int, int, int)>();
            var allowed = new List<(int, int, int)>();
            foreach (var o in all)
            {
                if (!seen.Add(o)) continue;
                if (o.Item1 <= _max.X && o.Item2 <= _max.Y && o.Item3 <= _max.Z && o.Item1 <= _w && o.Item2 <= _l && o.Item3 <= _h)
                    allowed.Add(o);
            }
            // Longest side along the grid's longest side, lowest profile first.
            var longAxisIsY = _l >= _w;
            return allowed
                .OrderBy(o => o.Item3)
                .ThenByDescending(o => longAxisIsY ? o.Item2 : o.Item1)
                .ToList();
        }

        public bool Place(int scu, List<(int DX, int DY, int DZ)> orientations)
        {
            for (var z = 0; z < _h; z++)
                for (var y = 0; y < _l; y++)
                    for (var x = 0; x < _w; x++)
                        foreach (var (dx, dy, dz) in orientations)
                        {
                            if (x + dx > _w || y + dy > _l || z + dz > _h) continue;
                            if (!Free(x, y, z, dx, dy, dz) || !Supported(x, y, z, dx, dy)) continue;
                            Fill(x, y, z, dx, dy, dz);
                            _placed.Add(new CratePlacement(scu, x, y, z, dx, dy, dz));
                            return true;
                        }
            return false;
        }

        private bool Free(int x, int y, int z, int dx, int dy, int dz)
        {
            for (var i = x; i < x + dx; i++)
                for (var j = y; j < y + dy; j++)
                    for (var k = z; k < z + dz; k++)
                        if (_taken[i, j, k]) return false;
            return true;
        }

        /// <summary>On the floor, or on crates under every cell of its footprint.</summary>
        private bool Supported(int x, int y, int z, int dx, int dy)
        {
            if (z == 0) return true;
            for (var i = x; i < x + dx; i++)
                for (var j = y; j < y + dy; j++)
                    if (!_taken[i, j, z - 1]) return false;
            return true;
        }

        private void Fill(int x, int y, int z, int dx, int dy, int dz)
        {
            for (var i = x; i < x + dx; i++)
                for (var j = y; j < y + dy; j++)
                    for (var k = z; k < z + dz; k++)
                        _taken[i, j, k] = true;
        }

        public GridFit Result() => new(Grid, (_w, _l, _h), _placed, _placed.Sum(p => p.DX * p.DY * p.DZ), RuleIgnored);
    }
}
