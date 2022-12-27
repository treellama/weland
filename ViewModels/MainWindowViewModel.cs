using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace Weland.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        OpenFile = ReactiveCommand.CreateFromTask(OpenFileAsync);

        ShowOpenFileDialog = new Interaction<Unit, string?>();
    }

    public ReactiveCommand<Unit, Unit> OpenFile { get; }

    public Interaction<Unit, string?> ShowOpenFileDialog { get; }

    private async Task OpenFileAsync()
    {
        var filename = await ShowOpenFileDialog.Handle(Unit.Default);

        if (filename is object)
        {
            System.Console.WriteLine("Opening {0}", filename);
        }
    }
}
