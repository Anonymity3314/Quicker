using Quicker.Windows.ToolWindows;
using Quicker.Windows;
using System.Windows;

namespace Quicker.Managers
{
    /// <summary>
    /// Toast消息类型枚举
    /// </summary>
    public enum ToastType
    {
        Common, // 普通消息
        Error,  // 错误消息
        Warning,// 警告消息
        Success // 成功消息
    }

    public class ToastManager : IDisposable
    {
        private ToastWindow? toastWindow;
        private bool isDisposed = false;

        /// <summary>
        /// 添加Toast消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        /// <param name="toastType"> 消息类型 </param>
        public void Show(string message, ToastType toastType)
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