using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for AddTagDlg.xaml
    /// </summary>
    public partial class AddTagDlg : Window
    {
        public AddTagDlg(IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = true; // TODO find "on text changed" event for editable combo
        }

        private void Taglist_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
//            btnDlgOK.IsEnabled = (taglist.SelectedIndex != -1);
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string Answer { get { return taglist.Text as string; } }
    }
}
