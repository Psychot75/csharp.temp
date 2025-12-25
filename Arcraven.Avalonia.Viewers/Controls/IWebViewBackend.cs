using System;
using Avalonia.Platform;

namespace Arcraven.Avalonia.Viewers.Controls;

internal interface IWebViewBackend : IDisposable
{
    IPlatformHandle Handle { get; }
    void Navigate(Uri? source);
}