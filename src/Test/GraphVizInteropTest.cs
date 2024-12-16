using GraphVizProxy;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Test;

[TestFixture]
public class GraphVizInteropTest
{
    private readonly string testDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    [SetUp]
    public void SetUp()
    {
        GraphViz.SetGraphVizBinariesLocation($"{testDllDir}/GraphViz_Binaries");
    }

    [Test]
    public void Returned_string_does_not_have_null_symbols_at_the_end()
    {
        var dot = "digraph G { a -> b }";
        var s = GraphViz.Generate(dot, LayoutEngine.Dot, OutputType.Dot);
        Assert.That(s.Last(), Is.Not.EqualTo('\0'));
    }

    [Test]
    public void Support_for_Unicode_symbols()
    {
        var dot = "digraph ザ { трじα -> b }";
        var output = GraphViz.Generate(dot, LayoutEngine.Dot, OutputType.Dot);
        Assert.That(Encoding.UTF8.GetString(output), Does.Contain("трじα"));
    }

    [Test]
    public void All_GraphViz_layouts_and_output_formats_are_supported()
    {
        var dot = "digraph G { a -> b }";
        foreach (var layoutProp in typeof(LayoutEngine).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            var layout = (LayoutEngine)layoutProp.GetValue(null);
            foreach (var outputTypeProp in typeof(OutputType).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var outputType = (OutputType)outputTypeProp.GetValue(null);

                // On Linux Webp format causes the following error on "Ubuntu 22.04.5 LTS":
                //   The active test run was aborted. Reason: Test host process crashed : /usr/share/dotnet/dotnet: symbol lookup error: /usr/lib/x86_64-linux-gnu/graphviz/libgvplugin_webp.so.6: undefined symbol: gvwrite
                if (outputType == OutputType.Webp && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Skip Webp format test on Linux");
                    continue;
                }

                Assert.DoesNotThrow(() => GraphViz.Generate(dot, layout, outputType));
            }
        }
    }

    [Test]
    public void Generates_graph_for_complex_input()
    {
        var dot = File.ReadAllText($"{testDllDir}/TestComplexInput.dot");
        var graph = GraphViz.Generate(dot, LayoutEngine.Dot);
    }

    [Test]
    public void Generates_graph_properly()
    {
        var dot = File.ReadAllText($"{testDllDir}/TestInput.dot");

        // Generate TestInput.json and TestOutput.json files
        // so that they can be manually compared to ensure that the output is correct.
        var json = GraphViz.Generate(dot, LayoutEngine.Dot, OutputType.Json);
        File.WriteAllText($"{testDllDir}/TestInput.json", Encoding.UTF8.GetString(json));
        var graph = GraphViz.Generate(dot, LayoutEngine.Dot);
        string jsonString = JsonConvert.SerializeObject(graph, Formatting.Indented);
        File.WriteAllText($"{testDllDir}/TestOutput.json", jsonString);

        // Check some properties of the generated graph.
        Assert.That(graph.Label, Is.EqualTo("Test Graph Label"));
        Assert.That(graph.Nodes.Count, Is.EqualTo(10));
        Assert.That(graph.Edges.Count, Is.EqualTo(13));
        Assert.That(graph.Clusters.Count, Is.EqualTo(2));
    }
}
