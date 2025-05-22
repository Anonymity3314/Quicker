using Quicker.Windows.Menus;
using Quicker.Windows;
using System.Windows;

namespace Quicker.Managers
{
    public class ToastManager : IDisposable
    {
        private ToastWindow? toastWindow;
        private bool isDisposed = false;

        // 添加Toast消息
        public void ShowToast(string message, string toastType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                toastWindow = Application.Current.Windows.OfType<ToastWindow>().FirstOrDefault();
                if (toastWindow == null)
                {
                    toastWindow = new ToastWindow();
                    toastWindow.Show();
                }
                toastWindow.AddToast(message, toastType);
            });
        }

        // 实现IDisposable接口
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // 告知垃圾回收器不需要调用终结器
        }

        // 释放资源
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed) isDisposed = true;
        }

        // 析构函数
        ~ToastManager()
        {
            Dispose(false);
        }
    }
}