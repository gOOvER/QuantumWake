using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Quantumwake.Data;

/// <summary>Rebuilds template SVG as drawing primitives and inert draw.io labels.</summary>
internal static class JoystickSvg
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace Html = "http://www.w3.org/1999/xhtml";
    private static readonly XNamespace Xlink = "http://www.w3.org/1999/xlink";
    private static readonly HashSet<string> Shapes = Words(
        "svg g a defs switch title desc path rect circle ellipse line polyline polygon text tspan image use " +
        "foreignObject clipPath mask linearGradient radialGradient stop pattern");
    private static readonly HashSet<string> Labels = Words("div span p a br b strong i em u s sub sup img");
    private static readonly HashSet<string> Attributes = Words(
        "x y x1 x2 y1 y2 cx cy r rx ry dx dy d points width height viewBox preserveAspectRatio transform " +
        "fill fill-opacity fill-rule pointer-events stroke stroke-width stroke-opacity stroke-linecap stroke-linejoin " +
        "stroke-miterlimit stroke-dasharray stroke-dashoffset opacity color font-family font-size font-weight " +
        "font-style text-anchor dominant-baseline alignment-baseline text-decoration letter-spacing " +
        "word-spacing textLength lengthAdjust clip-path clip-rule mask offset stop-color stop-opacity " +
        "gradientUnits gradientTransform spreadMethod fx fy fr patternUnits patternContentUnits patternTransform");
    private static readonly HashSet<string> Styles = Words(
        "display position left top right bottom width height min-width max-width min-height max-height " +
        "margin margin-left margin-top margin-right margin-bottom padding padding-left padding-top padding-right " +
        "padding-bottom box-sizing overflow vertical-align white-space overflow-wrap word-wrap word-break " +
        "font-family font-size font-weight font-style line-height text-align text-decoration text-shadow " +
        "color background-color border border-color border-width border-style border-radius opacity " +
        "align-items align-content justify-content flex-direction flex-wrap flex-shrink flex-grow " +
        "transform transform-origin fill fill-opacity stroke stroke-width stroke-opacity");
    private static readonly Regex PlainValue = new("^[a-zA-Z0-9\\s#.,()%+/'\"-]+$", RegexOptions.CultureInvariant);
    private static readonly Regex Raster = new("^data:image/(?:png|jpeg|gif|webp);base64,[a-zA-Z0-9+/=\\s]+$", RegexOptions.CultureInvariant);

    public static byte[] Sanitize(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                // draw.io includes the SVG 1.1 DOCTYPE. Ignore the declaration
                // entirely; neither external nor internal entities are loaded.
                DtdProcessing = DtdProcessing.Ignore, XmlResolver = null,
                MaxCharactersInDocument = 16 * 1024 * 1024,
            });
            var source = XDocument.Load(reader);
            if (source.Root?.Name != Svg + "svg") throw new InvalidDataException("Not an SVG.");

            // Imported ids must not shadow dashboard elements or let fragment
            // references reach out of the picture into the rest of the page.
            var prefix = "qw-template-" + Guid.NewGuid().ToString("N") + "-";
            var ids = source.Root.DescendantsAndSelf().Attributes("id")
                .Select(a => a.Value).Distinct(StringComparer.Ordinal)
                .Select((id, i) => (id, safe: prefix + i))
                .ToDictionary(p => p.id, p => p.safe, StringComparer.Ordinal);
            var clean = Copy(source.Root, ids, 0)!;
            return Encoding.UTF8.GetBytes(clean.ToString(SaveOptions.DisableFormatting));
        }
        catch (XmlException e) { throw new InvalidDataException("Invalid SVG template.", e); }
    }

    private static XElement? Copy(XElement source, Dictionary<string, string> ids, int depth)
    {
        if (depth > 128) throw new InvalidDataException("SVG template is nested too deeply.");
        var svg = source.Name.Namespace == Svg;
        var html = source.Name.Namespace == Html;
        var tag = source.Name.LocalName;
        if (!(svg && Shapes.Contains(tag)) && !(html && Labels.Contains(tag))) return null;
        var target = new XElement(source.Name);
        foreach (var attribute in source.Attributes())
        {
            var name = attribute.Name.LocalName;
            var value = attribute.Value;
            if (attribute.Name.Namespace == XNamespace.None)
            {
                if (name == "id") target.SetAttributeValue(name, ids[value]);
                else if (name == "style")
                {
                    var style = CleanStyle(value);
                    if (style.Length > 0) target.SetAttributeValue(name, style);
                }
                else if (svg && Attributes.Contains(name))
                {
                    var safe = DrawingValue(value, ids);
                    if (safe is not null) target.SetAttributeValue(name, safe);
                }
                else if (svg && name == "requiredFeatures" && value == "http://www.w3.org/TR/SVG11/feature#Extensibility")
                    target.SetAttributeValue(name, value);
                else if (html && (name is "width" or "height") && int.TryParse(value, out var size) && size >= 0)
                    target.SetAttributeValue(name, size);
                else if (html && name == "alt") target.SetAttributeValue(name, value);
            }
            if ((attribute.Name.Namespace == XNamespace.None || attribute.Name.Namespace == Xlink) && (name is "href" or "src"))
            {
                if ((svg && tag == "image" && name == "href" || html && tag == "img" && name == "src") && Raster.IsMatch(value))
                    target.SetAttributeValue(attribute.Name, value);
                else if (svg && tag == "use" && name == "href" && value.StartsWith('#') && ids.TryGetValue(value[1..], out var id))
                    target.SetAttributeValue(attribute.Name, "#" + id);
            }
        }
        foreach (var node in source.Nodes())
        {
            if (node is XElement child && Copy(child, ids, depth + 1) is { } safe) target.Add(safe);
            else if (node is XText text) target.Add(new XText(text.Value));
        }
        return target;
    }

    private static string? DrawingValue(string value, Dictionary<string, string> ids)
    {
        if (value.StartsWith("url(#", StringComparison.Ordinal) && value.EndsWith(')'))
            return ids.TryGetValue(value[5..^1], out var id) ? $"url(#{id})" : null;
        return Plain(value) ? value : null;
    }

    private static string CleanStyle(string value) => string.Join(';', value.Split(';').Select(declaration =>
    {
        var colon = declaration.IndexOf(':');
        if (colon < 0) return null;
        var property = declaration[..colon].Trim().ToLowerInvariant();
        var content = declaration[(colon + 1)..].Trim();
        return Styles.Contains(property) && Plain(content) ? property + ":" + content : null;
    }).Where(s => s is not null));

    // No CSS escapes, comments, at-rules or URL tokens. Raster data is only
    // accepted in image attributes; styles cannot fetch or embed documents.
    private static bool Plain(string value) => PlainValue.IsMatch(value)
        && !value.Contains("url", StringComparison.OrdinalIgnoreCase)
        && !value.Contains("expression", StringComparison.OrdinalIgnoreCase)
        && !value.Contains("/*", StringComparison.Ordinal);

    private static HashSet<string> Words(string value) => new(value.Split(' '), StringComparer.Ordinal);
}
