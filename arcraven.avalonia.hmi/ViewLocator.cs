using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Arcraven.Avalonia.ResourcesLib.ViewModels;
using Arcraven.Avalonia.ResourcesLib.Models;

namespace Arcraven.Avalonia.HMI;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _viewCache = new();

    public Control? Build(object? param)
    {
        if (param is null) return null;
        
        if (_viewCache.TryGetValue(param, out var cachedView))
        {
            return cachedView;
        }

        var name = param.GetType().FullName!
            .Replace("ViewModels", "Views", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var type = Type.GetType(name);
        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            
            var attr = param.GetType().GetCustomAttribute<ShellPageAttribute>();

            if (attr is { IsPersistent: true })
            {
                _viewCache[param] = control;
            }

            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}