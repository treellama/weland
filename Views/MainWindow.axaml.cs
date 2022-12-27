using System.Reactive;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Weland.ViewModels;
using ReactiveUI;

namespace Weland.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.ShowOpenFileDialog.RegisterHandler(ShowOpenFileDialog)));
    }

    private async Task ShowOpenFileDialog(InteractionContext<Unit, string?> interaction)
    {
        var dialog = new OpenFileDialog();
        var filenames = await dialog.ShowAsync(this);
        interaction.SetOutput(filenames?.FirstOrDefault());
    }
}
