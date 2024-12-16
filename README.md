# GraphVizProxy

.Net library to generate GraphViz output from DOT language.

# Supported platforms

* Windows - you need to have GraphViz binaries on your system.
* Linux - you need to have `libgraphviz-dev` package installed (because `libcgraph.so` and `libgvc.so` are used by this library).

# Features

* Generate various outputs like `PNG`, `SVG`, `JSON`.
* Generate rendering information like node positions, edge splines, drawing commands, etc.

# How it works

The library uses GraphViz binaries directly via P/Invoke.
To generate the rendering information, the library uses the JSON output and deserializes it into a convenient to use C# object.

# Examples
## Generate svg, png or other format from DOT language
```
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
```

## Generate rendering graph information
This example shows how you can get the rendering information about the graph.
It can be useful if you want to draw a graph yourself or if you want to use GraphViz to calculate the positions of UI elements.
```
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
```

# Prebuild event to download GraphViz binaries
If you have a Windows project, instead of downloading GraphViz binaries manually, you can use the following prebuild event.
It will download the required binaries to the `GraphViz_Binaries` folder next to the executable.
```
<Target Name="DownloadGraphviz" BeforeTargets="BeforeBuild" Condition="!Exists('$(BuildFolder)/Graphviz.zip')">
  <DownloadFile SourceUrl="https://gitlab.com/api/v4/projects/4207231/packages/generic/graphviz-releases/12.2.1/windows_10_cmake_Release_Graphviz-12.2.1-win64.zip" DestinationFolder="$(BuildFolder)" DestinationFileName="Graphviz.zip" />
  <Unzip SourceFiles="$(BuildFolder)Graphviz.zip" DestinationFolder="$(BuildFolder)" />
</Target>
<Target Name="CopyGraphvizBinariesToOutputFolder" AfterTargets="DownloadGraphviz" BeforeTargets="BeforeBuild" Condition="!Exists('$(OutputPath)GraphViz_Binaries')">
  <ItemGroup>
    <GraphVizBinaries Include="$(BuildFolder)/Graphviz-*/bin/*.dll;$(BuildFolder)/Graphviz-*/bin/config6" />
  </ItemGroup>
  <MakeDir Directories="$(OutputPath)GraphViz_Binaries" />
  <Copy SourceFiles="@(GraphVizBinaries)" DestinationFolder="$(OutputPath)GraphViz_Binaries" />
</Target>
```
