using System.Text;
using System.Xml.Linq;
using Quantumwake.Core.Controls;
using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The three readers under the Controls page: the engine's binary XML, the
/// keybinding profile, and the action catalogue.
/// </summary>
/// <remarks>
/// The fixtures are cut from this install's own files on 2026-09-17: the
/// live <c>actionmaps.xml</c> with its six sticks, and the shapes
/// <c>defaultProfile.xml</c> takes once decoded. The binary reader is
/// tested through an encoder written here, since the archive is not in the
/// test tree - the encoder writes the same tables the game does, and a
/// reader that round-trips a nested document with attributes and content is
/// the reader that decoded the 1,531-node catalogue in the probe.
/// </remarks>
public class ControlReaderTests
{
    /* ---------- CryXmlB ---------- */

    /// <summary>Writes a document the way the engine does: node, attribute, child and string tables.</summary>
    private static byte[] Encode(XDocument document)
    {
        var strings = new List<byte>();
        var offsets = new Dictionary<string, int>(StringComparer.Ordinal);
        int Intern(string s)
        {
            if (offsets.TryGetValue(s, out var at)) return at;
            at = strings.Count;
            strings.AddRange(Encoding.UTF8.GetBytes(s));
            strings.Add(0);
            offsets[s] = at;
            return at;
        }

        var nodes = new List<(int Name, int Content, int Attrs, int Children, int Parent, int FirstAttr, int FirstChild)>();
        var attributes = new List<(int Key, int Value)>();
        var children = new List<int>();

        int Walk(XElement e, int parent)
        {
            var index = nodes.Count;
            nodes.Add(default);
            var firstAttr = attributes.Count;
            foreach (var a in e.Attributes()) attributes.Add((Intern(a.Name.LocalName), Intern(a.Value)));
            var kids = e.Elements().ToList();
            var content = kids.Count == 0 ? e.Value : "";
            // Children are written after this node's own record is placed, so
            // the child table holds a contiguous run per parent.
            var childIndexes = new List<int>();
            foreach (var k in kids) childIndexes.Add(Walk(k, index));
            var firstChild = children.Count;
            children.AddRange(childIndexes);
            nodes[index] = (Intern(e.Name.LocalName), Intern(content), e.Attributes().Count(), kids.Count, parent, firstAttr, firstChild);
            return index;
        }
        Walk(document.Root!, -1);

        var header = 44;
        var nodeOffset = header;
        var attrOffset = nodeOffset + nodes.Count * 28;
        var childOffset = attrOffset + attributes.Count * 8;
        var stringOffset = childOffset + children.Count * 4;
        var total = stringOffset + strings.Count;

        var data = new byte[total];
        Encoding.ASCII.GetBytes("CryXmlB\0").CopyTo(data, 0);
        void Put(int at, int value) => BitConverter.GetBytes(value).CopyTo(data, at);
        Put(8, total); Put(12, nodeOffset); Put(16, nodes.Count); Put(20, attrOffset); Put(24, attributes.Count);
        Put(28, childOffset); Put(32, children.Count); Put(36, stringOffset); Put(40, strings.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            var at = nodeOffset + i * 28;
            Put(at, n.Name); Put(at + 4, n.Content);
            BitConverter.GetBytes((short)n.Attrs).CopyTo(data, at + 8);
            BitConverter.GetBytes((short)n.Children).CopyTo(data, at + 10);
            Put(at + 12, n.Parent); Put(at + 16, n.FirstAttr); Put(at + 20, n.FirstChild); Put(at + 24, 0);
        }
        for (var i = 0; i < attributes.Count; i++) { Put(attrOffset + i * 8, attributes[i].Key); Put(attrOffset + i * 8 + 4, attributes[i].Value); }
        for (var i = 0; i < children.Count; i++) Put(childOffset + i * 4, children[i]);
        strings.CopyTo(data, stringOffset);
        return data;
    }

    private const string Catalogue = """
        <profile version="1">
         <actionmap name="seat_general" version="1" UILabel="@ui_CGSeatGeneral" UICategory="@ui_CCSeatGeneral">
          <action name="v_eject" activationMode="delayed_press_long" keyboard="lalt+u" joystick=" " gamepad=" " UILabel="@ui_CIEject" UIDescription="@ui_CIEjectDesc"/>
          <action name="v_light_amplification_toggle" keyboard="n" UILabel="@ui_CILightAmp"/>
         </actionmap>
         <actionmap name="spaceship_movement" version="18" UILabel="@ui_CGSpaceFlightMovement" UICategory="@ui_CCSpaceFlight">
          <action name="v_pitch" joystick="y" gamepad="thumbly" optionGroup="flight_move_pitch" UILabel="@ui_CIPitch"/>
          <action name="v_afterburner" keyboard="lshift" joystick=" " UILabel="@ui_CIAfterburner" UIDescription="@ui_CIAfterburnerDesc"/>
         </actionmap>
         <actionmap name="mystery">
          <action name="v_unlabelled"/>
         </actionmap>
        </profile>
        """;

    private static readonly Dictionary<string, string> Text = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ui_CGSeatGeneral"] = "Seat - General", ["ui_CCSeatGeneral"] = "SEAT", ["ui_CIEject"] = "Eject",
        ["ui_CIEjectDesc"] = "Leave the seat in a hurry", ["ui_CGSpaceFlightMovement"] = "Vehicles - Movement",
        ["ui_CCSpaceFlight"] = "FLIGHT", ["ui_CIPitch"] = "Pitch", ["ui_CIAfterburner"] = "Afterburner", ["ui_CILightAmp,P"] = "Light amplification",
    };

    [Fact]
    public void Binary_xml_round_trips_a_nested_document_with_attributes_and_content()
    {
        var original = XDocument.Parse("""
            <root a="1" b="two words">
             <child name="first" empty=""/>
             <child name="second"><leaf>text content</leaf><leaf/></child>
             <child name="third" note="ünïcode ok"/>
            </root>
            """);

        var bytes = Encode(original);
        Assert.True(CryXml.IsBinary(bytes));

        var decoded = CryXml.Parse(bytes);
        Assert.Equal(original.ToString(), decoded.ToString());
    }

    [Fact]
    public void Text_xml_passes_through_and_a_truncated_binary_says_so()
    {
        var text = Encoding.UTF8.GetBytes("<a x=\"1\"><b/></a>");
        Assert.False(CryXml.IsBinary(text));
        Assert.Equal("1", (string?)CryXml.Parse(text).Root!.Attribute("x"));

        var broken = Encode(XDocument.Parse("<a><b/></a>"))[..40];
        Assert.Throws<InvalidDataException>(() => CryXml.Parse(broken));
    }

    /* ---------- inputs ---------- */

    [Theory]
    [InlineData("js2_button11", "js", 2, InputKind.Button, 11, "", "button11")]
    [InlineData("js1_hat1_up", "js", 1, InputKind.Hat, 1, "up", "hat1_up")]
    [InlineData("js4_x", "js", 4, InputKind.Axis, 0, "x", "x")]
    [InlineData("js2_rotz", "js", 2, InputKind.Axis, 0, "rotz", "rotz")]
    [InlineData("js2_slider1", "js", 2, InputKind.Axis, 0, "slider1", "slider1")]
    [InlineData("js1_ ", "js", 1, InputKind.None, 0, "", "")]
    [InlineData("kb1_lalt", "kb", 1, InputKind.Key, 0, "lalt", "lalt")]
    [InlineData("mo1_mouse1", "mo", 1, InputKind.Named, 0, "mouse1", "mouse1")]
    [InlineData("gp1_thumbly", "gp", 1, InputKind.Named, 0, "thumbly", "thumbly")]
    [InlineData("y", "", 0, InputKind.Axis, 0, "y", "y")]
    [InlineData("button7", "", 0, InputKind.Button, 7, "", "button7")]
    public void An_input_string_comes_apart_into_device_instance_and_control(
        string raw, string device, int instance, InputKind kind, int index, string name, string control)
    {
        var input = ControlInput.Parse(raw);

        Assert.Equal(device, input.Device);
        Assert.Equal(instance, input.Instance);
        Assert.Equal(kind, input.Kind);
        Assert.Equal(index, input.Index);
        Assert.Equal(name, input.Name);
        Assert.Equal(control, input.Control);
        Assert.Equal(raw, input.Raw);
    }

    [Fact]
    public void A_modifier_chord_keeps_its_parts_and_retargets_as_one()
    {
        var chord = ControlInput.Parse("js2_button5+js2_button11");

        Assert.Equal(InputKind.Button, chord.Kind);
        Assert.Equal(11, chord.Index);
        Assert.Equal(5, Assert.Single(chord.Modifiers).Index);
        Assert.Equal("button 5 + button 11", chord.Label);
        Assert.Equal("js3_button5+js3_button11", chord.OnInstance(3));
        Assert.Equal("js3_ ", ControlInput.Parse("js1_ ").OnInstance(3));
    }

    /* ---------- profile ---------- */

    private const string Profile = """
        <ActionMaps>
         <ActionProfiles version="1" optionsVersion="2" rebindVersion="2" profileName="default">
          <deviceoptions name="Throttle - HOTAS Warthog  {0404044F-0000-0000-0000-504944564944}">
           <option input="z" deadzone="0.049499996"/>
           <option input="slider1" deadzone="0.049499996"/>
          </deviceoptions>
          <options type="keyboard" instance="1" Product="Keyboard  {6F1D2B61-D5A0-11CF-BFC7-444553540000}"/>
          <options type="joystick" instance="1" Product="F16 MFD 2  {B352044F-0000-0000-0000-504944564944}"/>
          <options type="joystick" instance="2" Product="Throttle - HOTAS Warthog  {0404044F-0000-0000-0000-504944564944}">
           <flight_strafe_longitudinal invert="1"/>
           <flight_strafe_forward exponent="2"/>
          </options>
          <options type="joystick" instance="7"/>
          <modifiers />
          <actionmap name="seat_general">
           <action name="v_eject">
            <rebind input="js6_button7"/>
           </action>
           <action name="v_light_amplification_toggle">
            <rebind input="js1_ "/>
           </action>
          </actionmap>
          <actionmap name="spaceship_movement">
           <action name="v_pitch">
            <rebind input="js4_y"/>
           </action>
           <action name="v_afterburner">
            <rebind input="js2_button11" activationMode="hold"/>
            <rebind input="kb1_lshift"/>
           </action>
          </actionmap>
         </ActionProfiles>
        </ActionMaps>
        """;

    [Fact]
    public void The_live_profile_reads_its_devices_with_product_guid_deadzones_and_curves()
    {
        var profile = ControlProfile.Parse(XDocument.Parse(Profile));

        Assert.Equal("default", profile.Name);
        Assert.Null(profile.Label);
        Assert.Equal(4, profile.Devices.Count);

        var throttle = profile.Devices.Single(d => d.Instance == 2 && d.Type == "joystick");
        Assert.Equal("js2", throttle.Key);
        Assert.Equal("Throttle - HOTAS Warthog", throttle.Product);
        Assert.Equal("{0404044F-0000-0000-0000-504944564944}", throttle.Guid);
        Assert.Equal((0x044F, 0x0404), throttle.Usb);
        Assert.Equal(0.049499996, throttle.Deadzones["z"], 6);
        Assert.Equal(2, throttle.Deadzones.Count);
        Assert.Contains(throttle.Curves, c => c.Option == "flight_strafe_longitudinal" && c.Inverted && c.Exponent is null);
        Assert.Contains(throttle.Curves, c => c.Option == "flight_strafe_forward" && c.Exponent == 2);

        // An instance nothing sits in has no product and no ids.
        var empty = profile.Devices.Single(d => d.Instance == 7);
        Assert.Null(empty.Product);
        Assert.Null(empty.Usb);
        Assert.Null(profile.Devices.Single(d => d.Type == "keyboard").Usb);
    }

    [Fact]
    public void Bindings_carry_their_action_input_and_activation_mode()
    {
        var profile = ControlProfile.Parse(XDocument.Parse(Profile));

        Assert.Equal(5, profile.Bindings.Count);
        var afterburner = profile.Bindings.Where(b => b.Action == "v_afterburner").ToList();
        Assert.Equal(2, afterburner.Count);
        Assert.Equal("hold", afterburner[0].ActivationMode);
        Assert.Equal(11, afterburner[0].Input.Index);
        Assert.Equal(InputKind.Key, afterburner[1].Input.Kind);
        Assert.Equal("Throttle - HOTAS Warthog", profile.DeviceOf(afterburner[0].Input)?.Product);

        // The game writes "js1_ " when a default joystick binding was taken away.
        var cleared = profile.Bindings.Single(b => b.Action == "v_light_amplification_toggle");
        Assert.Equal(InputKind.None, cleared.Input.Kind);
        Assert.Equal(1, cleared.Input.Instance);

        // A binding to an instance the profile has no line for is kept, with no device.
        Assert.Null(profile.DeviceOf(profile.Bindings.Single(b => b.Action == "v_eject").Input));
    }

    /// <summary>The export and the shipped layouts are the same document without the outer wrapper, plus a header.</summary>
    [Fact]
    public void An_exported_layout_reads_the_same_way_with_its_label()
    {
        var export = XDocument.Parse("""
            <ActionMaps version="1" optionsVersion="2" rebindVersion="2" profileName="Thrustmaster Warthog HOTAS (no rudder pedals)">
             <CustomisationUIHeader label="@ui_input_TM_Warthog_HOTAS" description="" image="">
              <devices><keyboard instance="1"/><joystick instance="1"/><joystick instance="2"/></devices>
             </CustomisationUIHeader>
             <options type="joystick" instance="1" Product="Joystick - HOTAS Warthog {0402044F-0000-0000-0000-504944564944}"/>
             <actionmap name="spaceship_movement">
              <action name="v_pitch"><rebind input="js1_y"/></action>
             </actionmap>
            </ActionMaps>
            """);

        var layout = ControlProfile.Parse(export);

        Assert.Equal("Thrustmaster Warthog HOTAS (no rudder pedals)", layout.Name);
        Assert.Equal("@ui_input_TM_Warthog_HOTAS", layout.Label);
        var stick = Assert.Single(layout.Joysticks);
        Assert.Equal("Joystick - HOTAS Warthog", stick.Product);
        Assert.Equal("{0402044F-0000-0000-0000-504944564944}", stick.Guid);
        Assert.Equal("js1_y", Assert.Single(layout.Bindings).Input.Raw);
    }

    [Fact]
    public void The_guid_composes_from_usb_ids_and_back()
    {
        Assert.Equal("{0404044F-0000-0000-0000-504944564944}", ControlDevice.GuidFor(0x044F, 0x0404));
        Assert.Equal((0x2341, 0x8037), ControlDevice.UsbOf("{80372341-0000-0000-0000-504944564944}"));
        Assert.Null(ControlDevice.UsbOf("{6F1D2B61-D5A0-11CF-BFC7-444553540000}"));
    }

    /* ---------- catalogue ---------- */

    [Fact]
    public void The_catalogue_labels_maps_and_actions_from_the_games_strings_and_falls_back_to_the_id()
    {
        var catalogue = ControlCatalogue.Parse(Encode(XDocument.Parse(Catalogue)), Text);

        Assert.Equal(3, catalogue.ActionMaps.Count);
        Assert.Equal(5, catalogue.Actions.Count);

        var seat = catalogue.ActionMaps[0];
        Assert.Equal("Seat - General", seat.Label);
        Assert.Equal("SEAT", seat.Category);

        var eject = catalogue.Find("seat_general", "v_eject")!;
        Assert.Equal("Eject", eject.Label);
        Assert.Equal("Leave the seat in a hurry", eject.Description);
        Assert.Equal("lalt+u", eject.Keyboard);
        Assert.Equal("", eject.Joystick);
        Assert.Equal("delayed_press_long", eject.ActivationMode);

        var pitch = catalogue.Find("spaceship_movement", "v_pitch")!;
        Assert.Equal("y", pitch.Joystick);
        Assert.Equal("flight_move_pitch", pitch.OptionGroup);

        // A string only under its ",P" platform line resolves; none at all leaves the id, not a blank or the raw @key.
        Assert.Equal("Light amplification", catalogue.Find("seat_general", "v_light_amplification_toggle")!.Label);
        Assert.Equal("mystery", catalogue.ActionMaps[2].Label);
        Assert.Equal("", catalogue.ActionMaps[2].Category);
        Assert.Equal("v_unlabelled", catalogue.Find("mystery", "v_unlabelled")!.Label);
    }
}
