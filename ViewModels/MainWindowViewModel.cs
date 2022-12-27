using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Weland.Models;

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

    public MapFile MapFile { get; set; }
    public Level Level { get; set; }

    private async Task OpenFileAsync()
    {
        var filename = await ShowOpenFileDialog.Handle(Unit.Default);

        if (filename is object)
        {
            // for now, open the map and grab the first level
            System.Console.WriteLine("Opening {0}", filename);

            MapFile = new MapFile();
            MapFile.Load(filename);

            Level = new Level();
            Level.Load(MapFile.Directory[0]);
        }
    }
}
