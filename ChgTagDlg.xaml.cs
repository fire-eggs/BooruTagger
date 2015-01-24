using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

// TODO filter for illegal path/file characters at combobox level

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for ChgTagDlg.xaml
    /// </summary>
    public partial class ChgTagDlg : Window
    {
        public ChgTagDlg(string oldTag, IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = true; // TODO find "on text changed" event for editable combo
            OldTag.Text = string.Format("Changing tag '{0}'", oldTag);
            taglist.SelectedItem = oldTag;
        }

        private void Taglist_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //            btnDlgOK.IsEnabled = (taglist.SelectedIndex != -1);
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string Answer { get { return taglist.Text; } }
    }
}
