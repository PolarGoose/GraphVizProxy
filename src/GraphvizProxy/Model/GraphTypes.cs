using System.Globalization;

namespace GraphVizProxy.Model;

public sealed class Point
{
    public double X { get; }
    public double Y { get; }

    internal Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    // pos is a string like "1.0,2.0"
    internal Point(string pos)
    {
        var parts = pos.Split(',');
        X = double.Parse(parts[0]);
        Y = double.Parse(parts[1]);
    }
}

public sealed class BSpline
{
    public Point StartArrowTipPoint { get; }
    public Point EndArrowTipPoint { get; }
    public Point[] ControlPoints { get; }

    // pos is a string like "s,76.067,220 e,201.42,103.3 82.756,210.74 96.784,191.68 117.56,165.65 139.3,146.21 155.28,131.92 175.39,118.71 191.77,108.93"
    internal BSpline(string pos)
    {
        var controlPoints = new List<Point>();

        // Split the pos string by spaces to get tokens like:
        // "s,23.45,45.32", "50.00,60.00", "60.30,120.22", "44.22,22.23", "e,33.32,60.54"
        var tokens = pos.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            // Check for arrow prefix "s," or "e,"
            if (token.StartsWith("s,")) // start arrow tip
            {
                var xy = token.Substring(2).Split(','); // remove "s," then split
                StartArrowTipPoint = new Point(double.Parse(xy[0], CultureInfo.InvariantCulture), double.Parse(xy[1], CultureInfo.InvariantCulture));
            }
            else if (token.StartsWith("e,")) // end arrow tip
            {
                var xy = token.Substring(2).Split(','); // remove "e," then split
                EndArrowTipPoint = new Point(double.Parse(xy[0], CultureInfo.InvariantCulture), double.Parse(xy[1], CultureInfo.InvariantCulture));
            }
            else
            {
                var xy = token.Split(',');
                controlPoints.Add(new Point(double.Parse(xy[0], CultureInfo.InvariantCulture), double.Parse(xy[1], CultureInfo.InvariantCulture)));
            }
        }

        ControlPoints = controlPoints.ToArray();
    }
}

public sealed class BoundingBox
{
    public Point BottomLeft { get; }
    public Point TopRight { get; }

    internal BoundingBox(Point bottomLeft, Point topRight)
    {
        BottomLeft = bottomLeft;
        TopRight = topRight;
    }

    // bb is a string like "1.0,2.0,3.0,4.0",
    internal BoundingBox(string bb)
    {
        var xy1_xy2 = bb.Split(',').Select(el => double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
        BottomLeft = new Point(xy1_xy2[0], xy1_xy2[1]);
        TopRight = new Point(xy1_xy2[2], xy1_xy2[3]);
    }
}

public sealed class Node
{
    public string Name { get; }
    public uint GvId { get; }
    public string Label { get; }
    public string Comment { get; }
    public Point Position { get; }
    public string Style { get; }
    public string Shape { get; }
    public double Height { get; }
    public double Width { get; }
    public string Color { get; }
    public DrawCommand[] DrawCommands { get; }
    public DrawCommand[] LabelDrawCommands { get; }

    internal Node(JsonClusterOrNode node)
    {
        Name = node.name;
        GvId = node._gvid;
        Label = node.label;
        Comment = node.comment;
        Position = new Point(node.pos);
        Style = node.style;
        Shape = node.shape;
        Height = node.height;
        Width = node.width;
        Color = node.color;
        DrawCommands = DrawCommand.FromJson(node._draw_);
        LabelDrawCommands = DrawCommand.FromJson(node._ldraw_);
    }
}

public sealed class Edge
{
    public string Name { get; }
    public uint GvId { get; }
    public string Comment { get; }
    public string Style { get; }
    public string Label { get; }
    public Node HeadNode { get; }
    public Node TailNode { get; }
    public BSpline Path { get; }
    public DrawCommand[] EdgeDrawCommands { get; }
    public DrawCommand[] ArrowHeadDrawCommands { get; }
    public DrawCommand[] ArrowTailDrawCommands { get; }
    public DrawCommand[] EdgeLabelDrawCommands { get; }
    public DrawCommand[] HeadArrowLabelDrawCommands { get; }
    public DrawCommand[] TailArrowLabelDrawCommands { get; }

    internal Edge(JsonEdge edge, Dictionary<uint, Node> nodes)
    {
        Name = edge.name;
        GvId = edge._gvid;
        Comment = edge.comment;
        Label = edge.label;
        HeadNode = nodes[edge.head];
        TailNode = nodes[edge.tail];
        Path = new BSpline(edge.pos);
        EdgeDrawCommands = DrawCommand.FromJson(edge._draw_);
        ArrowHeadDrawCommands = DrawCommand.FromJson(edge._hdraw_);
        ArrowTailDrawCommands = DrawCommand.FromJson(edge._tdraw_);
        EdgeLabelDrawCommands = DrawCommand.FromJson(edge._ldraw_);
        HeadArrowLabelDrawCommands = DrawCommand.FromJson(edge._hldraw_);
        TailArrowLabelDrawCommands = DrawCommand.FromJson(edge._tldraw_);
    }
}

public sealed class Cluster
{
    public string Name { get; }
    public uint GvId { get; }
    public string Label { get; }
    public string Comment { get; }
    public double Width { get; }
    public string Style { get; }
    public string Shape { get; }
    public string Color { get; }
    public Node[] Nodes { get; }
    public Edge[] Edges { get; }
    public BoundingBox BoundingBox { get; }
    public DrawCommand[] DrawCommands { get; }
    public DrawCommand[] LabelDrawCommands { get; }

    internal Cluster(JsonClusterOrNode cluster, Dictionary<uint, Node> nodes, Dictionary<uint, Edge> edges)
    {
        Name = cluster.name;
        GvId = cluster._gvid;
        Label = cluster.label;
        Comment = cluster.comment;
        Width = cluster.width;
        Style = cluster.style;
        Shape = cluster.shape;
        Color = cluster.color;
        Nodes = cluster.nodes?.Select(gvId => nodes[gvId]).ToArray();
        Edges = cluster.edges?.Select(gvId => edges[gvId]).ToArray();
        if(cluster.bb != null) BoundingBox = new BoundingBox(cluster.bb);
        DrawCommands = DrawCommand.FromJson(cluster._draw_);
        LabelDrawCommands = DrawCommand.FromJson(cluster._ldraw_);
    }
}

public sealed class Graph
{
    public BoundingBox BoundingBox { get; }
    public string Name { get; }
    public string Comment { get; }
    public bool Directed { get; }
    public bool Strict { get; }
    public string Label { get; }
    public Cluster[] Clusters { get; }
    public Node[] Nodes { get; }
    public Edge[] Edges { get; }
    public DrawCommand[] DrawCommands { get; }
    public DrawCommand[] LabelDrawCommands { get; }

    internal Graph(JsonGraph deserializedGraph)
    {
        BoundingBox = new BoundingBox(deserializedGraph.bb);
        Name = deserializedGraph.name;
        Comment = deserializedGraph.comment;
        Directed = deserializedGraph.directed == "true";
        Strict = deserializedGraph.strict == "true";
        Label = deserializedGraph.label;

        var nodes = deserializedGraph.objects.Where(o => !o.name.StartsWith("cluster") && !o.name.StartsWith("%")).Select(n => new Node(n)).ToDictionary(n => n.GvId);
        var edges = deserializedGraph.edges.Select(e => new Edge(e, nodes)).ToDictionary(e => e.GvId);

        Nodes = nodes.Values.ToArray();
        Edges = edges.Values.ToArray();
        Clusters = deserializedGraph.objects.Where(o => o.name.StartsWith("cluster") || o.name.StartsWith("%")).Select(c => new Cluster(c, nodes, edges)).ToArray();

        DrawCommands = DrawCommand.FromJson(deserializedGraph._draw_);
        LabelDrawCommands = DrawCommand.FromJson(deserializedGraph._ldraw_);
    }
}
