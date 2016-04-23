using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageTag
{
    /// <summary>
    /// A dialog to allow the user to ADD or CHANGE a tag.
    /// The user can type in a new tag or select from the
    /// list of existing tags.
    /// </summary>
    public partial class ChgTagDlg
    {
        private readonly char[] _illegalChars;

        public ChgTagDlg(string oldTag, IEnumerable<string> tags)
        {
            InitializeComponent();
            taglist.ItemsSource = tags;
            btnDlgOK.IsEnabled = true; // TODO find "on text changed" event for editable combo

            // Eliminate a separate 'add tag' dialog: an empty "old tag" 
            // used to indicate this is an 'add' operation.
            if (oldTag == null)
            {
                OldTag.Visibility = Visibility.Collapsed;
            }
            else
            {
                OldTag.Visibility = Visibility.Visible;
                OldTag.Text = string.Format("Changing tag '{0}'", oldTag);
                taglist.SelectedItem = oldTag;
            }

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
