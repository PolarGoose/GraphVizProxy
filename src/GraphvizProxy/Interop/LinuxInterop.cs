using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace GraphVizProxy.Interop;

internal static class LinuxInterop
{
    private sealed class SafeGraphHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGraphHandle() : base(true) { }
        public SafeGraphHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { agclose(handle); return true; }
    }

    private sealed class SafeContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeContextHandle() : base(true) { }
        public SafeContextHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { gvFreeContext(handle); return true; }
    }

    private sealed class SafeRenderDataHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeRenderDataHandle() : base(true) { }
        public SafeRenderDataHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { gvFreeRenderData(handle); return true; }
    }

    // agmemread accepts "char*" that is a null-teminated UTF-8 string
    [DllImport("libcgraph.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern SafeGraphHandle agmemread(byte[] graphVizData);

    [DllImport("libcgraph.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void agclose(IntPtr file);

    [DllImport("libcgraph.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern string aglasterr();

    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern SafeContextHandle gvContext();

    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvFreeLayout(SafeContextHandle context, SafeGraphHandle graph);

    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvLayout(SafeContextHandle context, SafeGraphHandle graph, string engine);

    // The "result" is either a null-terminated UTF-8 string or a binary buffer, depending on the "format".
    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvRenderData(SafeContextHandle context, SafeGraphHandle graph, string format, out SafeRenderDataHandle result, out int length);

    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gvFreeRenderData(IntPtr buffer);

    [DllImport("libgvc.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvFreeContext(IntPtr gvc);

    public static byte[] Generate(string graphDescriptionInDotLanguage, LayoutEngine layoutEngine, OutputType outputFormat)
    {
        var nullTerminatedUtf8String = Encoding.UTF8.GetBytes(graphDescriptionInDotLanguage + '\0');

        using var graph = agmemread(nullTerminatedUtf8String);
        if (graph.IsInvalid)
            throw new ArgumentException($"Unable to read the given graph description\naglasterr: {aglasterr()}\nGraph description:\n{graphDescriptionInDotLanguage}");

        using var context = gvContext();

        if (gvLayout(context, graph, layoutEngine.Name) != 0)
            throw new ArgumentException($"Unable to create the gvContext using the layoutEngine={layoutEngine}\naglasterr: {aglasterr()}");

        if (gvRenderData(context, graph, outputFormat.Name, out var renderBuffer, out var length) != 0)
            throw new ArgumentException($"Unable to generate {outputFormat}\naglasterr: {aglasterr()}\nGraph description:\n{graphDescriptionInDotLanguage}");

        var data = new byte[length];
        Marshal.Copy(renderBuffer.DangerousGetHandle(), data, 0, length);

        return data;
    }
}
