using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

// ReSharper disable once InconsistentNaming

// TODO add an image viewer on double-click: can't see details in thumbs

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<ImageFile> MainImageList { get; set; }
        public ObservableCollection<String> MainTagList { get; set; }
        public MainWindow()
        {
            MainImageList = new ObservableCollection<ImageFile>();
            MainTagList = new ObservableCollection<string>();
            DataContext = this;

            InitializeComponent();
            LoadSettings();
            UpdateButtonState();
            Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }

        private readonly List<string> PathHistory = new List<string>();

        private string LastPath
        {
            get
            {
                if (PathHistory == null || PathHistory.Count < 1)
                    return null;
                return PathHistory[0]; // First entry is the most recent
            }
            set
            {
                // Make sure to wipe any older instance
                PathHistory.Remove(value);
                PathHistory.Insert(0, value); // First entry is the most recent
            }
        }

        public static IWin32Window GetIWin32Window(System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            if (source == null)
                return null;
            IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            IntPtr IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }

        private static bool IsImageFile(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == null)
                return false;
            switch (ext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                case ".png":
                    return true;
            }
            return false;
        }

        private void FolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 1. allow user to browse to a folder
            var dlg = new FolderBrowserDialog();
            if (LastPath != null)
                dlg.SelectedPath = LastPath;
            if (FolderBrowserLauncher.ShowFolderBrowser(dlg, GetIWin32Window(this)) != System.Windows.Forms.DialogResult.OK)
//            if (dlg.ShowDialog(GetIWin32Window(this)) != System.Windows.Forms.DialogResult.OK)
                return;
            LastPath = dlg.SelectedPath;

            // TODO Something stupid is happening, not releasing references?
            TagList.ItemsSource = null;
            ImageList.ItemsSource = null;
            MainImageList.Clear();
            MainTagList.Clear();
            GC.Collect();
            ImageList.ItemsSource = MainImageList;

            // 2. For each image in folder, add to MainImageList
            _imagesToFetch.Clear();
            var allFiles = Directory.GetFiles(LastPath);
            foreach (var aFile in allFiles)
            {
                if (!IsImageFile(aFile))
                    continue;
                _imagesToFetch.Add(aFile);
            }

            FetchImages();
        }

        private IEnumerable<string> MergedTagList(IEnumerable<ImageFile> images)
        {
            List<string> tags = new List<string>();
            foreach (var imageFile in images)
            {
                foreach (var tag in imageFile.Tags())
                {
                    if (!tags.Contains(tag))
                        tags.Add(tag);
                }
            }
            tags.Sort();
            return tags;
        }

        private void BuildTags()
        {
            TagList.SelectionChanged -= TagList_OnSelectionChanged;

            MainTagList.Clear();
            var tags = MergedTagList(MainImageList);

            foreach (var aTag in tags)
            {
                MainTagList.Add(aTag);
            }

            TagList.SelectionChanged += TagList_OnSelectionChanged;
            //TagList.SelectedIndex = 0;

            UpdateButtonState();
        }

        // There is probably a clever way to do this...
        private void UpdateButtonState()
        {
            int selCount = ImageList.SelectedItems.Count;
            AddTagButton.IsEnabled = selCount > 0;
            DelTagButton.IsEnabled = selCount > 0;
            MngTagButton.IsEnabled = selCount == 1;

            selCount = TagList.SelectedItems.Count;
            KillTagButton.IsEnabled = selCount > 0;
            ChgTagButton.IsEnabled = selCount == 1;
        }

        // Show which tags apply when one or more images are selected in the image list.
        private void ImageList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't want selection events from the taglist: we're _setting_ the selection
            TagList.SelectionChanged -= TagList_OnSelectionChanged;

            try
            {
                var images = ImageList.SelectedItems;

                TagList.SelectedItems.Clear();
                foreach (ImageFile image in images)
                {
                    var tags = image.Tags();
                    foreach (var tag in tags)
                    {
                        TagList.SelectedItems.Add(tag);
                    }
                }
            }
            finally
            {
                // Make sure to restore taglist selection handling
                TagList.SelectionChanged += TagList_OnSelectionChanged;
            }
            UpdateButtonState();
        }

        // Filters the image list to show only the images with the selected tag(s)
        private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageList.SelectedItems.Clear();

            var tags = TagList.SelectedItems;

            // setting selectedIndex in BuildTags doesn't update SelectedItems ???
            if (tags.Count == 0)
                tags = e.AddedItems;
            foreach (string tag in tags)
            {
                foreach (var imageFile in MainImageList)
                {
                    // "<show all>" being special
                    imageFile.IsVisible = imageFile.HasTag(tag);
                }
            }
            UpdateButtonState();
        }

        private void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedItems.Count < 1)
                return;

            // 1. build a list of all current tags
            // 2. show a dialog where the user can either:
            //  a. select from the current tags
            //  b. type in a new tag
            // 3. Add that tag to all selected images
            AddTagDlg dlg = new AddTagDlg(MainTagList) {Owner=this};
            if (dlg.ShowDialog() == false)
                return;

            List<ImageFile> fails = new List<ImageFile>();
            foreach (var image in ImageList.SelectedItems)
            {
                ImageFile img = image as ImageFile;
                if (img != null)
                    if (!img.AddTag(dlg.Answer))
                        fails.Add(img);
            }

            foreach (var imageFile in fails)
            {
                MainImageList.Remove(imageFile);
            }

            BuildTags();
        }

        private void KillTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Don't include the "show all" tag
            List<string> tagsCanKill = new List<string>();
            foreach (string selectedItem in TagList.SelectedItems)
            {
                tagsCanKill.Add(selectedItem);
            }
            if (tagsCanKill.Count < 1)
                return;

            // 1. Prompt the user confirming kill of selected tag(s)
            string selTags = tagsCanKill.Cast<object>().Aggregate("", (current, tag) => current + (tag + ","));
            selTags = selTags.Remove(selTags.LastIndexOf(','));
            var msg = string.Format("Are you sure you want to kill the tag(s):{0}{1}?", Environment.NewLine, selTags);
            var res = MessageBox.Show(msg, "Kill Tags", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
                return;

            // 2. For each image, kill selected tag(s)
            foreach (var imageFile in MainImageList)
            {
                imageFile.RemoveTags(TagList.SelectedItems);
            }

            BuildTags();
        }

        private void DelTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedItems.Count < 1)
                return;

            // 1. build a list of tags on selected images
            List<ImageFile> selImages = new List<ImageFile>();
            foreach (var image in ImageList.SelectedItems)
            {
                selImages.Add(image as ImageFile);
            }
            var tags = MergedTagList(selImages);

            // 2. show a dialog where user can select a single tag
            DelTagDlg dlg = new DelTagDlg(tags) {Owner = this};
            if (dlg.ShowDialog() == false)
                return;

            // 3. for each selected image, remove tag
            foreach (var image in selImages)
            {
                image.RemoveTags(new List<string> {dlg.Answer});
            }

            BuildTags();
        }

        private void ChgTagButton_OnClickTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TagList.SelectedItems.Count < 1 || TagList.SelectedItems.Count > 1)
                return;

            var oldTag = TagList.SelectedItem as string;

            // 1. Show dialog so user can edit tag
            // 2. For each image, change (old-tag) to (new-tag)
            var dlg = new ChgTagDlg(oldTag, MainTagList) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;

            foreach (var image in MainImageList) //ImageList.SelectedItems)
            {
                image.ChangeTag(oldTag, dlg.Answer);
            }

            BuildTags();
        }

        private void MngTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedItems.Count < 1 || ImageList.SelectedItems.Count > 1)
                return;
            ImageFile img = ImageList.SelectedItem as ImageFile;
            if (img == null)
                return;

            if (img.Tags().Count < 1)
                return;

            var dlg = new ManageTagDlg(img, MainTagList) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;
            // TODO when dialog can add/change tags, need to call something else here!
            img.RemoveTags(dlg.Answer);
            BuildTags();
        }

        private BackgroundWorker _imageFetcher;
        private List<string> _imagesToFetch = new List<string>();

        public void FetchImages()
        {
            if (_imageFetcher == null)
            {
                _imageFetcher = new BackgroundWorker();
                _imageFetcher.DoWork += ImageFetch;
                _imageFetcher.RunWorkerCompleted += ImageLoadDone;
            }
            if (_imagesToFetch.Count > 0)
                _imageFetcher.RunWorkerAsync();
        }

        private void doit()
        {
            BuildTags();
            TagList.ItemsSource = null;
            TagList.ItemsSource = MainTagList;
            ImageList.ScrollIntoView(MainImageList[0]); // I think this fixes issue #14
        }

        private void ImageLoadDone(object sender, RunWorkerCompletedEventArgs e)
        {
            _imagesToFetch.Clear();
            // Issue #23: by adding a delay, and doing the final update tasks in the background, I hope to fix the 'incomplete taglist' problem.
            // The cause appears to be due to the images not all being in the MainImageList at the time this method was invoked.
            Thread.Sleep(500);
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(doit));
        }

        private delegate void UpdateDelegate(ImageFile anImg);

        private void ImageFetch(object sender, DoWorkEventArgs e)
        {
            UpdateDelegate update = UpdateMainList;

            foreach (var aFile in _imagesToFetch)
            {
                ImageFile anImg = new ImageFile(aFile);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, update, anImg);
                Thread.Sleep(15); // Nothing happens unless we explicitly let the GUI do stuff. NOTE: 'yield()' doesn't work.
            }
        }

        private void UpdateMainList(ImageFile anImg)
        {
            MainImageList.Add(anImg);
        }

        // http://elegantcode.com/2009/07/03/wpf-multithreading-using-the-backgroundworker-and-reporting-the-progress-to-the-ui/
        // http://pooyakhamooshi.blogspot.com/2010/07/accessing-ui-elements-using-dispatcher.html

        // TODO push settings into separate class?

        private ITSettings mysettings;

        private void LoadSettings()
        {
            mysettings = ITSettings.Load();

            // No existing settings. Use default.
            if (mysettings.Fake)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                MyColumnWidthSetting = new GridLength(150.0, GridUnitType.Pixel);
            }
            else
            {
                // restore windows position
                WindowStartupLocation = WindowStartupLocation.Manual;
                Top = mysettings.WinTop;
                Left = mysettings.WinLeft;
                Height = mysettings.WinHigh;
                Width = mysettings.WinWide;
                LastPath = mysettings.LastPath;
                MyColumnWidthSetting = new GridLength(mysettings.SplitLoc, GridUnitType.Pixel);
            }
            InnerGrid.ColumnDefinitions[0].Width = MyColumnWidthSetting; // TODO the binding *should* have worked but doesn't ...
        }

        private void SaveSettings()
        {
            var bounds = RestoreBounds; // not minimized size
            mysettings.WinTop = (int)bounds.Top;
            mysettings.WinLeft = (int)bounds.Left;
            mysettings.WinHigh = (int)bounds.Height;
            mysettings.WinWide = (int)bounds.Width;
            mysettings.Fake = false;
            mysettings.LastPath = LastPath;
            mysettings.SplitLoc = MyColumnWidthSetting.Value;
            mysettings.Save();
        }

        public class ITSettings : AppSettings<ITSettings>
        {
            public bool Fake = true;
            public int WinLeft = -1;
            public int WinTop = -1;
            public int WinHigh = -1;
            public int WinWide = -1;
            public string LastPath = null;
            public double SplitLoc = 150.0;
        }

        public GridLength MyColumnWidthSetting { get; set; }

        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            TagList.SelectedItems.Clear();
            ImageList.SelectedItems.Clear();
            foreach (var imageFile in MainImageList)
            {
                imageFile.IsVisible = true;
            }
            UpdateButtonState();
        }
    }
}
