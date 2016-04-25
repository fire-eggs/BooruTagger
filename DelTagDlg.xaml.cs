using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for DelTagDlg.xaml
    /// </summary>
    public partial class DelTagDlg : Window
    {
        public DelTagDlg(IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = false;
        }

        private void Taglist_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDlgOK.IsEnabled = (taglist.SelectedIndex != -1);
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string Answer { get { return taglist.SelectedItem as string;  } }
    }
}
