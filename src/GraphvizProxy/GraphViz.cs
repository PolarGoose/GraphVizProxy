using GraphVizProxy.Interop;
using GraphVizProxy.Model;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;

namespace GraphVizProxy;

// https://graphviz.org/docs/layouts/
public sealed class LayoutEngine
{
    public string Name { get; }
    public LayoutEngine(string name) => Name = name;
    public override string ToString() => Name;

    public static readonly LayoutEngine Dot = new("dot");
    public static readonly LayoutEngine Neato = new("neato");
    public static readonly LayoutEngine Fdp = new("fdp");
    public static readonly LayoutEngine Sfdp = new("sfdp");
    public static readonly LayoutEngine Circo = new("circo");
    public static readonly LayoutEngine Twopi = new("twopi");
    public static readonly LayoutEngine Nop = new("nop");
    public static readonly LayoutEngine Nop2 = new("nop2");
    public static readonly LayoutEngine Osage = new("osage");
    public static readonly LayoutEngine Patchwork = new("patchwork");
}

// https://graphviz.org/docs/outputs/
public sealed class OutputType
{
    public string Name { get; }
    public OutputType(string name) => Name = name;
    public override string ToString() => Name;

    // String output
    public static readonly OutputType Canon = new("canon");
    public static readonly OutputType Dot = new("dot");
    public static readonly OutputType Gv = new("gv");
    public static readonly OutputType Xdot = new("xdot");
    public static readonly OutputType Xdot12 = new("xdot1.2");
    public static readonly OutputType Xdot14 = new("xdot1.4");
    public static readonly OutputType Eps = new("eps");
    public static readonly OutputType Fig = new("fig");
    public static readonly OutputType Imap = new("imap");
    public static readonly OutputType ImapNp = new("imap_np");
    public static readonly OutputType Ismap = new("ismap");
    public static readonly OutputType Cmap = new("cmap");
    public static readonly OutputType Cmapx = new("cmapx");
    public static readonly OutputType CmapxNp = new("cmapx_np");
    public static readonly OutputType Json = new("json");
    public static readonly OutputType Json0 = new("json0");
    public static readonly OutputType DotJson = new("dot_json");
    public static readonly OutputType XdotJson = new("xdot_json");
    public static readonly OutputType Pic = new("pic");
    public static readonly OutputType Plain = new("plain");
    public static readonly OutputType PlainExt = new("plain-ext");
    public static readonly OutputType Pov = new("pov");
    public static readonly OutputType Ps = new("ps");
    public static readonly OutputType Ps2 = new("ps2");
    public static readonly OutputType Svg = new("svg");
    public static readonly OutputType Vrml = new("vrml");
    public static readonly OutputType Tk = new("tk");

    // Binary output
    public static readonly OutputType Bmp = new("bmp");
    public static readonly OutputType Gd = new("gd");
    public static readonly OutputType Gd2 = new("gd2");
    public static readonly OutputType Gif = new("gif");
    public static readonly OutputType Jpg = new("jpg");
    public static readonly OutputType Jpeg = new("jpeg");
    public static readonly OutputType Jpe = new("jpe");
    public static readonly OutputType Pdf = new("pdf");
    public static readonly OutputType Png = new("png");
    public static readonly OutputType Svgz = new("svgz");
    public static readonly OutputType Tif = new("tif");
    public static readonly OutputType Tiff = new("tiff");
    public static readonly OutputType Wbmp = new("wbmp");
    public static readonly OutputType Webp = new("webp");
}

public static class GraphViz
{
    // For example "C:/Program Files/Graphviz/bin".
    // This method needs to be run before calling "Process*" methods.
    // You don't need to call this method is the GraphViz binaries are in the PATH or next to the executable.
    public static void SetGraphVizBinariesLocation(string folderWithGraphVizBinariesFullName)
    {
        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + folderWithGraphVizBinariesFullName);
    }

    public static byte[] Generate(string graphDescriptionInDotLanguage, LayoutEngine layoutEngine, OutputType outputFormat)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsInterop.Generate(graphDescriptionInDotLanguage, layoutEngine, outputFormat);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxInterop.Generate(graphDescriptionInDotLanguage, layoutEngine, outputFormat);
        }
        else
        {
            throw new PlatformNotSupportedException($"The platform '{RuntimeInformation.OSDescription}' is not supported.");
        }
    }

    public static Graph Generate(string graphDescriptionInDotLanguage, LayoutEngine layoutEngine)
    {
        var json = Generate(graphDescriptionInDotLanguage, layoutEngine, OutputType.Json);
        using var stream = new MemoryStream(json);
        var deserialized = (JsonGraph)new DataContractJsonSerializer(typeof(JsonGraph)).ReadObject(stream);
        return new Graph(deserialized);
    }
}
