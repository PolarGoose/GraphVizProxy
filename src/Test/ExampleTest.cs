using GraphVizProxy;
using NUnit.Framework;
using System.Reflection;
using System.Text;
namespace Test;

[TestFixture]
internal class ExampleTest
{
    [Test]
    public void Generate_various_outputs()
    {
        // Assume you have a graph description in the DOT language
        var graphDescription = "digraph G { a -> b }";

        // First you need to specify the location of the GraphViz binaries.
        // This location needs to contain dlls like "gvc.dll" and "cgraph.dll".
        // This step is not needed on Linux if you have the binaries installed via a package manager.
        GraphViz.SetGraphVizBinariesLocation($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/GraphViz_Binaries");

        // You can generate various outputs using GraphViz binaries

        var pngBinaryArray = GraphViz.Generate(graphDescription, LayoutEngine.Dot, OutputType.Png);

        // For text outputs you need to use `Encoding.UTF8.GetString` to get the string
        var svgBinaryArray = GraphViz.Generate(graphDescription, LayoutEngine.Dot, OutputType.Svg);
        var svg = Encoding.UTF8.GetString(svgBinaryArray);

        var jsonBinaryArray = GraphViz.Generate(graphDescription, LayoutEngine.Dot, OutputType.Json);
        var json = Encoding.UTF8.GetString(jsonBinaryArray);
    }

    [Test]
    public void Generate_rendering_graph_information()
    {
        // Assume that you have a graph description in the DOT language
        // and you want to get all information about how the graph should be rendered.
        // For example, node positions, edge splines, drawing commands, etc.
        var graphDescription = "digraph G { a -> b }";

        // First you need to specify the location of the GraphViz binaries.
        // This location needs to contain dlls like "gvc.dll" and "cgraph.dll".
        // This step is not needed on Linux if you have the binaries installed via a package manager.
        GraphViz.SetGraphVizBinariesLocation($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/GraphViz_Binaries");

        // You can generate the `Graph` object. It uses information from the GraphViz JSON output.
        var graph = GraphViz.Generate(graphDescription, LayoutEngine.Dot);

        // Then you can access the Graph object to get access to the rendering information.
        foreach(var node in graph.Nodes)
        {
            Console.WriteLine($"Node {node.Name} {node.Label}");
            foreach (var drawCommand in node.DrawCommands)
            {
                Console.WriteLine($"Command {drawCommand.GetType()}");
            }
        }
        foreach (var edge in graph.Edges)
        {
            Console.WriteLine($"Node {edge.Name} {edge.Label}");
        }
    }
}
