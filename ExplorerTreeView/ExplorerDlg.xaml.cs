using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace ImageTag.ExplorerTreeView
{
    /// <summary>
    /// A replacement folder browse dialog.
    /// </summary>
    public partial class ExplorerDlg : Window, INotifyPropertyChanged
    {
        public string SelectedPath
        {
            get { return explorer.SelectedPath; }
            set { explorer.SelectedPath = value; }
        }

        private string _hintText;
        public string HintText
        {
            get { return _hintText; }
            set
            {
                _hintText = value;
                OnPropertyChanged(() => HintText);
            }
        }

        private IList<string> _pathHistory;

        public IList<string> PathHistory
        {
            get { return _pathHistory; }
            set { _pathHistory = value; OnPropertyChanged(()=>PathHistory); }
        }

        public ExplorerDlg()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            explorer.SelectedPath = txtPath.Text;
        }

        private void explorer_ExplorerError(object sender, ExplorerErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void BtnDlgOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO User wants to browse to something else, e.g. an unmapped network drive
        }

        // TODO how can this be inherited and/or stored in one place?
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

        private void CmbHistory_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO can this be handled via binding somehow?
            explorer.SelectedPath = (string)cmbHistory.SelectedValue;
        }
    }
}
