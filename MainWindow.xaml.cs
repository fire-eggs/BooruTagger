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

// TODO a splitter so the tag list can be wider would be good
// TODO add an image viewer on double-click: can't see details in thumbs

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string RESET_FILTER = "<show all>";

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
            var bounds = RestoreBounds; // not minimized size
            mysettings.WinTop = (int)bounds.Top;
            mysettings.WinLeft = (int)bounds.Left;
            mysettings.WinHigh = (int)bounds.Height;
            mysettings.WinWide = (int)bounds.Width;
            mysettings.Fake = false;
            mysettings.LastPath = _lastPath;
            mysettings.Save();
        }

        private string _lastPath;
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
            if (_lastPath != null)
                dlg.SelectedPath = _lastPath;
            if (FolderBrowserLauncher.ShowFolderBrowser(dlg, GetIWin32Window(this)) != System.Windows.Forms.DialogResult.OK)
//            if (dlg.ShowDialog(GetIWin32Window(this)) != System.Windows.Forms.DialogResult.OK)
                return;
            _lastPath = dlg.SelectedPath;

            // TODO Something stupid is happening, not releasing references?
            ImageList.ItemsSource = null;
            MainImageList.Clear();
            GC.Collect();
            ImageList.ItemsSource = MainImageList;

            // 2. For each image in folder, add to MainImageList
            _imagesToFetch.Clear();
            var allFiles = Directory.GetFiles(_lastPath);
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
            var tags = MergedTagList(new List<ImageFile>(MainImageList));

            MainTagList.Add(RESET_FILTER); // make sure this is first
            foreach (var aTag in tags)
            {
                MainTagList.Add(aTag);
            }

            TagList.SelectionChanged += TagList_OnSelectionChanged;
            TagList.SelectedIndex = 0;

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
            if (selCount == 1 && (string)TagList.SelectedItems[0] == RESET_FILTER) // take 'show all' into account
                selCount = 0;
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
                    imageFile.IsVisible = imageFile.HasTag(tag) || tag == RESET_FILTER;
                }
            }
            UpdateButtonState();
        }

        private void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedItems.Count < 1)
                return;

            // Don't include the "show all" tag
            List<string> tagsCanAdd = new List<string>();
            foreach (string tagName in MainTagList)
            {
                if (tagName != RESET_FILTER)
                    tagsCanAdd.Add(tagName);
            }

            // 1. build a list of all current tags
            // 2. show a dialog where the user can either:
            //  a. select from the current tags
            //  b. type in a new tag
            // 3. Add that tag to all selected images
            AddTagDlg dlg = new AddTagDlg(tagsCanAdd) {Owner=this};
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
                if (selectedItem != RESET_FILTER)
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
            // Don't allow if the "show all" tag selected
            if (oldTag == RESET_FILTER)
                return;

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

            var dlg = new ManageTagDlg(img) {Owner = this};
            if (dlg.ShowDialog() == false)
                return;
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

        private void ImageLoadDone(object sender, RunWorkerCompletedEventArgs e)
        {
            _imagesToFetch.Clear();
            BuildTags();
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

        private ITSettings mysettings;

        private void LoadSettings()
        {
            mysettings = ITSettings.Load();

            // No existing settings. Use default.
            if (mysettings.Fake)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                // restore windows position
                WindowStartupLocation = WindowStartupLocation.Manual;
                Top = mysettings.WinTop;
                Left = mysettings.WinLeft;
                Height = mysettings.WinHigh;
                Width = mysettings.WinWide;
                _lastPath = mysettings.LastPath;
            }
        }

        public class ITSettings : AppSettings<ITSettings>
        {
            public bool Fake = true;
            public int WinLeft = -1;
            public int WinTop = -1;
            public int WinHigh = -1;
            public int WinWide = -1;
            public string LastPath = null;
        }
    }
}
