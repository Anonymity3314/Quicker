using System.Windows.Media.Animation;
using System.Windows;

namespace Quicker.Managers
{
    public class AnimationManager
    {
        private readonly TimeSpan _fadeDuration; // 淡出时长
        private readonly Window _window; // 目标窗口

        public AnimationManager(Window window, TimeSpan? fadeDuration = null)
        {
            _fadeDuration = fadeDuration ?? TimeSpan.FromMilliseconds(200); // 默认淡出时长为200ms
            _window = window;
        }

        /// <summary>
        /// 淡入动画
        /// </summary>
        /// <param name="completedCallback">淡入完成回调</param>
        public void FadeIn(Action completedCallback = null)
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(_fadeDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (completedCallback != null)
            {
                fadeInAnimation.Completed += (s, e) => completedCallback();
            }

            _window.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }

        /// <summary>
        /// 淡出动画
        /// </summary>
        /// <param name="completedCallback">淡出完成回调</param>
        public void FadeOut(Action completedCallback = null)
        {
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(_fadeDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOutAnimation.Completed += (s, e) =>
            {
                completedCallback?.Invoke();
                // 清除动画，防止影响后续操作
                _window.BeginAnimation(UIElement.OpacityProperty, null);
            };

            _window.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }

        /// <summary>
        /// 带淡出的关闭窗口
        /// </summary>
        public void CloseWithFade()
        {
            FadeOut(() =>
            {
                _window.Dispatcher.BeginInvoke(() =>
                {
                    _window.Close();
                });
            });
        }
    }
}