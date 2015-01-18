using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string RESET_FILTER = "<show all>";

        public ObservableCollection<ImageFile> MainImageList { get; set; }
        public ObservableCollection<String> MainTagList { get; set; }
        public MainWindow()
        {
            MainImageList = new ObservableCollection<ImageFile>();
            MainTagList = new ObservableCollection<string>();
            DataContext = this;

            InitializeComponent();
        }

        private string _lastPath = null;
        public static IWin32Window GetIWin32Window(System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
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
            if (dlg.ShowDialog(GetIWin32Window(this)) != System.Windows.Forms.DialogResult.OK)
                return;
            _lastPath = dlg.SelectedPath;

            MainImageList.Clear();

            // 2. For each image in folder, add to MainImageList
            var allFiles = Directory.GetFiles(_lastPath);
            foreach (var aFile in allFiles)
            {
                if (!IsImageFile(aFile))
                    continue;

                ImageFile anImg = new ImageFile(aFile);
                MainImageList.Add(anImg);
            }

            BuildTags();
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
            MainTagList.Clear();
            var tags = MergedTagList(new List<ImageFile>(MainImageList));

            MainTagList.Add(RESET_FILTER); // make sure this is first
            foreach (var aTag in tags)
            {
                MainTagList.Add(aTag);
            }
        }

        private void TagAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool _preventRecursion = false;

        private void ImageList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_preventRecursion)
                return;

            _preventRecursion = true;

            var images = ImageList.SelectedItems;
            TagList.SelectedItems.Clear();
            foreach (var image in images)
            {
                var tags = (image as ImageFile).Tags();
                foreach (var tag in tags)
                {
                    TagList.SelectedItems.Add(tag);
                }
            }

            _preventRecursion = false;
        }

        private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_preventRecursion)
                return;

            _preventRecursion = true;

            try
            {
                foreach (var imageFile in MainImageList)
                {
                    imageFile.IsSelected = false;
                    imageFile.IsVisible = true;
                }

                var tags = TagList.SelectedItems;
//            ImageList.SelectedItems.Clear();
                foreach (string tag in tags)
                {
                    if (tag == RESET_FILTER)
                        return;

                    foreach (var imageFile in MainImageList)
                    {
                        if (imageFile.HasTag(tag))
                        {
                            imageFile.IsSelected = true;
//                        ImageList.SelectedItems.Add(imageFile);
                        }
                        else
                        {
                            imageFile.IsVisible = false;
                                // Hide images which don't have the selected tag
                        }
                    }
                }
            }
            finally
            {
                _preventRecursion = false;
            }
        }

        private void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: disable if no images selected

            // 1. build a list of all current tags
            // 2. show a dialog where the user can either:
            //  a. select from the current tags
            //  b. type in a new tag
            // 3. Add that tag to all selected images
            AddTagDlg dlg = new AddTagDlg(MainTagList) {Owner=this};
            if (dlg.ShowDialog() == false)
                return;

            foreach (var image in ImageList.SelectedItems)
            {
                (image as ImageFile).AddTag(dlg.Answer);
            }

            BuildTags();
        }

        private void KillTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: disable if no tags selected

            // 1. Prompt the user confirming kill of selected tag(s)
            string selTags = TagList.SelectedItems.Cast<object>().Aggregate("", (current, tag) => current + (tag + ","));
            var msg = string.Format("Are you sure you want to kill the tag(s):{0}{1}", Environment.NewLine, selTags);
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
            // TODO: disable if no images selected

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
                image.RemoveTags(new List<string>(){dlg.Answer});
            }

            BuildTags();
        }

        private void ChgTagButton_OnClickTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO disable if no, or more than one, tag selected

            if (TagList.SelectedItems.Count < 1 || TagList.SelectedItems.Count > 1)
                return;

            var oldTag = TagList.SelectedItem as string;

            // 1. Show dialog so user can edit tag
            // 2. For each image, change (old-tag) to (new-tag)
            var dlg = new ChgTagDlg(oldTag, MainTagList) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;

            foreach (var image in ImageList.SelectedItems)
            {
                (image as ImageFile).ChangeTag(oldTag, dlg.Answer);
            }

            BuildTags();
        }
    }
}
