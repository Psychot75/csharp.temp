using Arcraven.Avalonia.ResourcesLib.ViewModels;

namespace Arcraven.Avalonia.ResourcesLib.Models;

public sealed class PageDefinition
{
    private ViewModelBase? _cachedInstance;

    public string Key { get; }
    public string Title { get; }
    public string IconUri { get; }
    public bool IsPersistent { get; }
    public Func<ViewModelBase> Factory { get; }

    public PageDefinition(string key, string title, string iconUri, Func<ViewModelBase> factory, bool isPersistent)
    {
        Key = key;
        Title = title;
        IconUri = iconUri;
        Factory = factory;
        IsPersistent = isPersistent;
    }

    public ViewModelBase GetInstance()
    {
        if (IsPersistent)
        {
            _cachedInstance ??= Factory();
            return _cachedInstance;
        }
        return Factory();
    }

    public override string ToString() => Title;
}