using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Avalon.Windows.Controls;
using Avalon.Windows.Utility;

namespace AvalonLibraryTest.Pages
{
    /// <summary>
    /// Interaction logic for TaskDialogs.xaml
    /// </summary>
    public partial class TaskDialogs : Page
    {
        private ObservableCollection<TaskDialogButtonData> _buttons;
        private bool _isPartialTrust;
        private Storyboard _inlineAnimation;
        private TaskDialog _sampleDialog;
       
        public TaskDialogs()
        {
            InitializeComponent();

            try
            {
                new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.AllWindows).Demand();
            }
            catch (System.Security.SecurityException)
            {
                _isPartialTrust = true;
            }

            if (_isPartialTrust)
            {
                ShowInline.IsChecked = true;
                ShowInline.IsEnabled = false;
                ShowInlineDisabled.Visibility = Visibility.Visible;
            }

            _inlineAnimation = (Storyboard)Resources["InlineAnimation"];
            _sampleDialog = (TaskDialog)Resources["SampleDialog"];
            
            PrepareSampleDialog();

            Array icons = Enum.GetValues(typeof(TaskDialogIcon));
            MainIcon.ItemsSource = icons;
            FooterIcon.ItemsSource = icons;

            _buttons = new ObservableCollection<TaskDialogButtonData>();
            ButtonsPanel.DataContext = _buttons;
        }

        private void PrepareSampleDialog()
        {
            _sampleDialog.Buttons.Add(new TaskDialogButtonData(TaskDialogButtons.Cancel, true));

            _sampleDialog.CommandLinks.Add(new TaskDialogButtonData(1, "Copy and Replace", "Replace the file in the destination folder with the file you are copying:")
            {
                Tag = new SampleFileInfo { Name = "MyFile.wmv", DirectoryName = @"C:\Users\Unity\Desktop", Length = 127398173, LastWriteTime = DateTime.Now }
            });
            _sampleDialog.CommandLinks.Add(new TaskDialogButtonData(2, "Don't Copy", "No files will be changed. Leave this file in the destination folder:")
            {
                Tag = new SampleFileInfo { Name = "MyFile.wmv", DirectoryName = @"C:\Movies", Length = 2423456, LastWriteTime = DateTime.Now.AddDays(-1) }
            });
            _sampleDialog.CommandLinks.Add(new TaskDialogButtonData(3, "Copy, but keep both files", "The file you are copying will be renamed."));
        }

        private void ShowTaskDialog_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog taskDialog = new TaskDialog();
            taskDialog.Title = TitleText.Text;
            taskDialog.Header = HeaderText.Text;
            taskDialog.Content = ContentText.Text;
            taskDialog.AllowDialogCancellation = AllowCancellation.IsChecked == true;
            if (FooterText.Text.Length > 0)
            {
                taskDialog.ShowFooter = true;
                taskDialog.Footer = FooterText.Text;
            }
            if (VerificationText.Text.Length > 0)
            {
                taskDialog.ShowVerification = true;
                taskDialog.Verification = VerificationText.Text;
            }
            taskDialog.ShowProgressBar = ShowProgresBar.IsChecked == true;
            taskDialog.IsProgressIndeterminate = ProgresBarIndeterminate.IsChecked == true;
            if (ExpansionText.Text.Length > 0)
            {
                taskDialog.ExpansionPosition = (ExpandFooter.IsChecked == true) ? TaskDialogExpansionPosition.Footer :
                    TaskDialogExpansionPosition.Header;
                taskDialog.ExpansionContent = ExpansionText.Text;
                taskDialog.ExpansionButtonContent = ExpansionButtonText.Text;
            }
            TaskDialogButtons standardButtons = TaskDialogButtons.None;
            if (OKButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.OK;
            }
            if (CancelButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.Cancel;
            }
            if (RetryButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.Retry;
            }
            if (YesButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.Yes;
            }
            if (NoButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.No;
            }
            if (CloseButton.IsChecked == true)
            {
                standardButtons |= TaskDialogButtons.Close;
            }

            foreach (object item in Radios.Items.Cast<RadioData>().Select(r => r.Value))
            {
                taskDialog.RadioButtons.Add(item);
            }

            foreach (object item in TaskDialogButtonData.FromStandardButtons(standardButtons))
            {
                taskDialog.Buttons.Add(item);
            }

            if (UseCommandLinks.IsChecked == true)
            {
                foreach (object item in _buttons)
                {
                    taskDialog.CommandLinks.Add(item);
                }
            }
            else
            {
                foreach (object item in _buttons)
                {
                    taskDialog.Buttons.Add(item);
                }
            }

            taskDialog.MainIcon = TaskDialogIconConverter.ConvertFrom((TaskDialogIcon)MainIcon.SelectedItem);
            taskDialog.FooterIcon = TaskDialogIconConverter.ConvertFrom((TaskDialogIcon)FooterIcon.SelectedItem);


            ShowDialog(taskDialog);
        }

        private void ShowDialog(TaskDialog taskDialog)
        {
            taskDialog.Owner = this;

            if (ShowInline.IsChecked == true)
            {
                taskDialog.Closed += new RoutedEventHandler(taskDialog_Closed);

                options.IsEnabled = false;
                inline.Child = taskDialog;
                inlineBorder.Visibility = Visibility.Visible;
                screen.Visibility = Visibility.Visible;

                _inlineAnimation.Begin(inlineBorder);
            }
            else
            {
                taskDialog.Show();
            }
        }

        private void taskDialog_Closed(object sender, RoutedEventArgs e)
        {
            TaskDialog taskDialog = (TaskDialog)sender;
            taskDialog.Closed -= taskDialog_Closed;

            options.IsEnabled = true;
            inline.Child = null;
            inlineBorder.Visibility = Visibility.Collapsed;
            screen.Visibility = Visibility.Collapsed;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            TaskDialogButtonData button = new TaskDialogButtonData(0, null, null);
            _buttons.Add(button);

            Buttons.SelectedItem = button;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Buttons.SelectedIndex >= 0)
            {
                _buttons.Remove((TaskDialogButtonData)Buttons.SelectedItem);
            }
        }

        private void Buttons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveButton.IsEnabled = ButtonFields.IsEnabled = Buttons.SelectedIndex >= 0;
        }

        private void AddRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Radios.Items.Add(new RadioData());

            RemoveRadioButton.IsEnabled = true;
        }

        private void RemoveRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (Radios.SelectedItem != null)
            {
                Radios.Items.Remove(Radios.SelectedItem);

                if (Radios.Items.Count == 0)
                {
                    RemoveRadioButton.IsEnabled = false;
                }
            }
        }

        private void RadioName_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            fe.FindVisualAncestorByType<ListBoxItem>().IsSelected = true;
        }

        private void ShowSample_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(_sampleDialog);
        }

        class RadioData
        {
            public string Value { get; set; }
        }

        public class SampleFileInfo
        {
            public string Name { get; set; }
            public string DirectoryName { get; set; }
            public int Length { get; set; }
            public DateTime LastWriteTime { get; set; }
        }
    }
}

