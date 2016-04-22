using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageTag
{
    /// <summary>
    /// Interaction logic for ChgTagDlg.xaml
    /// </summary>
    public partial class ChgTagDlg : Window
    {
        private readonly char[] _illegalChars;

        public ChgTagDlg(string oldTag, IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = true; // TODO find "on text changed" event for editable combo
            OldTag.Text = string.Format("Changing tag '{0}'", oldTag);
            taglist.SelectedItem = oldTag;

            _illegalChars = Path.GetInvalidFileNameChars();
        }

        private void Taglist_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //            btnDlgOK.IsEnabled = (taglist.SelectedIndex != -1);
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO need to double-check for illegal characters? i.e. text pasted from clipboard isn't filtered?
            DialogResult = true;
        }

        public string Answer { get { return taglist.Text; } }

        private void Taglist_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_illegalChars.Contains(e.Text[0]) || e.Text[0] == '+') // plus is special: tag separator
                e.Handled = true;
        }
    }
}
