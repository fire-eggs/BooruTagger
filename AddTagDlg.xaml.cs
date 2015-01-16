using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
