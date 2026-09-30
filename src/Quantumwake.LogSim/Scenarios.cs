namespace Quantumwake.LogSim;

/// <summary>A reproducible log story and the facts it is expected to produce.</summary>
/// <param name="StaysInGame">
/// True for a story that ends with the pilot still in the world - open
/// contracts are dropped at logout, so a scenario about them cannot log out.
/// </param>
public sealed record ScenarioDefinition(
    string Name,
    string Description,
    IReadOnlyList<string> ExpectedFacts,
    bool StaysInGame = false);

/// <summary>Named scenarios intended for parser, API, and UI regression testing.</summary>
public static class ScenarioCatalogue
{
    public static IReadOnlyList<ScenarioDefinition> All { get; } =
    [
        new("cargo-run",
            "Buy cargo at Port Tressler, fly to New Babbage, and sell it.",
            ["2 cargo requests", "16 SCU bought for 9,600 aUEC", "16 SCU sold for 12,480 aUEC", "1 ship sortie"]),
        new("multi-stop-trader",
            "Trade two commodities across Port Tressler, New Babbage, and Area18.",
            ["4 cargo requests", "2 commodities", "96 SCU bought and sold", "2 quantum jumps"]),
        new("spending",
            "Complete one equipment purchase and reject another.",
            ["1 confirmed purchase", "2,400 aUEC confirmed spend", "failed purchase excluded"]),
        new("purchase-pairing",
            "Ignore a wrong-kiosk answer and an intermediate state before confirmation.",
            ["1 confirmed purchase", "4 items for 2,080 aUEC", "wrong kiosk excluded"]),
        new("medical-respawn",
            "Become incapacitated at Port Tressler and wake at New Babbage.",
            ["1 incapacitation", "1 inferred respawn", "1 after-death medical-bed visit"]),
        new("medical-kinds",
            "Distinguish a login bed, post-casualty treatment, and an ordinary heal.",
            ["3 medical-bed visits", "wake, after-death, and heal classifications"]),
        new("death-recovery",
            "Group a corpse-item burst into one death and infer the recovery location.",
            ["3 corpse items", "1 death", "1 inferred death respawn"]),
        new("revived-in-place",
            "Become incapacitated and reappear at the same location without inventing a respawn.",
            ["1 incapacitation", "0 inferred respawns"]),
        new("crew-flight",
            "Receive party changes, fly together, and see one member disconnect.",
            ["4 party notes", "B-Kon becomes leader", "1 ship sortie", "1 quantum jump"]),
        new("party-lifecycle",
            "Connect, change leader, disconnect, reconnect, and disband a party.",
            ["5 party notes", "1 disband", "matchmaking chatter excluded"]),
        new("contract-complete",
            "Accept a two-step Covalex contract, complete it, and receive a blueprint.",
            ["1 completed contract", "2 of 2 visible steps complete", "1 blueprint received"]),
        new("contract-abandoned",
            "Accept a contract, begin its visible objective, and withdraw it.",
            ["1 abandoned contract", "1 visible step", "0 completed steps"]),
        new("hauling-run",
            "Accept three Red Wind hauls with the text mod's titles, load one pickup, and leave them open.",
            ["3 open hauling contracts", "titles name Stanton Gateway, Ruin Station and Ruin Station > Checkmate", "1 of 3 pickups done on the Aluminum haul"],
            StaysInGame: true),
        new("loadout-swap",
            "Equip a full kit, refresh the undersuit, and swap a held weapon.",
            ["11 attachment records", "8 current equipment cards", "repeat armour collapsed"]),
        new("stash-browse",
            "Browse two location inventories plus the player's personal inventory.",
            ["2 stashed items at 2 locations", "3 distinct first-seen pickups"]),
        new("fleet-growth",
            "Report changing entitlement totals during one session.",
            ["largest fleet observation is 14 vehicles"]),
        new("ship-retrieval",
            "Retrieve an unnamed ship, identify it later, and suppress the duplicate spawn line.",
            ["1 retrieved Freelancer MAX", "1 credited sortie"]),
        new("location-resolution",
            "Resolve a generic Rest Stop quantum target from the actual arrival.",
            ["1 quantum jump", "destination corrected to microTech L1"]),
        new("unexpected-disconnect",
            "Record a remote timeout beside player-requested and routine teardown.",
            ["2 recorded disconnects", "routine Nub teardown excluded"]),
        new("combat",
            "Exercise the archived kill, death, and vehicle-destruction formats.",
            ["1 player kill", "1 player death", "1 destroyed vehicle timeline entry"]),
        new("all",
            "Run every focused scenario in one deterministic session.",
            [
                "6 cargo requests and 2 confirmed item purchases",
                "3 incapacitations, 3 inferred respawns, and 4 medical-bed visits",
                "9 party notes and 5 ship sorties",
                "1 completed and 1 abandoned contract",
                "loadout, stash, fleet, retrieval, location-resolution, and 2 disconnect cases",
                "1 player kill, 2 player deaths, and 1 destroyed vehicle"
            ])
    ];

    public static ScenarioDefinition? Find(string name) =>
        All.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>Writes a named scenario using only log shapes understood from real captures.</summary>
public static class ScenarioRunner
{
    private static readonly string[] FocusedScenarioNames =
    [
        // The login-bed classification only exists at the start of a session.
        "medical-kinds",
        "multi-stop-trader",
        "cargo-run",
        "spending",
        "purchase-pairing",
        "medical-respawn",
        "death-recovery",
        "revived-in-place",
        "crew-flight",
        "party-lifecycle",
        "contract-complete",
        "contract-abandoned",
        "hauling-run",
        "loadout-swap",
        "stash-browse",
        "fleet-growth",
        "ship-retrieval",
        "location-resolution",
        "unexpected-disconnect",
        "combat"
    ];

    public static void Run(
        LogWriter log,
        ScenarioDefinition scenario,
        DateTimeOffset start,
        string handle = "testpilot",
        string geid = "100000000042",
        GameBuild? build = null,
        bool stayInGame = false)
    {
        var context = new ScenarioContext(log, start, handle, geid, build ?? GameBuild.Default);
        context.Begin();

        if (scenario.Name.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var name in FocusedScenarioNames)
                RunBody(name, context);
        }
        else
        {
            RunBody(scenario.Name, context);
        }

        if (stayInGame)
            Settle(context);
        else if (!scenario.StaysInGame)
            context.End();
    }

    /// <summary>
    /// Leaves the pilot in the world rather than logging out: back on a shard
    /// if a disconnect took them off one, standing at Port Tressler, in a
    /// retrieved ship, with company and any open contracts still in the
    /// journal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For recording the Now page, which with a finished log shows a game
    /// that has closed. Everything a live session carries has to be in the
    /// file: the shard from the last join, the location from the last
    /// inventory request, the ship from a retrieval whose id a navigation line
    /// later names, the clock from the first line.
    /// </para>
    /// <para>
    /// No route is plotted. A ship on the pad gets its name from the navigation
    /// computer's "No Route loaded!" grumble - quantum lines are events of
    /// their own and do not name it - and plotting a route would put the page
    /// in transit rather than at the station.
    /// </para>
    /// </remarks>
    private static void Settle(ScenarioContext c)
    {
        if (c.OffShard)
        {
            // A timeout drops the client to the menu; matchmaking puts it
            // back on a different server.
            c.Log.Context(c.Now, "SC_Frontend", ScenarioContext.SessionId);
            c.Log.LoadingScreen(c.Now.AddSeconds(1), "Frontend_Main", "SC_Frontend", 2.84);
            c.Advance(40);
            c.Log.JoinShard(c.Now, $"pub_use1b_{c.Build.Number}_117");
            c.Log.Context(c.Now, "SC_Default", ScenarioContext.SessionId);
            c.Log.LoadingScreen(c.Now.AddSeconds(1), "PU_Megamap", "SC_Default", 19.62);
            c.Advance(25);
            c.Log.Spawned(c.Now);
            c.OffShard = false;
        }

        c.Location("RR_MIC_LEO");
        c.Advance(30);

        const string entity = "700000009101";
        const string vehicle = "DRAK_Clipper_700000009101";

        c.Log.VehicleRetrieval(c.Now, entity, "700000009102", c.Handle, "Large Hangar");
        c.Advance(70);
        c.Notify("Hangar Request Completed: ");
        c.Log.SplitNotification(c.Now, $"You have joined channel 'Drake Clipper : {c.Handle}'.", ": ",
            c.NextNotificationId());
        c.Advance(10);

        c.Log.SplitNotification(c.Now, "Party", "Pilot-Two connected.: ", c.NextNotificationId());
        c.Advance(10);
        c.Log.SplitNotification(c.Now, "New Member Joined",
            $"Pilot-Two has joined the channel 'Drake Clipper : {c.Handle}'.: ", c.NextNotificationId());
        c.Advance(20);

        // Sat in the seat with nothing plotted; the navigation computer says
        // so, naming the ship as it does.
        c.Log.NoRouteLoaded(c.Now, vehicle, entity);
        c.Advance(20);
        c.Log.NoRouteLoaded(c.Now, vehicle, entity);
        c.Advance(5);
    }

    private static void RunBody(string name, ScenarioContext context)
    {
        switch (name.ToLowerInvariant())
        {
            case "cargo-run":
                CargoRun(context);
                break;
            case "multi-stop-trader":
                MultiStopTrader(context);
                break;
            case "spending":
                Spending(context);
                break;
            case "purchase-pairing":
                PurchasePairing(context);
                break;
            case "medical-respawn":
                MedicalRespawn(context);
                break;
            case "medical-kinds":
                MedicalKinds(context);
                break;
            case "death-recovery":
                DeathRecovery(context);
                break;
            case "revived-in-place":
                RevivedInPlace(context);
                break;
            case "crew-flight":
                CrewFlight(context);
                break;
            case "party-lifecycle":
                PartyLifecycle(context);
                break;
            case "contract-complete":
                ContractComplete(context);
                break;
            case "contract-abandoned":
                ContractAbandoned(context);
                break;
            case "hauling-run":
                HaulingRun(context);
                break;
            case "loadout-swap":
                LoadoutSwap(context);
                break;
            case "stash-browse":
                StashBrowse(context);
                break;
            case "fleet-growth":
                FleetGrowth(context);
                break;
            case "ship-retrieval":
                ShipRetrieval(context);
                break;
            case "location-resolution":
                LocationResolution(context);
                break;
            case "unexpected-disconnect":
                UnexpectedDisconnect(context);
                break;
            case "combat":
                Combat(context);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown log scenario.");
        }
    }

    private static void CargoRun(ScenarioContext c)
    {
        c.Location("RR_MIC_LEO");
        c.Advance(20);
        c.Log.CommodityTrade(c.Now, c.Geid, 9_600m, 16,
            "bde5a2c8-2ef4-46ac-9403-2fcb79e4016c", false, "Location");
        c.Advance(30);
        c.Flight("Stanton4_NewBabbage", "Stanton4_NewBabbage", "Port Tressler");
        c.Advance(20);
        c.Log.CommodityTrade(c.Now, c.Geid, 12_480m, 16,
            "bde5a2c8-2ef4-46ac-9403-2fcb79e4016c", true, "ResourceContainer");
        c.Advance(15);
    }

    private static void MultiStopTrader(ScenarioContext c)
    {
        c.Location("RR_MIC_LEO");
        c.Log.CommodityTrade(c.Now, c.Geid, 18_400m, 32,
            "7f4599b0-a2b2-4178-8c7e-13292054ab20", false, "Location");
        c.Advance(20);
        c.Flight("Stanton4_NewBabbage", "Stanton4_NewBabbage", "Port Tressler");
        c.Log.CommodityTrade(c.Now, c.Geid, 23_680m, 32,
            "7f4599b0-a2b2-4178-8c7e-13292054ab20", true, "ResourceContainer");
        c.Advance(15);
        c.Log.CommodityTrade(c.Now, c.Geid, 4_480m, 64,
            "accacd33-3a1a-4ec7-8b4a-14b9f028047c", false, "Location");
        c.Advance(20);
        c.Flight("Area18_City_objectContainer", "Stanton3_Area18", "New Babbage");
        c.Log.CommodityTrade(c.Now, c.Geid, 6_400m, 64,
            "accacd33-3a1a-4ec7-8b4a-14b9f028047c", true, "ResourceContainer");
        c.Advance(15);
    }

    private static void Spending(ScenarioContext c)
    {
        c.Location("Stanton4_NewBabbage");
        c.Advance(10);
        c.Log.ShopRequest(c.Now, c.Geid, "SCShop_CenterMass_NewBabbage",
            "752023944375", "752023944372", 2_400m, "behr_rifle_ballistic_01", 1);
        c.Advance(2);
        c.Log.ShopResponse(c.Now, "SCShop_CenterMass_NewBabbage", "752023944372", "Success");
        c.Advance(10);
        c.Log.ShopRequest(c.Now, c.Geid, "SCShop_OmegaPro_NewBabbage",
            "752023944376", "752023944373", 475_200m, "POWR_JUST_S02_Genoa_SCItem", 1);
        c.Advance(2);
        c.Log.ShopResponse(c.Now, "SCShop_OmegaPro_NewBabbage", "752023944373", "InsufficientFunds");
        c.Advance(10);
    }

    private static void PurchasePairing(ScenarioContext c)
    {
        const string shop = "SCShop_CubbyBlast_Area18";
        const string kiosk = "760000000001";

        c.Location("Stanton3_Area18");
        c.Log.ShopRequest(c.Now, c.Geid, shop, "760000000000", kiosk,
            2_080m, "behr_magazine_ballistic_01", 4);
        c.Advance(1);
        c.Log.ShopResponse(c.Now, shop, "760000009999", "Success");
        c.Advance(1);
        c.Log.ShopResponse(c.Now, shop, kiosk, "Processing", kioskState: "BuyRequestProcessing");
        c.Advance(1);
        c.Log.ShopResponse(c.Now, shop, kiosk, "Success");
        c.Advance(10);
    }

    private static void MedicalRespawn(ScenarioContext c)
    {
        c.Location("RR_MIC_LEO");
        c.Advance(10);
        c.Log.Incapacitated(c.Now, c.NextNotificationId());
        c.Advance(30);
        c.Location("Stanton4_NewBabbage");
        c.Advance(5);
        c.Notify("Medical Bed: The bed has restored your health and reset your BDL.");
    }

    private static void MedicalKinds(ScenarioContext c)
    {
        c.Notify("Medical Bed: The bed has restored your health and reset your BDL.");
        c.Location("Stanton4_NewBabbage");
        c.Log.Incapacitated(c.Now, c.NextNotificationId());
        c.Advance(30);
        c.Location("RR_MIC_L1");
        c.Notify("Medical Bed: The bed has restored your health and reset your BDL.");
        c.Advance(16 * 60);
        c.Location("Stanton3_Area18");
        c.Notify("Medical Bed: The bed has restored your health and reset your BDL.");
    }

    private static void DeathRecovery(ScenarioContext c)
    {
        c.Location("GrimHEX");
        c.Log.CorpseItem(c.Now, "behr_rifle_ballistic_01", "Body_ItemPort");
        c.Log.CorpseItem(c.Now.AddMilliseconds(100), "behr_magazine_ballistic_01", "Backpack_ItemPort");
        c.Log.CorpseItem(c.Now.AddMilliseconds(200), "medpen_hemozal", "Armor_ItemPort");
        c.Advance(30);
        c.Location("Stanton2_Orison");
        c.Advance(10);
    }

    private static void RevivedInPlace(ScenarioContext c)
    {
        c.Location("RR_MIC_LEO");
        c.Log.Incapacitated(c.Now, c.NextNotificationId());
        c.Advance(30);
        c.Location("RR_MIC_LEO");

        // The inference window expires before a later journey; otherwise a
        // subsequent arrival could legitimately look like a respawn.
        c.Advance(10 * 60 + 1);
    }

    private static void CrewFlight(ScenarioContext c)
    {
        c.Notify("Party B-Kon connected.:");
        c.Notify("Party nova_rime connected.:");
        c.Notify("New Party Leader B-Kon is now party leader.:");
        c.Flight("LOC_RR_S4_L1", "RR_MIC_L1", "New Babbage");
        c.Notify("Party nova_rime disconnected.:");
    }

    private static void PartyLifecycle(ScenarioContext c)
    {
        c.Notify("Party Pilot-One connected.:");
        c.Notify("New Party Leader Pilot-One is now party leader.:");
        c.Notify("Party Launch Initiated by party leader Pilot-One.:");
        c.Notify("Party Pilot-One disconnected.:");
        c.Notify("Party Pilot-One connected.:");
        c.Notify("Party Disbanded The party has been disbanded.:");
    }

    private static void ContractComplete(ScenarioContext c)
    {
        const string mission = "11111111-2222-3333-4444-555555555555";

        c.Log.ContractMarker(c.Now, mission, "Covalex_RecoverCargo",
            "Covalex_Stanton_VeryHard_RecoverCargo",
            "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            "99999999-8888-7777-6666-555555555555");
        c.Advance(1);
        c.Notify("Contract Accepted: Bulk Covalex Shipment Needs Recovering:", mission);
        c.Log.MissionObjective(c.Now, mission, "pickup_crate_0", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Advance(20);
        c.Log.MissionObjective(c.Now, mission, "pickup_crate_0", "MISSION_OBJECTIVE_STATE_COMPLETED");
        c.Advance(5);
        c.Log.MissionObjective(c.Now, mission, "deliver_crate_0", "MISSION_OBJECTIVE_STATE_COMPLETED");
        c.Advance(2);

        // The step above is not what finishes a contract - the real game says
        // so separately, and this install has to say it too or the scenario
        // describes a hauling run that ends the moment it is loaded.
        c.Log.MissionEnded(c.Now, mission, "MISSION_STATE_COMPLETED", "Complete", handle: c.Handle, geid: c.Geid);
        c.Advance(5);
        c.Notify("Received Blueprint: Omnisky IX");
    }

    /// <summary>
    /// What a hauler's session looks like with three cards open: the toast
    /// and the marker 20 ms apart, sharing the mission id, and a journal
    /// that names its steps pickup_ and dropoff_. Left open on purpose - the
    /// plan is for contracts still to fly.
    /// </summary>
    private static void HaulingRun(ScenarioContext c)
    {
        const string aluminum = "31111111-2222-3333-4444-555555555555";
        const string copper = "32111111-2222-3333-4444-555555555555";
        const string carbon = "33111111-2222-3333-4444-555555555555";

        c.Log.ContractMarker(c.Now, aluminum, "RedWind_CargoHauling",
            "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle",
            "caaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "79999999-8888-7777-6666-555555555555");
        c.Notify("Contract Accepted:  Junior | Stellar Small Haul | to Stanton Gateway <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>: ", aluminum);
        c.Advance(12);

        c.Log.ContractMarker(c.Now, copper, "RedWind_CargoHauling",
            "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Copper_CargoHauling_Multi2ToSingle",
            "cbaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "78999999-8888-7777-6666-555555555555");
        c.Notify("Contract Accepted:  Junior | Stellar Small Haul | to Ruin Station <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>: ", copper);
        c.Advance(9);

        c.Log.ContractMarker(c.Now, carbon, "RedWind_CargoHauling",
            "RedWind_Pyro_SupplyGrade_RegionA_CFP_StationToTradepost_Carbon_CargoHauling_AtoB_Intro",
            "ccaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "77999999-8888-7777-6666-555555555555");
        c.Notify("Contract Accepted:  Rookie | <EM3>DIRECT</EM3> Small Haul | Ruin Station > Checkmate <EM4>[BP]*</EM4>: ", carbon);
        c.Advance(600);

        c.Log.MissionObjective(c.Now, aluminum, "pickup_c196979f-d685-4dd9-bf23-880fa8bfb6e0_0", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Advance(240);
        c.Log.MissionObjective(c.Now, aluminum, "pickup_c196979f-d685-4dd9-bf23-880fa8bfb6e0_0", "MISSION_OBJECTIVE_STATE_COMPLETED");
        c.Log.MissionObjective(c.Now, aluminum, "pickup_c196979f-d685-4dd9-bf23-880fa8bfb6e0_1", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Log.MissionObjective(c.Now, aluminum, "pickup_c196979f-d685-4dd9-bf23-880fa8bfb6e0_2", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Log.MissionObjective(c.Now, aluminum, "dropoff_c196979f-d685-4dd9-bf23-880fa8bfb6e0_0", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Advance(30);
    }

    private static void ContractAbandoned(ScenarioContext c)
    {
        const string mission = "21111111-2222-3333-4444-555555555555";

        c.Log.ContractMarker(c.Now, mission, "Ling_RecoverCargo",
            "Ling_Stanton_VeryEasy_RecoverCargo",
            "baaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            "89999999-8888-7777-6666-555555555555");
        c.Advance(1);
        c.Notify("Contract Accepted: Cargo Retrieval Required:", mission);
        c.Log.MissionObjective(c.Now, mission, "recover_crate_0", "MISSION_OBJECTIVE_STATE_INPROGRESS");
        c.Advance(20);
        c.Log.MissionObjective(c.Now, mission, "recover_crate_0", "MISSION_OBJECTIVE_STATE_WITHDRAWN");
        c.Advance(2);
        c.Log.MissionEnded(c.Now, mission, "MISSION_STATE_WITHDRAWN", "Abandon", "Player left", c.Handle, c.Geid);
        c.Advance(10);
    }

    private static void LoadoutSwap(ScenarioContext c)
    {
        c.Log.Attachment(c.Now, c.Handle, "rsi_odyssey_undersuit_01_01_01", "200000000219", "Armor_Undersuit");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "rsi_odyssey_undersuit_01_01_01", "200000000219", "Armor_Undersuit");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "rsi_odyssey_helmet_01", "200000000222", "helmet_attach");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "rsi_odyssey_armor_core_01", "200000000223", "armor_core");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "rsi_backpack_01", "200000000224", "backpack_attach");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "frag_grenade", "200000000225", "grenade_attach_1");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "frag_grenade", "200000000226", "grenade_attach_2");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "medpen_hemozal", "200000000227", "medpen_attach");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "multitool_utility", "200000000228", "utility_tool_attach");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "behr_rifle_ballistic_01", "200000000229", "weapon_attach_back");
        c.Advance(2);
        c.Log.Attachment(c.Now, c.Handle, "behr_rifle_ballistic_01", "200000000220", "weapon_attach_hand_right");
        c.Advance(20);
        c.Log.Attachment(c.Now, c.Handle, "gmni_sniper_ballistic_01", "200000000221", "weapon_attach_hand_right");
        c.Advance(10);
    }

    private static void StashBrowse(ScenarioContext c)
    {
        c.Location("RR_MIC_LEO");
        c.Log.InventoryQuery(c.Now, c.Geid, "Location", "3531251586");
        c.Log.InventoryItem(c.Now.AddSeconds(1), c.Geid, "Location", "3531251586", "gmni_sniper_ballistic_01");
        c.Log.InventoryItem(c.Now.AddSeconds(2), c.Geid, "Location", "3531251586", "gmni_sniper_ballistic_01");
        c.Log.InventoryItem(c.Now.AddSeconds(3), c.Geid, "Player", "100000000042", "medpen_hemozal");
        c.Advance(10);
        c.Location("Stanton4_NewBabbage");
        c.Log.InventoryQuery(c.Now, c.Geid, "Location", "3531251587");
        c.Log.InventoryItem(c.Now.AddSeconds(1), c.Geid, "Location", "3531251587", "behr_rifle_ballistic_01");
        c.Advance(10);
    }

    private static void FleetGrowth(ScenarioContext c)
    {
        c.Log.FleetQuery(c.Now, 12, 12);
        c.Advance(10);
        c.Log.FleetQuery(c.Now, 13, 14);
        c.Advance(10);
        c.Log.FleetQuery(c.Now, 12, 13);
        c.Advance(10);
    }

    private static void ShipRetrieval(ScenarioContext c)
    {
        const string entity = "700000009001";
        const string vehicle = "MISC_Freelancer_MAX_700000009001";

        c.Log.VehicleSpawn(c.Now, entity, "LandingArea_ShipElevator_HangarMediumFront_Rund");
        c.Log.VehicleSpawn(c.Now.AddMilliseconds(200), entity, "LandingArea_ShipElevator_HangarMediumFront_Rund");
        c.Advance(2);
        c.Log.VehicleIdentity(c.Now, vehicle, entity);
        c.Advance(10);
    }

    private static void LocationResolution(ScenarioContext c)
    {
        const string entity = "700000009002";
        const string vehicle = "RSI_Aurora_Mk2_700000009002";

        c.Log.RouteWithOrigin(c.Now, vehicle, entity, "Port Tressler", "ObjectContainer_RestStop");
        c.Advance(60);
        c.Location("RR_MIC_L1");
        c.Advance(10);
    }

    private static void UnexpectedDisconnect(ScenarioContext c)
    {
        c.Log.Disconnect(c.Now, "Connection timeout", "SC_Default", remote: true);
        c.OffShard = true;
        c.Advance(10);
    }

    private static void Combat(ScenarioContext c)
    {
        c.Log.ActorDeath(c.Now, "AI_CRIM_Gunner_Medium_02", c.Handle,
            "behr_lmg_ballistic_01", "Bullet", "Stanton_Yela", "41001", "41002");
        c.Advance(5);
        c.Log.VehicleDestruction(c.Now, "DRAK_Cutlass_Black", "AI_CRIM_Gunner_Medium_02",
            c.Handle, 0, 2, "Combat", "7100001");
        c.Advance(5);
        c.Log.ActorDeath(c.Now, c.Handle, "AI_CRIM_Gunner_Medium_02",
            "behr_lmg_ballistic_01", "Bullet", "Stanton_Yela", "41003", "41004");
        c.Advance(10);
    }

    private sealed class ScenarioContext
    {
        public const string SessionId = "01234567-89ab-cdef-0123-456789abcdef";
        private int _notificationId = 400;
        private int _flightId;

        public ScenarioContext(LogWriter log, DateTimeOffset start, string handle, string geid, GameBuild build)
        {
            Log = log;
            Now = start;
            Handle = handle;
            Geid = geid;
            Build = build;
        }

        public LogWriter Log { get; }
        public DateTimeOffset Now { get; private set; }
        public string Handle { get; }
        public string Geid { get; }
        public GameBuild Build { get; }

        /// <summary>True after a world-channel disconnect, until a join puts the client back.</summary>
        public bool OffShard { get; set; }

        public void Advance(int seconds) => Now = Now.AddSeconds(seconds);

        public int NextNotificationId() => _notificationId++;

        public void Begin()
        {
            Log.Header(Now, Build);
            Advance(1);
            Log.Character(Now, Handle, Geid);
            Log.Login(Now.AddMilliseconds(120), Handle);
            Advance(2);
            Log.Context(Now, "SC_Frontend", SessionId);
            Log.LoadingScreen(Now.AddSeconds(1), "Frontend_Main", "SC_Frontend", 3.44);
            Advance(10);
            Log.JoinShard(Now, $"pub_use1b_{Build.Number}_042");
            Log.Context(Now, "SC_Default", SessionId);
            Log.LoadingScreen(Now.AddSeconds(1), "PU_Megamap", "SC_Default", 21.30);
            Advance(25);
            Log.Spawned(Now);
            Location("RR_MIC_LEO");
        }

        public void End()
        {
            Log.Disconnect(Now, "Remote Disconnect - Player requested disconnect", "SC_Default");
            Advance(2);
            Log.Context(Now, "SC_Frontend", SessionId);
            Log.LoadingScreen(Now.AddSeconds(1), "Frontend_Main", "SC_Frontend", 2.10);
            Advance(5);
            Log.Disconnect(Now, "Nub destroyed", "SC_Frontend");
        }

        public void Location(string id)
        {
            Log.LocationInventory(Now, Handle, id);
            Log.LocationInventory(Now.AddMilliseconds(200), Handle, id);
            Log.SpamDuplicate(Now.AddMilliseconds(400), Handle, id);
            Advance(2);
        }

        public void Notify(string text, string? missionId = null)
        {
            Log.Notification(Now, text, NextNotificationId(), missionId);
            // Notification follow-ups reach 9.4 seconds; keep entries chronological.
            Advance(10);
        }

        public void Flight(string routeDestination, string arrival, string origin)
        {
            var suffix = (++_flightId).ToString("D3");
            var entity = $"700000000{suffix}";
            var vehicle = $"MISC_Freelancer_MAX_{entity}";

            Log.QuantumTarget(Now, vehicle, entity, routeDestination);
            Log.RouteWithOrigin(Now.AddSeconds(1), vehicle, entity, origin, routeDestination);
            Advance(60);
            Log.VehicleRelease(Now, Geid, vehicle, entity);
            Advance(5);
            Location(arrival);
        }
    }
}
