namespace Quantumwake.LogSim;

/// <summary>Universe fixtures the simulator draws from, all seen in real logs.</summary>
internal static class Fixtures
{
    public static readonly (string Id, string Name)[] Locations =
    [
        ("RR_MIC_LEO", "microTech LEO"),
        ("RR_MIC_L1", "microTech L1"),
        ("RR_CRU_LEO", "Crusader LEO"),
        ("Stanton4_NewBabbage", "New Babbage"),
        ("Stanton1_Lorville", "Lorville"),
        ("Stanton2_Orison", "Orison"),
        ("Stanton3_Area18", "Area18"),
        ("Stanton4b_RayariHydro_Cantwell", "Rayari Cantwell"),
        ("Stanton3b_ArcCorp_Area061", "Area 061"),
        ("Stanton4_DistributionCentre_Covalex_S4DC05", "Covalex DC"),
        ("Pyro2_Outpost_col_m_scrp_indy_001", "Monox scrapyard"),
        ("RR_JP_StantonPyro", "Stanton-Pyro jump"),
        ("RR_P5_L2", "Pyro V L2"),
        ("GrimHEX", "GrimHEX"),
        ("Stanton4_Shubin_SM0_22", "Shubin SM0-22")
    ];

    /// <summary>
    /// Cargo the simulated pilot moves, as the ids the game logs.
    /// </summary>
    /// <remarks>
    /// Real resource ids from the community digest, so a simulated install
    /// exercises the name lookup rather than dodging it - the whole point of
    /// the cargo map is that a receipt can be tied to a named commodity. The
    /// base price, combined with a steady per-place multiplier, is what makes
    /// one terminal genuinely the best place to sell a given commodity.
    /// </remarks>
    public static readonly (string Resource, int BasePrice)[] Commodities =
    [
        ("bde5a2c8-2ef4-46ac-9403-2fcb79e4016c", 1540),  // Quantainium
        ("7f4599b0-a2b2-4178-8c7e-13292054ab20", 452),   // Laranite
        ("dc6fbcbb-5990-4ed5-82ee-93152dab7845", 268),   // Agricium
        ("accacd33-3a1a-4ec7-8b4a-14b9f028047c", 88)     // Processed Food
    ];

    public static readonly string[] Destinations =
    [
        "Stanton4_NewBabbage", "OOC_Stanton_4_Microtech", "LOC_rs_ext_stan-pyro_jp1",
        "ObjectContainer_RestStop", "LOC_RR_S4_L1", "Area18_City_objectContainer",
        "rs_ext_cru-leo1", "NavPoint_Dynamic_759722455016", "OOC_Stanton_2_Crusader",
        "ObjectContainer_Lorville_City", "rs_ext_pyro3_l3"
    ];

    public static readonly string[] Origins =
    [
        "Port Tressler", "New Babbage", "Seraphim Station", "Area18", "Gaslight", "Everus Harbor"
    ];

    /// <remarks>
    /// The channel name is what the ship's comms channel is called when the
    /// pilot boards - "Anvil F7C-M Super Hornet Mk II", not the entity class -
    /// each copied from a real "You have joined channel" line.
    /// </remarks>
    public static readonly (string Prefix, string Model, string Channel)[] Ships =
    [
        ("MISC", "Starlancer_Max", "MISC Starlancer MAX"),
        ("ANVL", "Hornet_F7CM_Mk2", "Anvil F7C-M Super Hornet Mk II"),
        ("DRAK", "Corsair", "Drake Corsair"),
        ("RSI", "Hermes", "RSI Hermes"),
        ("MISC", "Freelancer_MAX", "MISC Freelancer MAX"),
        ("RSI", "Aurora_Mk2", "RSI Aurora Mk II"),
        ("DRAK", "Cutter", "Drake Cutter"),
        ("ORIG", "325a", "Origin 325a"),
        ("DRAK", "Clipper", "Drake Clipper")
    ];

    /// <summary>
    /// Placeholder crew. Never a real handle: these are what the party toasts
    /// and ship channels name, and a demo install is shown to other people.
    /// </summary>
    public static readonly string[] Crew = ["Pilot-One", "Pilot-Two", "Pilot-Three", "Pilot-Four"];

    /// <summary>
    /// Armour sets as a spawn equips them, port by port. Every class is one a
    /// real <c>AttachmentReceived</c> line has put on that port.
    /// </summary>
    public static readonly (string Port, string Class)[][] Armour =
    [
        [
            ("Armor_Helmet", "cds_armor_medium_helmet_02_01_01"),
            ("Armor_Torso", "cds_armor_medium_core_02_01_01"),
            ("Armor_Arms", "cds_armor_medium_arms_02_01_01"),
            ("Armor_Legs", "cds_armor_medium_legs_02_01_01"),
            ("Armor_Undersuit", "rsi_odyssey_undersuit_01_01_01"),
            ("backpack", "hdtc_utility_light_backpack_01_01_01")
        ],
        [
            ("Armor_Helmet", "rrs_specialist_light_helmet_01_02_01"),
            ("Armor_Torso", "rrs_specialist_light_core_01_02_01"),
            ("Armor_Arms", "rrs_specialist_light_arms_01_02_01"),
            ("Armor_Legs", "rrs_specialist_light_legs_01_02_01"),
            ("Armor_Undersuit", "hdtc_undersuit_01_01_01"),
            ("backpack", "rrs_combat_light_backpack_01_02_01")
        ],
        [
            ("Armor_Helmet", "rrs_specialist_heavy_helmet_01_02_01"),
            ("Armor_Torso", "rrs_specialist_heavy_core_01_02_01"),
            ("Armor_Arms", "rrs_specialist_heavy_arms_01_02_01"),
            ("Armor_Legs", "rrs_specialist_heavy_legs_01_02_01"),
            ("Armor_Undersuit", "rsi_odyssey_undersuit_01_01_01"),
            ("backpack", "rrs_combat_heavy_backpack_01_02_01")
        ]
    ];

    /// <summary>Long guns with the magazine and sight the real logs show on them.</summary>
    public static readonly (string Gun, string Magazine, string Optic)[] Rifles =
    [
        ("hdgw_rifle_ballistic_01", "hdgw_rifle_ballistic_01_mag", "nvtc_optics_holo_x2_s1"),
        ("behr_rifle_ballistic_03", "behr_rifle_ballistic_03_mag", "behr_optics_tsco_x4_s2"),
        ("gmni_sniper_ballistic_01", "gmni_sniper_ballistic_01_mag", "gmni_optics_tsco_x8_s3")
    ];

    /// <summary>Everything a spawn carries besides armour and the long gun.</summary>
    public static readonly (string Port, string Class)[] Kit =
    [
        ("Body_ItemPort", "body_01_noMagicPocket"),
        ("mobiglas_attach", "MobiGlas"),
        ("radar", "FPS_DefaultRadar_Lens"),
        ("universal_necksock", "universal_necksock_01"),
        ("medPen_attach_1", "crlf_consumable_healing_01"),
        ("oxyPen_attach_1", "crlf_consumable_healing_01"),
        ("utility_attach_1", "grin_multitool_01"),
        ("wep_sidearm", "klwe_pistol_energy_01"),
        ("magazine_attach", "klwe_pistol_energy_01_mag")
    ];

    /// <summary>
    /// What turns up when a location's storage is browsed, all classes a real
    /// inventory listing has shown.
    /// </summary>
    public static readonly string[] StashItems =
    [
        "crlf_consumable_healing_01", "hdgw_rifle_ballistic_01_mag", "behr_gren_frag_01",
        "behr_lmg_ballistic_01_mag", "behr_rifle_ballistic_03_mag", "gmni_sniper_ballistic_01",
        "grin_multitool_01", "klwe_pistol_energy_01", "Drink_bottle_vestal_01_a",
        "cds_armor_medium_arms_02_01_01", "grin_tractor_01", "Food_burrito_01_musaka_a",
        "rrs_specialist_heavy_core_01_02_01", "behr_lmg_ballistic_01", "kegr_fire_extinguisher_01",
        "crlf_medgun_01", "crlf_consumable_painkiller_01", "lbco_sniper_energy_01",
        "nvtc_optics_holo_x2_s1", "mrai_flightsuit_01_07_12"
    ];

    /// <summary>
    /// Things Wikelo asks for, as the entity classes his trades name
    /// (docs/wikelo.md). Scrip, Carinite and the pearls also appear under
    /// these exact classes in real inventory lines; the Favor is the class the
    /// emporium's own contracts require.
    /// </summary>
    public static readonly string[] WikeloItems =
    [
        "Carryable_1H_CY_Physical_Currency_Scrip_Merc_1",
        "Harvestable_Mineral_1H_CarinitePure",
        "Carryable_2H_FL_Vlk_Pearl_Irradiated_Super_01",
        "Carryable_2H_FL_Vlk_Pearl_Irradiated_High_02",
        "Carryable_1H_CY_banu_favour_Wikelo"
    ];

    /// <summary>
    /// Kiosk purchases by where they happen, with the total and quantity a
    /// real request carried for exactly that shop and item.
    /// </summary>
    public static readonly Dictionary<string, (string Shop, string Item, decimal Total, int Quantity)[]> Shops = new()
    {
        ["RR_MIC_LEO"] =
        [
            ("SCShop_RestStop_Pharmacy-001", "crlf_consumable_healing_01", 2_915m, 11),
            ("SCShop_RestStop_Pharmacy-001", "crlf_consumable_healing_01", 5_565m, 21),
            ("SCShop_RestStop_Pharmacy-001", "crlf_consumable_overdoseRevival_01", 5_040m, 21)
        ],
        ["RR_MIC_L1"] =
        [
            ("SCShop_RestStop_Pharmacy-001", "crlf_consumable_healing_01", 5_565m, 21)
        ],
        ["RR_CRU_LEO"] =
        [
            ("SCShop_RestStop_Pharmacy-001", "crlf_consumable_healing_01", 10_865m, 41)
        ],
        ["RR_P5_L2"] =
        [
            ("SCShop_Pyro_RestStop_BlackMarket_FPSItems", "lbco_sniper_energy_01", 9_029m, 1),
            ("SCShop_Pyro_RestStop_BlackMarket_FPSItems", "slaver_armor_light_helmet_01_01_01", 1_547m, 1)
        ],
        ["Stanton4_NewBabbage"] =
        [
            ("SCShop_ShubinInterstellar_NewBabbage", "grin_multitool_01_salvage_repair", 375m, 1),
            ("SCShop_ShubinInterstellar_NewBabbage", "grin_tractor_01", 19_175m, 1),
            ("SCShop_ShubinInterstellar_NewBabbage", "rrs_specialist_heavy_helmet_01_02_01", 7_952m, 1),
            ("SCShop_Centermass_NewBabbage", "MISL_S02_EM_TALN_Dominator", 3_850m, 11),
            ("SCShop_Centermass_NewBabbage", "KLWE_LaserRepeater_S2", 32_276m, 2)
        ],
        ["Stanton3_Area18"] =
        [
            ("SCShop_Centermass_Area18", "KLWE_LaserRepeater_S2", 16_138m, 1),
            ("SCShop_Centermass_Area18", "KLWE_LaserRepeater_S3", 36_308m, 1)
        ]
    };

    /// <summary>The payouts real "Awarded" toasts have stated, all after hauls.</summary>
    public static readonly int[] Awards = [9_000, 39_750, 50_250, 53_000, 55_250, 55_750, 61_500, 76_750, 80_500];

    /// <summary>Hangar sizes as the retrieval line's second half names them.</summary>
    public static readonly string[] Hangars = ["Large Hangar", "Small Hangar", "Medium Hangar"];

    /// <summary>
    /// Archetype and the title the toast shows for it, paired: the title is
    /// what the mobiGlas prints, and for the Red Wind hauls it names the
    /// destination the way the StarStrings mod does, which is what the
    /// hauling plan reads.
    /// </summary>
    public static readonly (string Generator, string Contract, string Title)[] Contracts =
    [
        ("Covalex_RecoverCargo", "Covalex_Stanton_VeryHard_RecoverCargo", "Bulk Covalex Shipment Needs Recovering"),
        ("Ling_RecoverCargo", "Ling_Stanton_VeryEasy_RecoverCargo", "Small Covalex Shipment Needs Recovering"),
        ("RedWind_RecoverCargo", "RedWind_Stanton_Easy_RecoverCargo", "Cargo Retrieval Required"),
        ("FTL_Courier", "FTL_Courier_Stanton_AmmoCrate_Rank0_2", "Urgent Delivery Contract"),
        ("EchhartSecurity", "EchhartSecurity_Stanton_VeryEasy_RecoverCargo", "Cargo Retrieval Required"),
        ("HaulCargo", "HaulCargo_AToB_Interstellar_Bulk_DistSp_Dia_FresFoo_Gol_Aphor", "Junior Rank - Direct Small Cargo Haul"),
        ("RedWind_CargoHauling", "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle",
            "Junior | Stellar Small Haul | to Stanton Gateway <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>"),
        ("RedWind_CargoHauling", "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Copper_CargoHauling_Multi2ToSingle",
            "Junior | Stellar Small Haul | to Ruin Station <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>"),
        ("RedWind_CargoHauling", "RedWind_Pyro_SupplyGrade_RegionA_CFP_StationToTradepost_Carbon_CargoHauling_AtoB_Intro",
            "Rookie | <EM3>DIRECT</EM3> Small Haul | Ruin Station > Checkmate <EM4>[BP]*</EM4>"),
    ];

    public static readonly string[] Npcs =
    [
        "PU_Pilots-Human-NPC_Pilot_Criminal_Gunner_Light_01",
        "AI_CRIM_Gunner_Medium_02",
        "PU_Pilots-Human-NPC_Pilot_Pirate_Heavy_03",
        "Kopion_Ranger_01"
    ];

    public static readonly string[] Weapons =
    [
        "behr_lmg_ballistic_01", "klwe_laser_repeater_s3", "apar_special_ballistic_gatling_s4",
        "gemi_ballistic_cannon_s5"
    ];
}

/// <summary>
/// Generates one plausible play session as a sequence of log events.
/// </summary>
/// <remarks>
/// <para>
/// The session follows a believable arc - menu, spawn, travel between locations
/// by quantum, ship swaps, contracts, the occasional incapacitation and
/// disconnect - so the dashboard has a coherent story to render rather than
/// random noise.
/// </para>
/// <para>
/// Two random streams, not one. The trip itself - ships, destinations, trades -
/// draws from the original stream exactly as it always has; everything layered
/// on since (contract endings, kit, crew, stash, purchases, deaths) draws from
/// the second. Adding a draw to the first would reshuffle every ship and flight
/// in every install generated before, and a seed that stops meaning the same
/// trips is not much of a seed.
/// </para>
/// </remarks>
internal sealed class Simulation
{
    private readonly LogWriter _log;
    private readonly Random _random;
    private readonly Random _extra;
    private readonly SimOptions _options;

    private DateTimeOffset _now;
    private int _notificationId = 30;
    private int _noiseCounter;
    private int _inventoryRequest;
    private string _currentLocation = "RR_MIC_LEO";

    private (string Port, string Class)[] _armour = [];
    private (string Gun, string Magazine, string Optic) _rifle;
    private readonly List<string> _party = [];
    private string? _droppedMember;
    private bool _respawning;
    private OpenContract? _contract;

    public Simulation(LogWriter log, SimOptions options, DateTimeOffset start, int seed)
    {
        _log = log;
        _options = options;
        _random = new Random(seed);
        _extra = new Random(unchecked(seed * 7919 + 17));
        _now = start;
    }

    /// <summary>Real time elapsed so far, for live pacing.</summary>
    public TimeSpan Elapsed { get; private set; }

    /// <summary>Called after each beat so live mode can sleep proportionally.</summary>
    public event Action<TimeSpan>? Beat;

    private void Advance(int minSeconds, int maxSeconds) =>
        Pass(TimeSpan.FromSeconds(_random.Next(minSeconds, maxSeconds + 1)));

    /// <summary>Time passing for something drawn from the second stream.</summary>
    private void Wait(int minSeconds, int maxSeconds) =>
        Pass(TimeSpan.FromSeconds(_extra.Next(minSeconds, maxSeconds + 1)));

    private void Pass(TimeSpan span)
    {
        _now += span;
        Elapsed += span;
        Beat?.Invoke(span);
    }

    private void Noise(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _log.Noise(_now.AddMilliseconds(i * 37), _noiseCounter++);
        }
    }

    private T Pick<T>(T[] items) => items[_random.Next(items.Length)];

    private T Draw<T>(T[] items) => items[_extra.Next(items.Length)];

    private bool Chance(double p) => _extra.NextDouble() < p;

    /// <summary>A GUID from the seeded stream, so reruns write the same ids.</summary>
    private string NewGuid()
    {
        var bytes = new byte[16];
        _extra.NextBytes(bytes);
        bytes[7] = (byte)(bytes[7] & 0x0F | 0x40);
        bytes[8] = (byte)(bytes[8] & 0x3F | 0x80);
        return new Guid(bytes).ToString();
    }

    /// <summary>A twelve-digit entity id in the range live item ids fall in.</summary>
    private string NewEntityId() => _extra.NextInt64(780000000000, 849999999999).ToString();

    /// <summary>Writes a complete session.</summary>
    public void Run()
    {
        var sessionId = NewGuid();

        _log.Header(_now, _options.Build);
        Advance(1, 3);

        _log.Character(_now, _options.Handle, _options.Geid);
        _log.Login(_now.AddMilliseconds(120), _options.Handle);
        Advance(1, 2);

        // Menus first.
        _log.Context(_now, "SC_Frontend", sessionId);
        _log.LoadingScreen(_now.AddSeconds(1), "Frontend_Main", "SC_Frontend", 3.44);
        Noise(6);
        Advance(_options.MenuSeconds / 2, _options.MenuSeconds);

        // Into the persistent universe.
        _log.JoinShard(_now, $"pub_use1b_{_options.Build.Number}_{_random.Next(1, 200):D3}");
        _log.Context(_now, "SC_Default", sessionId);
        _log.LoadingScreen(_now.AddSeconds(2), "PU_Megamap", "SC_Default", 21.3);
        Advance(20, 30);
        _log.Spawned(_now);

        // Most evenings in the same armour; now and then something else.
        _armour = Chance(0.6) ? Fixtures.Armour[0] : Draw(Fixtures.Armour);
        _rifle = Draw(Fixtures.Rifles);
        Equip();

        VisitLocation(_currentLocation);

        // A quarter of evenings are flown with company.
        if (Chance(_options.PartyChance))
            FormParty();

        Advance(5, 15);

        for (var leg = 0; leg < _options.Legs; leg++)
            RunLeg(leg);

        if (_party.Count > 0 && Chance(0.3))
        {
            Wait(20, 90);
            PartyNote("Party Disbanded", "The party has been disbanded.: ");
            _party.Clear();
        }

        // Wind down.
        Advance(10, 30);
        _log.Disconnect(_now, "Remote Disconnect - Player requested disconnect", "SC_Default");
        _log.Context(_now.AddSeconds(2), "SC_Frontend", sessionId);
        _log.LoadingScreen(_now.AddSeconds(3), "Frontend_Main", "SC_Frontend", 2.10);
        Noise(4);
        Advance(5, 20);
        _log.Disconnect(_now, "Nub destroyed", "SC_Frontend");
        _log.Quit(_now.AddSeconds(1));
    }

    /// <summary>One trip: pick a ship, take a contract, fly somewhere, land.</summary>
    private void RunLeg(int leg)
    {
        var (prefix, model, channel) = Pick(Fixtures.Ships);
        var entityId = _random.NextInt64(700000000000, 799999999999).ToString();
        var vehicleId = $"{prefix}_{model}_{entityId}";

        Board(vehicleId, entityId, channel);

        // Contract, sometimes.
        if (_random.NextDouble() < 0.6)
        {
            Advance(20, 90);
            var (generator, contract, title) = Pick(Fixtures.Contracts);
            AcceptContract(generator, contract, title);
        }

        Noise(_random.Next(4, 12));

        // Cargo loaded, or the card given up on before leaving.
        ProgressContract();

        // Quantum travel to somewhere new.
        Advance(30, 180);
        var destination = Pick(Fixtures.Destinations);

        _log.QuantumTarget(_now, vehicleId, entityId, destination);

        // Alternate the two route forms so both parser paths are exercised.
        if (leg % 2 == 0)
            _log.RouteWithOrigin(_now.AddSeconds(2), vehicleId, entityId, Pick(Fixtures.Origins), destination);
        else
            _log.RouteDestinationOnly(_now.AddSeconds(2), vehicleId, entityId, destination);

        Noise(_random.Next(3, 9));
        Advance(_options.FlightSeconds / 2, _options.FlightSeconds);

        // Combat, when enabled. Absent from real 4.9 and 4.10 logs by default.
        if (_options.Combat && _random.NextDouble() < 0.5)
            RunCombat();

        // Occasionally go down, and once in a while nobody comes.
        if (_random.NextDouble() < _options.IncapacitationChance)
        {
            _log.Incapacitated(_now, _notificationId++);

            if (Chance(_options.DeathChance))
            {
                Wait(60, 150);
                Die();
            }

            Advance(20, 60);
        }

        // Land and leave the ship.
        _log.VehicleRelease(_now, _options.Geid, vehicleId, entityId);
        Advance(5, 20);

        VisitLocation(Pick(Fixtures.Locations).Id);

        if (_respawning)
        {
            // A new body comes with the kit put back on, port by port.
            _respawning = false;
            Wait(20, 60);
            Equip();
        }

        FinishContract();
        Shop();
        PartyChatter();

        // Cargo, at about half the stops. Buying and selling both happen, so
        // the map has two sides of the counter to shade.
        if (_random.NextDouble() < 0.55)
        {
            Advance(30, 240);
            RunTrade();
        }
    }

    /// <summary>
    /// Getting into this leg's ship: sometimes fetched from a hangar first,
    /// always the comms channel joined, sometimes crew following.
    /// </summary>
    /// <remarks>
    /// The retrieval does not name the ship, as the real one does not. The
    /// navigation computer grumbling that no route is loaded, once the pilot
    /// is in the seat, is what ties the id to a model.
    /// </remarks>
    private void Board(string vehicleId, string entityId, string channel)
    {
        if (Chance(0.45))
        {
            _log.VehicleRetrieval(_now, entityId, NewEntityId(), _options.Handle, Draw(Fixtures.Hangars));
            Wait(40, 120);
            _log.Notification(_now, "Hangar Request Completed: ", _notificationId++);
            Wait(15, 60);
        }

        // The quote closes on the next line: "...'.\n: " is how it is written.
        _log.SplitNotification(
            _now,
            $"You have joined channel '{channel} : {_options.Handle}'.",
            ": ",
            _notificationId++);

        _log.NoRouteLoaded(_now.AddSeconds(4), vehicleId, entityId);

        if (_party.Count > 0 && Chance(0.5))
        {
            Wait(5, 40);
            _log.SplitNotification(
                _now,
                "New Member Joined",
                $"{Draw(_party.ToArray())} has joined the channel '{channel} : {_options.Handle}'.: ",
                _notificationId++);
        }

        Wait(5, 20);
    }

    /// <summary>
    /// Everything worn and carried, as the burst a spawn writes.
    /// </summary>
    private void Equip()
    {
        var elapsed = 20 + _extra.NextDouble() * 10;

        foreach (var (port, itemClass) in Carried())
        {
            _log.Attachment(_now, _options.Handle, itemClass, NewEntityId(), port, elapsed: elapsed);
            elapsed += 0.000015;
        }
    }

    private IEnumerable<(string Port, string Class)> Carried()
    {
        foreach (var item in Fixtures.Kit)
            yield return item;

        foreach (var item in _armour)
            yield return item;

        yield return ("wep_stocked_2", _rifle.Gun);
        yield return ("magazine_attach", _rifle.Magazine);
        yield return ("optics_attach", _rifle.Optic);
        yield return ("magazine_attach_1", _rifle.Magazine);
        yield return ("magazine_attach_2", _rifle.Magazine);
    }

    /// <summary>
    /// A death: the corpse's items written in one millisecond, which is the
    /// only way current builds say it happened.
    /// </summary>
    /// <remarks>
    /// Never <c>&lt;Actor Death&gt;</c>, which the game stopped writing. The
    /// next arrival is where the body wakes, and the kit goes back on there.
    /// </remarks>
    private void Die()
    {
        foreach (var (port, itemClass) in Carried())
            _log.CorpseItem(_now, itemClass, port, NewEntityId(), NewGuid());

        _respawning = true;
    }

    /// <summary>The contract card taken on this leg, carried until it ends.</summary>
    private sealed record OpenContract(
        string MissionId,
        string Title,
        bool Hauling,
        string Step,
        ContractFate Fate);

    private enum ContractFate { Complete, Fail, Abandon, Open }

    /// <summary>
    /// Accepts a contract and decides now how it will end, so the ending can be
    /// written wherever in the leg it falls.
    /// </summary>
    /// <remarks>
    /// Most taken contracts complete, which is what the real logs show: 285
    /// completions against 73 abandonments and 6 failures. A few are left
    /// open, as a card still in the journal at logout is.
    /// </remarks>
    private void AcceptContract(string generator, string contract, string title)
    {
        var missionId = NewGuid();

        _log.ContractMarker(_now, missionId, generator, contract, NewGuid(), NewGuid());
        _log.Notification(
            _now.AddSeconds(1),
            $"Contract Accepted:  {title}: ",
            _notificationId++,
            missionId);

        var roll = _extra.NextDouble();
        var fate = roll switch
        {
            < 0.78 => ContractFate.Complete,
            < 0.84 => ContractFate.Fail,
            < 0.96 => ContractFate.Abandon,
            _ => ContractFate.Open
        };

        var hauling = contract.Contains("Haul", StringComparison.Ordinal);

        // Hauling steps are named pickup_/dropoff_ around a GUID the contract
        // archetype shares across every mission of that kind; other contracts'
        // steps are bare GUIDs.
        var step = hauling ? StableGuid(contract) : NewGuid();

        _contract = new OpenContract(missionId, title, hauling, step, fate);

        if (hauling)
            Objective($"pickup_{step}_0", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        else
            Objective(step, "MISSION_OBJECTIVE_STATE_INPROGRESS");
    }

    /// <summary>The leg's middle: cargo on board, or the card abandoned.</summary>
    private void ProgressContract()
    {
        if (_contract is not { } open)
            return;

        if (open.Fate == ContractFate.Abandon && Chance(0.5))
        {
            Abandon(open);
            return;
        }

        if (open.Hauling)
        {
            Wait(60, 240);
            Objective($"pickup_{open.Step}_0", "MISSION_OBJECTIVE_STATE_COMPLETED");
            Objective($"dropoff_{open.Step}_1", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        }
    }

    /// <summary>The leg's end: delivered, failed, or walked away from.</summary>
    private void FinishContract()
    {
        if (_contract is not { } open)
            return;

        switch (open.Fate)
        {
            case ContractFate.Complete:
                Wait(20, 120);
                if (open.Hauling)
                    Objective($"dropoff_{open.Step}_1", "MISSION_OBJECTIVE_STATE_COMPLETED");
                else
                    Objective(open.Step, "MISSION_OBJECTIVE_STATE_COMPLETED");

                _log.MissionEnded(_now.AddMilliseconds(20), open.MissionId,
                    "MISSION_STATE_COMPLETED", "Complete", "Mission Ended", _options.Handle, _options.Geid);
                _log.Notification(_now.AddMilliseconds(24), $"Contract Complete: {open.Title}: ",
                    _notificationId++, open.MissionId);

                // Only hauls ever state a payout, and not all of them.
                if (open.Hauling && Chance(0.35))
                {
                    _log.Notification(_now.AddMilliseconds(330),
                        $"Awarded {Draw(Fixtures.Awards)} aUEC: ", _notificationId++);
                }

                Wait(10, 20);
                _contract = null;
                break;

            case ContractFate.Fail:
                Wait(20, 120);
                _log.MissionEnded(_now, open.MissionId,
                    "MISSION_STATE_FAILED", "Fail", "Mission Ended", _options.Handle, _options.Geid);
                _log.Notification(_now.AddMilliseconds(4), $"Contract Failed: {open.Title}: ",
                    _notificationId++, open.MissionId);
                Wait(10, 20);
                _contract = null;
                break;

            case ContractFate.Abandon:
                Abandon(open);
                break;

            default:
                // Left in the journal: the next contract taken replaces it as
                // the one being followed, and this one never says it ended.
                _contract = null;
                break;
        }
    }

    private void Abandon(OpenContract open)
    {
        Wait(10, 60);
        _log.EndMission(_now, open.MissionId, "Abandon", "Player left", _options.Handle, _options.Geid);
        Wait(5, 15);
        _contract = null;
    }

    private void Objective(string objectiveId, string state)
    {
        if (_contract is not { } open)
            return;

        _log.MissionObjective(_now, open.MissionId, objectiveId, state,
            flags: open.Hauling ? "ShowInLog|" : "ShowInLog|RespectInheritedVisibility|");
    }

    /// <summary>A kiosk purchase, where this stop has a shop the real logs bought from.</summary>
    private void Shop()
    {
        if (!Fixtures.Shops.TryGetValue(_currentLocation, out var offers) || !Chance(0.35))
            return;

        var (shop, item, total, quantity) = Draw(offers);
        var shopId = NewEntityId();
        var kioskId = NewEntityId();

        Wait(30, 120);
        _log.ShopRequest(_now, _options.Geid, shop, shopId, kioskId, total, item, quantity, NewGuid());
        _log.ShopResponse(_now.AddMilliseconds(_extra.Next(250, 700)), shop, kioskId, "Success",
            kioskState: "BuyRequestProcessing", geid: _options.Geid, shopId: shopId);
        Wait(5, 20);
    }

    /// <summary>
    /// Opens a party: a few placeholder members coming online, and sometimes
    /// the lead changing hands.
    /// </summary>
    private void FormParty()
    {
        var size = _extra.Next(1, 4);
        var pool = Fixtures.Crew.OrderBy(_ => _extra.Next()).Take(size);

        foreach (var member in pool)
        {
            Wait(10, 90);
            _party.Add(member);
            PartyNote("Party", $"{member} connected.: ");
        }

        if (Chance(0.4))
        {
            Wait(5, 30);
            PartyNote("New Party Leader", $"{Draw(_party.ToArray())} is now party leader.: ");
        }
    }

    /// <summary>A member dropping, or coming back from having dropped.</summary>
    private void PartyChatter()
    {
        if (_party.Count == 0)
            return;

        if (_droppedMember is { } back)
        {
            Wait(10, 60);
            PartyNote("Party", $"{back} connected.: ");
            _droppedMember = null;
        }
        else if (Chance(0.2))
        {
            Wait(10, 60);
            _droppedMember = Draw(_party.ToArray());
            PartyNote("Party", $"{_droppedMember} disconnected.: ");
        }
    }

    /// <summary>
    /// A party toast: the title on one line, the body on the next, the way
    /// every one of them is written.
    /// </summary>
    private void PartyNote(string title, string body) =>
        _log.SplitNotification(_now, title, body, _notificationId++);

    /// <summary>One kiosk trade at wherever the pilot is standing.</summary>
    /// <remarks>
    /// The log never says where a trade happened - every cargo terminal reports
    /// the same shop id - so this deliberately writes no location of its own.
    /// Recovering the place from the last arrival is the app's job, and leaving
    /// the line bare is what keeps that path honest.
    /// </remarks>
    private void RunTrade()
    {
        var (resource, basePrice) = Pick(Fixtures.Commodities);
        var selling = _random.NextDouble() < 0.65;

        // Whole boxes, as the kiosk deals in.
        var quantity = _random.Next(1, 21) * 16;

        // Buying costs less per SCU than selling pays, or there would be no
        // trade to plan; the jitter stops every visit reading the same price.
        var unit = basePrice
            * PriceFactor(_currentLocation, resource)
            * (selling ? 1.0 : 0.78)
            * (0.96 + _random.NextDouble() * 0.08);

        _log.CommodityTrade(
            _now,
            _options.Geid,
            Math.Round((decimal)(unit * quantity), 2),
            quantity,
            resource,
            selling,
            _random.NextDouble() < 0.5 ? "Location" : "ResourceContainer");

        Noise(_random.Next(2, 6));
    }

    /// <summary>
    /// How good one place is for one commodity, steady across sessions.
    /// </summary>
    /// <remarks>
    /// Rolled from the ids rather than stored, so it survives a reseed: a map
    /// whose best terminal moved on every run would be untestable.
    /// </remarks>
    private static double PriceFactor(string place, string resource) =>
        0.78 + Hash($"{place}|{resource}") % 45 / 100.0;

    private static uint Hash(string text)
    {
        var hash = 17u;

        foreach (var c in text)
            hash = unchecked(hash * 31 + c);

        return hash;
    }

    /// <summary>A GUID that is the same every time for the same text.</summary>
    private static string StableGuid(string text)
    {
        var bytes = new byte[16];
        new Random((int)Hash(text)).NextBytes(bytes);
        return new Guid(bytes).ToString();
    }

    private void RunCombat()
    {
        var kills = _random.Next(1, 4);

        for (var i = 0; i < kills; i++)
        {
            Advance(10, 60);

            if (_random.NextDouble() < 0.75)
            {
                // The player scores a kill.
                _log.ActorDeath(_now, Pick(Fixtures.Npcs), _options.Handle,
                    Pick(Fixtures.Weapons), _random.NextDouble() < 0.5 ? "Bullet" : "Combat", "Stanton_Yela");

                if (_random.NextDouble() < 0.5)
                {
                    _log.VehicleDestruction(_now.AddMilliseconds(150),
                        $"{Pick(Fixtures.Ships).Prefix}_Paladin_6763231335005",
                        Pick(Fixtures.Npcs), _options.Handle, 0, _random.Next(1, 3), "Combat");
                }
            }
            else
            {
                // The player is on the receiving end.
                _log.ActorDeath(_now, _options.Handle, Pick(Fixtures.Npcs),
                    Pick(Fixtures.Weapons), "Bullet", "Stanton_Yela");
            }
        }
    }

    private void VisitLocation(string locationId)
    {
        _currentLocation = locationId;
        _log.LocationInventory(_now, _options.Handle, locationId);

        // Real logs repeat these constantly, and shadow some with SPAM tags.
        _log.LocationInventory(_now.AddSeconds(3), _options.Handle, locationId);
        _log.SpamDuplicate(_now.AddSeconds(4), _options.Handle, locationId);

        if (_random.NextDouble() < 0.15)
            _log.LocationNoInventory(_now.AddSeconds(5), _options.Handle);

        Noise(_random.Next(3, 8));

        if (Chance(_options.StashChance))
            BrowseStash(locationId);
    }

    /// <summary>
    /// A look at the local storage: the query that names its scope, then the
    /// items on the page, all within a few seconds of arriving.
    /// </summary>
    /// <remarks>
    /// What a place holds is rolled from its id, not the session, so the same
    /// station shows the same shelf every visit - a stash that reshuffled
    /// nightly would never let the Stash page settle. Wikelo's wants sit on a
    /// few of those shelves so his trades have something to count.
    /// </remarks>
    private void BrowseStash(string locationId)
    {
        var key = Hash($"stash|{locationId}").ToString();
        var shelf = new Random((int)Hash($"shelf|{locationId}"));

        var held = Fixtures.StashItems
            .OrderBy(_ => shelf.Next())
            .Take(shelf.Next(3, 8))
            .ToList();

        if (shelf.NextDouble() < 0.5)
            held.AddRange(Fixtures.WikeloItems.OrderBy(_ => shelf.Next()).Take(shelf.Next(1, 4)));

        var at = _now.AddSeconds(6);
        _log.InventoryQuery(at, _options.Geid, "Location", key, _inventoryRequest++);

        // One page's worth: most of the shelf, not always all of it.
        foreach (var item in held.Where(_ => Chance(0.8)))
        {
            at = at.AddMilliseconds(_extra.Next(3, 40));
            _log.InventoryItem(at, _options.Geid, "Location", key, item, Rank());
        }

        Wait(10, 45);
    }

    /// <summary>A sort key shaped like the game's: "amr" and a run of letters.</summary>
    private string Rank()
    {
        var letters = new char[_extra.Next(10, 14)];

        for (var i = 0; i < letters.Length; i++)
            letters[i] = (char)('a' + _extra.Next(26));

        return "amr" + new string(letters);
    }
}

/// <summary>Knobs for a generated session.</summary>
internal sealed record SimOptions
{
    public string Handle { get; init; } = "testpilot";
    public string Geid { get; init; } = "100000000042";

    /// <summary>The build every header and shard name carries.</summary>
    public GameBuild Build { get; init; } = GameBuild.Default;

    /// <summary>Trips per session.</summary>
    public int Legs { get; init; } = 6;

    public int MenuSeconds { get; init; } = 300;
    public int FlightSeconds { get; init; } = 600;
    public double IncapacitationChance { get; init; } = 0.2;

    /// <summary>
    /// How often an incapacitation ends in death. Low: the real logs hold
    /// far more toasts saying the pilot went down than corpses.
    /// </summary>
    public double DeathChance { get; init; } = 0.08;

    /// <summary>Evenings flown in a party.</summary>
    public double PartyChance { get; init; } = 0.25;

    /// <summary>Arrivals at which the local storage is opened.</summary>
    public double StashChance { get; init; } = 0.35;

    /// <summary>
    /// Emit combat events. Off by default because SC 4.9 and 4.10 do not produce them;
    /// turning it on is the only way to see the dormant parser light up.
    /// </summary>
    public bool Combat { get; init; }
}
