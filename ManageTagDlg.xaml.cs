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

        public ManageTagDlg(ImageFile img, IList<string> alltags)
        {
            InitializeComponent();

            TagSet = new ObservableCollection<CheckedListItem<ATag>>();
            var tags = img.Tags();
            tags.Sort();
            foreach (string tag in tags)
            {
                TagSet.Add(new CheckedListItem<ATag>(new ATag(tag), isChecked:true));
            }
            ImageName = img.BaseName;

            DataContext = this;
            _alltags = alltags;
        }

        public string ImageName
        {
            get { return _imageName; }
            set { _imageName = value; OnPropertyChanged(() => ImageName); }
        }

        public ObservableCollection<CheckedListItem<ATag>> TagSet { get; set; }

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

        }

        private void AddTag_OnClick(object sender, RoutedEventArgs e)
        {
            AddTagDlg dlg = new AddTagDlg(_alltags) { Owner = this };
            if (dlg.ShowDialog() == false)
                return;
        }
    }

    public class MyAppCmds
    {
        public static RoutedUICommand EditBtn_Click = new RoutedUICommand("Desc", "EditBtn_Click", typeof(ManageTagDlg));
    }
}
