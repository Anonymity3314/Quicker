using Quicker.Windows.Menus;
using Quicker.Windows;
using System.Windows;

namespace Quicker.Managers
{
    public static class ToastManager
    {
        public static void AddToast(string message, string toastType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ToastWindow toastWindow = Application.Current.Windows.OfType<ToastWindow>().FirstOrDefault(); // 获取ToastWindow窗体
                if (toastWindow == null) // 如果ToastWindow窗体不存在
                {
                    toastWindow = new ToastWindow(); // 创建ToastWindow窗体
                    toastWindow.Show(); // 显示ToastWindow窗体
                }
                toastWindow.AddToast(message, toastType); // 添加Toast
            });
        }
    }
}