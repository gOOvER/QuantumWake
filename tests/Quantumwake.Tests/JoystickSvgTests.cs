using System.Text;
using System.Xml.Linq;
using Quantumwake.Data;

namespace Quantumwake.Tests;

public class JoystickSvgTests
{
    private static string Clean(string content) => Encoding.UTF8.GetString(JoystickSvg.Sanitize(Encoding.UTF8.GetBytes(content)));
    private static string Svg(string content) => "<svg xmlns=\"http://www.w3.org/2000/svg\">" + content + "</svg>";

    [Theory]
    [InlineData("<script>evil()</script>")]
    [InlineData("<image href='https://example.invalid/secret'/>")]
    [InlineData("<image href='data:image/svg+xml;base64,PHN2Zz4='/>")]
    [InlineData("<a href='javascript:evil()'><text>link</text></a>")]
    [InlineData("<set attributeName='onload' to='evil()'/>")]
    [InlineData("<style>@import 'https://example.invalid/style';</style>")]
    [InlineData("<foreignObject><iframe xmlns='http://www.w3.org/1999/xhtml' srcdoc='evil()'/></foreignObject>")]
    [InlineData("<foreignObject><img xmlns='http://www.w3.org/1999/xhtml' src='missing' onerror='evil()'/></foreignObject>")]
    [InlineData("<foreignObject><div xmlns='http://www.w3.org/1999/xhtml' style='background-image:url(https://example.invalid/image)' onclick='evil()'>label</div></foreignObject>")]
    [InlineData("<rect fill='url(https://example.invalid/image)' style='fill:u\\72l(https://example.invalid/image)'/>")]
    public void Active_content_and_external_resources_are_removed(string content)
    {
        var clean = Clean(Svg(content));
        Assert.DoesNotContain("evil", clean);
        Assert.DoesNotContain("example.invalid", clean);
        Assert.DoesNotContain("onerror", clean);
        Assert.DoesNotContain("data:image/svg", clean);
        Assert.DoesNotContain("javascript:", clean);
    }

    [Fact]
    public void The_standard_draw_io_doctype_is_ignored_without_loading_it()
    {
        var clean = Clean("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">" + Svg("<text>Button_1</text>"));
        Assert.Contains("Button_1", clean);
        Assert.DoesNotContain("DOCTYPE", clean);
    }

    [Fact]
    public void Draw_io_labels_geometry_and_embedded_raster_images_survive()
    {
        var clean = Clean(Svg("""
            <g transform="translate(10,20)"><rect x="0" y="0" width="200" height="100" fill="#fff"/>
            <image href="data:image/png;base64,aGVsbG8=" width="10" height="10"/>
            <switch><foreignObject width="200" height="100">
            <div xmlns="http://www.w3.org/1999/xhtml" style="display:flex;align-items:center;justify-content:center;font-size:12px;color:rgb(0, 0, 0)">
            <span>Button_1</span></div></foreignObject><text x="5" y="10">Button_1</text></switch></g>
            """));
        Assert.Contains("translate(10,20)", clean);
        Assert.Contains("data:image/png;base64,aGVsbG8=", clean);
        Assert.Contains("display:flex;align-items:center;justify-content:center;font-size:12px;color:rgb(0, 0, 0)", clean);
        Assert.Contains("Button_1", clean);
        Assert.Contains("foreignObject", clean);
    }

    [Fact]
    public void Fragment_references_are_scoped_to_the_template()
    {
        var clean = XDocument.Parse(Clean(Svg("""
            <defs><linearGradient id="paint"><stop offset="0" stop-color="#fff"/></linearGradient></defs>
            <rect id="controls-pending" fill="url(#paint)"/><use href="#controls-pending"/><use href="#dashboard"/>
            """)));
        XNamespace svg = "http://www.w3.org/2000/svg";
        var id = clean.Descendants(svg + "linearGradient").Single().Attribute("id")!.Value;
        Assert.StartsWith("qw-template-", id);
        Assert.Equal($"url(#{id})", clean.Descendants(svg + "rect").Single().Attribute("fill")!.Value);
        Assert.Null(clean.Descendants(svg + "use").Last().Attribute("href"));
        Assert.DoesNotContain(clean.Descendants().Attributes("id"), a => a.Value == "controls-pending");
    }

    [Theory]
    [InlineData("<!DOCTYPE svg [<!ENTITY x SYSTEM 'file:///test'>]><svg xmlns='http://www.w3.org/2000/svg'>&x;</svg>")]
    [InlineData("<html/>")]
    [InlineData("<svg>")]
    public void Non_SVG_malformed_XML_and_entity_references_are_rejected(string source) =>
        Assert.Throws<InvalidDataException>(() => Clean(source));
}
