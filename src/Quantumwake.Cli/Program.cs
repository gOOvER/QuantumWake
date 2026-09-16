using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Quantumwake.Core.Events;
using Quantumwake.Core.Logging;
using Quantumwake.Core.Parsing;
using Quantumwake.Core.GameData;
using Quantumwake.Core.State;
using Quantumwake.Data;

// Quantumwake CLI - backfill and verification harness.
//
// Phase 1 has no UI by design: the parser must be proven against real logs
// before anything is built on top of it. This tool ingests an install's log
// backups and reports what it found, so the numbers can be checked against the
// ground truth recorded in docs/findings.md.

// The garage check needs no install at all: it reads the two community dump
// files and says whether the sheet reproduces their own totals, ship by ship.
if (GetOption(args, "--garage-check") is { } dumpDir)
    return GarageCheck(dumpDir);

var pathArg = GetOption(args, "--path");
var install = pathArg is not null
    ? GameInstallLocator.FromPath(pathArg)
    : GameInstallLocator.Preferred();

if (install is null)
{
    Console.Error.WriteLine("No Star Citizen install found. Pass --path <StarCitizen\\LIVE>.");
    return 1;
}

// The table behind docs/garage.md's maker-logo claim: every non-paint
// SCItemManufacturer record's Code and Logo, whether the archive holds the
// texture, and how a split mip chain is cut. Run it again after a patch.
if (args.Contains("--maker-logos"))
{
    var p4k = new P4kArchive(P4kArchive.PathFor(install.RootPath));
    var blob = p4k.TryRead(@"Data\Game2.dcb");
    if (blob is null) { Console.Error.WriteLine("no Game2.dcb"); return 1; }
    var core = new DataCore(blob);
    var folder = p4k.List(@"Data\UI\SharedAssets\ManufacturerLogos\");
    Console.WriteLine($"{folder.Count} entries under ManufacturerLogos");
    var byName = folder.ToDictionary(e => System.IO.Path.GetFileName(e.Path), e => e.Size, StringComparer.OrdinalIgnoreCase);
    var n = 0;
    foreach (var record in core.Records())
    {
        if (!record.Name.StartsWith("SCItemManufacturer.", StringComparison.OrdinalIgnoreCase) || record.Name.Contains("Paint_", StringComparison.OrdinalIgnoreCase)) continue;
        var at = core.InstanceAt(record, record.VariantIndex);
        var code = core.StringAt(at, record.StructIndex, "Code");
        var logo = core.StringAt(at, record.StructIndex, "Logo");
        var file = logo is { Length: > 0 } ? System.IO.Path.GetFileNameWithoutExtension(logo) + ".dds" : null;
        var has = file is not null && byName.TryGetValue(file, out var size)
            ? $"archive {size}" + (size < 1024 ? " parts " + string.Join(",", folder.Where(e => e.Path.Contains(file + ".", StringComparison.OrdinalIgnoreCase)).Select(e => System.IO.Path.GetExtension(e.Path) + "=" + e.Size)) : "")
            : "archive -";
        Console.WriteLine($"{code,-6} {record.Name,-40} {logo ?? "-",-70} {has}");
        n++;
    }
    Console.WriteLine($"{n} makers");
    return 0;
}


// Step 2 of docs/screen-insight.md. Takes the lines an OCR engine returned -
// a text file, one per line - and says what the catalogue thinks they are.
//
// Text in and not an image, because reading a frame and naming what was read
// are separate problems and only the first needs Windows. It is also what
// lets a bad match be reproduced by editing a file.
if (GetOption(args, "--screen") is { } screenFile)
{
    return Screen(screenFile, install.RootPath, GetOption(args, "--catalogue"));
}

// The mining model as the install has it: the constants, every mineral's
// resistance and instability, every laser's power and modifiers, the modules
// and the gadgets. The table behind docs/mining.md; run it again after a
// patch and against the community tools, which read the same files.
if (args.Contains("--mining"))
{
    var cache = Path.Combine(Path.GetDirectoryName(SessionStore.DatabasePathFor(install.RootPath))!, "commodities.json");
    var mining = GameCommodities.Load(install.RootPath, cache).Mining;

    if (mining.Constants is { } k)
    {
        Console.WriteLine($"constants: capacity {k.PowerCapacityPerMass}/kg, decay {k.DecayPerMass}/kg/s, window {k.OptimalWindowSize} (max {k.OptimalWindowMaxSize}, factor {k.OptimalWindowFactor}), "
            + $"resistance curve {k.ResistanceCurveFactor}, thinness curve {k.OptimalWindowThinnessCurveFactor}, fill {k.ControlledBreakingFillRate}/s (danger {k.DangerBreakingFillRate}/s), "
            + $"absorbable below {k.AbsorbableVolumeThreshold}, {k.CentiScuPerVolume} cSCU per volume");
    }
    else Console.WriteLine("constants: none read");

    Console.WriteLine($"\n{mining.Minerals.Count} minerals");
    foreach (var m in mining.Minerals)
        Console.WriteLine($"  {m.Name,-24} {m.Method,-6} resistance {m.Resistance,6:0.00}  instability {m.Instability,6:0}  window {m.WindowMidpoint:0.00}±{m.WindowRandomness:0.00} thin {m.WindowThinness,5:0.0}  explosion ×{m.ExplosionMultiplier,-5:0}  cluster {m.ClusterFactor:0.00}  [{m.Class}]");

    static string Mods(MiningModifiers x) => string.Join(" ", new[]
    {
        x.Resistance != 0 ? $"res {x.Resistance:+0.#;-0.#}%" : null,
        x.Instability != 0 ? $"inst {x.Instability:+0.#;-0.#}%" : null,
        x.WindowSize != 0 ? $"window {x.WindowSize:+0.#;-0.#}%" : null,
        x.WindowRate != 0 ? $"rate {x.WindowRate:+0.#;-0.#}%" : null,
        x.CatastrophicRate != 0 ? $"overcharge {x.CatastrophicRate:+0.#;-0.#}%" : null,
        x.ShatterDamage != 0 ? $"shatter {x.ShatterDamage:+0.#;-0.#}%" : null,
        x.ClusterFactor != 0 ? $"cluster {x.ClusterFactor:+0.#;-0.#}%" : null,
    }.Where(s => s is not null));

    Console.WriteLine($"\n{mining.Lasers.Count} lasers");
    foreach (var l in mining.Lasers)
        Console.WriteLine($"  S{l.Size} {l.Name,-28} power {l.Power,6:0}  extraction {l.ExtractionPower,5:0}  slots {l.Slots}  filter {l.FilterModifier,3:0}%  throttle min {l.ThrottleMinimum:0.00}  {Mods(l.Modifiers)}  [{l.Class}]");

    Console.WriteLine($"\n{mining.Modules.Count} modules");
    foreach (var m in mining.Modules)
        Console.WriteLine($"  {(m.Active ? "active " : "passive")} {m.Name,-22} power ×{m.PowerMultiplier:0.00}  extraction ×{m.ExtractionMultiplier:0.00}  filter {m.FilterModifier,4:0.#}%  {(m.Active ? $"{m.Lifetime:0}s × {m.Charges}  " : "")}{Mods(m.Modifiers)}  [{m.Class}]");

    Console.WriteLine($"\n{mining.Gadgets.Count} gadgets");
    foreach (var g in mining.Gadgets)
        Console.WriteLine($"  {g.Name,-12} {Mods(g.Modifiers)}  [{g.Class}]");

    Console.WriteLine($"\n{mining.Compositions.Count} compositions");
    foreach (var c in mining.Compositions.OrderBy(c => c.Class, StringComparer.OrdinalIgnoreCase))
        Console.WriteLine($"  {c.Class,-36} {c.Name,-24} min {c.MinimumDistinctElements} distinct: {string.Join(", ", c.Parts.Select(p => $"{p.Element} {p.MinPercent:0}-{p.MaxPercent:0}% p{p.Probability:0.00}"))}");

    return 0;
}

// The personal armoury as the install has it: every gun with its damage,
// rate, magazine and fire modes, every piece of armour with its resistances
// and the figures that actually differ between pieces. The table behind
// docs/armoury.md; run it again after a patch and against the community
// tables, which read the same files. --armoury=all lists the finishes too.
// Cargo fit: the crates as the install sizes them, every hull's grids as the
// community digest places them, the check that the grids sum to the dump's
// own capacity, and the question that started it. The table behind
// docs/cargo-fit.md; run it again after a patch or a dataset refresh.
if (args.Contains("--cargo", StringComparer.OrdinalIgnoreCase))
{
    var cache = Path.Combine(Path.GetDirectoryName(SessionStore.DatabasePathFor(install.RootPath))!, "commodities.json");
    var crates = GameCommodities.Load(install.RootPath, cache).Crates;
    Console.WriteLine($"{crates.Count} crate sizes read from the install" + (crates.Count == 0 ? " - the built-in table stands in" : ""));
    foreach (var c in crates.Count > 0 ? crates : CargoFit.StandardCrates)
        Console.WriteLine($"  {c.Scu,3} SCU  {c.X:0.##} x {c.Y:0.##} x {c.Z:0.##} m  ({c.X / CargoFit.CellMetres:0} x {c.Y / CargoFit.CellMetres:0} x {c.Z / CargoFit.CellMetres:0} cells)  {c.Mass:N0} kg full");

    var community = new CommunityData();
    if (!community.HasCargoGrids) { Console.WriteLine("\nThe community digest has no cargo grids - refresh the reference data first."); return 1; }
    var withGrids = community.GarageShips.Values.Where(s => s.CargoGrids is { Count: > 0 }).ToList();
    var agree = withGrids.Count(s => Math.Abs(s.CargoGrids!.Sum(g => g.Scu) - s.CargoScu) < 0.01);
    var offLattice = withGrids.SelectMany(s => s.CargoGrids!).Count(g => !CargoFit.OnLattice(g));
    Console.WriteLine($"\n{withGrids.Count} hulls with grids of {community.GarageShips.Count}; the grids sum to the dump's capacity on {agree}; {offLattice} grids off the 1.25 m lattice");
    foreach (var s in withGrids.Where(s => Math.Abs(s.CargoGrids!.Sum(g => g.Scu) - s.CargoScu) >= 0.01))
        Console.WriteLine($"  ! {s.Class}: grids {s.CargoGrids!.Sum(g => g.Scu)} vs {s.CargoScu}");
    foreach (var s in withGrids.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        Console.WriteLine($"  {s.Name,-34} {s.CargoScu,6:0} SCU  " + string.Join("; ", s.CargoGrids!.GroupBy(g => g.Class).Select(g => $"{g.Count()}x {g.Key} {g.First().X:0.##}x{g.First().Y:0.##}x{g.First().Z:0.##} m, up to {g.First().MaxBox.X:0.##}x{g.First().MaxBox.Y:0.##}x{g.First().MaxBox.Z:0.##}")));

    if (community.GarageShip("RSI_Hermes") is { CargoGrids: { Count: > 0 } } hermes)
    {
        var fit = CargoFit.Pack(hermes.CargoGrids, new CargoLoad(new Dictionary<int, int> { [32] = 4, [16] = 2 }), crates.Count > 0 ? crates : null);
        Console.WriteLine($"\n4 x 32 + 2 x 16 in the Hermes: {(fit.Fits ? "fits" : "no packing found")}, {fit.PlacedScu} of {fit.LoadScu} SCU placed in {fit.CapacityScu}");
        foreach (var g in fit.Grids) Console.WriteLine($"  {g.Grid.Class} {g.Cells.W}x{g.Cells.L}x{g.Cells.H}: " + string.Join(", ", g.Placed.Select(p => $"{p.Scu}@({p.X},{p.Y},{p.Z}) {p.DX}x{p.DY}x{p.DZ}")) + (g.RuleIgnored ? "  [one-cell rule ignored]" : ""));
    }
    return 0;
}

if (args.Any(a => a.StartsWith("--armoury", StringComparison.OrdinalIgnoreCase)))
{
    var all = args.Contains("--armoury=all", StringComparer.OrdinalIgnoreCase);
    var cache = Path.Combine(Path.GetDirectoryName(SessionStore.DatabasePathFor(install.RootPath))!, "commodities.json");
    var armoury = GameCommodities.Load(install.RootPath, cache).Armoury;

    static string Kinds(DamageKinds d) => string.Join("+", new[]
    {
        d.Physical > 0 ? $"{d.Physical:0.##} phys" : null, d.Energy > 0 ? $"{d.Energy:0.##} energy" : null,
        d.Distortion > 0 ? $"{d.Distortion:0.##} dist" : null, d.Thermal > 0 ? $"{d.Thermal:0.##} therm" : null,
        d.Biochemical > 0 ? $"{d.Biochemical:0.##} bio" : null, d.Stun > 0 ? $"{d.Stun:0.##} stun" : null,
    }.Where(s => s is not null));

    static string Mode(FireMode m) => m.Kind switch
    {
        "Beam" => $"{m.Name} {Kinds(m.BeamDamagePerSecond ?? DamageKinds.None)}/s to {m.BeamFullRange:0}m (none past {m.BeamZeroRange:0}m) {m.BeamAmmoPerSecond:0.#} rounds/s",
        "Charge" => $"{m.Name} {m.RoundsPerMinute:0} rpm, ×{m.ChargeDamageMultiplier:0.##} after {m.ChargeSeconds:0.##}s for ×{m.ChargeAmmoMultiplier:0.##} rounds",
        "Burst" => $"{m.Name} {m.BurstShots}×{m.Pellets} at {m.RoundsPerMinute:0} rpm, {m.BurstCooldown:0.##}s between (sustained {Armoury.SustainedRoundsPerMinute(m):0})",
        _ => $"{m.Name} {m.RoundsPerMinute:0} rpm" + (m.Pellets > 1 ? $" ×{m.Pellets} pellets" : "") + (m.AmmoPerShot != 1 ? $" {m.AmmoPerShot} rounds/shot" : ""),
    } + (m.Condition.Length > 0 ? $" when {m.Condition}" : "");

    var guns = armoury.Weapons.Where(w => all || w.BaseClass is null).ToList();
    Console.WriteLine($"{armoury.Weapons.Count} personal weapons, {armoury.Weapons.Count(w => w.BaseClass is null)} of them plain and the rest finishes of one");
    foreach (var w in guns)
    {
        var finishes = armoury.Weapons.Count(o => o.BaseClass == w.Class);
        Console.WriteLine($"  {w.Name,-38} {w.Kind,-16} {w.Weight,-6} S{w.Size} {w.Mass,5:0.##}kg  hit {Kinds(w.Damage),-22} {w.ProjectileSpeed,5:0} m/s for {w.ProjectileLifetime:0.#}s"
            + (w.DropStart > 0 ? $"  drops {w.DropPerMetre:0.###}/m past {w.DropStart:0}m to {w.DropFloor:0.##}" : "")
            + $"  mag {w.Magazine}"
            + (w.Explosion is { } x ? $"  blast {Kinds(x.Damage)} over {x.Radius:0.#}-{x.OuterRadius:0.#}m" : "")
            + (finishes > 0 ? $"  +{finishes} finishes" : "") + (w.BaseClass is not null ? $"  finish of {w.BaseClass}" : "")
            + $"  [{w.Class}] {w.Manufacturer}");
        foreach (var m in w.Modes)
            Console.WriteLine($"      {Mode(m),-80} {Armoury.DamagePerSecond(w, m),7:0} dps  {Armoury.DamagePerMagazine(w, m),7:0}/mag  empties in {Armoury.SecondsToEmpty(w, m),5:0.#}s");
    }

    Console.WriteLine($"\n{armoury.Armour.Count} pieces of armour");
    foreach (var group in armoury.Armour.GroupBy(a => a.Resistances?.Macro ?? "(none)").OrderBy(g => g.Key))
    {
        var r = group.First().Resistances;
        Console.WriteLine(r is null
            ? $"  {group.Key}: {group.Count()} pieces, no resistance block"
            : $"  {group.Key}: {group.Count()} pieces at ×{r.Physical:0.###} phys ×{r.Energy:0.###} energy ×{r.Distortion:0.###} dist ×{r.Thermal:0.###} therm ×{r.Biochemical:0.###} bio ×{r.Stun:0.###} stun, impact ×{r.Impact:0.###}");
    }
    Console.WriteLine();
    foreach (var slot in armoury.Armour.GroupBy(a => a.Slot))
    {
        Console.WriteLine($"  {slot.Key}: {slot.Count()} pieces in {slot.Select(a => a.Family).Distinct(StringComparer.OrdinalIgnoreCase).Count()} sets");
        foreach (var family in slot.GroupBy(a => (a.Family, a.Weight, a.Resistances?.Macro, a.TemperatureMin, a.TemperatureMax, a.RadiationCapacity, a.CapacityMicroScu, a.EmSignature, a.IrSignature, a.Mass)).OrderBy(g => g.Key.Weight).ThenBy(g => g.Key.Family))
        {
            var a = family.First();
            Console.WriteLine($"    {a.Family,-34} {a.Weight,-6} {family.Count(),3}× {a.Resistances?.Macro ?? "-",-20} {a.TemperatureMin,4:0}..{a.TemperatureMax,-4:0}C rad {a.RadiationCapacity,6:0} -{a.RadiationDissipation:0.#}/s"
                + (a.GForceResistance != 0 ? $" g {a.GForceResistance:+0.##;-0.##}" : "")
                + (a.CapacityMicroScu > 0 ? $" carries {a.CapacityMicroScu / 1_000_000.0:0.###} SCU" : "")
                + $" em {a.EmSignature:0.#} ir {a.IrSignature:0.#} {a.Mass:0.#}kg" + (a.MotionPenalty > 0 ? $" broken -{a.MotionPenalty:P0} move" : "")
                + $"  {a.Manufacturer}" + (all ? "  " + string.Join(" | ", family.Select(p => p.Name)) : ""));
        }
    }

    return 0;
}

var liveOnly = args.Contains("--live-only");

// Machine-readable mode. Everything the human report would say goes to stderr
// instead, so stdout carries nothing but events and the thing stays pipeable.
var asJson = args.Contains("--events");
var only = GetOption(args, "--kind")?.Split(',', StringSplitOptions.RemoveEmptyEntries
    | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);

var log = asJson ? Console.Error : Console.Out;

var json = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
};

log.WriteLine("Quantumwake CLI  ·  by nekron");
log.WriteLine();
log.WriteLine($"Install : {install.RootPath}");
log.WriteLine($"Channel : {install.Channel}");

var files = new List<string>();
if (!liveOnly)
    files.AddRange(install.BackupLogs());
if (install.HasGameLog)
    files.Add(install.GameLogPath);

if (files.Count == 0)
{
    Console.Error.WriteLine("No log files found.");
    return 1;
}

var totalBytes = files.Sum(f => new FileInfo(f).Length);
log.WriteLine($"Files   : {files.Count} ({totalBytes / 1024.0 / 1024.0:F1} MB)");
log.WriteLine();

var report = new Report();
var parser = new LogEventParser();
var stopwatch = Stopwatch.StartNew();

for (var i = 0; i < files.Count; i++)
{
    var file = files[i];
    log.Write($"\r  parsing {i + 1}/{files.Count} ...");

    // A fresh parser per file: session headers are per-file state, and a
    // truncated final line in one log must not leak into the next.
    var fileParser = new LogEventParser();
    report.BeginFile(Path.GetFileName(file));

    foreach (var ev in LogFileReader.ReadEvents(file, fileParser))
    {
        report.Add(ev);

        // One event per line, serialised as the concrete record so each kind
        // carries its own fields rather than a lowest common denominator.
        if (asJson && (only is null || only.Contains(ev.Kind)))
            Console.Out.WriteLine(JsonSerializer.Serialize(ev, ev.GetType(), json));
    }

    report.Merge(fileParser);
}

stopwatch.Stop();
log.Write("\r".PadRight(40));
log.WriteLine($"\rParsed in {stopwatch.Elapsed.TotalSeconds:F1}s\n");

// In --events mode stdout carries events and nothing else, so the report that
// would otherwise follow them is skipped rather than mixed in.
if (!asJson) report.Print();

return 0;

static string? GetOption(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

/// <summary>Reads the probe's own output, boxes and all.</summary>
/// <remarks>
/// The probe prints each line as <c>[ left top hNN]  the text</c>, and the
/// boxes are what let a tooltip be found by where it sat rather than by the
/// order the engine happened to return it in. A file without them still works
/// and is read as one column, which is what a hand-written fixture is.
/// </remarks>
static List<ScreenTextLine> Placed(IEnumerable<string> raw)
{
    var Box = new Regex(
        @"^\[\s*(?<left>\d+)\s+(?<top>\d+)\s+h\s*(?<height>\d+)\]\s+(?<text>.*)$");

    var placed = new List<ScreenTextLine>();
    var row = 0;

    foreach (var line in raw)
    {
        var boxed = Box.Match(line);

        if (boxed.Success)
        {
            placed.Add(new ScreenTextLine(
                boxed.Groups["text"].Value,
                double.Parse(boxed.Groups["left"].Value, CultureInfo.InvariantCulture),
                double.Parse(boxed.Groups["top"].Value, CultureInfo.InvariantCulture),
                double.Parse(boxed.Groups["height"].Value, CultureInfo.InvariantCulture)));
        }
        else if (line.Trim().Length > 0)
        {
            placed.Add(new ScreenTextLine(line, 0, row * 20, 16));
        }

        row++;
    }

    return placed;
}

/// <summary>Prints what a screenshot's text was matched to, and why.</summary>
/// <remarks>
/// The catalogue is loaded straight from the install rather than through
/// <c>LogLibrary</c>: naming a thing on screen has nothing to do with what is
/// in anybody's logs, and parsing 400 MB to answer it would make the harness
/// too slow to use while iterating on the matcher.
/// </remarks>
/// <summary>
/// The stock-fit comparison from docs/garage.md over the whole dump: computes
/// every spaceship's sheet from its parts and compares with the totals the
/// dump wrote beside them, which the sheet never reads.
/// </summary>
/// <param name="dumpDir">A folder holding ships.json and ship-items.json from scunpacked-data.</param>
static int GarageCheck(string dumpDir)
{
    var shipsPath = Path.Combine(dumpDir, "ships.json");
    var itemsPath = Path.Combine(dumpDir, "ship-items.json");

    if (!File.Exists(shipsPath) || !File.Exists(itemsPath))
    {
        Console.Error.WriteLine($"Expected ships.json and ship-items.json in {dumpDir}.");
        return 2;
    }

    var parts = CommunityData.DigestPartStats(File.ReadAllText(itemsPath));
    var ships = CommunityData.DigestShipStats(File.ReadAllText(shipsPath), parts);

    var counted = ships.Values.Where(s => s.IsSpaceship && s.Dataset.EmShields > 0).ToList();
    var tally = new Dictionary<string, (int Ok, int Tried)>();
    var misses = new List<string>();

    void Check(string figure, string ship, double got, double want, double tolerance)
    {
        if (want <= 0) return;
        var (ok, tried) = tally.GetValueOrDefault(figure);
        var hit = Math.Abs(got - want) / want <= tolerance;
        tally[figure] = (ok + (hit ? 1 : 0), tried + 1);
        if (!hit && misses.Count < 40) misses.Add($"  {figure,-14} {ship,-44} {got,14:0.#} vs {want,14:0.#}");
    }

    foreach (var ship in counted)
    {
        var sheet = ShipSheet.Compute(ship, parts);
        var d = ship.Dataset;

        Check("EM shields", ship.Name, sheet.Shields.Em, d.EmShields, 0.01);
        Check("EM quantum", ship.Name, sheet.Quantum.Em, d.EmQuantum, 0.01);
        Check("IR shields", ship.Name, sheet.Shields.Ir, d.IrShields, 0.01);
        Check("IR quantum", ship.Name, sheet.Quantum.Ir, d.IrQuantum, 0.01);
        Check("power seg", ship.Name, sheet.Power.Available, d.PowerSegments, 0);
        Check("cooling seg", ship.Name, sheet.Cooling.Generated, d.CoolingSegments, 0.01);
        Check("shield hp", ship.Name, sheet.Shield.Hp, d.ShieldHp, 0.01);
        Check("fixed dps", ship.Name, sheet.Weapons.FixedDps, d.FixedDps, 0.02);
        if (sheet.QuantumDrive is { } q) Check("qt range", ship.Name, q.Range, d.QuantumRange, 0.01);
        Check("mass", ship.Name, sheet.Mass, d.MassTotal, 0.02);
    }

    Console.WriteLine($"Garage check: {counted.Count} spaceships with a stock fit, {parts.Count} parts");
    Console.WriteLine();
    foreach (var (figure, (ok, tried)) in tally.OrderBy(t => t.Key))
        Console.WriteLine($"  {figure,-14} {ok,4} of {tried,-4} {(ok == tried ? "" : $"  ({tried - ok} miss)")}");

    if (misses.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Misses (first 40):");
        foreach (var line in misses) Console.WriteLine(line);
    }

    // Fixed-vs-turret is the one split the dump itself does not settle - see
    // ShipSheet.IsTurret - so it is reported but does not fail the check.
    return tally.Where(t => t.Key != "fixed dps").All(t => t.Value.Ok == t.Value.Tried) ? 0 : 1;
}

static int Screen(string linesFile, string installRoot, string? catalogueQuery)
{
    if (!File.Exists(linesFile))
    {
        Console.Error.WriteLine($"No such file: {linesFile}");
        return 1;
    }

    var cache = Path.Combine(
        Path.GetDirectoryName(SessionStore.DatabasePathFor(installRoot))!,
        "commodities.json");

    var game = GameCommodities.Load(installRoot, cache);

    var items = game.ItemFacts
        .Select(kv => new ItemReference(
            kv.Key, kv.Value.Name, kv.Value.Type, kv.Value.SubType,
            kv.Value.Size, kv.Value.Grade,
            kv.Value.Manufacturer is { Length: > 0 } maker ? maker : null,
            null, "install", MicroScu: kv.Value.MicroScu))
        .ToList();

    Console.WriteLine($"Catalogue : {items.Count} items from the install");

    // A plain substring dump, so the catalogue's own wording for a thing can
    // be read next to the tooltip's. The two disagree more than expected.
    if (catalogueQuery is { Length: > 0 })
    {
        Console.WriteLine();
        Console.WriteLine($"Catalogue entries matching \"{catalogueQuery}\":");

        foreach (var item in items
            .Where(i => i.Name?.Contains(catalogueQuery, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Take(20))
        {
            Console.WriteLine($"  {item.Name}");
            Console.WriteLine($"      class    {item.ClassName}");
            Console.WriteLine($"      type     {item.Type} / {item.SubType}");
            Console.WriteLine($"      maker    {item.Manufacturer ?? "(none)"}");
            Console.WriteLine($"      size {item.Size}  grade {item.Grade}  {item.MicroScu} uSCU");
        }
    }

    var lines = Placed(File.ReadAllLines(linesFile));

    // Which screen this is, and what that screen carries, before the tooltip
    // question is asked of it. The install types a ship as NOITEM_Vehicle, so
    // the ship names come from the same catalogue as the parts.
    var shipNames = items
        .Where(i => i.Type?.Contains("Vehicle", StringComparison.OrdinalIgnoreCase) == true)
        .Select(i => i.Name)
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Select(n => n!)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    var commodities = game.All.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    var frame = ScreenFrames.Read(lines, items, shipNames, commodities);

    Console.WriteLine();
    Console.WriteLine($"Screen    : {frame.Kind}");

    if (frame.Wallet is { } wallet)
        Console.WriteLine($"Wallet    : {(wallet.Balance is { } b ? $"{b:N0} aUEC" : wallet.Trouble)}");

    if (frame.Map is { } map)
    {
        Console.WriteLine($"System    : {map.SystemRead ?? "(not read)"}");
        Console.WriteLine($"Place     : {map.PlaceRead ?? "(not read)"}");
        Console.WriteLine($"Position  : {map.Latitude}° {map.Longitude}° {map.Gigametres} Gm");
        Console.WriteLine($"Contracts : {(map.AcceptedContracts switch { false => "none accepted", true => "some accepted", _ => "(not stated)" })}");
    }

    if (frame.Kiosk is { } kiosk)
    {
        Console.WriteLine($"Side      : {(kiosk.Buying == false ? "selling" : kiosk.Buying == true ? "buying" : "(not read)")}");
        Console.WriteLine($"Ship      : {kiosk.Ship ?? kiosk.ShipRead ?? "(not read)"}  cargo {kiosk.CargoUsed}/{kiosk.CargoCapacity} SCU");
        Console.WriteLine($"Balance   : {kiosk.BalanceRead ?? "(not read)"}  {(kiosk.Balance is { } full ? $"({full:N0} aUEC, printed in full)" : "(abbreviated, so no number is taken from it)")}");

        foreach (var row in kiosk.Rows)
        {
            Console.WriteLine($"  {row.Commodity ?? $"\"{row.Read}\""}"
                + $"  stock {row.Quantity?.ToString("N0") ?? "?"} {row.QuantityUnit ?? ""}"
                + $"  price {row.Price?.ToString("N0") ?? "?"} per {row.PriceUnit ?? "?"}"
                + $"  {row.State ?? ""}");
        }
    }

    if (frame.Contracts is { } contracts)
    {
        Console.WriteLine($"Accepted  : {contracts.Accepted} of {contracts.Capacity}");
        foreach (var card in contracts.Cards)
            Console.WriteLine($"  card     {card.Title}  [{card.Reward ?? "?"}]  {card.Issuer ?? "(issuer not read)"}");
        Console.WriteLine($"Selected  : {contracts.SelectedTitle}  reward {contracts.SelectedReward}  by {contracts.SelectedIssuer}");
        foreach (var o in contracts.Objectives) Console.WriteLine($"  objective {o}");
    }

    if (frame.Fleet is { } fleet)
    {
        foreach (var row in fleet.Ships)
            Console.WriteLine($"  ship     {row.Ship ?? "?"}  read \"{row.Read}\"  at {row.Location ?? "?"}  {row.State ?? "?"}  {row.Focus ?? "?"}  cargo {row.Cargo}" + (row.LooksLike.Count > 0 ? $"  looks like {string.Join(", ", row.LooksLike)}" : ""));
    }

    if (frame.Reputation is { } rep)
    {
        Console.WriteLine($"Org       : {rep.Organisation}  standing {rep.Standing}  rank {rep.Rank ?? "(not readable)"}");
        Console.WriteLine($"Orgs      : {string.Join(", ", rep.Organisations)}");
    }

    if (frame.Map is { } m2 && m2.PathRead is not null) Console.WriteLine($"Path      : {m2.PathRead}");

    if (frame.Loadout is { } loadout)
    {
        Console.WriteLine($"Ship      : {loadout.Ship ?? "(not named)"}  read \"{loadout.ShipRead}\""
            + (loadout.LooksLike.Count > 0 ? $"  looks like {string.Join(", ", loadout.LooksLike)}" : ""));
        Console.WriteLine($"Scope     : {loadout.Scope ?? "(none)"}");
        Console.WriteLine($"Ports     : {loadout.Fittings.Count}");

        foreach (var fitting in loadout.Fittings)
        {
            var what = fitting.NothingRead ? "(nothing read under it)"
                : fitting.IsEmpty ? "(empty, so the game says)"
                : fitting.Name is not null ? $"{fitting.Name}  [{fitting.Tier}{(fitting.Agrees.Count > 0 ? ", " + string.Join(", ", fitting.Agrees) + " agree" : "")}{(fitting.Disagrees.Count > 0 ? ", " + string.Join(", ", fitting.Disagrees) + " disagree" : "")}]"
                : $"\"{fitting.Read}\"  [unmatched]";

            Console.WriteLine($"  {fitting.Slot,-32} {what}");
        }
    }

    var result = ScreenInsight.Look(lines, items);

    // A frame with no tooltip on it still has names all over it, and that is
    // the commoner shape by a distance: six of the nine screenshots measured
    // for this were loadout screens. A tooltip whose name matched nothing gets
    // the same treatment rather than a dead end - on the component tooltip
    // measured here the name was not in the reading at all.
    if (result.Reading.Name is null || result.Candidates.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Name read : {result.Reading.Name ?? "(none)"}");

        // Printed even with no name. A tooltip that gave up a manufacturer and
        // a type and no name is not nothing - it is most of an answer, and
        // hiding it would make this look like a frame with no tooltip on it.
        foreach (var (label, value) in result.Reading.Fields.OrderBy(f => f.Key, StringComparer.Ordinal))
            Console.WriteLine($"  {label,-16} {value}");

        if (result.Reading.Name is not null)
        {
            Console.WriteLine();
            Console.WriteLine($"Not certain: {result.Trouble}");
        }

        var swept = ScreenInsight.Sweep(lines, items);

        Console.WriteLine();
        Console.WriteLine($"No tooltip. Swept {lines.Count} lines, {swept.Count} named something:");

        foreach (var line in swept)
        {
            var how = line.Named
                ? "exact"
                : $"{line.Candidates.Count} x {line.Candidates[0].Tier}";

            Console.WriteLine();
            Console.WriteLine($"  \"{line.Text}\"  [{how}]");

            foreach (var candidate in line.Candidates.Take(4))
                Console.WriteLine($"      {candidate.Item.Name}  ({candidate.Item.ClassName})");
        }

        return 0;
    }

    Console.WriteLine();
    Console.WriteLine($"Name read : {result.Reading.Name}");

    foreach (var (label, value) in result.Reading.Fields.OrderBy(f => f.Key, StringComparer.Ordinal))
        Console.WriteLine($"  {label,-16} {value}");

    Console.WriteLine();
    Console.WriteLine(result.Certain
        ? "Certain."
        : $"Not certain: {result.Trouble}");

    foreach (var candidate in result.Candidates.Take(10))
    {
        Console.WriteLine();
        Console.WriteLine($"  {candidate.Item.Name}  [{candidate.Tier}]");
        Console.WriteLine($"      class    {candidate.Item.ClassName}");
        Console.WriteLine($"      type     {candidate.Item.Type} / {candidate.Item.SubType}");
        Console.WriteLine($"      maker    {candidate.Item.Manufacturer ?? "(none)"}");
        Console.WriteLine($"      {candidate.Item.MicroScu} uSCU");
        Console.WriteLine($"      agrees   {(candidate.Agrees.Count == 0 ? "(nothing)" : string.Join(", ", candidate.Agrees))}");
        Console.WriteLine($"      against  {(candidate.Disagrees.Count == 0 ? "(nothing)" : string.Join(", ", candidate.Disagrees))}");
    }

    return 0;
}

/// <summary>Aggregates parsed events into the figures worth eyeballing.</summary>
internal sealed class Report
{
    private readonly Dictionary<string, int> _locations = [];
    private readonly Dictionary<string, int> _ships = [];
    private readonly Dictionary<string, int> _gameRules = [];
    private readonly Dictionary<string, int> _contracts = [];
    private readonly Dictionary<string, int> _quantumDestinations = [];
    private readonly Dictionary<string, int> _partyMoments = [];
    private readonly Dictionary<string, int> _channelMoments = [];
    private readonly Dictionary<string, int> _berths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _partyHandles = [];
    private readonly Dictionary<string, int> _eventKinds = [];
    private readonly HashSet<string> _handles = [];
    private readonly HashSet<string> _geids = [];
    private readonly HashSet<string> _notificationIds = [];
    private readonly HashSet<string> _incapacitationFiles = [];
    private readonly HashSet<string> _sessionIds = [];
    private readonly Dictionary<string, int> _shards = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _shardRegions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (int Count, string Sample)> _unmatchedByTag = [];

    private string _currentFile = "";
    private int _sessionHeaders;
    private int _incapacitations;
    private int _partyNotifications;
    private int _channelNotifications;
    private int _unmatchedKnownTags;
    private int _corpseDeaths;
    private int _shipRetrievals;
    private int _contractCompletions;
    private int _awards;
    private decimal _awarded;
    private DateTimeOffset? _lastCorpseAt;

    public void BeginFile(string fileName) => _currentFile = fileName;

    public void Add(GameEvent ev)
    {
        _eventKinds[ev.Kind] = _eventKinds.GetValueOrDefault(ev.Kind) + 1;

        switch (ev)
        {
            case SessionStartEvent:
                _sessionHeaders++;
                break;

            case LoginEvent login:
                _handles.Add(login.Handle);
                break;

            case CharacterEvent character:
                _geids.Add($"{character.Name} ({character.Geid})");
                break;

            case LoadingScreenEvent loading:
                Bump(_gameRules, loading.GameRules);
                break;

            case ContextEvent context:
                _sessionIds.Add(context.SessionId);
                break;

            case ShardJoinEvent join:
                Bump(_shards, join.Shard);
                Bump(_shardRegions, ShardName.TryParse(join.Shard, out var shard) ? shard.Region : "(unparsed)");
                break;

            case LocationInventoryEvent location:
                Bump(_locations, location.LocationId);
                break;

            // Count each ship once per seat entry/exit pair boundary; ClearDriver
            // is the reliably present half on current logs.
            case VehicleControlEvent vehicle:
                Bump(_ships, vehicle.Manufacturer is null
                    ? vehicle.Model
                    : $"{vehicle.Manufacturer} {vehicle.Model}");
                break;

            case QuantumRouteEvent quantum:
                Bump(_quantumDestinations, quantum.Destination);
                break;

            case ContractEvent contract:
                Bump(_contracts, contract.Contract);
                break;

            // Deduplicate on the notification id: each fires 3-5 times.
            // Corpse item lines arrive in a burst per death, so group by time.
            case CorpseItemEvent corpse:
                if (_lastCorpseAt is not { } last || corpse.Timestamp - last >= TimeSpan.FromSeconds(30))
                    _corpseDeaths++;

                _lastCorpseAt = corpse.Timestamp;
                break;

            case VehicleSpawnEvent:
                _shipRetrievals++;
                break;

            case NotificationEvent notification:
                if (!_notificationIds.Add($"{_currentFile}|{notification.NotificationId}|{notification.Text}"))
                    break;

                if (notification.IsIncapacitation)
                {
                    _incapacitations++;
                    _incapacitationFiles.Add(_currentFile);
                }

                if (notification.IsContractComplete)
                    _contractCompletions++;

                if (notification.Awarded is { } awarded)
                {
                    _awards++;
                    _awarded += awarded;
                }

                // Both sides are counted rather than just the successes: the gap
                // between notifications seen and notes read is the number of
                // lines the reader declined to guess at, and that number should
                // stay small without ever being forced to zero.
                if (ShipChannel.IsChannel(notification.Text))
                {
                    _channelNotifications++;

                    if (ShipChannel.Read(notification.Timestamp, notification.Text) is { } berth)
                    {
                        var moment = berth.Moment.ToString();
                        _channelMoments[moment] = _channelMoments.GetValueOrDefault(moment) + 1;

                        var berthName = $"{berth.Ship} : {berth.Owner}";
                        _berths[berthName] = _berths.GetValueOrDefault(berthName) + 1;
                    }
                }

                if (notification.IsParty)
                {
                    _partyNotifications++;

                    if (Party.Read(notification.Timestamp, notification.Text) is { } note)
                    {
                        var moment = note.Moment.ToString();
                        _partyMoments[moment] = _partyMoments.GetValueOrDefault(moment) + 1;

                        if (note.Handle is not null)
                            _partyHandles[note.Handle] =
                                _partyHandles.GetValueOrDefault(note.Handle) + 1;
                    }
                }

                break;
        }
    }

    public void Merge(LogEventParser parser)
    {
        _unmatchedKnownTags += parser.UnmatchedKnownTags;

        foreach (var (tag, (count, sample)) in parser.UnmatchedByTag)
        {
            if (_unmatchedByTag.TryGetValue(tag, out var existing))
                _unmatchedByTag[tag] = (existing.Count + count, existing.Sample);
            else
                _unmatchedByTag[tag] = (count, sample);
        }
    }

    public void Print()
    {
        Section("Identity");
        Console.WriteLine($"  handles : {Join(_handles)}");
        Console.WriteLine($"  chars   : {Join(_geids)}");

        Section("Sessions");
        Console.WriteLine($"  session headers : {_sessionHeaders}");
        Console.WriteLine($"  client sessions : {_sessionIds.Count}");
        Console.WriteLine($"  shard joins     : {_shards.Values.Sum()} on {_shards.Count} distinct shards");
        foreach (var (region, count) in _shardRegions.OrderByDescending(p => p.Value))
            Console.WriteLine($"  {region,-24} {count,6} joins");
        foreach (var (rules, count) in _gameRules.OrderByDescending(p => p.Value))
            Console.WriteLine($"  {rules,-16} {count,6} loading screens");

        Top("Locations visited", _locations);
        Top("Ships flown", _ships, 10);
        Top("Quantum destinations", _quantumDestinations);
        Top("Contracts", _contracts);

        Section("Contract payouts");
        Console.WriteLine($"  completions   : {_contractCompletions}");
        Console.WriteLine($"  awards stated : {_awards}");
        Console.WriteLine($"  total         : {_awarded:N0} aUEC");

        // Both numbers, always, because the gap between them is the point: the
        // game states a payout for a fraction of what it completes, and a total
        // shown on its own reads as contract income rather than a floor over
        // the few it bothered to price.
        Console.WriteLine("  -> a floor: most completions state no payout at all.");

        Section("Party");
        Console.WriteLine($"  notifications : {_partyNotifications}");
        Console.WriteLine($"  read as notes : {_partyMoments.Values.Sum()}");
        Console.WriteLine($"  players named : {_partyHandles.Count}");

        foreach (var (moment, count) in _partyMoments.OrderByDescending(p => p.Value))
            Console.WriteLine($"    {moment,-14}{count,6}");

        // The unread remainder is queue and matchmaking chatter naming nobody.
        // Printed rather than hidden: if it ever grows, the channel has gained a
        // sentence worth reading.
        var unread = _partyNotifications - _partyMoments.Values.Sum();
        if (unread > 0)
            Console.WriteLine($"  -> {unread} named nobody (join queue, broadcasts), left unread.");

        Top("Flown with", _partyHandles, 10);

        Section("Ship comms");
        Console.WriteLine($"  notifications : {_channelNotifications}");
        Console.WriteLine($"  read as notes : {_channelMoments.Values.Sum()}");
        Console.WriteLine($"  ships named   : {_berths.Count}");

        foreach (var (moment, count) in _channelMoments.OrderByDescending(p => p.Value))
            Console.WriteLine($"    {moment,-14}{count,6}");

        Top("Berths", _berths, 8);

        Section("Combat");
        Console.WriteLine($"  incapacitations   : {_incapacitations} across {_incapacitationFiles.Count} sessions");

        var deaths = _eventKinds.GetValueOrDefault("combat.death");
        var destructions = _eventKinds.GetValueOrDefault("combat.vehicle");

        Console.WriteLine($"  deaths            : {_corpseDeaths}   (from corpse item-recovery bursts)");
        Console.WriteLine($"  ship retrievals   : {_shipRetrievals}");
        Console.WriteLine($"  actor deaths      : {deaths}");
        Console.WriteLine($"  vehicle destroyed : {destructions}");

        if (deaths == 0 && destructions == 0)
        {
            Console.WriteLine("  -> no <Actor Death> or <Vehicle Destruction>, as expected on SC 4.9 and 4.10.");
            Console.WriteLine("     Deaths above come from corpse bursts, which the game still emits.");
        }

        Section("Parser health");
        foreach (var (kind, count) in _eventKinds.OrderByDescending(p => p.Value))
            Console.WriteLine($"  {kind,-24} {count,8}");
        Console.WriteLine($"  {"! unmatched known tags",-24} {_unmatchedKnownTags,8}");

        if (_unmatchedByTag.Count > 0)
        {
            Section("Unmatched, by tag");
            foreach (var (tag, (count, sample)) in _unmatchedByTag.OrderByDescending(p => p.Value.Count))
            {
                Console.WriteLine($"  {count,6}  <{tag}>");
                Console.WriteLine($"          {sample}");
            }
        }
    }

    private static void Bump(Dictionary<string, int> map, string key) =>
        map[key] = map.GetValueOrDefault(key) + 1;

    private static string Join(IEnumerable<string> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? "(none)" : string.Join(", ", list);
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }

    private static void Top(string title, Dictionary<string, int> map, int take = 15)
    {
        Section($"{title} ({map.Count} distinct)");

        foreach (var (key, count) in map.OrderByDescending(p => p.Value).ThenBy(p => p.Key).Take(take))
            Console.WriteLine($"  {count,6}  {key}");

        if (map.Count > take)
            Console.WriteLine($"  {"...",6}  and {map.Count - take} more");
    }
}
