using Arcraven.Avalonia.HMI.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Arcraven.Avalonia.HMI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        this.DataContext = new SettingsViewModel();
    }
}