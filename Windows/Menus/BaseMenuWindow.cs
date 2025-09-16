using System.Windows.Media.Animation;
using System.Windows.Threading;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Menus
{
    /// <summary>
    /// 所有菜单窗口的抽象基类
    /// 提供淡入淡出动画、失焦关闭等通用行为
    /// </summary>
    public abstract class BaseMenuWindow : Window
    {
        #region 动画与行为开关
        /// <summary>
        /// 是否启用淡入淡出动画
        /// </summary>
        protected bool UseAnimation { get; set; } = true;

        /// <summary>
        /// 是否在失去焦点后关闭/隐藏
        /// </summary>
        protected bool CloseOnDeactivated { get; set; } = true;

        /// <summary>
        /// 失焦检测延迟
        /// </summary>
        protected TimeSpan DeactivatedDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// 动画执行时长
        /// </summary>
        protected TimeSpan FadeDuration { get; set; } = TimeSpan.FromMilliseconds(200);
        #endregion

        #region 动画管理器（内置）
        /// <summary>
        /// 内置动画管理器，子类可直接使用
        /// </summary>
        protected AnimationManager AnimationManager { get; private set; }
        #endregion

        #region 公共事件
        /// <summary>
        /// 窗口即将关闭或隐藏时触发
        /// </summary>
        public event Action ClosingOrHiding;
        #endregion

        #region 构造与初始化
        protected BaseMenuWindow()
        {
            // 初始化动画管理器
            AnimationManager = new AnimationManager(this, FadeDuration);

            // 默认透明，等待淡入
            Opacity = 0;

            // 注册生命周期事件
            Loaded += OnWindowLoaded;
            Deactivated += OnWindowDeactivated;
        }
        #endregion

        #region 生命周期钩子
        /// <summary>
        /// 窗口加载完成后执行淡入动画
        /// </summary>
        protected virtual void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (UseAnimation)
                AnimationManager.FadeIn();
            else
                Opacity = 1;
        }

        /// <summary>
        /// 失去焦点时统一处理
        /// </summary>
        protected virtual void OnWindowDeactivated(object sender, EventArgs e)
        {
            if (!CloseOnDeactivated) return;

            // 延迟执行，避免鼠标点击其他窗口时立即触发
            Dispatcher.BeginInvoke(() =>
            {
                if (!IsActive && CloseOnDeactivated)
                    HandleDeactivated();
            }, DispatcherPriority.Render, DeactivatedDelay);
        }
        #endregion

        #region 失焦处理
        /// <summary>
        /// 实际执行失焦后的关闭/隐藏逻辑
        /// </summary>
        protected virtual void HandleDeactivated()
        {
            ClosingOrHiding?.Invoke();

            if (UseAnimation)
                HideWithAnimation();
            else
                Visibility = Visibility.Hidden;
        }
        #endregion

        #region 动画辅助（供子类调用）
        /// <summary>
        /// 带动画的隐藏（淡出→隐藏→重置透明度）
        /// </summary>
        protected void HideWithAnimation()
        {
            AnimationManager.FadeOut(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    Visibility = Visibility.Hidden;
                    Opacity = 0; // 为下次淡入做准备
                });
            });
        }

        /// <summary>
        /// 带动画的关闭（淡出→真正Close）
        /// </summary>
        protected void CloseWithAnimation()
        {
            AnimationManager.CloseWithFade();
        }
        #endregion

        #region 资源清理
        protected override void OnClosed(EventArgs e)
        {
            Loaded -= OnWindowLoaded;
            Deactivated -= OnWindowDeactivated;

            // 释放动画管理器
            //AnimationManager?.Dispose();
            AnimationManager = null;

            // 触发事件
            ClosingOrHiding = null;

            base.OnClosed(e);
        }
        #endregion
    }
}