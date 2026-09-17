namespace Quantumwake.Core.GameData;

/// <summary>
/// The standard cargo crates as the install describes them.
/// </summary>
/// <remarks>
/// <para>
/// Every commodity ships in a family of crates - <c>Carryable_TBO_FL_32SCU_
/// Commodity_Organic_Oza</c> is a 32 of Oza - and each record's
/// <c>SAttachableComponentParams.AttachDef.inventoryOccupancyDimensions</c>
/// is the box it is in metres. Read on 2026-09-16 the seven sizes are 1.25³,
/// 1.25 × 2.5 × 1.25, 2.5 × 2.5 × 1.25, 2.5³, and 2.5 × 5 / 7.5 / 10 × 2.5:
/// the 16, 24 and 32 are long boxes one lane wide, not squares, which is
/// what decides whether a grid takes them. The <c>SCU_Cargo_Template_*</c>
/// records read 0.15 m on every side and are placeholders.
/// </para>
/// <para>
/// <c>CargoGridOccupantProperties</c> on the same record says which faces
/// may point up on a grid - the top alone, on every crate read - and that
/// anything may stack on any face. So a crate turns on the spot and never
/// stands on its side, which is what the packer honours.
/// </para>
/// <para>
/// One size, one box: the first crate read of each size is kept. Nothing
/// was seen to differ between commodities of a size on this install.
/// </para>
/// </remarks>
public static class GameCrates
{
    public static List<CargoCrate> Read(DataCore core)
    {
        var attach = core.StructIndexOf("SAttachableComponentParams");
        var occupant = core.StructIndexOf("CargoGridOccupantProperties");
        var physics = core.StructIndexOf("SEntityPhysicsControllerParams");
        if (attach < 0) return [];

        var found = new Dictionary<int, CargoCrate>();
        foreach (var record in core.Records())
        {
            if (!record.Name.StartsWith("EntityClassDefinition.Carryable_", StringComparison.OrdinalIgnoreCase)) continue;
            var cls = record.Name["EntityClassDefinition.".Length..];
            var scu = SizeOf(cls);
            if (scu <= 0 || found.ContainsKey(scu)) continue;

            double x = 0, y = 0, z = 0, mass = 0;
            var uprightOnly = true;
            foreach (var component in core.PointerArray(record, "Components"))
            {
                var at = core.InstanceAt(component);
                if (component.StructIndex == attach)
                {
                    var (def, field) = core.FieldAt(at, attach, "AttachDef");
                    if (def < 0 || field is null) continue;
                    if (GameMining.Nested(core, def, field.StructIndex, "inventoryOccupancyDimensions") is { } dims)
                    {
                        x = core.SingleAt(dims.At, dims.StructIndex, "x") ?? 0;
                        y = core.SingleAt(dims.At, dims.StructIndex, "y") ?? 0;
                        z = core.SingleAt(dims.At, dims.StructIndex, "z") ?? 0;
                    }
                }
                else if (component.StructIndex == occupant)
                {
                    // Any face other than the top allowed upward would let the
                    // crate lie on its side; none does, but the file is asked.
                    foreach (var face in new[] { "Bottom", "Front", "Back", "Right", "Left" })
                        if (GameMining.Nested(core, at, occupant, face) is { } f && core.BoolAt(f.At, f.StructIndex, "faceUpwardAllowed") == true)
                            uprightOnly = false;
                }
                else if (component.StructIndex == physics && core.PointerAt(at, physics, "PhysType") is { } phys)
                {
                    mass = core.SingleAt(core.InstanceAt(phys), phys.StructIndex, "Mass") ?? 0;
                }
            }

            // A crate the lattice can hold is at least a cell on every side.
            if (x < 1 || y < 1 || z < 1) continue;
            found[scu] = new CargoCrate(scu, Math.Round(x, 3), Math.Round(y, 3), Math.Round(z, 3), Math.Round(mass, 1), uprightOnly);
        }

        return found.Values.OrderBy(c => c.Scu).ToList();
    }

    /// <summary>The size on the label - the <c>_32SCU_</c> in the class - or 0.</summary>
    private static int SizeOf(string cls)
    {
        var at = cls.IndexOf("SCU_Commodity", StringComparison.OrdinalIgnoreCase);
        if (at <= 0) return 0;
        var start = cls.LastIndexOf('_', at - 1);
        return int.TryParse(cls.AsSpan(start + 1, at - start - 1), out var scu) ? scu : 0;
    }
}
