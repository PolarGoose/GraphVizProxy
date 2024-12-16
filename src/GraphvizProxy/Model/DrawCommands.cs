namespace GraphVizProxy.Model;

// The draw commands are described in the https://graphviz.org/docs/outputs/canon/#xdot

public abstract class DrawCommand
{
    internal static DrawCommand[] FromJson(JsonDrawCommand[] drawCommands)
    {
        if (drawCommands == null)
        {
            return Array.Empty<DrawCommand>();
        }

        return drawCommands.Select(FromJson).ToArray();
    }

    private static DrawCommand FromJson(JsonDrawCommand drawCommand)
    {
        switch (drawCommand.op)
        {
            case "e":
                return new UnfilledEllipseDrawCommand(drawCommand);
            case "E":
                return new FilledEllipseDrawCommand(drawCommand);
            case "p":
                return new UnfilledPolygonDrawCommand(drawCommand);
            case "P":
                return new FilledPolygonDrawCommand(drawCommand);
            case "L":
                return new PolyLineDrawCommand(drawCommand);
            case "B":
                return new FilledBSplineDrawCommand(drawCommand);
            case "b":
                return new UnfilledBSplineDrawCommand(drawCommand);
            case "T":
                return new TextDrawCommand(drawCommand);
            case "t":
                return new SetFontCharacteristicsDrawCommand(drawCommand);
            case "c":
                return new SetPenColorDrawCommand(drawCommand);
            case "C":
                return new SetFillColorDrawCommand(drawCommand);
            case "F":
                return new SetFontDrawCommand(drawCommand);
            case "S":
                return new SetStyleDrawCommand(drawCommand);
            default:
                throw new ArgumentOutOfRangeException(nameof(drawCommand.op), drawCommand.op, $"Unknown draw command");
        }
    }
}

public abstract class EllipseDrawCommand : DrawCommand
{
    public Point Center { get; }
    public double W { get; } // The distance from the center to the furthest point on the ellipse along the x-axis.
    public double H { get; } // The distance from the center to the furthest point on the ellipse along the y-axis.

    internal EllipseDrawCommand(JsonDrawCommand deserializedCommand)
    {
        Center = new Point(deserializedCommand.rect[0], deserializedCommand.rect[1]);
        W = deserializedCommand.rect[2];
        H = deserializedCommand.rect[3];
    }
}

public sealed class FilledEllipseDrawCommand : EllipseDrawCommand
{
    internal FilledEllipseDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public sealed class UnfilledEllipseDrawCommand : EllipseDrawCommand
{
    internal UnfilledEllipseDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public abstract class PolygonDrawCommand : DrawCommand
{
    public Point[] Points { get; }
    internal PolygonDrawCommand(JsonDrawCommand deserializedCommand)
    {
        Points = deserializedCommand.points.Select(p => new Point(p[0], p[1])).ToArray();
    }
}

public sealed class FilledPolygonDrawCommand : PolygonDrawCommand
{
    internal FilledPolygonDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public sealed class UnfilledPolygonDrawCommand : PolygonDrawCommand
{
    internal UnfilledPolygonDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public sealed class PolyLineDrawCommand : DrawCommand
{
    public Point[] Points { get; }
    internal PolyLineDrawCommand(JsonDrawCommand deserializedCommand)
    {
        Points = deserializedCommand.points.Select(p => new Point(p[0], p[1])).ToArray();
    }
}

public abstract class BSplineDrawCommand : DrawCommand
{
    public Point[] Points { get; }
    internal BSplineDrawCommand(JsonDrawCommand deserializedCommand)
    {
        Points = deserializedCommand.points.Select(p => new Point(p[0], p[1])).ToArray();
    }
}

public sealed class FilledBSplineDrawCommand : BSplineDrawCommand
{
    internal FilledBSplineDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public sealed class UnfilledBSplineDrawCommand : BSplineDrawCommand
{
    internal UnfilledBSplineDrawCommand(JsonDrawCommand deserializedCommand) : base(deserializedCommand) { }
}

public enum TextAlignment { Left, Center, Right }

public sealed class TextDrawCommand : DrawCommand
{
    public Point BottomLeft { get; }
    public double TextWidth { get; }
    public TextAlignment TextAlignment { get; }
    public string Text { get; }

    internal TextDrawCommand(JsonDrawCommand deserializedCommand)
    {
        BottomLeft = new Point(deserializedCommand.pt[0], deserializedCommand.pt[1]);
        TextWidth = deserializedCommand.width;
        TextAlignment = deserializedCommand.align switch
        {
            "l" => TextAlignment.Left,
            "c" => TextAlignment.Center,
            "r" => TextAlignment.Right,
            _ => throw new ArgumentException($"Unknown text alignment: {deserializedCommand.align}")
        };
        Text = deserializedCommand.text;
    }
}

public sealed class SetFontCharacteristicsDrawCommand : DrawCommand
{
    public bool IsBold { get; }
    public bool IsItalic { get; }
    public bool IsUnderline { get; }
    public bool IsSuperscript { get; }
    public bool IsSubscript { get; }
    public bool IsStrikeThrough { get; }
    public bool IsOverline { get; }

    internal SetFontCharacteristicsDrawCommand(JsonDrawCommand deserializedCommand)
    {
        var fontchar = deserializedCommand.fontchar;

        IsBold = (fontchar & 1) != 0;
        IsItalic = (fontchar & 2) != 0;
        IsUnderline = (fontchar & 4) != 0;
        IsSuperscript = (fontchar & 8) != 0;
        IsSubscript = (fontchar & 16) != 0;
        IsStrikeThrough = (fontchar & 32) != 0;
        IsOverline = (fontchar & 64) != 0;
    }
}

public sealed class SetFillColorDrawCommand : DrawCommand
{
    public string FillColor { get; }
    public string GradientDescription { get; }
    internal SetFillColorDrawCommand(JsonDrawCommand deserializedCommand)
    {
        FillColor = deserializedCommand.color;
        GradientDescription = deserializedCommand.grad;
    }
}

public sealed class SetPenColorDrawCommand : DrawCommand
{
    public string PenColor { get; }
    public string GradientDescription { get; }
    internal SetPenColorDrawCommand(JsonDrawCommand deserializedCommand)
    {
        PenColor = deserializedCommand.color;
        GradientDescription = deserializedCommand.grad;
    }
}

public sealed class SetFontDrawCommand : DrawCommand
{
    public double FontSize { get; }
    public string FontFace { get; }
    internal SetFontDrawCommand(JsonDrawCommand deserializedCommand)
    {
        FontFace = deserializedCommand.face;
        FontSize = deserializedCommand.size;
    }
}

public sealed class SetStyleDrawCommand : DrawCommand
{
    public string Style { get; } // Style names: https://graphviz.org/docs/attr-types/style/
    internal SetStyleDrawCommand(JsonDrawCommand deserializedCommand)
    {
        Style = deserializedCommand.style;
    }
}
