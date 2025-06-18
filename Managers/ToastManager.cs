using Quicker.Windows.ToolWindows;
using Quicker.Windows;
using System.Windows;

namespace Quicker.Managers
{
    public class ToastManager : IDisposable
    {
        private ToastWindow? toastWindow;
        private bool isDisposed = false;

        // 添加Toast消息
        public void Show(string message, string toastType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                toastWindow = Application.Current.Windows.OfType<ToastWindow>().FirstOrDefault();
                if (toastWindow == null)
                {
                    toastWindow = new ToastWindow(); // 创建新的ToastWindow实例
                    toastWindow.Show(); // 显示ToastWindow
                }
                toastWindow.AddToast(message, toastType); // 添加Toast消息
            });
        }

        // 实现IDisposable接口
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // 告知垃圾回收器不需要调用终结器
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed) isDisposed = true; // 设置为已释放
        }

        // 析构函数
        ~ToastManager()
        {
            Dispose(false); // 释放非托管资源
        }
    }
}