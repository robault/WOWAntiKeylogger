using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Avalon.Internal.Win32;
using Avalon.Internal.Utility;
using System.Security;

namespace Avalon.Windows.Controls
{
    /// <summary>
    /// Specifies a component that creates an icon in the notification area.
    /// </summary>
    [ContentProperty("Text"), DefaultEvent("MouseDoubleClick"),
     System.Security.Permissions.UIPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Window=System.Security.Permissions.UIPermissionWindow.AllWindows)]
    public sealed class NotifyIcon : FrameworkElement, IDisposable, IAddChild
    {
        #region Fields

        private const int WM_TRAYMOUSEMESSAGE = 0x800;
        private static readonly int WM_TASKBARCREATED;

        private static readonly System.Security.Permissions.UIPermission _allWindowsPermission = new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.AllWindows);
        private static int _nextId;

        private readonly object _syncObj = new object();

        private NotifyIconNativeWindow _window;
        private int _id = _nextId++;
        private bool _iconCreated;
        private bool _doubleClick;

        #endregion

        #region Events

        /// <summary>
        /// Identifies the <see cref="BalloonTipClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent BalloonTipClickEvent = EventManager.RegisterRoutedEvent("BalloonTipClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the balloon tip is clicked.
        /// </summary>
        public event RoutedEventHandler BalloonTipClick
        {
            add { AddHandler(BalloonTipClickEvent, value); }
            remove { RemoveHandler(BalloonTipClickEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BalloonTipClosed"/> routed event.
        /// </summary>
        public static readonly RoutedEvent BalloonTipClosedEvent = EventManager.RegisterRoutedEvent("BalloonTipClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the balloon tip is closed by the user.
        /// </summary>
        public event RoutedEventHandler BalloonTipClosed
        {
            add { AddHandler(BalloonTipClosedEvent, value); }
            remove { RemoveHandler(BalloonTipClosedEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BalloonTipShown"/> routed event.
        /// </summary>
        public static readonly RoutedEvent BalloonTipShownEvent = EventManager.RegisterRoutedEvent("BalloonTipShown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the balloon tip is displayed on the screen.
        /// </summary>
        public event RoutedEventHandler BalloonTipShown
        {
            add { AddHandler(BalloonTipShownEvent, value); }
            remove { RemoveHandler(BalloonTipShownEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Click"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the user clicks the icon in the notification area.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DoubleClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent DoubleClickEvent = EventManager.RegisterRoutedEvent("DoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the user double-clicks the icon in the notification area of the taskbar.
        /// </summary>
        public event RoutedEventHandler DoubleClick
        {
            add { AddHandler(DoubleClickEvent, value); }
            remove { RemoveHandler(DoubleClickEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MouseClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent("MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the user clicks a <see cref="NotifyIcon"/> with the mouse.
        /// </summary>
        public event MouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MouseDoubleClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Occurs when the user double-clicks the <see cref="NotifyIcon"/> with the mouse.
        /// </summary>
        public event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }

        #endregion

        #region Constructor/Destructor

        [SecurityCritical]
        static NotifyIcon()
        {
            WM_TASKBARCREATED = NativeMethods.RegisterWindowMessage("TaskbarCreated");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyIcon"/> class.
        /// </summary>
        [SecurityCritical]
        public NotifyIcon()
        {
            _window = new NotifyIconNativeWindow(this);
            UpdateIcon(IsVisible);

            IsVisibleChanged += OnIsVisibleChanged;
        }

        [SecurityCritical]
        ~NotifyIcon()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        [SecurityCritical]
        public void Dispose()
        {
            Dispose(true);
        }

        [SecurityCritical]
        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_window != null)
                {
                    UpdateIcon(false);
                    _window.DestroyHandle();
                }
            }
            else if (_window != null && _window.Handle != IntPtr.Zero)
            {
                NativeMethods.PostMessage(new HandleRef(_window, _window.Handle), NativeMethods.WindowMessage.Close, 0, 0);
                _window.DestroyHandle();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays a balloon tip in the taskbar for the specified time period.
        /// </summary>
        /// <param name="timeout">The time period, in milliseconds, the balloon tip should display.</param>
        public void ShowBalloonTip(int timeout)
        {
            ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
        }

        /// <summary>
        /// Displays a balloon tip with the specified title, text, and icon in the taskbar for the specified time period.
        /// </summary>
        /// <param name="timeout">The time period, in milliseconds, the balloon tip should display.</param>
        /// <param name="tipTitle">The title to display on the balloon tip.</param>
        /// <param name="tipText">The text to display on the balloon tip.</param>
        /// <param name="tipIcon">One of the <see cref="NotifyBalloonIcon"/> values.</param>
        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, NotifyBalloonIcon tipIcon)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout cannot be negative.");
            }
            ArgumentValidator.NotNullOrEmptyString(tipText, "tipText");
            ArgumentValidator.EnumValueIsDefined(typeof(NotifyBalloonIcon), tipIcon, "tipIcon");

            if (_iconCreated)
            {
                _allWindowsPermission.Demand();
                if (_window.Handle == IntPtr.Zero)
                {
                    _window.CreateHandle(new System.Windows.Forms.CreateParams());
                }

                NativeMethods.NOTIFYICONDATA pnid = new NativeMethods.NOTIFYICONDATA
                {
                    hWnd = _window.Handle,
                    uID = _id,
                    uFlags = NativeMethods.NotifyIconFlags.Balloon,
                    uTimeoutOrVersion = timeout,
                    szInfoTitle = tipTitle,
                    szInfo = tipText,
                    dwInfoFlags = (int)tipIcon
                };
                NativeMethods.Shell_NotifyIcon(1, pnid);
            }
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
        private void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                NativeMethods.SetForegroundWindow(new HandleRef(_window, _window.Handle));

                ContextMenuService.SetPlacement(ContextMenu, PlacementMode.MousePoint);
                ContextMenu.IsOpen = true;
            }
        }

        [SecurityCritical]
        private void UpdateIcon(bool showIconInTray)
        {
            lock (_syncObj)
            {
                IntPtr iconHandle = IntPtr.Zero;

                try
                {
                    _allWindowsPermission.Demand();

                    _window.LockReference(showIconInTray);
                    if (showIconInTray && (_window.Handle == IntPtr.Zero))
                    {
                        _window.CreateHandle(new System.Windows.Forms.CreateParams());
                    }

                    NativeMethods.NOTIFYICONDATA pnid = new NativeMethods.NOTIFYICONDATA
                    {
                        uCallbackMessage = WM_TRAYMOUSEMESSAGE,
                        uFlags = NativeMethods.NotifyIconFlags.Message | NativeMethods.NotifyIconFlags.ToolTip,
                        hWnd = _window.Handle,
                        uID = _id,
                        szTip = Text
                    };
                    if (Icon != null)
                    {
                        iconHandle = NativeMethods.GetHIcon(Icon);

                        pnid.uFlags |= NativeMethods.NotifyIconFlags.Icon;
                        pnid.hIcon = iconHandle;
                    }

                    if (showIconInTray && iconHandle != null)
                    {
                        if (!_iconCreated)
                        {
                            NativeMethods.Shell_NotifyIcon(0, pnid);
                            _iconCreated = true;
                        }
                        else
                        {
                            NativeMethods.Shell_NotifyIcon(1, pnid);
                        }
                    }
                    else if (_iconCreated)
                    {
                        NativeMethods.Shell_NotifyIcon(2, pnid);
                        _iconCreated = false;
                    }
                }
                finally
                {
                    if (iconHandle != IntPtr.Zero)
                    {
                        NativeMethods.DestroyIcon(iconHandle);
                    }
                }
            }
        }

        [SecurityCritical]
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIcon(IsVisible);
        }

        #endregion

        #region WndProc Methods

        private void WmMouseDown(MouseButton button, int clicks)
        {
            MouseButtonEventArgs args = null;

            if (clicks == 2)
            {
                RaiseEvent(new RoutedEventArgs(DoubleClickEvent));

                args = new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, button);
                args.RoutedEvent = MouseDoubleClickEvent;
                RaiseEvent(args);

                _doubleClick = true;
            }

            args = new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, button);
            args.RoutedEvent = MouseDownEvent;
            RaiseEvent(args);
        }

        private void WmMouseMove()
        {
            MouseEventArgs args = new MouseEventArgs(InputManager.Current.PrimaryMouseDevice, 0);
            args.RoutedEvent = MouseMoveEvent;
            RaiseEvent(args);
        }

        private void WmMouseUp(MouseButton button)
        {
            MouseButtonEventArgs args = new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, button);
            args.RoutedEvent = MouseUpEvent;
            RaiseEvent(args);

            if (!_doubleClick)
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent));

                args = new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, button);
                args.RoutedEvent = MouseClickEvent;
                RaiseEvent(args);
            }

            _doubleClick = false;
        }

        [SecurityCritical]
        private void WmTaskbarCreated()
        {
            _iconCreated = false;
            UpdateIcon(IsVisible);
        }

        [SecurityCritical]
        private void WndProc(ref System.Windows.Forms.Message msg)
        {
            int message = msg.Msg;
            if (message <= 0x2c) //WM_MEASUREITEM
            {
                if (message == 2)
                {
                    UpdateIcon(false);
                    return;
                }
            }
            else
            {
                if (message != WM_TRAYMOUSEMESSAGE)
                {
                    if (msg.Msg == WM_TASKBARCREATED)
                    {
                        WmTaskbarCreated();
                    }
                    _window.DefWndProc(ref msg);
                    return;
                }
                switch ((int)msg.LParam)
                {
                    case 0x200:
                        WmMouseMove();
                        return;
                    case 0x201:
                        WmMouseDown(MouseButton.Left, 1);
                        return;
                    case 0x202:
                        WmMouseUp(MouseButton.Left);
                        return;
                    case 0x203:
                        WmMouseDown(MouseButton.Left, 2);
                        return;
                    case 0x204:
                        WmMouseDown(MouseButton.Right, 1);
                        return;
                    case 0x205:
                        ShowContextMenu();
                        WmMouseUp(MouseButton.Right);
                        return;
                    case 0x206:
                        WmMouseDown(MouseButton.Right, 2);
                        return;
                    case 0x207:
                        WmMouseDown(MouseButton.Middle, 1);
                        return;
                    case 520:
                        WmMouseUp(MouseButton.Middle);
                        return;
                    case 0x209:
                        WmMouseDown(MouseButton.Middle, 2);
                        return;
                    case 0x402:
                        RaiseEvent(new RoutedEventArgs(BalloonTipShownEvent));
                        return;
                    case 0x403:
                    case 0x404:
                        RaiseEvent(new RoutedEventArgs(BalloonTipClosedEvent));
                        return;
                    case 0x405:
                        RaiseEvent(new RoutedEventArgs(BalloonTipClickEvent));
                        return;
                }
                return;
            }
            if (msg.Msg == WM_TASKBARCREATED)
            {
                WmTaskbarCreated();
            }
            _window.DefWndProc(ref msg);
        }

        #endregion

        #region Properties

        #region BalloonTipIcon

        /// <summary>
        /// Gets or sets the icon to display on the balloon tip.
        /// </summary>
        /// <value>The balloon tip icon.</value>
        public NotifyBalloonIcon BalloonTipIcon
        {
            get { return (NotifyBalloonIcon)GetValue(BalloonTipIconProperty); }
            set { SetValue(BalloonTipIconProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BalloonTipIcon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BalloonTipIconProperty =
            DependencyProperty.Register("BalloonTipIcon", typeof(NotifyBalloonIcon), typeof(NotifyIcon), new FrameworkPropertyMetadata(NotifyBalloonIcon.None));

        #endregion

        #region BalloonTipText

        /// <summary>
        /// Gets or sets the text to display on the balloon tip.
        /// </summary>
        /// <value>The balloon tip text.</value>
        public string BalloonTipText
        {
            get { return (string)GetValue(BalloonTipTextProperty); }
            set { SetValue(BalloonTipTextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BalloonTipText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BalloonTipTextProperty =
            DependencyProperty.Register("BalloonTipText", typeof(string), typeof(NotifyIcon), new FrameworkPropertyMetadata());

        #endregion

        #region BalloonTipTitle

        /// <summary>
        /// Gets or sets the title of the balloon tip.
        /// </summary>
        /// <value>The balloon tip title.</value>
        public string BalloonTipTitle
        {
            get { return (string)GetValue(BalloonTipTitleProperty); }
            set { SetValue(BalloonTipTitleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BalloonTipTitle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BalloonTipTitleProperty =
            DependencyProperty.Register("BalloonTipTitle", typeof(string), typeof(NotifyIcon), new FrameworkPropertyMetadata());

        #endregion

        #region Text

        /// <summary>
        /// Gets or sets the tooltip text displayed when the mouse pointer rests on a notification area icon.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NotifyIcon), new FrameworkPropertyMetadata(OnTextPropertyChanged, OnCoerceTextProperty), ValidateTextPropety);

        private static bool ValidateTextPropety(object baseValue)
        {
            string value = (string)baseValue;

            return value == null || value.Length <= 0x3f;
        }

        [SecurityCritical]
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NotifyIcon notifyIcon = (NotifyIcon)d;

            if (notifyIcon._iconCreated)
            {
                notifyIcon.UpdateIcon(true);
            }
        }

        private static object OnCoerceTextProperty(DependencyObject d, object baseValue)
        {
            string value = (string)baseValue;

            if (value == null)
            {
                value = string.Empty;
            }

            return value;
        }

        #endregion

        #region Icon

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NotifyIcon), new FrameworkPropertyMetadata(OnNotifyIconChanged));

        [SecurityCritical]
        private static void OnNotifyIconChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NotifyIcon notifyIcon = (NotifyIcon)o;

            notifyIcon.UpdateIcon(notifyIcon.IsVisible);
        }

        #endregion

        #endregion

        #region IAddChild Members

        void IAddChild.AddChild(object value)
        {
            throw new InvalidOperationException("Cannot have non-text children.");
        }

        void IAddChild.AddText(string text)
        {
            Text = text;
        }

        #endregion

        #region NotifyIconNativeWindow Class

        private class NotifyIconNativeWindow : System.Windows.Forms.NativeWindow
        {
            internal NotifyIcon _reference;
            private GCHandle _rootRef;

            internal NotifyIconNativeWindow(NotifyIcon component)
            {
                _reference = component;
            }

            [SecurityCritical]
            ~NotifyIconNativeWindow()
            {
                if (Handle != IntPtr.Zero)
                {
                    NativeMethods.PostMessage(new HandleRef(this, base.Handle), NativeMethods.WindowMessage.Close, 0, 0);
                }
            }

            public void LockReference(bool locked)
            {
                if (locked)
                {
                    if (!_rootRef.IsAllocated)
                    {
                        _rootRef = GCHandle.Alloc(_reference, GCHandleType.Normal);
                    }
                }
                else if (_rootRef.IsAllocated)
                {
                    _rootRef.Free();
                }
            }

            [SecurityCritical]
            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                _reference.WndProc(ref m);
            }
        }

        #endregion
    }
}
