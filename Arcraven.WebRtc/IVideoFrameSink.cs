namespace Arcraven.WebRtc;

public interface IVideoFrameSink
{
    /// <summary>
    /// Push a BGRA32 frame.
    /// </summary>
    void OnFrameBgra32(ReadOnlySpan<byte> bgra, int width, int height, int strideBytes);
}