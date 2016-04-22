using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// TODO are the AddTag and ChgTag dialogs *the same* ?

namespace ImageTag
{
    /// <summary>
    /// Add a tag to an image. User can select from an existing tag or type in a new tag.
    /// </summary>
    public partial class AddTagDlg : Window
    {
        private readonly char[] _illegalChars;

        public AddTagDlg(IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = true; // TODO find "on text changed" event for editable combo
            _illegalChars = Path.GetInvalidFileNameChars();
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

        private void Taglist_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_illegalChars.Contains(e.Text[0]) || e.Text[0] == '+') // plus is special: tag separator
                e.Handled = true;
        }
    }
}
