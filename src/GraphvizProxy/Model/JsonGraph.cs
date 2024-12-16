using System.Runtime.Serialization;

namespace GraphVizProxy.Model;

#pragma warning disable CS0649

[DataContract]
class JsonGradientStops
{
    [DataMember] public double frac;
    [DataMember] public string color;
}

[DataContract]
class JsonDrawCommand
{
    [DataMember] public string op;
    [DataMember] public string grad;
    [DataMember] public double[] p0;
    [DataMember] public double[] p1;
    [DataMember] public JsonGradientStops[] stops;
    [DataMember] public string color;
    [DataMember] public double[][] points;
    [DataMember] public double[] pt;
    [DataMember] public string align;
    [DataMember] public double width;
    [DataMember] public string text;
    [DataMember] public uint fontchar;
    [DataMember] public double size;
    [DataMember] public string face;
    [DataMember] public string style;
    [DataMember] public double[] rect;
}

// All possible attributes:
// * Cluster: https://graphviz.org/docs/clusters/
// * Node: https://graphviz.org/docs/nodes/
[DataContract]
class JsonClusterOrNode
{
    [DataMember] public string name;
    [DataMember] public uint _gvid;
    [DataMember] public string label;
    [DataMember] public string comment;
    [DataMember] public uint[] nodes;
    [DataMember] public uint[] edges;
    [DataMember] public string pos;
    [DataMember] public string shape;
    [DataMember] public double height;
    [DataMember] public double width;
    [DataMember] public string bb;
    [DataMember] public string color;
    [DataMember] public string style;
    [DataMember] public double lheight;
    [DataMember] public double lwidth;
    [DataMember] public string lp;
    [DataMember] public JsonDrawCommand[] _draw_;
    [DataMember] public JsonDrawCommand[] _ldraw_;
}

// All possible attributes: https://graphviz.org/docs/edges/
[DataContract]
class JsonEdge
{
    [DataMember] public uint _gvid;
    [DataMember] public string name;
    [DataMember] public string label;
    [DataMember] public string comment;
    [DataMember] public uint tail;
    [DataMember] public uint head;
    [DataMember] public string pos;
    [DataMember] public JsonDrawCommand[] _draw_;
    [DataMember] public JsonDrawCommand[] _hdraw_;
    [DataMember] public JsonDrawCommand[] _hldraw_;
    [DataMember] public JsonDrawCommand[] _ldraw_;
    [DataMember] public JsonDrawCommand[] _tdraw_;
    [DataMember] public JsonDrawCommand[] _tldraw_;
}

// All possible attributes: https://graphviz.org/docs/graph/
[DataContract]
class JsonGraph
{
    [DataMember] public string name;
    [DataMember] public string comment;
    [DataMember] public string bb;
    [DataMember] public string directed;
    [DataMember] public string strict;
    [DataMember] public string label;
    [DataMember] public JsonClusterOrNode[] objects;
    [DataMember] public JsonEdge[] edges;
    [DataMember] public JsonDrawCommand[] _draw_;
    [DataMember] public JsonDrawCommand[] _ldraw_;
}
