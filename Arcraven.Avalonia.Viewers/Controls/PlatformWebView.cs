using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Arcraven.Avalonia.Viewers.Controls;

public sealed class PlatformWebView : NativeControlHost
{
    public static readonly StyledProperty<Uri?> SourceProperty =
        AvaloniaProperty.Register<PlatformWebView, Uri?>(nameof(Source));

    private IWebViewBackend? _backend;

    public Uri? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public PlatformWebView()
    {
        this.GetObservable(SourceProperty).Subscribe(new SourceObserver(this));
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _backend = WebViewBackendFactory.Create(parent);
        if (_backend != null)
        {
            _backend.Navigate(Source);
            return _backend.Handle;
        }

        return base.CreateNativeControlCore(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _backend?.Dispose();
        _backend = null;
        base.DestroyNativeControlCore(control);
    }

    private sealed class SourceObserver : IObserver<Uri?>
    {
        private readonly PlatformWebView _owner;

        public SourceObserver(PlatformWebView owner)
        {
            _owner = owner;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Uri? value)
        {
            _owner._backend?.Navigate(value);
        }
    }
}