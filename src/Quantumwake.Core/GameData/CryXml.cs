using System.Buffers.Binary;
using System.Text;
using System.Xml.Linq;

namespace Quantumwake.Core.GameData;

/// <summary>
/// Reads the engine's binary XML, <c>CryXmlB</c>, into an <see cref="XDocument"/>.
/// </summary>
/// <remarks>
/// <para>
/// The keybinding catalogue (<c>Data\Libs\Config\defaultProfile.xml</c>) and
/// the reference layouts beside it are XML in name only: the archive holds
/// them as CryXmlB, a table format - an 8-byte magic, then offsets and
/// counts for a node table, an attribute table, a child-index table and a
/// string table, with every name and value a string-table offset. A node is
/// 28 bytes: name, content, attribute count, child count, parent, first
/// attribute, first child, a reserved word. An attribute is two offsets.
/// </para>
/// <para>
/// A plain-text file is passed through, so a caller need not know which it
/// has: the game's own <c>actionmaps.xml</c> under the user folder is text,
/// the same document from the archive is not.
/// </para>
/// </remarks>
public static class CryXml
{
    private const int NodeSize = 28;
    private const int AttributeSize = 8;

    /// <summary>True when the bytes start with the CryXmlB magic.</summary>
    public static bool IsBinary(ReadOnlySpan<byte> data) =>
        data.Length >= 8 && data[0] == (byte)'C' && data[1] == (byte)'r' && data[2] == (byte)'y'
        && data[3] == (byte)'X' && data[4] == (byte)'m' && data[5] == (byte)'l' && data[6] == (byte)'B';

    /// <summary>Parses either form. Throws <see cref="InvalidDataException"/> on a table that does not fit the bytes.</summary>
    public static XDocument Parse(byte[] data)
    {
        if (!IsBinary(data))
        {
            // Text, possibly with a BOM the XML reader copes with.
            using var stream = new MemoryStream(data);
            return XDocument.Load(stream);
        }

        if (data.Length < 44)
            throw new InvalidDataException("CryXmlB header is truncated.");

        var nodeOffset = ReadInt(data, 12);
        var nodeCount = ReadInt(data, 16);
        var attributeOffset = ReadInt(data, 20);
        var attributeCount = ReadInt(data, 24);
        var childOffset = ReadInt(data, 28);
        var childCount = ReadInt(data, 32);
        var stringOffset = ReadInt(data, 36);
        var stringLength = ReadInt(data, 40);

        if (nodeCount <= 0
            || !Fits(data, nodeOffset, (long)nodeCount * NodeSize)
            || !Fits(data, attributeOffset, (long)attributeCount * AttributeSize)
            || !Fits(data, childOffset, (long)childCount * 4)
            || !Fits(data, stringOffset, stringLength))
            throw new InvalidDataException("CryXmlB tables do not fit the file.");

        string StringAt(int offset)
        {
            if (offset < 0 || offset >= stringLength) throw new InvalidDataException("CryXmlB string offset out of range.");
            var start = stringOffset + offset;
            var end = Array.IndexOf(data, (byte)0, start, stringLength - offset);
            if (end < 0) end = stringOffset + stringLength;
            return Encoding.UTF8.GetString(data, start, end - start);
        }

        XElement Node(int index, int depth)
        {
            if (index < 0 || index >= nodeCount) throw new InvalidDataException("CryXmlB node index out of range.");
            // A cycle in the child table would recurse for ever; no real file nests this deep.
            if (depth > 256) throw new InvalidDataException("CryXmlB nesting is too deep.");

            var at = nodeOffset + index * NodeSize;
            var element = new XElement(StringAt(ReadInt(data, at)));
            var content = StringAt(ReadInt(data, at + 4));
            int attributes = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(at + 8));
            int children = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(at + 10));
            var firstAttribute = ReadInt(data, at + 16);
            var firstChild = ReadInt(data, at + 20);

            for (var a = 0; a < attributes; a++)
            {
                var ao = attributeOffset + (firstAttribute + a) * AttributeSize;
                if (!Fits(data, ao, AttributeSize)) throw new InvalidDataException("CryXmlB attribute index out of range.");
                element.SetAttributeValue(StringAt(ReadInt(data, ao)), StringAt(ReadInt(data, ao + 4)));
            }
            if (content.Length > 0) element.Value = content;
            for (var c = 0; c < children; c++)
            {
                var co = childOffset + (firstChild + c) * 4;
                if (!Fits(data, co, 4)) throw new InvalidDataException("CryXmlB child index out of range.");
                element.Add(Node(ReadInt(data, co), depth + 1));
            }
            return element;
        }

        return new XDocument(Node(0, 0));
    }

    private static int ReadInt(byte[] data, int at) => BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(at));

    private static bool Fits(byte[] data, long offset, long length) =>
        offset >= 0 && length >= 0 && offset + length <= data.LongLength;
}
