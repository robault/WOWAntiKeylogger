using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WOWAntiKeylogger
{
    /// <summary>
    /// A class that manages a global low level keyboard hook
    /// </summary>
    class GlobalKeyboardHook
    {
        #region Constant, Structure and Delegate Definitions
        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        private delegate int KeyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

        /// <summary>
        /// This windows structure contains information about a low-level keyboard input event. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        /// <summary>
        /// Windows NT/2000/XP: Installs a hook procedure that monitors low-level keyboard  input events.
        /// </summary>
        private const int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// windows messages
        /// </summary>
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        /// <summary>
        /// windows virtual key codes
        /// </summary>
        private const byte VK_RETURN = 0X0D; //Enter
        private const byte VK_SPACE = 0X20; //Space
        private const byte VK_SHIFT = 0x10;
        private const byte VK_CAPITAL = 0x14;
        #endregion

        #region Instance Variables
        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        private IntPtr hookID = IntPtr.Zero;
        /// <summary>
        /// Store delegate as a member variable to prevent it being garbage collected!!!
        /// </summary>
        private KeyboardHookProc hookProc;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a key is pressed
        /// </summary>
        public event KeyPressEventHandler KeyPress;
        #endregion

        #region Constructors and Destructors
        /// <summary>
        /// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
        /// </summary>
        public GlobalKeyboardHook(bool swallowKeystrokes)
        {
            Hook(swallowKeystrokes);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
        /// </summary>
        ~GlobalKeyboardHook()
        {
            Unhook();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Installs the global hook
        /// </summary>
        public void Hook(bool swallowKeystrokes)
        {
            SwallowKeystrokes = swallowKeystrokes;
            hookProc = new KeyboardHookProc(HookCallback);
            IntPtr hInstance = LoadLibrary("User32");
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hInstance, 0);
        }

        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        public void Unhook()
        {
            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
            SwallowKeystrokes = false;
            hookProc = null;
        }

        public bool SwallowKeystrokes { get; set; }

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        private int HookCallback(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            if (!SwallowKeystrokes)
            {
                if (code >= 0)
                {
                    if (wParam == WM_KEYDOWN)
                    {
                        byte[] keyState = new byte[256];
                        GetKeyboardState(keyState);
                        byte[] inBuffer = new byte[2];
                        if (ToAscii(lParam.vkCode, lParam.scanCode, keyState, inBuffer, lParam.flags) == 1)
                        {
                            char key = (char)inBuffer[0];
                            bool isDownShift = ((GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
                            bool isDownCapslock = (GetKeyState(VK_CAPITAL) != 0 ? true : false);
                            if ((isDownCapslock ^ isDownShift) && Char.IsLetter(key))
                            {
                                key = Char.ToUpper(key);
                            }

                            if (KeyPress != null)
                            {
                                KeyPressEventArgs e = new KeyPressEventArgs(key);
                                KeyPress(this, e);
                                if (e.Handled)
                                    return 1;
                            }
                        }
                    }
                }
                return CallNextHookEx(hookID, code, wParam, ref lParam);
            }
            else
                return 0;
        }
        #endregion

        #region DLL imports
        
        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// The ToAscii function translates the specified virtual-key code and keyboard 
        /// state to the corresponding character or characters. The function translates the code 
        /// using the input language and physical keyboard layout identified by the keyboard layout handle.
        /// </summary>
        /// <param name="uVirtKey">
        /// [in] Specifies the virtual-key code to be translated. 
        /// </param>
        /// <param name="uScanCode">
        /// [in] Specifies the hardware scan code of the key to be translated. 
        /// The high-order bit of this value is set if the key is up (not pressed). 
        /// </param>
        /// <param name="lpbKeyState">
        /// [in] Pointer to a 256-byte array that contains the current keyboard state. 
        /// Each element (byte) in the array contains the state of one key. 
        /// If the high-order bit of a byte is set, the key is down (pressed). 
        /// The low bit, if set, indicates that the key is toggled on. In this function, 
        /// only the toggle bit of the CAPS LOCK key is relevant. The toggle state 
        /// of the NUM LOCK and SCROLL LOCK keys is ignored.
        /// </param>
        /// <param name="lpwTransKey">
        /// [out] Pointer to the buffer that receives the translated character or characters. 
        /// </param>
        /// <param name="fuState">
        /// [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise. 
        /// </param>
        /// <returns>
        /// If the specified key is a dead key, the return value is negative. Otherwise, it is one of the following values. 
        /// Value Meaning 
        /// 0 The specified virtual key has no translation for the current state of the keyboard. 
        /// 1 One character was copied to the buffer. 
        /// 2 Two characters were copied to the buffer. This usually happens when a dead-key character 
        /// (accent or diacritic) stored in the keyboard layout cannot be composed with the specified 
        /// virtual key to form a single character. 
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/keyboardinput/keyboardinputreference/keyboardinputfunctions/toascii.asp
        /// </remarks>
        [DllImport("user32")]
        private static extern int ToAscii(
            int uVirtKey,
            int uScanCode,
            byte[] lpbKeyState,
            byte[] lpwTransKey,
            int fuState);

        /// <summary>
        /// The GetKeyboardState function copies the status of the 256 virtual keys to the 
        /// specified buffer. 
        /// </summary>
        /// <param name="pbKeyState">
        /// [in] Pointer to a 256-byte array that contains keyboard key states. 
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. 
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/keyboardinput/keyboardinputreference/keyboardinputfunctions/toascii.asp
        /// </remarks>
        [DllImport("user32")]
        private static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        #endregion
    }
}
