using Arcraven.Avalonia.HMI.ViewModels;
using Avalonia.Controls;

namespace Arcraven.Avalonia.HMI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new ShellViewModel();
    }
}