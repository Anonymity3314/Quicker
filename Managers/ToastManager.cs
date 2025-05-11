using Quicker.Windows.Menus;
using Quicker.Windows;
using System.Windows;

namespace Quicker.Managers
{
    internal class ToastManager
    {
        public void ShowToast(string message, string toastType)
        {
            ToastWindow toastWindow = Application.Current.Windows.OfType<ToastWindow>().FirstOrDefault(); // 获取当前应用程序中已打开的ToastWindow
            if (toastWindow == null)
            {
                toastWindow = new ToastWindow();
                toastWindow.Show();
            } // 如果ToastWindow不存在，则创建并显示
        }
    }
}