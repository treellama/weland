using Gtk;
using System;
using System.Collections.Generic;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public partial class MapWindow
    {
        [Widget] IconView textureIcons;
        [Widget] ComboBox textureCollection;
        [Widget] ComboBox textureTransferMode;

        ListStore textureStore = new ListStore(typeof(Gdk.Pixbuf));

        static readonly List<int> collectionMapping = new List<int> { 17, 18, 19, 20, 21, 27, 28, 29, 30 };
        static readonly List<int> transferModeMapping = new List<int> { 0, 4, 5, 6, 9, 15, 16, 17, 18, 19, 20 };

        // sets up texture palette for this collection
        void UpdateCollection(int collectionIndex)
        {
            bool landscape = collectionIndex >= 27;
            textureStore.Clear();
            Collection coll = Weland.Shapes.GetCollection(collectionIndex);
            ShapeDescriptor d = new ShapeDescriptor();
            d.CLUT = 0;
            d.Collection = (byte)collectionIndex;
            int textureSize = 64;

            for (byte i = 0; i < coll.BitmapCount; ++i)
            {
                d.Bitmap = i;
                var bitmap = Weland.Shapes.GetShape(d);
                if (landscape)
                {
                    int W = 192;
                    int H = (int)Math.Round((double)bitmap.Height * W / bitmap.Width);
                    bitmap = ImageUtilities.ResizeImage(bitmap, W, H);
                }
                else
                {
                    bitmap = bitmap.RotateCW();
                    bitmap = ImageUtilities.ResizeImage(bitmap, textureSize, textureSize);
                }
                textureStore.AppendValues(ImageUtilities.ImageToPixbuf(bitmap));
            }

            textureIcons.PixbufColumn = 0;
            textureIcons.Columns = landscape ? 1 : 3;
            textureIcons.RowSpacing = 0;
            textureIcons.ColumnSpacing = 0;
            textureIcons.Model = textureStore;

            if (!landscape)
            {
                if (textureTransferMode.Active == 4)
                {
                    textureTransferMode.Active = 0;
                }
                textureTransferMode.Sensitive = true;
            }
            else
            {
                textureTransferMode.Active = 4;
                textureTransferMode.Sensitive = false;
            }
        }

        void SelectBitmap(int bitmap)
        {
            textureIcons.SelectionChanged -= OnTextureSelected;
            TreePath path = new TreePath(new int[] { bitmap });
            textureIcons.SelectPath(path);
            textureIcons.ScrollToPath(path, 0.5f, 0.5f);
            textureIcons.SelectionChanged += OnTextureSelected;
        }

        void BuildTexturePalette()
        {
            editor.PaintDescriptor.Collection = (byte)(Level.Environment + 17);
            UpdateTexturePalette(false);
        }

        void OnShapesChanged()
        {
            UpdateTexturePalette(true);
        }

        // update palette based on settings in editor
        void UpdateTexturePalette(bool forceCollection)
        {
            int textureCollectionIndex = collectionMapping.IndexOf(editor.PaintDescriptor.Collection);
            if (forceCollection || textureCollection.Active != textureCollectionIndex)
            {
                UpdateCollection(editor.PaintDescriptor.Collection);
                textureCollection.Changed -= OnCollectionChanged;
                textureCollection.Active = textureCollectionIndex;
                textureCollection.Changed += OnCollectionChanged;
            }

            SelectBitmap(editor.PaintDescriptor.Bitmap);
            textureTransferMode.Changed -= OnTransferModeChanged;
            textureTransferMode.Active = transferModeMapping.IndexOf(editor.PaintTransferMode);
            textureTransferMode.Changed += OnTransferModeChanged;
        }

        void OnCollectionChanged(object o, EventArgs args)
        {
            editor.PaintDescriptor.Collection = (byte)collectionMapping[textureCollection.Active];
            editor.PaintDescriptor.Bitmap = 0;
            UpdateCollection(editor.PaintDescriptor.Collection);
            SelectBitmap(0);
        }

        void OnTextureSelected(object o, EventArgs args)
        {
            if (textureIcons.SelectedItems.Length > 0)
            {
                editor.PaintDescriptor.Bitmap = (byte)textureIcons.SelectedItems[0].Indices[0];
            }
            else
            {
                SelectBitmap(editor.PaintDescriptor.Bitmap);
            }
        }

        void OnTransferModeChanged(object o, EventArgs args)
        {
            editor.PaintTransferMode = (short)transferModeMapping[textureTransferMode.Active];
        }
    }
}
