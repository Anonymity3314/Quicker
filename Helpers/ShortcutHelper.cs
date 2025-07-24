using System.Windows.Input;
using SharpHook.Native;
using SharpHook.Data;

namespace Quicker.Helpers
{
    /// <summary>
    /// 快捷键辅助类，提供快捷键字符串生成、友好显示、比对等功能。
    /// </summary>
    public static class ShortcutHelper
    {
        /// <summary>
        /// 将按键事件(KeyEventArgs)转换为标准快捷键字符串（如 Ctrl+Alt+S）。
        /// </summary>
        /// <param name="e">按键事件参数</param>
        /// <returns>标准快捷键字符串</returns>
        public static string GetShortcutString(KeyEventArgs e)
        {
            List<string> keys = new List<string>();
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                keys.Add("Ctrl");
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                keys.Add("Shift");
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                keys.Add("Alt");
            if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                keys.Add("Windows");

            // 获取主键（排除修饰键）
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (!IsModifierKey(key))
            {
                keys.Add(GetFriendlyKeyName(key));
            }
            return string.Join("+", keys);
        }

        /// <summary>
        /// 判断指定的Key是否为修饰键（Ctrl、Shift、Alt、Windows）。
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>是修饰键返回true，否则返回false</returns>
        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LWin || key == Key.RWin;
        }

        /// <summary>
        /// 获取按键的友好显示名称（如数字、常见符号等）。
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>友好显示的按键名称</returns>
        public static string GetFriendlyKeyName(Key key)
        {
            // 主键盘数字
            if (key >= Key.D0 && key <= Key.D9)
                return ((char)('0' + (key - Key.D0))).ToString();
            // 小键盘数字
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return "Num" + (key - Key.NumPad0);
            // 常见符号键
            switch (key)
            {
                case Key.OemMinus: return "-";
                case Key.OemPlus: return "=";
                case Key.OemOpenBrackets: return "[";
                case Key.OemCloseBrackets: return "]";
                case Key.OemPipe: return "\\";
                case Key.OemSemicolon: return ";";
                case Key.OemQuotes: return "'";
                case Key.OemComma: return ",";
                case Key.OemPeriod: return ".";
                case Key.OemQuestion: return "/";
                case Key.OemTilde: return "`";
            }
            // 其它按键直接ToString
            return key.ToString();
        }

        /// <summary>
        /// 比较两个快捷键字符串是否一致（忽略大小写）。
        /// </summary>
        /// <param name="shortcut1">第一个快捷键字符串</param>
        /// <param name="shortcut2">第二个快捷键字符串</param>
        /// <returns>一致返回true，否则返回false</returns>
        public static bool IsShortcutMatch(string shortcut1, string shortcut2)
        {
            return string.Equals(shortcut1, shortcut2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 将SharpHook的KeyCode和修饰键状态转换为标准快捷键字符串
        /// </summary>
        /// <param name="keyCode">主键</param>
        /// <param name="ctrl">Ctrl是否按下</param>
        /// <param name="shift">Shift是否按下</param>
        /// <param name="alt">Alt是否按下</param>
        /// <param name="win">Win是否按下</param>
        /// <returns>标准快捷键字符串</returns>
        public static string GetShortcutStringFromHook(
            KeyCode keyCode, bool ctrl, bool shift, bool alt, bool win)
        {
            List<string> keys = new List<string>();
            if (ctrl) keys.Add("Ctrl");
            if (shift) keys.Add("Shift");
            if (alt) keys.Add("Alt");
            if (win) keys.Add("Windows");

            // 友好显示主键
            string keyName = GetFriendlyKeyNameFromKeyCode(keyCode);
            if (!IsModifierKeyFromKeyCode(keyCode))
                keys.Add(keyName);

            return string.Join("+", keys);
        }

        /// <summary>
        /// 判断SharpHook的KeyCode是否为修饰键
        /// </summary>
        /// <param name="keyCode"></param>
        public static bool IsModifierKeyFromKeyCode(KeyCode keyCode)
        {
            return keyCode == KeyCode.VcLeftControl || keyCode == KeyCode.VcRightControl ||
                   keyCode == KeyCode.VcLeftShift || keyCode == KeyCode.VcRightShift ||
                   keyCode == KeyCode.VcLeftAlt || keyCode == KeyCode.VcRightAlt ||
                   keyCode == KeyCode.VcLeftMeta || keyCode == KeyCode.VcRightMeta;
        }

        /// <summary>
        /// 获取SharpHook的KeyCode的友好显示名称
        /// </summary>
        /// <param name="keyCode"></param>
        public static string GetFriendlyKeyNameFromKeyCode(KeyCode keyCode)
        {
            // 主键盘数字
            if (keyCode >= KeyCode.Vc0 && keyCode <= KeyCode.Vc9)
                return ((char)('0' + (keyCode - KeyCode.Vc0))).ToString();
            // 小键盘数字
            if (keyCode >= KeyCode.VcNumPad0 && keyCode <= KeyCode.VcNumPad9)
                return "Num" + (keyCode - KeyCode.VcNumPad0);
            // 常见符号键
            switch (keyCode)
            {
                case KeyCode.VcMinus: return "-";
                case KeyCode.VcEquals: return "=";
                case KeyCode.VcOpenBracket: return "[";
                case KeyCode.VcCloseBracket: return "]";
                case KeyCode.VcBackslash: return "\\";
                case KeyCode.VcSemicolon: return ";";
                case KeyCode.VcQuote: return "'";
                case KeyCode.VcComma: return ",";
                case KeyCode.VcPeriod: return ".";
                case KeyCode.VcSlash: return "/";
                case KeyCode.VcBackQuote: return "`";
            }
            // 其它按键直接ToString
            return keyCode.ToString();
        }
    }
}