using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AvalonLibraryTest.Pages
{
    /// <summary>
    /// Interaction logic for FolderBrowserDialog.xaml
    /// </summary>
    public partial class FolderBrowserDialog : Page
    {
        public FolderBrowserDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Avalon.Windows.Dialogs.FolderBrowserDialog dialog = new Avalon.Windows.Dialogs.FolderBrowserDialog
            {
                ShowEditBox = ShowEditBox.IsChecked == true,
                BrowseShares = BrowseShares.IsChecked == true
            };
            if (dialog.ShowDialog() == true)
            {
                folder.Text = dialog.SelectedPath;
            }
        }
    }
}
