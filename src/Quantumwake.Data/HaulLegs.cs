namespace Quantumwake.Data;

/// <summary>One movement of cargo a contract asks for: from a place, to a place, how much.</summary>
/// <param name="Scu">The leg's own count when the screen printed one; null on a multi-pickup, where only the delivery total is printed.</param>
/// <param name="ScuDone">The first figure of the count - what the screen said was already delivered.</param>
public sealed record HaulLeg(
    string? Pickup,
    string? PickupBody,
    string? Delivery,
    string? DeliveryBody,
    string? Commodity,
    int? Scu,
    int? ScuDone,
    string Id = "");

/// <summary>
/// Turns the objectives the Contracts app printed into legs.
/// </summary>
/// <remarks>
/// <para>
/// The indent is the structure. A step at depth 0 opens a group and every
/// depth-1 step under it belongs to it, until the next depth-0 step. Which
/// end is the parent depends on the contract's shape, and both occur:
/// </para>
/// <list type="bullet">
/// <item>Collect parent, Deliver children - a direct haul, or a multi-drop
/// where each source has its own destination. One leg per child, the cargo
/// from whichever line named it.</item>
/// <item>Deliver parent, Collect children - a multi-pickup. One leg per
/// child, all to the parent's place; the parent's count is the delivery
/// total and no child carries its own share, so the leg's count is null and
/// the total rides on <see cref="Delivery"/>.</item>
/// </list>
/// <para>
/// A step with no children is a leg on its own with one end missing, which is
/// what the app shows rather than pairing it with a neighbour by guesswork.
/// </para>
/// </remarks>
public static class HaulLegs
{
    /// <summary>A delivery the screen printed a total for, before it is split into legs.</summary>
    public sealed record Delivery(string Place, string? Body, string? Commodity, int? Scu, int? ScuDone);

    /// <summary>The legs, and the delivery totals the multi-pickup shape prints on the parent.</summary>
    public static (IReadOnlyList<HaulLeg> Legs, IReadOnlyList<Delivery> Deliveries) From(ContractsReading reading)
    {
        var steps = reading.Steps ?? [];
        var sites = reading.PickupSites ?? [];
        var legs = new List<HaulLeg>();
        var deliveries = new List<Delivery>();

        for (var i = 0; i < steps.Count; i++)
        {
            var parent = steps[i];
            if (parent.Depth != 0 || parent.Kind == "other") continue;

            var children = new List<ContractStep>();
            for (var j = i + 1; j < steps.Count && steps[j].Depth > 0; j++)
                if (steps[j].Kind != "other") children.Add(steps[j]);

            if (parent.Kind == "collect")
            {
                if (children.Count == 0)
                {
                    legs.Add(new HaulLeg(parent.Place, BodyOf(parent, sites), null, null, parent.Commodity, null, null));
                    continue;
                }

                foreach (var child in children.Where(c => c.Kind == "deliver"))
                    legs.Add(new HaulLeg(
                        parent.Place, BodyOf(parent, sites),
                        child.Place, child.Body,
                        child.Commodity ?? parent.Commodity,
                        child.Total, child.Done));
            }
            else
            {
                deliveries.Add(new Delivery(parent.Place!, parent.Body, parent.Commodity, parent.Total, parent.Done));

                if (children.Count == 0)
                {
                    legs.Add(new HaulLeg(null, null, parent.Place, parent.Body, parent.Commodity, parent.Total, parent.Done));
                    continue;
                }

                foreach (var child in children.Where(c => c.Kind == "collect"))
                    legs.Add(new HaulLeg(
                        child.Place, BodyOf(child, sites),
                        parent.Place, parent.Body,
                        child.Commodity ?? parent.Commodity,
                        null, null));
            }
        }

        return (legs, deliveries);
    }

    /// <summary>The body a pickup is on: from the objective if it said, else from the contract text's own list.</summary>
    private static string? BodyOf(ContractStep step, IReadOnlyList<PickupSite> sites)
    {
        if (step.Body is not null) return step.Body;
        if (step.Place is null) return null;

        return sites.FirstOrDefault(s => ScreenInsight.Fold(s.Place) == ScreenInsight.Fold(step.Place))?.Body;
    }
}
