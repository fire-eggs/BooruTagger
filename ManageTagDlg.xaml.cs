using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for ManageTagDlg.xaml
    /// </summary>
    public partial class ManageTagDlg
    {
        public class ATag
        {
            public ATag(string tag)
            {
                Name = tag;
            }

            public string Name { get; set; }
        }

        private string _imageName;
        private IList<string> _alltags;
        private ImageFile _activeImage;

        public ManageTagDlg(ImageFile img, IList<string> alltags)
        {
            InitializeComponent();
            _activeImage = img;

            BuildTagSet();
            ImageName = img.BaseName;

            DataContext = this;
            _alltags = alltags;
        }

        public string ImageName
        {
            get { return _imageName; }
            set { _imageName = value; OnPropertyChanged(() => ImageName); }
        }

        private ObservableCollection<CheckedListItem<ATag>> _tags;

        public ObservableCollection<CheckedListItem<ATag>> TagSet
        {
            get { return _tags; }
            set { _tags = value; OnPropertyChanged(() => TagSet); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            //the cast will always succeed
            MemberExpression memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public IList Answer
        {
            get
            {
                // All tags which are UNchecked
                return (from checkedListItem in TagSet where !checkedListItem.IsChecked select checkedListItem.Item.Name).ToList();
            }
        }

        private void EditBtn(object sender, ExecutedRoutedEventArgs e)
        {
            string tag = (string)e.Parameter;

            var dlg = new ChgTagDlg(tag, _alltags) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;
            _activeImage.ChangeTag(tag, dlg.Answer); // actually change the tag
            BuildTagSet();

            // TODO by changing the imageFile, user cannot cancel an Add/Change?
        }

        private void AddTag_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new ChgTagDlg(null, _alltags) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;
            _activeImage.AddTag(dlg.Answer); // actually add the tag
            BuildTagSet();

            // TODO by changing the imageFile, user cannot cancel an Add/Change?
        }

        private void BuildTagSet()
        {
            if (TagSet != null)
                foreach (var checkedListItem in TagSet)
                {
                    if (!checkedListItem.IsChecked)
                        _activeImage.RemoveTag(checkedListItem.Item.Name);
                }

            var tmpTags = new ObservableCollection<CheckedListItem<ATag>>();
            var tags = _activeImage.Tags();
            tags.Sort();
            foreach (string tag in tags)
            {
                tmpTags.Add(new CheckedListItem<ATag>(new ATag(tag), isChecked: true));
            }
            TagSet = tmpTags;

            // TODO why doesn't the listbox rebuild when TagSet is updated???
            TagList.ItemsSource = null;
            TagList.ItemsSource = TagSet;
        }
    }

    public class MyAppCmds
    {
        public static RoutedUICommand EditBtn_Click = new RoutedUICommand("Desc", "EditBtn_Click", typeof(ManageTagDlg));
    }
}
