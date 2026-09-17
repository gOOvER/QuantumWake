namespace Quantumwake.Core.GameData;

/// <summary>
/// What one ship component does, reduced from the community dump's
/// <c>stdItem</c> to the figures the Garage sheet uses.
/// </summary>
/// <param name="Type">The group - WeaponGun, Shield, Cooler, PowerPlant - which is the first part of the dump's dotted type.</param>
/// <param name="Em">The part's own EM at full output. A ship's figure is not the sum of these - see <see cref="ShipSheet"/>.</param>
/// <param name="Ir">The part's own IR. Coolers carry nearly all of a ship's.</param>
/// <param name="PowerGen">Power segments a plant generates.</param>
/// <param name="CoolantGen">Coolant segments a cooler generates.</param>
public sealed record PartStats(
    string Class,
    string Type,
    int Size,
    int Grade,
    string Name,
    string? Manufacturer,
    string? Uuid,
    double Mass,
    double Em,
    double Ir,
    double Health,
    double PowerGen,
    double PowerUseMin,
    double PowerUseMax,
    double CoolantGen,
    double CoolantUseMin,
    double CoolantUseMax,
    WeaponStats? Weapon = null,
    ShieldStats? Shield = null,
    QuantumStats? Quantum = null,
    MissileStats? Missile = null,
    ArmorSignals? Armor = null,
    string? SubType = null,
    // The dump counts emissions and draw only for parts on the resource
    // network; a part without one still weighs and still shoots.
    bool Networked = true,
    // The maker's code - KLWE, BEHR, AEGS - which is what the marks are filed under.
    string? MakerCode = null);

public sealed record WeaponStats(
    double Dps,
    double SustainedDps,
    double Alpha,
    double RateOfFire,
    double Range,
    double AmmoSpeed,
    IReadOnlyDictionary<string, double> DpsByType);

/// <param name="AbsorptionMax">Per damage type, the share the shield stops at full charge.</param>
public sealed record ShieldStats(
    double Hp,
    double Regen,
    double DownedDelay,
    double DamagedDelay,
    IReadOnlyDictionary<string, double> AbsorptionMax);

/// <param name="FuelRate">Quantum fuel per metre; range is the tank divided by this.</param>
public sealed record QuantumStats(
    double Speed,
    double SpoolTime,
    double Cooldown,
    double FuelRate,
    double DisconnectRange,
    double StageOneAccel,
    double StageTwoAccel);

public sealed record MissileStats(
    double Damage,
    double Speed,
    double LockTime,
    double Range,
    string? TrackingSignal);

/// <summary>
/// The ship's armour multiplies every signature. This is the factor that
/// looked like a constant 1.13 on the Gladius and was not one anywhere else.
/// </summary>
public sealed record ArmorSignals(double Em, double Ir, double CrossSection);

/// <summary>
/// One port in a ship's loadout tree, with what the ship comes with.
/// </summary>
/// <param name="PortId">The dump's own id for the port, unique in the ship. Two turrets both call their gun port hardpoint_class_2.</param>
/// <param name="Class">The fitted part's class, or null for an empty port.</param>
/// <param name="Type">The fitted part's group, so a walk can classify without the part table.</param>
/// <param name="Accepts">Groups the port takes - what a swap may put here.</param>
public sealed record FitPort(
    string PortId,
    string Hardpoint,
    string? Class,
    string? Type,
    bool Editable,
    int MinSize,
    int MaxSize,
    IReadOnlyList<string> Accepts,
    IReadOnlyList<FitPort> Children);

public sealed record Vec3(double X, double Y, double Z);

public sealed record FlightStats(double Scm, double Boost, double Max, double Pitch, double Yaw, double Roll);

/// <summary>
/// The dump's own totals for the stock fit. Not shown as such - the sheet
/// recomputes them - but kept so the recomputation can be checked against a
/// figure it never read, on every ship, every time the tests run.
/// </summary>
public sealed record DatasetTotals(
    double EmShields,
    double EmQuantum,
    double IrShields,
    double IrQuantum,
    int PowerSegments,
    double CoolingSegments,
    double ShieldHp,
    double FixedDps,
    double TurretDps,
    double QuantumRange,
    double MassTotal);

/// <summary>
/// A ship as the dump describes it before any part is changed.
/// </summary>
/// <param name="HullMass">The hull alone, as the dump states it. Shown, not summed: see <see cref="ShipSheet.Mass"/>.</param>
/// <param name="PowerPools">Per group, how many items draw from the pool (shields) or how many segments the pool allows (guns).</param>
public sealed record ShipBase(
    string Class,
    string Name,
    string? Manufacturer,
    string? Role,
    string? Career,
    int Size,
    int Crew,
    bool IsSpaceship,
    double HullMass,
    double LoadoutMass,
    double Health,
    Vec3 CrossSection,
    FlightStats Flight,
    double QuantumFuel,
    double HydrogenFuel,
    double CargoScu,
    IReadOnlyDictionary<string, int> PowerPools,
    DatasetTotals Dataset,
    IReadOnlyList<FitPort> Loadout,
    IReadOnlyList<CargoGrid>? CargoGrids = null);

/// <summary>EM and IR for one power scenario, with the EM by group.</summary>
public sealed record Signature(double Em, double Ir, IReadOnlyDictionary<string, double> EmByGroup);

/// <param name="Available">Segments the plants provide.</param>
/// <param name="UsedShields">Drawn with shields up and the drive idle.</param>
/// <param name="UsedQuantum">Drawn with the drive spooled and shields down.</param>
public sealed record PowerBudget(int Available, double UsedShields, double UsedQuantum, IReadOnlyDictionary<string, double> UsedByGroup)
{
    public bool OverShields => UsedShields > Available;
    public bool OverQuantum => UsedQuantum > Available;
}

/// <param name="LoadShields">Cooling used over cooling generated, shields up. Above 1 is a ship that runs hot.</param>
public sealed record CoolingBudget(double Generated, double UsedShields, double UsedQuantum, double LoadShields, double LoadQuantum);

public sealed record ShieldSummary(double Hp, double Regen, int Generators, int PoolLimit);

/// <param name="FixedDps">Guns on fixed mounts and gimbals the pilot aims.</param>
/// <param name="TurretDps">Guns on crewed or remote turrets, which the pilot does not fire.</param>
/// <param name="MissileDamage">Every missile on every rack, fired once.</param>
/// <param name="Caveat">
/// Said beside the pilot and turret rows when the stock fit's split between
/// them disagrees with the dataset's own total by more than the 2% the rest
/// of the sheet holds to: one of the seven hulls whose remote turrets are
/// named as pilot mounts in the loadout. Null when the two agree.
/// </param>
/// <param name="Speeds">
/// The pilot's guns grouped by projectile speed, fastest first. The game
/// draws one lead indicator - a pip - per speed, so two entries here is two
/// pips on the HUD and a lead that is right for one gun and wrong for the
/// other. The dataset's <c>Ammunition.Speed</c>, in metres a second; a gun
/// without one is left out rather than counted as a speed of its own.
/// </param>
public sealed record WeaponSummary(
    double FixedDps,
    double FixedSustainedDps,
    double FixedAlpha,
    double TurretDps,
    double MissileDamage,
    int Missiles,
    IReadOnlyList<string> Guns,
    string? Caveat = null,
    IReadOnlyList<GunSpeed>? Speeds = null)
{
    /// <summary>How many lead indicators the pilot's guns put on the HUD.</summary>
    public int Pips => Speeds?.Count ?? 0;
}

/// <summary>One projectile speed among the pilot's guns, and which guns fire at it.</summary>
public sealed record GunSpeed(double Speed, IReadOnlyList<string> Guns);

/// <param name="Range">Metres on a full tank: tank over the drive's fuel rate.</param>
public sealed record QuantumSummary(string Drive, double Speed, double SpoolTime, double Cooldown, double Range, double FuelPerGm);

/// <summary>One port as the sheet saw it: what is in it now, and what was stock.</summary>
public sealed record FittedPart(string PortId, string Hardpoint, string Group, int MinSize, int MaxSize, string? Class, string? Name, string? StockClass, bool Changed);

/// <summary>
/// Everything the sheet says about a ship with a given set of parts on it.
/// </summary>
/// <remarks>
/// <para>
/// The signature figures are the dump's own model, re-implemented from
/// octfx/ScDataDumper's <c>EmissionAggregator</c> and checked against its
/// output on 269 ships (see docs/garage.md). Every group's EM is multiplied by
/// the armour's signal multiplier; the power plant's EM is per segment drawn,
/// not its maximum; shields count only up to the pool; weapon EM scales down
/// to the weapon pool; IR is every part's IR times the armour times the
/// cooling load. Two scenarios, as the dump publishes them: shields up with
/// the drive idle, and the drive spooled with the shields down.
/// </para>
/// <para>
/// Everything at its maximum. A pilot who has pulled power off the guns is
/// quieter than this says; the sheet says so where it shows the number.
/// </para>
/// </remarks>
public sealed record ShipSheet(
    string Class,
    string Name,
    double Mass,
    Signature Shields,
    Signature Quantum,
    PowerBudget Power,
    CoolingBudget Cooling,
    ShieldSummary Shield,
    WeaponSummary Weapons,
    QuantumSummary? QuantumDrive,
    ArmorSignals ArmorSignals,
    Vec3 CrossSection,
    IReadOnlyList<FittedPart> Parts,
    IReadOnlyList<string> Notes)
{
    /// <summary>
    /// Computes the sheet for a ship with the given parts on it.
    /// </summary>
    /// <param name="parts">Part stats by class; a class the table does not know contributes nothing and is noted.</param>
    /// <param name="swaps">Port id to the class to put there instead of stock. Null or empty means the stock fit.</param>
    public static ShipSheet Compute(
        ShipBase ship,
        IReadOnlyDictionary<string, PartStats> parts,
        IReadOnlyDictionary<string, string?>? swaps = null)
    {
        var acc = new Accumulator(ship, parts, swaps ?? new Dictionary<string, string?>());
        acc.Walk(ship.Loadout, underTurret: false);
        var sheet = acc.Finish();

        // The pilot/turret split is the one figure this model gets from a port
        // name rather than a number, and the dump disagrees on seven hulls. The
        // stock fit is what the dump's total describes, so the stock fit is
        // what is checked - a bench with the guns swapped is not evidence
        // either way - and the caveat then rides on every fit of that hull.
        var stock = swaps is { Count: > 0 } ? Compute(ship, parts, null) : sheet;
        var published = ship.Dataset.FixedDps;
        if (published > 0 && Math.Abs(stock.Weapons.FixedDps - published) / published > 0.02)
        {
            var caveat = $"The dataset's own total puts {published:0.#} DPS in the pilot's hands for this hull and the rest on turrets; "
                + $"here the stock fit reads {stock.Weapons.FixedDps:0.#}, because its remote turrets are named as pilot mounts in the loadout "
                + "and the file that says who fires them is not published. The guns are on the sheet either way; only the row differs.";
            sheet = sheet with { Weapons = sheet.Weapons with { Caveat = caveat } };
        }

        return sheet;
    }

    private sealed class Accumulator(
        ShipBase ship,
        IReadOnlyDictionary<string, PartStats> parts,
        IReadOnlyDictionary<string, string?> swaps)
    {
        private static readonly HashSet<string> WeaponGroups = new(StringComparer.Ordinal) { "Turret", "TurretBase", "WeaponGun" };

        private readonly int _maxShields = ship.PowerPools.GetValueOrDefault("Shield");
        private readonly int? _weaponPool = ship.PowerPools.TryGetValue("WeaponGun", out var w) ? w : null;

        private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _em = new(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _powerUse = new(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _coolantUse = new(StringComparer.Ordinal);
        private readonly List<(double Segments, int Size)> _plants = [];
        private readonly List<FittedPart> _fitted = [];
        private readonly List<string> _notes = [];
        private readonly List<string> _guns = [];
        private readonly List<(string Name, double Speed)> _gunSpeeds = [];
        private readonly HashSet<string> _unknown = new(StringComparer.Ordinal);

        private double _ir, _plantEm, _plantSegments, _coolantGen, _weaponPowerUncapped;
        private double _armorEm = 1, _armorIr = 1, _armorCross = 1;
        private double _massDelta;

        private double _shieldHp, _shieldRegen;
        private int _shieldGenerators;

        private double _fixedDps, _fixedSustained, _fixedAlpha, _turretDps, _missileDamage;
        private int _missiles;

        private PartStats? _drive;

        public void Walk(IReadOnlyList<FitPort> ports, bool underTurret)
        {
            foreach (var port in ports)
            {
                var swapped = swaps.TryGetValue(port.PortId, out var to);
                var cls = swapped ? to : port.Class;
                var part = cls is not null && parts.TryGetValue(cls, out var p) ? p : null;

                // Only a part the bench offers is worth a sentence: a door or a seat
                // with no figures is the dump's business, not the pilot's.
                if (cls is not null && part is null && port.Editable)
                    _unknown.Add(cls);

                if (port.Editable || port.Class is not null)
                {
                    _fitted.Add(new FittedPart(
                        port.PortId, port.Hardpoint,
                        part?.Type ?? port.Type ?? port.Accepts.FirstOrDefault() ?? "?",
                        port.MinSize, port.MaxSize,
                        cls, part?.Name, port.Class,
                        swapped && !string.Equals(to, port.Class, StringComparison.Ordinal)));
                }

                if (part is not null)
                    Add(part, underTurret);

                if (swapped)
                {
                    var stock = port.Class is not null && parts.TryGetValue(port.Class, out var was) ? was.Mass : 0;
                    _massDelta += (part?.Mass ?? 0) - stock;
                }

                // The children are the stock part's - a rack's missiles, a
                // turret's guns. A port swapped to another part, or emptied,
                // takes them with it: the dump says what the stock rack carries
                // and nothing about what another would, so the new rack counts
                // for itself and the missile row says why it has fewer.
                var childrenLeft = swapped && !string.Equals(to, port.Class, StringComparison.Ordinal) && port.Children.Count > 0;
                if (childrenLeft)
                {
                    var carried = port.Children.Count(c => c.Class is not null);
                    if (part is not null && carried > 0)
                        _notes.Add($"{part.Name} is fitted with nothing counted on it: the reference says what the stock {(port.Type ?? "part")} carried ({carried}), not what this one does.");
                    continue;
                }

                Walk(port.Children, underTurret || IsTurret(port));
            }
        }


        /// <summary>
        /// Whether the guns under this port are somebody else's to fire.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A crewed turret is a <c>TurretBase</c> port. A <c>Turret</c> port is
        /// remote when its <em>name</em> says "remote" or "pdc", and a pilot
        /// hardpoint otherwise - gimbals, canard and ball mounts included. The
        /// base item of a crewed turret has no stat block at all, so this reads
        /// the port, never the part.
        /// </para>
        /// <para>
        /// The name alone, and deliberately. The published dump agrees with
        /// this on 231 of 238 armed ships; with the dumper's newer rule, which
        /// also reads the fitted class, on 206. What decides the other seven -
        /// whether a remote turret is slaved to the pilot's seat - lives in
        /// controller tags in the vehicle XML that ships.json does not carry.
        /// A gun this gets wrong moves between the pilot and turret rows of the
        /// sheet; it is never lost. The seven are listed in docs/garage.md.
        /// </para>
        /// </remarks>
        private static bool IsTurret(FitPort port)
        {
            if (port.Type == "TurretBase" || port.Accepts.Any(a => a.StartsWith("TurretBase", StringComparison.Ordinal)))
                return true;

            if (port.Type != "Turret" && !port.Accepts.Any(a => a.StartsWith("Turret", StringComparison.Ordinal)))
                return false;

            return Says(port.Hardpoint, "remote") || Says(port.Hardpoint, "pdc");

            static bool Says(string text, string word) =>
                text.Contains(word, StringComparison.OrdinalIgnoreCase);
        }

        private void Add(PartStats part, bool underTurret)
        {
            var t = part.Type;

            // A docked vehicle's parts are its own; counting them inflates the carrier.
            if (t.StartsWith("NOITEM_Vehicle", StringComparison.Ordinal))
                return;

            _counts.TryAdd(t, 0);
            _em.TryAdd(t, 0);
            _powerUse.TryAdd(t, 0);
            _coolantUse.TryAdd(t, 0);

            var isShield = t == "Shield";
            var isQuantum = t == "QuantumDrive";
            var isPlant = t == "PowerPlant";
            var isCooler = t == "Cooler";
            var isWeapon = WeaponGroups.Contains(t);
            var isFlight = t == "FlightController";

            if (isPlant)
                _plantEm += part.Em;

            // ---- the emission model, as the dump computes it ----
            // Only for parts on the resource network; the armour below is not,
            // and still multiplies everything.
            if (part.Networked)
            {
                if (isQuantum) { _em[t] += part.Em; _counts[t]++; }
                else if (isShield) { if (_counts[t] < _maxShields) _em[t] += part.Em; }
                else if (isCooler) { _em[t] += part.Em; _counts[t]++; }
                else if (isWeapon) { _em[t] += part.Em; }
                else if (isPlant)
                {
                    _em[t] += part.Em;
                    _counts[t]++;
                    if (part.PowerGen > 0) _plants.Add((part.PowerGen, part.Size));
                }
                else _em[t] += part.Em;

                if (isQuantum)
                {
                    _coolantUse[t] += part.CoolantUseMax;
                    _coolantUse[t] += part.PowerUseMax;
                }
                else if (isShield)
                {
                    if (_counts[t] < _maxShields)
                    {
                        _counts[t]++;
                        _coolantUse[t] += part.CoolantUseMax;
                        _powerUse[t] += part.PowerUseMax;
                    }
                }
                else if (isWeapon)
                {
                    _coolantUse[t] += part.CoolantUseMax;
                    _powerUse[t] += part.PowerUseMax;
                    _weaponPowerUncapped += part.PowerUseMax;
                }
                else if (isFlight)
                {
                    _powerUse[t] += part.PowerUseMax;
                }
                else
                {
                    if (!isCooler && !isPlant) _coolantUse[t] += part.PowerUseMax;
                    else _coolantGen += part.CoolantGen;

                    if (!isPlant) _powerUse[t] += part.PowerUseMax;
                }

                _plantSegments += part.PowerGen;
                _ir += part.Ir;
            }

            if (part.Armor is { } armor)
            {
                _armorEm *= armor.Em;
                _armorIr *= armor.Ir;
                _armorCross *= armor.CrossSection;
            }

            // ---- the rest of the sheet ----
            if (part.Shield is { } sh)
            {
                // The pool cap applies to the fight as much as to the signature:
                // a third generator on a two-pool ship never comes online.
                if (_shieldGenerators < _maxShields || _maxShields == 0)
                {
                    _shieldHp += sh.Hp;
                    _shieldRegen += sh.Regen;
                }
                _shieldGenerators++;
            }

            if (part.Weapon is { } wp && t == "WeaponGun")
            {
                if (underTurret) _turretDps += wp.Dps;
                else
                {
                    _fixedDps += wp.Dps;
                    _fixedSustained += wp.SustainedDps;
                    _fixedAlpha += wp.Alpha;
                    _guns.Add(part.Name);
                    if (wp.AmmoSpeed > 0) _gunSpeeds.Add((part.Name, wp.AmmoSpeed));
                }
            }

            if (part.Missile is { } ms && t == "Missile")
            {
                _missileDamage += ms.Damage;
                _missiles++;
            }

            if (part.Quantum is not null && isQuantum)
                _drive = part;
        }

        /// <summary>"CF-447 Rhino Repeater ×4, Deadbolt IV Cannon ×2" - names once each, counted.</summary>
        private static string Describe(IEnumerable<string> names) =>
            string.Join(", ", names.GroupBy(n => n, StringComparer.Ordinal)
                .Select(g => g.Count() > 1 ? $"{g.Key} ×{g.Count()}" : g.Key));

        public ShipSheet Finish()
        {
            int? available = null;
            if (_plants.Count > 0)
            {
                // Σ round(gen ÷ n) + (n − 1) × Σ size, which is how the game
                // credits a second plant: more than one, less than two.
                var n = _plants.Count;
                // Half away from zero, as the generator rounds: two 13-segment
                // plants are 7 + 7, not 6 + 6.
                var baseSegments = _plants.Where(p => p.Segments > 0).Sum(p => (int)Math.Round(p.Segments / n, MidpointRounding.AwayFromZero));
                var sizes = _plants.Where(p => p.Segments > 0).Sum(p => p.Size);
                if (baseSegments > 0) available = baseSegments + (n - 1) * sizes;
            }
            if (available is null && _plantSegments > 0)
                available = (int)Math.Round(_plantSegments);

            var weaponUse = _powerUse.GetValueOrDefault("WeaponGun");
            _powerUse["WeaponGun"] = Math.Min(_weaponPool ?? weaponUse, weaponUse);
            _coolantUse["WeaponGun"] = 0;

            if (_weaponPool is { } pool && _weaponPowerUncapped > pool)
                _em["WeaponGun"] = _em.GetValueOrDefault("WeaponGun") * (pool / _weaponPowerUncapped);

            static bool NotThruster(KeyValuePair<string, double> kv) => !kv.Key.Contains("Thruster", StringComparison.Ordinal);

            var powerUse = _powerUse.Where(NotThruster).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
            var coolantUse = _coolantUse.Where(NotThruster).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

            var powerShields = powerUse.Where(kv => kv.Key != "QuantumDrive").Sum(kv => kv.Value);
            var powerQuantum = powerUse.Where(kv => kv.Key != "Shield").Sum(kv => kv.Value);
            var coolingShields = coolantUse.Where(kv => kv.Key != "QuantumDrive").Sum(kv => kv.Value) + powerShields;
            var coolingQuantum = coolantUse.Where(kv => kv.Key != "Shield").Sum(kv => kv.Value) + powerQuantum;

            var emPerSegment = _counts.GetValueOrDefault("PowerPlant") > 0 && available is > 0
                ? _plantEm / available.Value
                : 0;

            var loadShields = _coolantGen > 0 ? coolingShields / _coolantGen : 0;
            var loadQuantum = _coolantGen > 0 ? coolingQuantum / _coolantGen : 0;

            var ir = _ir * _armorIr;

            var groups = _em.Where(NotThruster)
                .ToDictionary(kv => kv.Key, kv => kv.Value * _armorEm, StringComparer.Ordinal);
            var plantShields = Math.Round(emPerSegment * powerShields * _armorEm);
            // The generator puts the shields-up draw on the plant in both
            // scenarios; matching it is worth more than a figure of our own.
            var plantQuantum = plantShields;

            var emShields = groups
                .Where(kv => kv.Key != "QuantumDrive" && kv.Key != "PowerPlant" && kv.Value > 0)
                .ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value), StringComparer.Ordinal);
            if (plantShields > 0) emShields["PowerPlant"] = plantShields;

            var emQuantum = groups
                .Where(kv => kv.Key != "Shield" && kv.Key != "PowerPlant" && kv.Value > 0)
                .ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value), StringComparer.Ordinal);
            if (plantQuantum > 0) emQuantum["PowerPlant"] = plantQuantum;

            var power = new PowerBudget(available ?? 0, Math.Round(powerShields, 2), Math.Round(powerQuantum, 2),
                powerUse.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal));

            if (power.OverShields)
                _notes.Add($"Draws {power.UsedShields:0.#} power segments with shields up; the plant provides {power.Available}. The game will brown out something.");
            if (loadShields > 1)
                _notes.Add($"Cooling load {loadShields:P0} with shields up: the coolers cannot keep up at full draw.");
            foreach (var cls in _unknown)
                _notes.Add($"{cls} is fitted but the reference has no figures for it; it counts for nothing here.");

            // Speeds within a metre a second are one pip: the dataset carries
            // 1345.5 beside 1296 and 1440, and those are three pips, but a
            // rounding difference is not.
            var speeds = _gunSpeeds
                .GroupBy(g => Math.Round(g.Speed))
                .OrderByDescending(g => g.Key)
                .Select(g => new GunSpeed(g.Key, [.. g.Select(x => x.Name)]))
                .ToList();
            if (speeds.Count > 1)
            {
                var told = string.Join(", ", speeds.Select(sp => $"{Describe(sp.Guns)} at {sp.Speed:N0} m/s"));
                _notes.Add($"The pilot's guns fire at {speeds.Count} speeds - {told} - so the HUD shows {speeds.Count} pips, and a lead that lands one gun misses the other.");
            }

            var quantum = _drive?.Quantum is { } q
                ? new QuantumSummary(_drive.Name, q.Speed, q.SpoolTime, q.Cooldown,
                    q.FuelRate > 0 ? ship.QuantumFuel / q.FuelRate : 0,
                    q.FuelRate * 1e9)
                : null;

            return new ShipSheet(
                ship.Class,
                ship.Name,
                // The dump's stock mass, moved by what was swapped. A sum of the
                // parts undershoots on every large hull - a Carrack carries a
                // fifth of its mass in parts the dump has no block for - and the
                // swap is the only thing this page changes about it.
                Math.Round(ship.Dataset.MassTotal + _massDelta),
                new Signature(emShields.Values.Sum(), Math.Round(ir * loadShields), emShields),
                new Signature(emQuantum.Values.Sum(), Math.Round(ir * loadQuantum), emQuantum),
                power,
                new CoolingBudget(_coolantGen, Math.Round(coolingShields, 2), Math.Round(coolingQuantum, 2),
                    Math.Round(loadShields, 3), Math.Round(loadQuantum, 3)),
                new ShieldSummary(_shieldHp, _shieldRegen, _shieldGenerators, _maxShields),
                new WeaponSummary(
                    Math.Round(_fixedDps, 1), Math.Round(_fixedSustained, 1), Math.Round(_fixedAlpha, 1),
                    Math.Round(_turretDps, 1), Math.Round(_missileDamage), _missiles, _guns, null, speeds),
                quantum,
                new ArmorSignals(_armorEm, _armorIr, _armorCross),
                new Vec3(
                    Math.Round(ship.CrossSection.X * _armorCross),
                    Math.Round(ship.CrossSection.Y * _armorCross),
                    Math.Round(ship.CrossSection.Z * _armorCross)),
                _fitted,
                _notes);
        }
    }
}
