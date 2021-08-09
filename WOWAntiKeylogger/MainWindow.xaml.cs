using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

//for the type of hooks that can be installed, see MSDN at:
//http://msdn.microsoft.com/en-us/library/ms644990(VS.85).aspx

//FWIW - logging without hooks:
//http://www.osix.net/modules/article/?id=162

namespace WOWAntiKeylogger
{
    public partial class MainWindow : Window
    {
        private GlobalKeyboardHook keyboardHook = new GlobalKeyboardHook(true);
        System.Windows.Threading.DispatcherTimer startupTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer rehookTimer = new System.Windows.Threading.DispatcherTimer();
        int minimizeCountDown = 4;

        public MainWindow()
        {
            InitializeComponent();
            
            keyboardHook.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress); 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableAntiKeylogger();

            startupTimer.Interval = TimeSpan.FromMilliseconds(1000);
            startupTimer.Tick += new EventHandler(startupTimer_Tick);
            startupTimer.Start();

            rehookTimer.Interval = TimeSpan.FromMilliseconds(1000);
            rehookTimer.Tick += new EventHandler(rehookTimer_Tick); //started after minimize
        }
                
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Dispose();
            keyboardHook.Unhook();
        }
        
        //Private Event Handlers

        private void DraggableThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }

        private void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            capturedKeystrokesTextBox.AppendText(e.KeyChar.ToString());
            capturedKeystrokesTextBox.ScrollToEnd();
        }

        private void startupTimer_Tick(object sender, EventArgs e)
        {
            MinimizeLabel.Content = "Minimizing on start up in: " + minimizeCountDown.ToString();

            if (minimizeCountDown == 0)
            {
                MinimizeWindow();
            }
            else
            {
                minimizeCountDown--;
            }
        }
        
        private void rehookTimer_Tick(object sender, EventArgs e)
        {
            MinimizeLabel.Content = "";

            //if the software is setup to block keystrokes then
            if (antiKeyloggerCheckBox.IsChecked == true && 
                startupTimer.IsEnabled == false)
            {
                
                //drop our event handler and unhook from windows
                keyboardHook.KeyPress -= new KeyPressEventHandler(keyboardHook_KeyPress);
                keyboardHook.Unhook();
                
                //re-add our event handler and rehook to ensure our hook is always at the top of the linked list
                keyboardHook.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress);
                keyboardHook.Hook(true);
                                
                MinimizeLabel.Content = "System check: Running  [" + DateTime.Now.ToLongTimeString() + "]"; 
            }
        }

        //Tray Event Handlers

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void openConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState != System.Windows.WindowState.Normal)
                trayIcon_DoubleClick(this, e);
        }

        private void enableAntiKeyloggerButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAntiKeyloggerEnabled == false)
                EnableAntiKeylogger();
        }

        private void disableAntiKeyloggerButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAntiKeyloggerEnabled == true)
                DisableAntiKeylogger();
        }

        private void trayIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(delegate()
                    {
                        this.WindowState = WindowState.Normal;
                        this.Activate();
                    })
                );
            }
            else
            {
                this.Visibility = Visibility.Hidden;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(delegate()
                    {
                        this.WindowState = WindowState.Minimized;
                        this.Activate();
                    })
                );
            }

            trayIcon.Visibility = System.Windows.Visibility.Hidden;
        }

        //UI Event Handlers

        private void antiKeyloggerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAntiKeyloggerEnabled == false)
                EnableAntiKeylogger();
        }

        private void antiKeyloggerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsAntiKeyloggerEnabled == true)
                DisableAntiKeylogger();

            keyloggerCheckBox.IsChecked = false;
        }

        private void keyloggerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)antiKeyloggerCheckBox.IsChecked)
                keyboardHook.SwallowKeystrokes = true;
            else
                keyboardHook.SwallowKeystrokes = false;

            if (antiKeyloggerCheckBox.IsChecked == true)
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Info;
                trayIcon.BalloonTipText = "Protection is enabled and your keystrokes cannot be caught and will NOT show up in the WOWAntiKeylogger window. This program protects your keystrokes from being collected even if you have a virus or spyware installed!.";
                trayIcon.BalloonTipTitle = "Keylogging Enabled ::: Protection ON";
                trayIcon.ShowBalloonTip(5000);
            }
            else
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Error;
                trayIcon.BalloonTipText = "Your keystrokes are now being captured and logged in the WOWAntiKeylogger window. Try typing in several programs like Word, Notepad and Internet Explorer or FireFox. Your keystrokes will be logged to the WOWAntiKeylogger UI.";
                trayIcon.BalloonTipTitle = "Keylogging Enabled ::: Protection OFF";
                trayIcon.ShowBalloonTip(5000);
            }
        }

        private void keyloggerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            keyboardHook.SwallowKeystrokes = true;
            capturedKeystrokesTextBox.Text = "";

            antiKeyloggerCheckBox.IsChecked = true;

            trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Info;
            trayIcon.BalloonTipText = "WOWAntiKeylogger is no longer logging your keystrokes. If this were a virus, spyware or monitoring software you wouldn't know it!";
            trayIcon.BalloonTipTitle = "Keylogging Disabled";
            trayIcon.ShowBalloonTip(5000);
        }

        private void minimizeButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MinimizeWindow();
        }

        private void mainExitButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void helpLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.robault.com/");
        }

        //Private Methods

        private bool IsAntiKeyloggerEnabled { get; set; }

        private void EnableAntiKeylogger()
        {
            IsAntiKeyloggerEnabled = true;

            keyboardHook.SwallowKeystrokes = true;

            antiKeyloggerCheckBox.IsChecked = true;
            enableAntiKeyloggerButton.IsChecked = true;
            disableAntiKeyloggerButton.IsChecked = false;

            LockStatusImage.Visibility = System.Windows.Visibility.Visible;
            UnLockStatusImage.Visibility = System.Windows.Visibility.Hidden;

            if (keyloggerCheckBox != null && (bool)keyloggerCheckBox.IsChecked == true)
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Info;
                trayIcon.BalloonTipText = "Protection is enabled and your keystrokes cannot be caught and will NOT show up in the WOWAntiKeylogger window. This program protects your keystrokes from being collected even if you have a virus or spyware installed!.";
                trayIcon.BalloonTipTitle = "Keylogging Enabled ::: Protection ON";
                trayIcon.ShowBalloonTip(5000);
            }
            else
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Info;
                trayIcon.BalloonTipText = "Typing Protection Enabled";
                trayIcon.BalloonTipTitle = "WOWAntiKeylogger Status Change";
                trayIcon.ShowBalloonTip(500);
            }
        }

        private void DisableAntiKeylogger()
        {
            IsAntiKeyloggerEnabled = false;

            if (keyloggerCheckBox != null && (bool)keyloggerCheckBox.IsChecked == true)
                keyboardHook.SwallowKeystrokes = false;
            else
                keyboardHook.SwallowKeystrokes = true;

            antiKeyloggerCheckBox.IsChecked = false;
            enableAntiKeyloggerButton.IsChecked = false;
            disableAntiKeyloggerButton.IsChecked = true;

            LockStatusImage.Visibility = System.Windows.Visibility.Hidden;
            UnLockStatusImage.Visibility = System.Windows.Visibility.Visible;

            if ((bool)keyloggerCheckBox.IsChecked == true)
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Error;
                trayIcon.BalloonTipText = "Your keystrokes are now being captured and logged in the WOWAntiKeylogger window. Try typing in several programs like Word, Notepad and Internet Explorer or FireFox. Your keystrokes will be logged to the WOWAntiKeylogger UI.";
                trayIcon.BalloonTipTitle = "Keylogging Enabled ::: Protection OFF";
                trayIcon.ShowBalloonTip(5000);
            }
            else
            {
                trayIcon.BalloonTipIcon = Avalon.Windows.Controls.NotifyBalloonIcon.Error;
                trayIcon.BalloonTipText = "Typing Protection Disabled";
                trayIcon.BalloonTipTitle = "WOWAntiKeylogger Status Change";
                trayIcon.ShowBalloonTip(500);
            }
        }

        private void MinimizeWindow()
        {
            MinimizeLabel.Content = "";
            startupTimer.Stop();
            startupTimer.IsEnabled = false;

            this.WindowState = System.Windows.WindowState.Minimized;
            this.Visibility = System.Windows.Visibility.Hidden;
            trayIcon.Visibility = System.Windows.Visibility.Hidden;

            rehookTimer.Start();
        }
    }
}

