using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Avalon.Windows.Controls;

namespace AvalonLibraryTest.Pages
{
    /// <summary>
    /// Interaction logic for NotifyIcon.xaml
    /// </summary>
    public partial class NotifyIconDemo : Page
    {
        Storyboard iconAnimation;

        public NotifyIconDemo()
        {
            InitializeComponent();

            iconAnimation = Resources["IconAnimation"] as Storyboard;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            notifyIcon.ShowBalloonTip(1000, "Balloon", "Balloon tip demo.", Avalon.Windows.Controls.NotifyBalloonIcon.Info);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            iconAnimation.Begin(this, true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            iconAnimation.Stop(this);
        }
    }
}
