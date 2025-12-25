namespace Arcraven.Avalonia.ResourcesLib.Models;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ShellPageAttribute : Attribute
{
    public string Key { get; }
    public string Title { get; }
    public string IconUri { get; }
    public int Order { get; }
    public bool IsPersistent { get; }
    public ShellPageAttribute(string key, string title, string iconUri, int order = 0, bool isPersistent = false)
    {
        Key = key;
        Title = title;
        IconUri = iconUri;
        Order = order;
        IsPersistent = isPersistent;
    }
}