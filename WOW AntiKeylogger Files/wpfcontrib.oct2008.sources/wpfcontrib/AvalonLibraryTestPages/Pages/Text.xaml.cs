using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Media.Animation;
using Avalon.Windows.Controls;

namespace AvalonLibraryTest.Pages
{
    /// <summary>
    /// Interaction logic for Text.xaml
    /// </summary>
    public partial class Text : Page
    {
        public Text()
        {
            InitializeComponent();

            var colors = typeof(Brushes).GetProperties().Where(p => p.PropertyType == typeof(SolidColorBrush)).Select(pi => new KeyValuePair<string, SolidColorBrush>(pi.Name, (SolidColorBrush)pi.GetValue(null, null))).ToArray();
            bgColor.ItemsSource = colors;
            bgColor.SelectedItem = colors.First(c => c.Key == "Transparent");
            fgColor.ItemsSource = colors;
            fgColor.SelectedItem = colors.First(c => c.Key == "Black");
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (tb != null)
            {
                if (((CheckBox)sender).IsChecked == true)
                {
                    tb.RepeatBehavior = RepeatBehavior.Forever;
                }
                else
                {
                    tb.ClearValue(AnimatedTextBlock.RepeatBehaviorProperty);
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer target = (sender == wpfScroll) ? gdiScroll : wpfScroll;
            target.ScrollToVerticalOffset(e.VerticalOffset);
        }
    }
}
