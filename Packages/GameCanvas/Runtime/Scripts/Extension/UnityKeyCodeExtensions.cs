/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// <see cref="KeyCode"/> と <see cref="char"/> (UTF-16) の相互変換
    /// </summary>
    public static class UnityKeyCodeExtensions
    {
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
            }
            c = default;
            return false;
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
            }
            key = KeyCode.None;
            return false;
        }
    }
}
