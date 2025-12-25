using Arcraven.Avalonia.HMI.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Arcraven.Avalonia.HMI.Views;

public partial class GeneralView : UserControl
{
    public GeneralView()
    {
        InitializeComponent();
        this.DataContext = new GeneralViewModel();
    }
}