using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Arcraven.WebRtc.Avalonia;

public sealed class WebRtcVideoView : Control, Arcraven.WebRtc.IVideoFrameSink
{
    private WriteableBitmap? _bmp;
    private int _w, _h, _stride;

    // backpressure: keep only latest frame
    private byte[]? _latest;
    private int _latestLen;
    private int _latestW, _latestH, _latestStride;
    private int _uiPending; // 0/1

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<WebRtcVideoView, Stretch>(nameof(Stretch), Stretch.Uniform);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public void OnFrameBgra32(ReadOnlySpan<byte> bgra, int width, int height, int strideBytes)
    {
        int srcBytes = checked(strideBytes * height);

        // Copy immediately (span cannot outlive this call)
        byte[] buf = ArrayPool<byte>.Shared.Rent(srcBytes);
        bgra.CopyTo(buf.AsSpan(0, srcBytes));

        // Swap "latest", return previous buffer to pool
        var prev = Interlocked.Exchange(ref _latest, buf);
        _latestLen = srcBytes;
        _latestW = width;
        _latestH = height;
        _latestStride = strideBytes;

        if (prev is not null)
            ArrayPool<byte>.Shared.Return(prev);

        // Ensure only one UI work item pending (prevents backlog at 30/60fps)
        if (Interlocked.Exchange(ref _uiPending, 1) == 0)
        {
            Dispatcher.UIThread.Post(RenderLatestFrame, DispatcherPriority.Render);
        }
    }

    private void RenderLatestFrame()
    {
        try
        {
            var frame = Interlocked.Exchange(ref _latest, null);
            if (frame is null) return;

            int width = _latestW;
            int height = _latestH;
            int strideBytes = _latestStride;
            int srcBytes = _latestLen;

            EnsureBitmap(width, height, strideBytes);
            if (_bmp is null) return;

            using var fb = _bmp.Lock();
            var dstStride = fb.RowBytes;
            var dstBase = fb.Address;

            if (dstStride == strideBytes)
            {
                Marshal.Copy(frame, 0, dstBase, srcBytes);
            }
            else
            {
                int rowCopy = Math.Min(strideBytes, dstStride);
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(
                        frame,
                        y * strideBytes,
                        IntPtr.Add(dstBase, y * dstStride),
                        rowCopy);
                }
            }

            InvalidateVisual();

            ArrayPool<byte>.Shared.Return(frame);
        }
        finally
        {
            Volatile.Write(ref _uiPending, 0);

            // If a newer frame arrived while rendering, schedule another pass.
            if (Volatile.Read(ref _latest) is not null &&
                Interlocked.Exchange(ref _uiPending, 1) == 0)
            {
                Dispatcher.UIThread.Post(RenderLatestFrame, DispatcherPriority.Render);
            }
        }
    }

    private void EnsureBitmap(int width, int height, int strideBytes)
    {
        if (_bmp != null && width == _w && height == _h && strideBytes == _stride)
            return;

        _w = width;
        _h = height;
        _stride = strideBytes;

        _bmp = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_bmp is null) return;

        var src = new Rect(0, 0, _bmp.PixelSize.Width, _bmp.PixelSize.Height);

        var scaled = Stretch.CalculateScaling(Bounds.Size, _bmp.Size, StretchDirection.Both);
        var drawSize = _bmp.Size * scaled;
        var drawRect = new Rect(
            x: (Bounds.Width - drawSize.Width) * 0.5,
            y: (Bounds.Height - drawSize.Height) * 0.5,
            width: drawSize.Width,
            height: drawSize.Height);

        context.DrawImage(_bmp, src, drawRect);
    }
}