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
