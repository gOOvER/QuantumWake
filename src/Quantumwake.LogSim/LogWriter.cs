using System.Globalization;
using System.Text;

namespace Quantumwake.LogSim;

/// <summary>
/// Emits lines in Star Citizen's exact Game.log format.
/// </summary>
/// <remarks>
/// <para>
/// Every template here is copied from real log lines captured on a 4.9.188.23497
/// install and documented in <c>docs/log-format-reference.md</c>. That fidelity
/// is the whole point: a generator that emits tidy, well-behaved lines would
/// prove nothing, because the parser's hard cases are all quirks.
/// </para>
/// <para>
/// The awkward shapes are reproduced deliberately:
/// notifications fire three to five times with differing <c>Action:</c> values,
/// some entries split across physical lines with the continuation carrying its
/// own timestamp, <c>[SPAM nnn]</c> duplicates shadow real lines, and route
/// calculation uses both of its two forms.
/// </para>
/// </remarks>
public sealed class LogWriter : IDisposable
{
    private readonly StreamWriter _writer;

    public LogWriter(string path, bool append = false)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var stream = new FileStream(
            path,
            append ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            // Match the game: readers must be able to follow while we write.
            FileShare.ReadWrite | FileShare.Delete);

        _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
    }

    /// <summary>Writes a raw line with a timestamp envelope.</summary>
    public void Line(DateTimeOffset at, string body) =>
        _writer.WriteLine($"<{Stamp(at)}> {body}");

    /// <summary>Writes a line with no envelope, for continuation fragments.</summary>
    public void Raw(string line) => _writer.WriteLine(line);

    private static string Stamp(DateTimeOffset at) =>
        at.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

    // ---------------- session header ----------------

    public void Header(DateTimeOffset at, GameBuild build) =>
        Header(at, build.Number, build.Version, build.BuiltOn);

    public void Header(DateTimeOffset at, string build, string version, string builtOn = "Jul 29 2026 15:21:13")
    {
        Line(at, $"BackupNameAttachment=\" Build({build}) {at:dd MMM yy} ({at:HH mm ss})\"  -- used by backup system");
        Line(at, $"Log started on {at.UtcDateTime:ddd MMM dd HH:mm:ss yyyy}");
        Line(at, $"Built on {builtOn}");
        Line(at, "Running 64 bit version");
        Line(at, @"Executable: C:\Program Files\Roberts Space Industries\StarCitizen\LIVE\Bin64\StarCitizen.exe");
        Line(at, $"FileVersion: {version}");
        Line(at, $"ProductVersion: {version}");
        Line(at, "Using Microsoft (tm) C++ Standard Library implementation");
        Line(at, "Host CPU: AMD Ryzen 7 9800X3D 8-Core Processor");
        Line(at, "Logical CPU Count: 16");
        Line(at, "[Trace] Environment:   PUB");
    }

    public void Login(DateTimeOffset at, string handle) =>
        Line(at, $"[Notice] <Legacy login response> [CIG-net] User Login Success - " +
                 $"Handle[{handle}] - Time[177332566] [Team_GameServices][Login]");

    public void Character(DateTimeOffset at, string handle, string geid) =>
        Line(at, $"[Notice] <AccountLoginCharacterStatus_Character> Character: " +
                 $"createdAt 1784476187540 - updatedAt 1786844282957 - geid {geid} - " +
                 $"accountId 50001 - name {handle} - state STATE_CURRENT [Team_GameServices][Login]");

    public void Context(DateTimeOffset at, string gameRules, string sessionId) =>
        Line(at, $"[Notice] <Context Establisher Done> establisher=\"Game\" runningTime=1.980013 " +
                 $"map=\"megamap\" gamerules=\"{gameRules}\" sessionId=\"{sessionId}\" " +
                 $"[Team_Network][Network][Replication][Loading][Persistence]");

    /// <summary>Loading screens carry no severity tag at all - a real parsing trap.</summary>
    public void LoadingScreen(DateTimeOffset at, string screen, string gameRules, double seconds) =>
        Line(at, $"Loading screen for {screen} : {gameRules} closed after " +
                 $"{seconds.ToString("F2", CultureInfo.InvariantCulture)} seconds");

    public void Spawned(DateTimeOffset at) =>
        Line(at, "[CSessionManager::OnClientSpawned] Spawned!");

    // ---------------- gameplay ----------------

    public void LocationInventory(DateTimeOffset at, string handle, string locationId) =>
        Line(at, $"[Notice] <RequestLocationInventory> Player[{handle}] requested inventory " +
                 $"for Location[{locationId}] [Team_CoreGameplayFeatures][Inventory]");

    /// <summary>The "no inventory here" variant, which is expected and not a failure.</summary>
    public void LocationNoInventory(DateTimeOffset at, string handle) =>
        Line(at, $"[Notice] <RequestLocationInventory> Player[{handle}] requested " +
                 $"Location[INVALID_LOCATION_ID] doesn't have inventory. [Team_CoreGameplayFeatures][Inventory]");

    /// <summary>A duplicate shadowed by a [SPAM nnn] tag; parsers must not double-count.</summary>
    public void SpamDuplicate(DateTimeOffset at, string handle, string locationId) =>
        Line(at, $"[SPAM 299][Notice] <RequestLocationInventory> Player[{handle}] requested " +
                 $"inventory for Location[{locationId}] [Team_CoreGameplayFeatures][Inventory]");

    public void VehicleRelease(DateTimeOffset at, string geid, string vehicleId, string entityId) =>
        Line(at, $"[Notice] <Vehicle Control Flow> CVehicleMovementBase::ClearDriver: " +
                 $"Local client node [{geid}] releasing control token for '{vehicleId}' " +
                 $"[{entityId}] [Team_CGP4][Vehicle]");

    /// <summary>Route form one: names both endpoints.</summary>
    public void RouteWithOrigin(DateTimeOffset at, string vehicleId, string entityId, string origin, string destination) =>
        Line(at, $"[Notice] <Calculate Route> [ItemNavigation][CL][35872] | NOT AUTH | " +
                 $"{vehicleId}[{entityId}]|CSCItemNavigation::CalculateRoute|" +
                 $"Projected Start Location is {origin} for route to destination {destination} " +
                 $"[Team_CGP4][QuantumTravel]");

    /// <summary>Route form two: destination only. Half of all routes use this shape.</summary>
    public void RouteDestinationOnly(DateTimeOffset at, string vehicleId, string entityId, string destination) =>
        Line(at, $"[Notice] <Calculate Route> [ItemNavigation][CL][35872] | NOT AUTH | " +
                 $"{vehicleId}[{entityId}]|CSCItemNavigation::CalculateRoute|" +
                 $"Successfully calculated route to {destination} [Team_CGP4][QuantumTravel]");

    public void QuantumTarget(DateTimeOffset at, string vehicleId, string entityId, string destination) =>
        Line(at, $"[Notice] <Player Selected Quantum Target - Local> [ItemNavigation][CL][35872] | " +
                 $"NOT AUTH | {vehicleId}[{entityId}]|CSCItemNavigation::OnPlayerSelectedQuantumTarget|" +
                 $"Player has selected point {destination} as their destination, routing locally " +
                 $"[Team_CGP4][QuantumTravel]");

    /// <summary>
    /// The navigation computer complaining that no route is plotted - written
    /// over and over while a pilot sits in a ship without one.
    /// </summary>
    /// <remarks>
    /// Chatter, but useful chatter: it is not an event in itself, so the
    /// parser reads the <c>RSI_Hermes_700000000001[700000000001]</c> in it as
    /// the ship's name, which is how a retrieved ship gets identified before
    /// it has flown anywhere.
    /// </remarks>
    public void NoRouteLoaded(DateTimeOffset at, string vehicleId, string entityId) =>
        Line(at, $"[Notice] <Failed to get starmap route data!> [ItemNavigation][CL][9028] | NOT AUTH | " +
                 $"{vehicleId}[{entityId}]|CSCItemNavigation::GetStarmapRouteSegmentData|No Route loaded! " +
                 "[Team_CGP4][QuantumTravel]");

    /// <summary>
    /// A cargo kiosk trade, in the shape the real client writes.
    /// </summary>
    /// <remarks>
    /// Two details matter for the parser. The commodity appears only as
    /// <c>resourceGUID</c> - there is no name anywhere on the line - and the box
    /// breakdown follows on an unstamped continuation line, so a reader that
    /// assumes one entry per line loses it.
    /// </remarks>
    public void CommodityTrade(
        DateTimeOffset at,
        string geid,
        decimal amount,
        int quantity,
        string resourceGuid,
        bool isSell,
        string mode)
    {
        var verb = isSell ? "Sell" : "Buy";
        var total = amount.ToString("F6", CultureInfo.InvariantCulture);

        // Invariant for the same reason as the total above, and it is easy to
        // lose here: the game writes a dot, the parser's quantity pattern only
        // accepts digits and dots, and a machine with a comma decimal separator
        // would emit "1600,000000" - which does not match, so every simulated
        // purchase would vanish. That is exactly the centi-SCU conversion these
        // scenarios exist to prove, so it would fail silently in the one place
        // built to catch it.
        var centi = (quantity * 100).ToString("F6", CultureInfo.InvariantCulture);

        // The live game uses different fields and units on each side. In
        // particular, buy quantity is centi-SCU; smoothing both into the sell
        // shape would leave the parser's hundredfold conversion untested.
        var transaction = isSell
            ? $"amount[{total}] resourceGUID[{resourceGuid}] autoLoading[0] " +
              $"quantity[{quantity}] transactionMode[{mode}]"
            : $"price[{total}] resourceGUID[{resourceGuid}] autoLoading[0] " +
              $"quantity[{centi} cSCU]";

        Line(at, $"[Notice] <CEntityComponentCommodityUIProvider::SendCommodity{verb}Request> " +
                 $"Sending SShopCommodity{verb}Request - playerId[{geid}] shopId[730090005328] " +
                 $"shopName[SCShop_Admin_lt_base_g] kioskId[730090005327] " +
                 $"{transaction} [Team_ActorFeatures][Shops]");

        Raw($"Cargo Box Data:  [boxSize[16] | unitAmount[{Math.Max(1, quantity / 16)}]]");
    }

    /// <summary>An item attached to a player slot, as emitted on spawn and refresh.</summary>
    /// <remarks>
    /// A spawn writes the whole kit in one burst, every line carrying the same
    /// <c>Elapsed</c> give or take a few microseconds, which is why the caller
    /// passes it rather than this inventing one per line.
    /// </remarks>
    public void Attachment(
        DateTimeOffset at,
        string handle,
        string itemClass,
        string entityId,
        string port,
        string status = "persistent",
        double elapsed = 22.216066) =>
        Line(at, $"[Notice] <AttachmentReceived> Player[{handle}] " +
                 $"Attachment[{itemClass}_{entityId}, {itemClass}, {entityId}] " +
                 $"Status[{status}] Port[{port}] " +
                 $"Elapsed[{elapsed.ToString("F6", CultureInfo.InvariantCulture)}] " +
                 "[Team_CoreGameplayFeatures][Inventory]");

    /// <summary>Binds an opaque inventory scope to the location most recently named.</summary>
    /// <remarks>
    /// The request number is the client's own counter for the session; the
    /// game writes it on every query, so a line without one is not a shape it
    /// produces.
    /// </remarks>
    public void InventoryQuery(DateTimeOffset at, string geid, string scope, string key, int request = 0) =>
        Line(at, $"[Notice] <Query Inventory> Request[{request}] Inventory[{geid}:{scope}:{key}] " +
                 "[Team_CoreGameplayFeatures][Inventory]");

    /// <summary>An item observed while one inventory page is being browsed.</summary>
    /// <remarks>
    /// Rank is the game's sort key for the slot - a run of lowercase letters,
    /// "amrqrwdiuwbon" and the like - and means nothing to the parser.
    /// </remarks>
    public void InventoryItem(
        DateTimeOffset at,
        string geid,
        string scope,
        string key,
        string itemClass,
        string rank = "amrqrwdiuwbon") =>
        Line(at, $"[Notice] <Update Container Items Add New Item> End Page " +
                 $"Entity Class[{itemClass}] Rank[{rank}] " +
                 $"SourceInventory[{geid}:{scope}:{key}] [Team_CoreGameplayFeatures][Inventory]");

    /// <summary>The owned-vehicle totals returned by an ASOP entitlement query.</summary>
    public void FleetQuery(DateTimeOffset at, int entitlements, int vehicles) =>
        Line(at, $"[Notice] <VehicleListQuery> Fetching vehicle list completed. " +
                 $"Retrieved {entitlements} entitlements out of {vehicles} vehicules. " +
                 "[Team_GameServices][ASOP][Entitlement][Insurance]");

    /// <summary>A ship elevator reports a retrieved entity before its model is known.</summary>
    public void VehicleSpawn(DateTimeOffset at, string entityId, string landingArea) =>
        Line(at, "[Notice] <CEntityComponentShipListProvider::SetVehicleSpawnedInformations> " +
                 $"VehicleEntityId: [{entityId}] LandingArea: {landingArea} " +
                 "[Team_GameServices][ASOP]");

    /// <summary>
    /// A ship retrieval as current builds write it: the request, then the ship
    /// on the pad a moment later.
    /// </summary>
    /// <remarks>
    /// The landing area is the player's hangar, and the game spells it
    /// "&lt;handle&gt;'s" on one line and "Large Hangar [Team_...]" on the next -
    /// the possessive's newline is in the string. Neither line names the ship;
    /// that arrives later, on whatever navigation line next carries the id.
    /// </remarks>
    public void VehicleRetrieval(
        DateTimeOffset at,
        string entityId,
        string landingAtcId,
        string handle,
        string hangar)
    {
        Line(at, "[Notice] <CEntityComponentShipListProvider::SetVehicleSpawningInformations> " +
                 $"SetVehicleSpawningInformations - VehicleEntityId: [{entityId}], LandingArea: {handle}'s");
        Line(at, $"{hangar} [Team_GameServices][ASOP]");

        var spawned = at.AddMilliseconds(830);
        Line(spawned, "[Notice] <CEntityComponentShipListProvider::SetVehicleSpawnedInformations> " +
                      $"SetVehicleSpawnedInformations - VehicleEntityId: [{entityId}], " +
                      $"LandingATCId: [{landingAtcId}], LandingArea: {handle}'s");
        Line(spawned, $"{hangar} [Team_GameServices][ASOP]");
    }

    /// <summary>An incidental line that ties a retrieved entity id to a ship model.</summary>
    public void VehicleIdentity(DateTimeOffset at, string vehicleId, string entityId) =>
        Line(at, $"[Notice] <Vehicle Initialization> Registered {vehicleId}[{entityId}] " +
                 "with ItemNavigation and local navigation [Team_VehicleFeatures][Vehicle]");

    /// <summary>One item in the tight burst produced when a corpse is created.</summary>
    /// <remarks>
    /// The whole kit is written in the same millisecond, one line per port,
    /// which is what lets a reader group a burst into one death. The class
    /// GUID is the item's record id in the game data; nothing reads it.
    /// </remarks>
    public void CorpseItem(
        DateTimeOffset at,
        string itemClass,
        string port,
        string entityId = "200000000218",
        string classGuid = "dbaa8a7d-755f-4104-8b24-7b58fd1e76f6") =>
        Line(at, "[Notice] <Adding non kept item " +
                 "[CSCActorCorpseUtils::PopulateItemPortForItemRecoveryEntitlement]> " +
                 $"Item '{itemClass}_{entityId} - Class({itemClass}) - Context(Streamable Runtime-spawned) - Socpak()', " +
                 $"Recorded data is: Port Name '{port}', Class GUID: '{classGuid}' " +
                 "[Team_CoreGameplayFeatures][Unknown]");

    public void ContractMarker(
        DateTimeOffset at,
        string missionId,
        string generator,
        string contract,
        string definitionId,
        string? markerId = null) =>
        Line(at, $"[Notice] <SMarkerHandler_Base::CreateMissionObjectiveMarker> Creating objective marker: " +
                 $"missionId [{missionId}], generator name [{generator}], " +
                 $"contract [{contract}][{markerId ?? Guid.NewGuid().ToString()}], contractDefinitionId[{definitionId}] " +
                 $"[Team_Missions]");

    /// <summary>A non-commodity kiosk purchase request, pending a server answer.</summary>
    /// <remarks>The two spaces before the team tag are the game's, not a typo.</remarks>
    public void ShopRequest(
        DateTimeOffset at,
        string geid,
        string shopName,
        string shopId,
        string kioskId,
        decimal price,
        string itemName,
        int quantity,
        string itemClassGuid = "00000000-0000-0000-0000-000000000001") =>
        Line(at, $"[Notice] <CEntityComponentShopUIProvider::SendShopBuyRequest> " +
                 $"Sending SShopBuyRequest - playerId[{geid}] shopId[{shopId}] " +
                 $"shopName[{shopName}] kioskId[{kioskId}] " +
                 $"client_price[{price.ToString("F6", CultureInfo.InvariantCulture)}] " +
                 $"itemClassGUID[{itemClassGuid}] " +
                 $"itemName[{itemName}] quantity[{quantity}]  [Team_CoreGameplayFeatures][Shops][UI]");

    /// <summary>The server outcome paired with the latest request at this kiosk.</summary>
    /// <remarks>
    /// Every answer in the real logs is <c>kioskState[BuyRequestProcessing]</c>
    /// with <c>result[Success]</c>; the other states are for the scenarios that
    /// test the pairing rules.
    /// </remarks>
    public void ShopResponse(
        DateTimeOffset at,
        string shopName,
        string kioskId,
        string result,
        string type = "Buying",
        string kioskState = "Idle",
        string geid = "100000000042",
        string shopId = "0") =>
        Line(at, $"[Notice] <CEntityComponentShopUIProvider::RmShopFlowResponse> " +
                 $"Received ShopFlowResponse - playerId[{geid}] shopId[{shopId}] " +
                 $"shopName[{shopName}] kioskId[{kioskId}] kioskState[{kioskState}] " +
                 $"result[{result}] type[{type}] [Team_CoreGameplayFeatures][Shops][UI]");

    /// <summary>A mission journal objective changing state.</summary>
    /// <remarks>
    /// Hauling steps carry the bare <c>ShowInLog|</c>; other contracts'
    /// visible steps carry more, <c>ShowInLog|RespectInheritedVisibility|</c>
    /// among them. Pass the flags to write one of those.
    /// </remarks>
    public void MissionObjective(
        DateTimeOffset at,
        string missionId,
        string objectiveId,
        string state,
        bool shownInLog = true,
        string? flags = null) =>
        Line(at, $"[Notice] <ObjectiveUpserted> Received ObjectiveUpserted push message for: " +
                 $"mission_id {missionId} - objective_id {objectiveId} - state {state} " +
                 $"- created 0 - flags={flags ?? (shownInLog ? "ShowInLog|" : "Internal|")} " +
                 "[Team_GameServices][Missions]");

    /// <summary>
    /// A contract ending, said the two ways the game says it.
    /// </summary>
    /// <remarks>
    /// Both, because the real logs carry both for every completion and a
    /// simulated install that wrote only one would let a reader that handles
    /// only the other pass its tests.
    /// </remarks>
    public void MissionEnded(
        DateTimeOffset at,
        string missionId,
        string state,
        string completionType,
        string reason = "Objectives complete",
        string handle = "testpilot",
        string geid = "100000000042")
    {
        Line(at, $"[Notice] <MissionEnded> Received MissionEnded push message for: " +
                 $"mission_id {missionId} - mission_state {state} " +
                 $"[Team_GameServices][Missions]");

        EndMission(at, missionId, completionType, reason, handle, geid);
    }

    /// <summary>
    /// The client's half of a contract ending, on its own.
    /// </summary>
    /// <remarks>
    /// An abandonment usually says only this: across the real backups there
    /// are 73 <c>CompletionType[Abandon]</c> lines and one withdrawn
    /// <c>MissionEnded</c>.
    /// </remarks>
    public void EndMission(
        DateTimeOffset at,
        string missionId,
        string completionType,
        string reason,
        string handle = "testpilot",
        string geid = "100000000042") =>
        Line(at, $"[Notice] <EndMission> Ending mission for player. MissionId[{missionId}] " +
                 $"Player[{handle}] PlayerId[{geid}] " +
                 $"CompletionType[{completionType}] Reason[{reason}] " +
                 $"[Team_MissionFeatures][Missions]");

    /// <summary>
    /// A notification and its follow-up Action lines. The repeats are the point:
    /// counting them naively inflates every statistic three to five times.
    /// </summary>
    public void Notification(DateTimeOffset at, string text, int id, string? missionId = null)
    {
        var mission = missionId ?? "00000000-0000-0000-0000-000000000000";

        Line(at, $"[Notice] <SHUDEvent_OnNotification> Added notification \"{text}\" [{id}] to queue. " +
                 $"New queue size: 1, MissionId: [{mission}], ObjectiveId: [] " +
                 $"[Team_CoreGameplayFeatures][Missions][Comms]");

        foreach (var (action, offset) in new[] { ("Next", 40), ("StartFade", 9000), ("Remove", 9400) })
        {
            Line(at.AddMilliseconds(offset),
                $"[Notice] <UpdateNotificationItem> Notification \"{text}\" [{id}], Action: {action} " +
                $"[Team_CoreGameplayFeatures][Missions][Comms]");
        }
    }

    /// <summary>
    /// A notification whose text contains a newline. The continuation carries its
    /// own identical timestamp, so nothing about the prefix distinguishes it from
    /// a new entry - only the unbalanced quote does.
    /// </summary>
    public void SplitNotification(DateTimeOffset at, string firstHalf, string secondHalf, int id)
    {
        Line(at, $"[Notice] <SHUDEvent_OnNotification> Added notification \"{firstHalf}");
        Line(at, $"{secondHalf}\" [{id}] to queue. New queue size: 1, " +
                 $"MissionId: [00000000-0000-0000-0000-000000000000], ObjectiveId: [] " +
                 $"[Team_CoreGameplayFeatures][Missions][Comms]");
    }

    public void Incapacitated(DateTimeOffset at, int id) =>
        Notification(at,
            "Incapacitated: While incapacitated, ask others in your party, in chat, or through " +
            "rescue service beacons to revive you before the 'Time to Death' timer expires.",
            id);

    /// <summary>
    /// Matchmaking placing the client. Written once per placement, just before
    /// the menu channel's "Nub destroyed" teardown.
    /// </summary>
    public void JoinShard(DateTimeOffset at, string shard) =>
        Line(at, $"[Notice] <Join PU> address[35.245.92.221] port[64291] shard[{shard}] " +
                 $"locationId[844429225164801] [Team_GameServices][GIM][Matchmaking]");

    public void Quit(DateTimeOffset at, string reason = "Quit via console command") =>
        Line(at, $"[Notice] <SystemQuit> CSystem::Quit invoked with - cause=30016, reason={reason}, " +
                 $"exitCode=0, thread id=10360, main thread id=10360 [Team_Unknown][System]");
    public void Disconnect(DateTimeOffset at, string reason, string gameRules, bool remote = false) =>
        Line(at, $"[Notice] <Channel Disconnected> cause=30010 reason=\"{reason}\" frame=10136 " +
                 $"isRemote={(remote ? 1 : 0)} viewState=eCVS_InGame map=\"megamap\" gamerules=\"{gameRules}\" " +
                 $"hostType=\"Replicant\" remoteAddr=<local>:12300 localAddr=<local>:16");

    // ---------------- dormant combat ----------------

    /// <summary>
    /// Emits the archived <c>&lt;Actor Death&gt;</c> format. Star Citizen 4.9 and 4.10 do
    /// not produce this any more; the simulator can, which is the only way to
    /// exercise the dormant combat parser end to end.
    /// </summary>
    public void ActorDeath(
        DateTimeOffset at, string victim, string killer, string weapon, string damageType, string zone,
        string? victimId = null, string? killerId = null) =>
        Line(at, $"[Notice] <Actor Death> CActor::Kill: '{victim}' [{victimId ?? Random.Shared.Next(10000, 99999).ToString()}] " +
                 $"in zone '{zone}' killed by '{killer}' [{killerId ?? Random.Shared.Next(10000, 99999).ToString()}] " +
                 $"using '{weapon}' [Class {weapon}] with damage type '{damageType}' " +
                 $"from direction x: 0.512, y: -0.234, z: 0.100 [Team_ActorFeatures][Actor]");

    public void VehicleDestruction(
        DateTimeOffset at, string vehicle, string driver, string attacker, int from, int to, string cause,
        string? entityId = null) =>
        Line(at, $"[Notice] <Vehicle Destruction> CVehicle::OnAdvanceDestroyLevel: Vehicle '{vehicle}' " +
                 $"[{entityId ?? Random.Shared.Next(1000000, 9999999).ToString()}] in zone 'Stanton_Yela' " +
                 $"[pos x: 1.0, y: 2.0, z: 3.0 vel x: 0.0, y: 0.0, z: 0.0] " +
                 $"driven by '{driver}' [999] advanced from destroy level {from} to {to} " +
                 $"caused by '{attacker}' [888] with '{cause}' [Team_VehicleFeatures][Vehicle]");

    // ---------------- filler ----------------

    /// <summary>
    /// Background chatter. Real logs are over 95% noise, so including it keeps
    /// the generated file representative of what the parser actually faces.
    /// </summary>
    public void Noise(DateTimeOffset at, int index)
    {
        switch (index % 5)
        {
            case 0:
                Line(at, "[Notice] <InvalidateAllTerrainCells> Invalidating all terrain cells [Team_Graphics]");
                break;
            case 1:
                Line(at, $"[Notice] <CSCLoadingPlatformManager::LoadEntitiesReference> Loading entities {index}");
                break;
            case 2:
                Line(at, "[Notice] <Local Route Guard - Server Rerouted> [CL][35872] | NULL ENTITY|" +
                         "CSCItemNavigation::PostInitialize::<lambda_1>::operator ()|FinalStop=0 [Team_CGP4][QuantumTravel]");
                break;
            case 3:
                Line(at, "[Error] <Actor Physics> CSCActorPhysicsController::Physicalize: " +
                         $"Failed to physicalize 'ui_entity_000{index % 10} (comms_user)' [23316] [Team_ActorFeatures][Actor]");
                break;
            default:
                Line(at, $"[Notice] <UpdateNotificationItem> Notification \"System status {index}\" [{900 + index % 90}], Action: Next");
                break;
        }
    }

    public void Dispose() => _writer.Dispose();
}

/// <summary>
/// A game build and what its header says about itself.
/// </summary>
/// <remarks>
/// The header's version and compile date change with the build, so a build
/// number written beside another build's version is a header no client ever
/// wrote. The known ones are copied from real backups; any other number keeps
/// the default's version, which is the best that can be said without a log
/// from that build.
/// </remarks>
public sealed record GameBuild(string Number, string Version, string BuiltOn)
{
    public static GameBuild Default { get; } = new("12344265", "4.9.188.23497", "Jul 29 2026 15:21:13");

    private static readonly GameBuild[] Known =
    [
        Default,
        new("12568521", "1.0.191.51145", "Sep  2 2026 19:13:33"),
        new("12572603", "1.0.191.55227", "Sep  3 2026 13:46:55"),
        new("12660092", "4.10.193.11644", "Sep 15 2026 13:12:47")
    ];

    public static GameBuild For(string number) =>
        Known.FirstOrDefault(b => b.Number == number) ?? Default with { Number = number };
}
