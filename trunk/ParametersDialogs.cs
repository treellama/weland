using Gtk;
using System;
using System.Collections.Generic;

namespace Weland {
    public class ParametersDialog {
	public ParametersDialog(Window parent, string title, List<Placement> p, string[] n) {
	    dialog = new Dialog(title, parent, DialogFlags.Modal | DialogFlags.DestroyWithParent);
	    dialog.SetSizeRequest(600, 400);
	    placements = p;
	    names = n;
	}

	void RandomCountData(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
	    int value = (int) model.GetValue(iter, 4);
	    if (value == -1) {
		(cell as CellRendererText).Text = "\u221e";
	    } else {
		(cell as CellRendererText).Text = String.Format("{0}", value);
	    }
	}

	void InfiniteAvailableData(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
	    (cell as CellRendererToggle).Active = (((int) model.GetValue(iter, 4)) == -1);
	}

	EditedHandler BuildCountEdited(int column) {
	    return delegate(object o, EditedArgs args) {
		TreeIter iter;
		store.GetIter(out iter, new TreePath(args.Path));
		int value;
		if (int.TryParse(args.NewText, out value)) {
		    if (value < 0) value = 0;
		    if (value > short.MaxValue) value = short.MaxValue;
		    store.SetValue(iter, column, value);
		}
	    };
	}

	void RandomCountEdited(object o, EditedArgs args) {
	    TreeIter iter;
	    store.GetIter(out iter, new TreePath(args.Path));
	    int value;
	    if (args.NewText == "\u221e") {
		store.SetValue(iter, 4, -1);
	    } else if (int.TryParse(args.NewText, out value)) {
		if (value < -1) value = 0;
		if (value > short.MaxValue) value = short.MaxValue;
		store.SetValue(iter, 4, value);
	    }
	}
	
	void AppearanceEdited(object o, EditedArgs args) {
	    TreeIter iter;
	    store.GetIter(out iter, new TreePath(args.Path));
	    int value;
	    string NewText;
	    if (args.NewText.EndsWith("%")) {
		NewText = args.NewText.Substring(0, args.NewText.Length - 1);
	    } else {
		NewText = args.NewText;
	    }
	    if (int.TryParse(NewText, out value)) {
		if (value < 0) value = 0;
		if (value > 100) value = 100;
		store.SetValue(iter, 5, value);
	    }
	}

	void InfiniteAvailableToggled(object o, ToggledArgs args) {
	    TreeIter iter;
	    store.GetIter(out iter, new TreePath(args.Path));
	    int value = (int) store.GetValue(iter, 4);
	    if (value == -1) {
		store.SetValue(iter, 4, 0);
	    } else {
		store.SetValue(iter, 4, -1);
	    }
	}

	void RandomLocationToggled(object o, ToggledArgs args) {
	    TreeIter iter;
	    store.GetIter(out iter, new TreePath(args.Path));
	    store.SetValue(iter, 6, !((bool) store.GetValue(iter, 6)));
	}

	public void Run() {
	    dialog.AddActionWidget(new Button(Stock.Cancel), ResponseType.Cancel);
	    dialog.AddActionWidget(new Button(Stock.Ok), ResponseType.Ok);

	    TreeView tree = new Gtk.TreeView();
	    tree.RulesHint = true;
	    ScrolledWindow w = new ScrolledWindow();
	    w.Add(tree);
	    dialog.VBox.Add(w);

	    CellRendererText nameCell = new CellRendererText();
	    
	    CellRendererText initialCountCell = new CellRendererText();
	    initialCountCell.Editable = true;
	    initialCountCell.WidthChars = 4;
	    initialCountCell.Xalign = 1;
	    initialCountCell.Edited += BuildCountEdited(1);

	    CellRendererText minimumCountCell = new CellRendererText();
	    minimumCountCell.Editable = true;
	    minimumCountCell.WidthChars = 4;
	    minimumCountCell.Xalign = 1;
	    minimumCountCell.Edited += BuildCountEdited(2);

	    CellRendererText maximumCountCell = new CellRendererText();
	    maximumCountCell.Editable = true;
	    maximumCountCell.WidthChars = 4;
	    maximumCountCell.Xalign = 1;
	    maximumCountCell.Edited += BuildCountEdited(3);

	    TreeViewColumn randomCountColumn = new TreeViewColumn();
	    randomCountColumn.Title = "Total\nAvailable";
	    CellRendererText randomCountCell = new CellRendererText();
	    randomCountCell.Editable = true;
	    randomCountCell.WidthChars = 4;
	    randomCountCell.Xalign = 1;
	    randomCountColumn.PackStart(randomCountCell, true);
	    randomCountColumn.SetCellDataFunc(randomCountCell, (TreeCellDataFunc) RandomCountData);
	    randomCountCell.Edited += RandomCountEdited;

	    CellRendererText appearanceCell = new CellRendererText();
	    appearanceCell.Editable = true;
	    appearanceCell.WidthChars = 3;
	    appearanceCell.Xalign = 1;
	    appearanceCell.Edited += AppearanceEdited;

	    TreeViewColumn infiniteAvailableColumn = new TreeViewColumn();
	    infiniteAvailableColumn.Title = "Infinite\nAvailable";
	    CellRendererToggle infiniteAvailableCell = new CellRendererToggle();
	    infiniteAvailableCell.Activatable = true;
	    infiniteAvailableColumn.PackStart(infiniteAvailableCell, true);
	    infiniteAvailableColumn.SetCellDataFunc(infiniteAvailableCell, (TreeCellDataFunc) InfiniteAvailableData);
	    infiniteAvailableCell.Toggled += InfiniteAvailableToggled;

	    CellRendererToggle randomLocationCell = new CellRendererToggle();
	    randomLocationCell.Activatable = true;
	    randomLocationCell.Toggled += RandomLocationToggled;

	    tree.AppendColumn("Name", nameCell, "text", 0);
	    tree.Columns[0].Expand = true;
	    tree.AppendColumn("Initial\nCount", initialCountCell, "text", 1);
	    tree.AppendColumn("Min\nCount", minimumCountCell, "text", 2);
	    tree.AppendColumn("Max\nCount", maximumCountCell, "text", 3);
	    tree.AppendColumn(randomCountColumn);
	    tree.AppendColumn(infiniteAvailableColumn);
	    tree.AppendColumn("Appearance (%)", appearanceCell, "text", 5);
	    tree.AppendColumn("Random\nLocation", randomLocationCell, "active", 6);

	    store = new ListStore(typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool));
	    tree.Model = store;

	    for (int i = 1; i < names.Length; ++i) {
		store.AppendValues(names[i], (int) placements[i].InitialCount, (int) placements[i].MinimumCount, (int) placements[i].MaximumCount, (int) placements[i].RandomCount, placements[i].RandomPercent, placements[i].RandomLocation);
	    }

	    dialog.ShowAll();
	    if (dialog.Run() == (int) ResponseType.Ok) {
		int i = 1;
		foreach (object[] row in store) {
		    placements[i].InitialCount = (short) (int) row[1];
		    placements[i].MinimumCount = (short) (int) row[2];
		    placements[i].MaximumCount = (short) (int) row[3];
		    placements[i].RandomCount = (short) (int) row[4];
		    placements[i].RandomPercent = (int) row[5];
		    placements[i].RandomLocation = (bool) row[6];
		    ++i;
		}
	    }
	    dialog.Destroy();
	}
	Dialog dialog;
	List<Placement> placements;
	ListStore store;
	String[] names;
    }

    public class ItemParametersDialog {
	public ItemParametersDialog(Window w, Level theLevel) {
	    parent = w;
	    level = theLevel;
	}
	
	public void Run() {
	    ParametersDialog d = new ParametersDialog(parent, "Item Parameters", level.ItemPlacement, Enum.GetNames(typeof(ItemType)));
	    d.Run();
	}

	Window parent;
	Level level;
    }

    public class MonsterParametersDialog {
	public MonsterParametersDialog(Window w, Level theLevel) {
	    parent = w;
	    level = theLevel;
	}

	public void Run() {
	    ParametersDialog d = new ParametersDialog(parent, "Monster Parameters", level.MonsterPlacement, Enum.GetNames(typeof(MonsterType)));
	    d.Run();

	}

	Window parent;
	Level level;
    }
}