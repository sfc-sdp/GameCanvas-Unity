/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCanvas
{
    /// <summary>
    /// <see cref="Key"/> と <see cref="char"/> (UTF-16) の相互変換
    /// </summary>
    public static class UnityKeyCodeExtensions
    {
        public static Key ToKey(this KeyCode code)
        {
            return code switch
            {
                KeyCode.None => Key.None,
                KeyCode.Backspace => Key.Backspace,
                KeyCode.Delete => Key.Delete,
                KeyCode.Tab => Key.Tab,
                KeyCode.Return => Key.Enter,
                KeyCode.Pause => Key.Pause,
                KeyCode.Escape => Key.Escape,
                KeyCode.Space => Key.Space,
                KeyCode.Keypad0 => Key.Numpad0,
                KeyCode.Keypad1 => Key.Numpad1,
                KeyCode.Keypad2 => Key.Numpad2,
                KeyCode.Keypad3 => Key.Numpad3,
                KeyCode.Keypad4 => Key.Numpad4,
                KeyCode.Keypad5 => Key.Numpad5,
                KeyCode.Keypad6 => Key.Numpad6,
                KeyCode.Keypad7 => Key.Numpad7,
                KeyCode.Keypad8 => Key.Numpad8,
                KeyCode.Keypad9 => Key.Numpad9,
                KeyCode.KeypadPeriod => Key.NumpadPeriod,
                KeyCode.KeypadDivide => Key.NumpadDivide,
                KeyCode.KeypadMultiply => Key.NumpadMultiply,
                KeyCode.KeypadMinus => Key.NumpadMinus,
                KeyCode.KeypadPlus => Key.NumpadPlus,
                KeyCode.KeypadEnter => Key.NumpadEnter,
                KeyCode.KeypadEquals => Key.NumpadEquals,
                KeyCode.UpArrow => Key.UpArrow,
                KeyCode.DownArrow => Key.DownArrow,
                KeyCode.RightArrow => Key.RightArrow,
                KeyCode.LeftArrow => Key.LeftArrow,
                KeyCode.Insert => Key.Insert,
                KeyCode.Home => Key.Home,
                KeyCode.End => Key.End,
                KeyCode.PageUp => Key.PageUp,
                KeyCode.PageDown => Key.PageDown,
                KeyCode.F1 => Key.F1,
                KeyCode.F2 => Key.F2,
                KeyCode.F3 => Key.F3,
                KeyCode.F4 => Key.F4,
                KeyCode.F5 => Key.F5,
                KeyCode.F6 => Key.F6,
                KeyCode.F7 => Key.F7,
                KeyCode.F8 => Key.F8,
                KeyCode.F9 => Key.F9,
                KeyCode.F10 => Key.F10,
                KeyCode.F11 => Key.F11,
                KeyCode.F12 => Key.F12,
                KeyCode.Alpha0 => Key.Digit0,
                KeyCode.Alpha1 => Key.Digit1,
                KeyCode.Alpha2 => Key.Digit2,
                KeyCode.Alpha3 => Key.Digit3,
                KeyCode.Alpha4 => Key.Digit4,
                KeyCode.Alpha5 => Key.Digit5,
                KeyCode.Alpha6 => Key.Digit6,
                KeyCode.Alpha7 => Key.Digit7,
                KeyCode.Alpha8 => Key.Digit8,
                KeyCode.Alpha9 => Key.Digit9,
                KeyCode.Quote => Key.Quote,
                KeyCode.Comma => Key.Comma,
                KeyCode.Minus => Key.Minus,
                KeyCode.Period => Key.Period,
                KeyCode.Slash => Key.Slash,
                KeyCode.Semicolon => Key.Semicolon,
                KeyCode.Equals => Key.Equals,
                KeyCode.LeftBracket => Key.LeftBracket,
                KeyCode.Backslash => Key.Backslash,
                KeyCode.RightBracket => Key.RightBracket,
                KeyCode.BackQuote => Key.Backquote,
                KeyCode.A => Key.A,
                KeyCode.B => Key.B,
                KeyCode.C => Key.C,
                KeyCode.D => Key.D,
                KeyCode.E => Key.E,
                KeyCode.F => Key.F,
                KeyCode.G => Key.G,
                KeyCode.H => Key.H,
                KeyCode.I => Key.I,
                KeyCode.J => Key.J,
                KeyCode.K => Key.K,
                KeyCode.L => Key.L,
                KeyCode.M => Key.M,
                KeyCode.N => Key.N,
                KeyCode.O => Key.O,
                KeyCode.P => Key.P,
                KeyCode.Q => Key.Q,
                KeyCode.R => Key.R,
                KeyCode.S => Key.S,
                KeyCode.T => Key.T,
                KeyCode.U => Key.U,
                KeyCode.V => Key.V,
                KeyCode.W => Key.W,
                KeyCode.X => Key.X,
                KeyCode.Y => Key.Y,
                KeyCode.Z => Key.Z,
                KeyCode.Numlock => Key.NumLock,
                KeyCode.CapsLock => Key.CapsLock,
                KeyCode.ScrollLock => Key.ScrollLock,
                KeyCode.RightShift => Key.RightShift,
                KeyCode.LeftShift => Key.LeftShift,
                KeyCode.RightAlt => Key.RightAlt,
                KeyCode.LeftAlt => Key.LeftAlt,
                KeyCode.LeftCommand => Key.LeftCommand,
                KeyCode.LeftWindows => Key.LeftWindows,
                KeyCode.RightCommand => Key.RightCommand,
                KeyCode.RightWindows => Key.RightWindows,
                KeyCode.AltGr => Key.AltGr,
                KeyCode.Print => Key.PrintScreen,
                KeyCode.Menu => Key.ContextMenu,
                _ => Key.None,
            };
        }

        public static bool TryGetChar(this KeyCode key, out char c)
        {
            switch (key)
            {
                case KeyCode.Tab:
                    c = '\t'; return true;
                case KeyCode.Return:
                    c = '\n'; return true;
                case KeyCode.Space:
                    c = ' '; return true;
                case KeyCode.Alpha0:
                case KeyCode.Keypad0:
                    c = '0'; return true;
                case KeyCode.Alpha1:
                case KeyCode.Keypad1:
                    c = '1'; return true;
                case KeyCode.Alpha2:
                case KeyCode.Keypad2:
                    c = '2'; return true;
                case KeyCode.Alpha3:
                case KeyCode.Keypad3:
                    c = '3'; return true;
                case KeyCode.Alpha4:
                case KeyCode.Keypad4:
                    c = '4'; return true;
                case KeyCode.Alpha5:
                case KeyCode.Keypad5:
                    c = '5'; return true;
                case KeyCode.Alpha6:
                case KeyCode.Keypad6:
                    c = '6'; return true;
                case KeyCode.Alpha7:
                case KeyCode.Keypad7:
                    c = '7'; return true;
                case KeyCode.Alpha8:
                case KeyCode.Keypad8:
                    c = '8'; return true;
                case KeyCode.Alpha9:
                case KeyCode.Keypad9:
                    c = '9'; return true;
                case KeyCode.Period:
                case KeyCode.KeypadPeriod:
                    c = '.'; return true;
                case KeyCode.Slash:
                case KeyCode.KeypadDivide:
                    c = '/'; return true;
                case KeyCode.Asterisk:
                case KeyCode.KeypadMultiply:
                    c = '*'; return true;
                case KeyCode.Minus:
                case KeyCode.KeypadMinus:
                    c = '-'; return true;
                case KeyCode.Plus:
                case KeyCode.KeypadPlus:
                    c = '+'; return true;
                case KeyCode.KeypadEnter:
                    c = '\n'; return true;
                case KeyCode.Equals:
                case KeyCode.KeypadEquals:
                    c = '='; return true;
                case KeyCode.UpArrow:
                    c = '↑'; return true;
                case KeyCode.DownArrow:
                    c = '↓'; return true;
                case KeyCode.RightArrow:
                    c = '→'; return true;
                case KeyCode.LeftArrow:
                    c = '←'; return true;
                case KeyCode.Exclaim:
                    c = '!'; return true;
                case KeyCode.DoubleQuote:
                    c = '\"'; return true;
                case KeyCode.Hash:
                    c = '#'; return true;
                case KeyCode.Dollar:
                    c = '$'; return true;
                case KeyCode.Percent:
                    c = '%'; return true;
                case KeyCode.Ampersand:
                    c = '&'; return true;
                case KeyCode.Quote:
                    c = '\''; return true;
                case KeyCode.LeftParen:
                    c = '('; return true;
                case KeyCode.RightParen:
                    c = ')'; return true;
                case KeyCode.Comma:
                    c = ','; return true;
                case KeyCode.Colon:
                    c = ':'; return true;
                case KeyCode.Semicolon:
                    c = ';'; return true;
                case KeyCode.Less:
                    c = '<'; return true;
                case KeyCode.Greater:
                    c = '>'; return true;
                case KeyCode.Question:
                    c = '?'; return true;
                case KeyCode.At:
                    c = '@'; return true;
                case KeyCode.LeftBracket:
                    c = '['; return true;
                case KeyCode.Backslash:
                    c = '\\'; return true;
                case KeyCode.RightBracket:
                    c = ']'; return true;
                case KeyCode.Caret:
                    c = '^'; return true;
                case KeyCode.Underscore:
                    c = '_'; return true;
                case KeyCode.BackQuote:
                    c = '`'; return true;
                case KeyCode.A:
                case KeyCode.B:
                case KeyCode.C:
                case KeyCode.D:
                case KeyCode.E:
                case KeyCode.F:
                case KeyCode.G:
                case KeyCode.H:
                case KeyCode.I:
                case KeyCode.J:
                case KeyCode.K:
                case KeyCode.L:
                case KeyCode.M:
                case KeyCode.N:
                case KeyCode.O:
                case KeyCode.P:
                case KeyCode.Q:
                case KeyCode.R:
                case KeyCode.S:
                case KeyCode.T:
                case KeyCode.U:
                case KeyCode.V:
                case KeyCode.W:
                case KeyCode.X:
                case KeyCode.Y:
                case KeyCode.Z:
                    c = (char)(key - KeyCode.A + 'a'); return true;
                case KeyCode.LeftCurlyBracket:
                    c = '{'; return true;
                case KeyCode.Pipe:
                    c = '|'; return true;
                case KeyCode.RightCurlyBracket:
                    c = '}'; return true;
                case KeyCode.Tilde:
                    c = '~'; return true;

                case KeyCode.None:
                case KeyCode.Backspace:
                case KeyCode.Delete:
                case KeyCode.Clear:
                case KeyCode.Pause:
                case KeyCode.Escape:
                case KeyCode.Insert:
                case KeyCode.Home:
                case KeyCode.End:
                case KeyCode.PageUp:
                case KeyCode.PageDown:
                case KeyCode.F1:
                case KeyCode.F2:
                case KeyCode.F3:
                case KeyCode.F4:
                case KeyCode.F5:
                case KeyCode.F6:
                case KeyCode.F7:
                case KeyCode.F8:
                case KeyCode.F9:
                case KeyCode.F10:
                case KeyCode.F11:
                case KeyCode.F12:
                case KeyCode.F13:
                case KeyCode.F14:
                case KeyCode.F15:
                case KeyCode.Numlock:
                case KeyCode.CapsLock:
                case KeyCode.ScrollLock:
                case KeyCode.RightShift:
                case KeyCode.LeftShift:
                case KeyCode.RightControl:
                case KeyCode.LeftControl:
                case KeyCode.RightAlt:
                case KeyCode.LeftAlt:
                case KeyCode.LeftCommand:
                case KeyCode.LeftWindows:
                case KeyCode.RightCommand:
                case KeyCode.RightWindows:
                case KeyCode.AltGr:
                case KeyCode.Help:
                case KeyCode.Print:
                case KeyCode.SysReq:
                case KeyCode.Break:
                case KeyCode.Menu:
                case KeyCode.Mouse0:
                case KeyCode.Mouse1:
                case KeyCode.Mouse2:
                case KeyCode.Mouse3:
                case KeyCode.Mouse4:
                case KeyCode.Mouse5:
                case KeyCode.Mouse6:
                case KeyCode.JoystickButton0:
                case KeyCode.JoystickButton1:
                case KeyCode.JoystickButton2:
                case KeyCode.JoystickButton3:
                case KeyCode.JoystickButton4:
                case KeyCode.JoystickButton5:
                case KeyCode.JoystickButton6:
                case KeyCode.JoystickButton7:
                case KeyCode.JoystickButton8:
                case KeyCode.JoystickButton9:
                case KeyCode.JoystickButton10:
                case KeyCode.JoystickButton11:
                case KeyCode.JoystickButton12:
                case KeyCode.JoystickButton13:
                case KeyCode.JoystickButton14:
                case KeyCode.JoystickButton15:
                case KeyCode.JoystickButton16:
                case KeyCode.JoystickButton17:
                case KeyCode.JoystickButton18:
                case KeyCode.JoystickButton19:
                case KeyCode.Joystick1Button0:
                case KeyCode.Joystick1Button1:
                case KeyCode.Joystick1Button2:
                case KeyCode.Joystick1Button3:
                case KeyCode.Joystick1Button4:
                case KeyCode.Joystick1Button5:
                case KeyCode.Joystick1Button6:
                case KeyCode.Joystick1Button7:
                case KeyCode.Joystick1Button8:
                case KeyCode.Joystick1Button9:
                case KeyCode.Joystick1Button10:
                case KeyCode.Joystick1Button11:
                case KeyCode.Joystick1Button12:
                case KeyCode.Joystick1Button13:
                case KeyCode.Joystick1Button14:
                case KeyCode.Joystick1Button15:
                case KeyCode.Joystick1Button16:
                case KeyCode.Joystick1Button17:
                case KeyCode.Joystick1Button18:
                case KeyCode.Joystick1Button19:
                case KeyCode.Joystick2Button0:
                case KeyCode.Joystick2Button1:
                case KeyCode.Joystick2Button2:
                case KeyCode.Joystick2Button3:
                case KeyCode.Joystick2Button4:
                case KeyCode.Joystick2Button5:
                case KeyCode.Joystick2Button6:
                case KeyCode.Joystick2Button7:
                case KeyCode.Joystick2Button8:
                case KeyCode.Joystick2Button9:
                case KeyCode.Joystick2Button10:
                case KeyCode.Joystick2Button11:
                case KeyCode.Joystick2Button12:
                case KeyCode.Joystick2Button13:
                case KeyCode.Joystick2Button14:
                case KeyCode.Joystick2Button15:
                case KeyCode.Joystick2Button16:
                case KeyCode.Joystick2Button17:
                case KeyCode.Joystick2Button18:
                case KeyCode.Joystick2Button19:
                case KeyCode.Joystick3Button0:
                case KeyCode.Joystick3Button1:
                case KeyCode.Joystick3Button2:
                case KeyCode.Joystick3Button3:
                case KeyCode.Joystick3Button4:
                case KeyCode.Joystick3Button5:
                case KeyCode.Joystick3Button6:
                case KeyCode.Joystick3Button7:
                case KeyCode.Joystick3Button8:
                case KeyCode.Joystick3Button9:
                case KeyCode.Joystick3Button10:
                case KeyCode.Joystick3Button11:
                case KeyCode.Joystick3Button12:
                case KeyCode.Joystick3Button13:
                case KeyCode.Joystick3Button14:
                case KeyCode.Joystick3Button15:
                case KeyCode.Joystick3Button16:
                case KeyCode.Joystick3Button17:
                case KeyCode.Joystick3Button18:
                case KeyCode.Joystick3Button19:
                case KeyCode.Joystick4Button0:
                case KeyCode.Joystick4Button1:
                case KeyCode.Joystick4Button2:
                case KeyCode.Joystick4Button3:
                case KeyCode.Joystick4Button4:
                case KeyCode.Joystick4Button5:
                case KeyCode.Joystick4Button6:
                case KeyCode.Joystick4Button7:
                case KeyCode.Joystick4Button8:
                case KeyCode.Joystick4Button9:
                case KeyCode.Joystick4Button10:
                case KeyCode.Joystick4Button11:
                case KeyCode.Joystick4Button12:
                case KeyCode.Joystick4Button13:
                case KeyCode.Joystick4Button14:
                case KeyCode.Joystick4Button15:
                case KeyCode.Joystick4Button16:
                case KeyCode.Joystick4Button17:
                case KeyCode.Joystick4Button18:
                case KeyCode.Joystick4Button19:
                case KeyCode.Joystick5Button0:
                case KeyCode.Joystick5Button1:
                case KeyCode.Joystick5Button2:
                case KeyCode.Joystick5Button3:
                case KeyCode.Joystick5Button4:
                case KeyCode.Joystick5Button5:
                case KeyCode.Joystick5Button6:
                case KeyCode.Joystick5Button7:
                case KeyCode.Joystick5Button8:
                case KeyCode.Joystick5Button9:
                case KeyCode.Joystick5Button10:
                case KeyCode.Joystick5Button11:
                case KeyCode.Joystick5Button12:
                case KeyCode.Joystick5Button13:
                case KeyCode.Joystick5Button14:
                case KeyCode.Joystick5Button15:
                case KeyCode.Joystick5Button16:
                case KeyCode.Joystick5Button17:
                case KeyCode.Joystick5Button18:
                case KeyCode.Joystick5Button19:
                case KeyCode.Joystick6Button0:
                case KeyCode.Joystick6Button1:
                case KeyCode.Joystick6Button2:
                case KeyCode.Joystick6Button3:
                case KeyCode.Joystick6Button4:
                case KeyCode.Joystick6Button5:
                case KeyCode.Joystick6Button6:
                case KeyCode.Joystick6Button7:
                case KeyCode.Joystick6Button8:
                case KeyCode.Joystick6Button9:
                case KeyCode.Joystick6Button10:
                case KeyCode.Joystick6Button11:
                case KeyCode.Joystick6Button12:
                case KeyCode.Joystick6Button13:
                case KeyCode.Joystick6Button14:
                case KeyCode.Joystick6Button15:
                case KeyCode.Joystick6Button16:
                case KeyCode.Joystick6Button17:
                case KeyCode.Joystick6Button18:
                case KeyCode.Joystick6Button19:
                case KeyCode.Joystick7Button0:
                case KeyCode.Joystick7Button1:
                case KeyCode.Joystick7Button2:
                case KeyCode.Joystick7Button3:
                case KeyCode.Joystick7Button4:
                case KeyCode.Joystick7Button5:
                case KeyCode.Joystick7Button6:
                case KeyCode.Joystick7Button7:
                case KeyCode.Joystick7Button8:
                case KeyCode.Joystick7Button9:
                case KeyCode.Joystick7Button10:
                case KeyCode.Joystick7Button11:
                case KeyCode.Joystick7Button12:
                case KeyCode.Joystick7Button13:
                case KeyCode.Joystick7Button14:
                case KeyCode.Joystick7Button15:
                case KeyCode.Joystick7Button16:
                case KeyCode.Joystick7Button17:
                case KeyCode.Joystick7Button18:
                case KeyCode.Joystick7Button19:
                case KeyCode.Joystick8Button0:
                case KeyCode.Joystick8Button1:
                case KeyCode.Joystick8Button2:
                case KeyCode.Joystick8Button3:
                case KeyCode.Joystick8Button4:
                case KeyCode.Joystick8Button5:
                case KeyCode.Joystick8Button6:
                case KeyCode.Joystick8Button7:
                case KeyCode.Joystick8Button8:
                case KeyCode.Joystick8Button9:
                case KeyCode.Joystick8Button10:
                case KeyCode.Joystick8Button11:
                case KeyCode.Joystick8Button12:
                case KeyCode.Joystick8Button13:
                case KeyCode.Joystick8Button14:
                case KeyCode.Joystick8Button15:
                case KeyCode.Joystick8Button16:
                case KeyCode.Joystick8Button17:
                case KeyCode.Joystick8Button18:
                case KeyCode.Joystick8Button19:
                default:
                    c = default;
                    return false;
            }
        }

        public static bool TryGetChar(this Key key, out char c)
        {
            switch (key)
            {
                case Key.Tab:
                    c = '\t'; return true;
                case Key.Enter:
                    c = '\n'; return true;
                case Key.Space:
                    c = ' '; return true;
                case Key.Digit0:
                case Key.Numpad0:
                    c = '0'; return true;
                case Key.Digit1:
                case Key.Numpad1:
                    c = '1'; return true;
                case Key.Digit2:
                case Key.Numpad2:
                    c = '2'; return true;
                case Key.Digit3:
                case Key.Numpad3:
                    c = '3'; return true;
                case Key.Digit4:
                case Key.Numpad4:
                    c = '4'; return true;
                case Key.Digit5:
                case Key.Numpad5:
                    c = '5'; return true;
                case Key.Digit6:
                case Key.Numpad6:
                    c = '6'; return true;
                case Key.Digit7:
                case Key.Numpad7:
                    c = '7'; return true;
                case Key.Digit8:
                case Key.Numpad8:
                    c = '8'; return true;
                case Key.Digit9:
                case Key.Numpad9:
                    c = '9'; return true;
                case Key.Period:
                case Key.NumpadPeriod:
                    c = '.'; return true;
                case Key.Slash:
                case Key.NumpadDivide:
                    c = '/'; return true;
                case Key.NumpadMultiply:
                    c = '*'; return true;
                case Key.Minus:
                case Key.NumpadMinus:
                    c = '-'; return true;
                case Key.NumpadPlus:
                    c = '+'; return true;
                case Key.NumpadEnter:
                    c = '\n'; return true;
                case Key.Equals:
                case Key.NumpadEquals:
                    c = '='; return true;
                case Key.UpArrow:
                    c = '↑'; return true;
                case Key.DownArrow:
                    c = '↓'; return true;
                case Key.RightArrow:
                    c = '→'; return true;
                case Key.LeftArrow:
                    c = '←'; return true;
                case Key.Quote:
                    c = '\''; return true;
                case Key.Comma:
                    c = ','; return true;
                case Key.Semicolon:
                    c = ';'; return true;
                case Key.LeftBracket:
                    c = '['; return true;
                case Key.Backslash:
                    c = '\\'; return true;
                case Key.RightBracket:
                    c = ']'; return true;
                case Key.Backquote:
                    c = '`'; return true;
                case Key.A:
                case Key.B:
                case Key.C:
                case Key.D:
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.P:
                case Key.Q:
                case Key.R:
                case Key.S:
                case Key.T:
                case Key.U:
                case Key.V:
                case Key.W:
                case Key.X:
                case Key.Y:
                case Key.Z:
                    c = (char)(key - Key.A + 'a'); return true;

                case Key.None:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftMeta:
                case Key.RightMeta:
                case Key.ContextMenu:
                case Key.Escape:
                case Key.Backspace:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Home:
                case Key.End:
                case Key.Insert:
                case Key.Delete:
                case Key.CapsLock:
                case Key.NumLock:
                case Key.PrintScreen:
                case Key.ScrollLock:
                case Key.Pause:
                case Key.F1:
                case Key.F2:
                case Key.F3:
                case Key.F4:
                case Key.F5:
                case Key.F6:
                case Key.F7:
                case Key.F8:
                case Key.F9:
                case Key.F10:
                case Key.F11:
                case Key.F12:
                case Key.OEM1:
                case Key.OEM2:
                case Key.OEM3:
                case Key.OEM4:
                case Key.OEM5:
                case Key.IMESelected:
                default:
                    c = default;
                    return false;
            }
        }

        public static bool TryGetKey(this in char c, out Key key)
        {
            if (c >= 'a' && c <= 'z')
            {
                key = Key.A + (c - 'a');
                return true;
            }
            if (c >= 'A' && c <= 'Z')
            {
                key = Key.A + (c - 'A');
                return true;
            }
            if (c >= '0' && c <= '9')
            {
                key = Key.Digit0 + (c - '0');
                return true;
            }
            switch (c)
            {
                case ' ':
                    key = Key.Space; return true;
                case '\t':
                    key = Key.Tab; return true;
                case '\r':
                case '\n':
                    key = Key.Enter; return true;
                case '\'':
                    key = Key.Quote; return true;
                case '\\':
                    key = Key.Backslash; return true;
                case '+':
                    key = Key.NumpadPlus; return true;
                case '-':
                    key = Key.Minus; return true;
                case '*':
                    key = Key.NumpadMultiply; return true;
                case '/':
                    key = Key.Slash; return true;
                case '=':
                    key = Key.Equals; return true;
                case ';':
                    key = Key.Semicolon; return true;
                case ',':
                    key = Key.Comma; return true;
                case '.':
                    key = Key.Period; return true;
                case '[':
                    key = Key.LeftBracket; return true;
                case ']':
                    key = Key.RightBracket; return true;
                default:
                    key = Key.None;
                    return false;
            }
        }

        public static bool TryGetKeyCode(this in char c, out KeyCode key)
        {
            if (c >= 'a' && c <= 'z')
            {
                key = KeyCode.A + (c - 'a');
                return true;
            }
            if (c >= 'A' && c <= 'Z')
            {
                key = KeyCode.A + (c - 'A');
                return true;
            }
            if (c >= '0' && c <= '9')
            {
                key = KeyCode.Alpha0 + (c - '0');
                return true;
            }
            switch (c)
            {
                case ' ':
                    key = KeyCode.Space; return true;
                case '\t':
                    key = KeyCode.Tab; return true;
                case '\r':
                case '\n':
                    key = KeyCode.Return; return true;
                case '!':
                    key = KeyCode.Exclaim; return true;
                case '\"':
                    key = KeyCode.DoubleQuote; return true;
                case '#':
                    key = KeyCode.Hash; return true;
                case '$':
                    key = KeyCode.Dollar; return true;
                case '%':
                    key = KeyCode.Percent; return true;
                case '&':
                    key = KeyCode.Ampersand; return true;
                case '\'':
                    key = KeyCode.Quote; return true;
                case '(':
                    key = KeyCode.LeftParen; return true;
                case ')':
                    key = KeyCode.RightParen; return true;
                case '^':
                    key = KeyCode.Caret; return true;
                case '\\':
                    key = KeyCode.Backslash; return true;
                case '+':
                    key = KeyCode.Plus; return true;
                case '-':
                    key = KeyCode.Minus; return true;
                case '*':
                    key = KeyCode.Asterisk; return true;
                case '/':
                    key = KeyCode.Slash; return true;
                case '=':
                    key = KeyCode.Equals; return true;
                case '~':
                    key = KeyCode.Tilde; return true;
                case '|':
                    key = KeyCode.Pipe; return true;
                case '?':
                    key = KeyCode.Question; return true;
                case '@':
                    key = KeyCode.At; return true;
                case ';':
                    key = KeyCode.Semicolon; return true;
                case ':':
                    key = KeyCode.Colon; return true;
                case ',':
                    key = KeyCode.Comma; return true;
                case '.':
                    key = KeyCode.Period; return true;
                case '_':
                    key = KeyCode.Underscore; return true;
                case '[':
                    key = KeyCode.LeftBracket; return true;
                case ']':
                    key = KeyCode.RightBracket; return true;
                case '{':
                    key = KeyCode.LeftCurlyBracket; return true;
                case '}':
                    key = KeyCode.RightCurlyBracket; return true;
                case '<':
                    key = KeyCode.Less; return true;
                case '>':
                    key = KeyCode.Greater; return true;
                default:
                    key = KeyCode.None;
                    return false;
            }
        }
    }
}
