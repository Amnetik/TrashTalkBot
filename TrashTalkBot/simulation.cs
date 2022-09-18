using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using static TrashTalkBot.KeyMessage;

namespace TrashTalkBot
{
    /// <summary>
    /// Native methods
    /// </summary>
    internal static class NativeMethods
    {
        //User32 wrappers cover API's used for Mouse input
        #region User32
        // Two special bitmasks we define to be able to grab
        // shift and character information out of a VKey.
        internal const int VKeyShiftMask = 0x0100;
        internal const int VKeyCharMask = 0x00FF;

        // Various Win32 constants
        internal const int KeyeventfExtendedkey = 0x0001;
        internal const int KeyeventfKeyup = 0x0002;
        internal const int KeyeventfScancode = 0x0008;

        internal const int MouseeventfVirtualdesk = 0x4000;

        internal const int SMXvirtualscreen = 76;
        internal const int SMYvirtualscreen = 77;
        internal const int SMCxvirtualscreen = 78;
        internal const int SMCyvirtualscreen = 79;

        internal const int XButton1 = 0x0001;
        internal const int XButton2 = 0x0002;
        internal const int WheelDelta = 120;

        internal const int InputMouse = 0;
        internal const int InputKeyboard = 1;

        // Various Win32 data structures
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            internal int type;
            internal INPUTUNION union;
        };

        [StructLayout(LayoutKind.Explicit)]
        internal struct INPUTUNION
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mouseInput;
            [FieldOffset(0)]
            internal KEYBDINPUT keyboardInput;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal int dwFlags;
            internal int time;
            internal IntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal short wVk;
            internal short wScan;
            internal int dwFlags;
            internal int time;
            internal IntPtr dwExtraInfo;
        };

        [Flags]
        internal enum SendMouseInputFlags
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            Absolute = 0x8000,
        };

        // Importing various Win32 APIs that we need for input
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int MapVirtualKey(int nVirtKey, int nMapType);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int SendInput(int nInputs, ref INPUT mi, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr windowHandle, int hotkeyId, uint modifierKeys, uint virtualKey);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr windowHandle, int hotkeyId);

        #endregion
    }

    public static class Keyboard
    {
        #region Public Members

        /// <summary>
        /// Presses down a key. Made by DrustZ : https://gist.github.com/DrustZ
        /// </summary>
        /// <param name="key">The key to press.</param>
        public static void Press(Key key)
        {
            SendKeyboardInput(key, true);
        }

        /// <summary>
        /// Releases a key.
        /// </summary>
        /// <param name="key">The key to release.</param>
        public static void Release(Key key)
        {
            SendKeyboardInput(key, false);
        }

        /// <summary>
        /// Resets the system keyboard to a clean state.
        /// </summary>
        public static void Reset()
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key != Key.None && (System.Windows.Input.Keyboard.GetKeyStates(key) & KeyStates.Down) > 0)
                {
                    Release(key);
                }
            }
        }

        /// <summary>
        /// Performs a press-and-release operation for the specified key, which is effectively equivallent to typing.
        /// </summary>
        /// <param name="key">The key to press.</param>
        public static void Type(Key key)
        {
            bool isInput = TryGetKeyMessage(key, out var km);
            if (isInput)
                km.Unregister();

            Press(key);
            Release(key);

            if (isInput)
                km.Register();
        }

        /// <summary>
        /// Types the specified text.
        /// </summary>
        /// <param name="text">The text to type.</param>
        public static void Type(string text)
        {
            foreach (char c in text)
            {
                // We get the vKey value for the character via a Win32 API. We then use bit masks to pull the
                // upper and lower bytes to get the shift state and key information. We then use WPF KeyInterop
                // to go from the vKey key info into a System.Windows.Input.Key data structure. This work is
                // necessary because Key doesn't distinguish between upper and lower case, so we have to wrap
                // the key type inside a shift press/release if necessary.
                int vKeyValue = NativeMethods.VkKeyScan(c);
                bool keyIsShifted = (vKeyValue & NativeMethods.VKeyShiftMask) == NativeMethods.VKeyShiftMask;
                Key key = KeyInterop.KeyFromVirtualKey(vKeyValue & NativeMethods.VKeyCharMask);

                if (keyIsShifted)
                {
                    Type(key, new Key[] { Key.LeftShift });
                }
                else
                {
                    Type(key);
                }
            }
        }

        public static void Type(string text, int ms)
        {
            Thread.Sleep(10);
            foreach (char c in text)
            {
                
                // We get the vKey value for the character via a Win32 API. We then use bit masks to pull the
                // upper and lower bytes to get the shift state and key information. We then use WPF KeyInterop
                // to go from the vKey key info into a System.Windows.Input.Key data structure. This work is
                // necessary because Key doesn't distinguish between upper and lower case, so we have to wrap
                // the key type inside a shift press/release if necessary.
                int vKeyValue = NativeMethods.VkKeyScan(c);
                bool keyIsShifted = (vKeyValue & NativeMethods.VKeyShiftMask) == NativeMethods.VKeyShiftMask;
                Key key = KeyInterop.KeyFromVirtualKey(vKeyValue & NativeMethods.VKeyCharMask);

                if (keyIsShifted)
                {
                    Type(key, new Key[] { Key.LeftShift });
                }
                else
                {
                    Type(key);
                }
                Thread.Sleep(ms);
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Types a key while a set of modifier keys are being pressed. Modifer keys
        /// are pressed in the order specified and released in reverse order.
        /// </summary>
        /// <param name="key">Key to type.</param>
        /// <param name="modifierKeys">Set of keys to hold down with key is typed.</param>
        private static void Type(Key key, Key[] modifierKeys)
        {
            foreach (Key modiferKey in modifierKeys)
            {
                Press(modiferKey);
            }

            Type(key);

            foreach (Key modifierKey in modifierKeys.Reverse())
            {
                Release(modifierKey);
            }
        }

        /// <summary>
        /// Injects keyboard input into the system.
        /// </summary>
        /// <param name="key">Indicates the key pressed or released. Can be one of the constants defined in the Key enum.</param>
        /// <param name="press">True to inject a key press, false to inject a key release.</param>
        private static void SendKeyboardInput(Key key, bool press)
        {

            NativeMethods.INPUT ki = new NativeMethods.INPUT();
            ki.type = NativeMethods.InputKeyboard;
            ki.union.keyboardInput.wVk = (short)KeyInterop.VirtualKeyFromKey(key);
            ki.union.keyboardInput.wScan = (short)NativeMethods.MapVirtualKey(ki.union.keyboardInput.wVk, 0);

            int dwFlags = 0;

            if (ki.union.keyboardInput.wScan > 0)
            {
                dwFlags |= NativeMethods.KeyeventfScancode;
            }

            if (!press)
            {
                dwFlags |= NativeMethods.KeyeventfKeyup;
            }

            ki.union.keyboardInput.dwFlags = dwFlags;

            if (ExtendedKeys.Contains(key))
            {
                ki.union.keyboardInput.dwFlags |= NativeMethods.KeyeventfExtendedkey;
            }

            ki.union.keyboardInput.time = 0;
            ki.union.keyboardInput.dwExtraInfo = new IntPtr(0);

            if (NativeMethods.SendInput(1, ref ki, Marshal.SizeOf(ki)) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        // From the SDK:
        // The extended-key flag indicates whether the keystroke message originated from one of
        // the additional keys on the enhanced keyboard. The extended keys consist of the ALT and
        // CTRL keys on the right-hand side of the keyboard; the INS, DEL, HOME, END, PAGE UP,
        // PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; the NUM LOCK
        // key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in
        // the numeric keypad. The extended-key flag is set if the key is an extended key. 
        //
        // - docs appear to be incorrect. Use of Spy++ indicates that break is not an extended key.
        // Also, menu key and windows keys also appear to be extended.
        private static readonly Key[] ExtendedKeys = new Key[] {
                                                                   Key.RightAlt,
                                                                   Key.RightCtrl,
                                                                   Key.NumLock,
                                                                   Key.Insert,
                                                                   Key.Delete,
                                                                   Key.Home,
                                                                   Key.End,
                                                                   Key.Prior,
                                                                   Key.Next,
                                                                   Key.Up,
                                                                   Key.Down,
                                                                   Key.Left,
                                                                   Key.Right,
                                                                   Key.Apps,
                                                                   Key.RWin,
                                                                   Key.LWin };
        // Note that there are no distinct values for the following keys:
        // numpad divide
        // numpad enter

        #endregion
    }
}