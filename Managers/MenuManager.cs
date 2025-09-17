using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Quicker.Windows.Menus;

namespace Quicker.Managers
{
    /// <summary>
    /// 菜单管理器 - 管理所有菜单窗口的生命周期和焦点
    /// </summary>
    public static class MenuManager
    {
        #region 私有字段
        /// <summary>
        /// 所有活跃的菜单窗口
        /// </summary>
        private static readonly HashSet<BaseMenuWindow> _activeMenus = new();
        
        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly object _lock = new object();
        #endregion

        #region 公共方法
        /// <summary>
        /// 注册菜单窗口
        /// </summary>
        /// <param name="menu">菜单窗口</param>
        public static void RegisterMenu(BaseMenuWindow menu)
        {
            if (menu == null) return;

            lock (_lock)
            {
                // 关闭所有其他菜单，确保只有一个菜单存在
                CloseOtherMenus(menu);
                
                _activeMenus.Add(menu);
                
                // 绑定菜单关闭事件，自动从列表中移除
                menu.ClosingOrHiding += () => UnregisterMenu(menu);
            }
        }

        /// <summary>
        /// 关闭除指定菜单外的所有其他菜单
        /// </summary>
        /// <param name="excludeMenu">要排除的菜单</param>
        private static void CloseOtherMenus(BaseMenuWindow excludeMenu)
        {
            var menusToClose = _activeMenus.Where(menu => menu != excludeMenu && menu.IsVisible).ToList();
            foreach (var menu in menusToClose)
            {
                try
                {
                    // 直接从列表中移除，避免触发事件
                    _activeMenus.Remove(menu);
                    menu.CloseWithAnimation();
                }
                catch
                {
                    // 忽略关闭时的异常，但确保从列表中移除
                    _activeMenus.Remove(menu);
                }
            }
        }

        /// <summary>
        /// 注销菜单窗口
        /// </summary>
        /// <param name="menu">菜单窗口</param>
        public static void UnregisterMenu(BaseMenuWindow menu)
        {
            if (menu == null) return;

            lock (_lock)
            {
                _activeMenus.Remove(menu);
            }
        }

        /// <summary>
        /// 检查是否有其他菜单窗口处于活跃状态
        /// </summary>
        /// <param name="currentMenu">当前菜单窗口</param>
        /// <returns>是否有其他活跃菜单</returns>
        public static bool HasOtherActiveMenus(BaseMenuWindow currentMenu)
        {
            lock (_lock)
            {
                return _activeMenus.Any(menu => menu != currentMenu && 
                                               menu.IsVisible && 
                                               menu.IsActive == false);
            }
        }

        /// <summary>
        /// 检查是否有任何菜单窗口处于活跃状态
        /// </summary>
        /// <returns>是否有活跃菜单</returns>
        public static bool HasAnyActiveMenus()
        {
            lock (_lock)
            {
                return _activeMenus.Any(menu => menu.IsVisible);
            }
        }

        /// <summary>
        /// 获取所有活跃的菜单窗口
        /// </summary>
        /// <returns>活跃菜单窗口列表</returns>
        public static List<BaseMenuWindow> GetActiveMenus()
        {
            lock (_lock)
            {
                return _activeMenus.Where(menu => menu.IsVisible).ToList();
            }
        }

        /// <summary>
        /// 关闭所有菜单窗口
        /// </summary>
        public static void CloseAllMenus()
        {
            lock (_lock)
            {
                var menusToClose = _activeMenus.ToList();
                _activeMenus.Clear(); // 先清空列表，避免事件触发
                
                foreach (var menu in menusToClose)
                {
                    try
                    {
                        menu.CloseWithAnimation();
                    }
                    catch
                    {
                        // 忽略关闭时的异常
                    }
                }
            }
        }

        /// <summary>
        /// 检查焦点是否在菜单窗口上
        /// </summary>
        /// <returns>焦点是否在菜单上</returns>
        public static bool IsFocusOnMenu()
        {
            var activeWindow = Application.Current.Windows.OfType<BaseMenuWindow>()
                .FirstOrDefault(w => w.IsActive);
            return activeWindow != null;
        }
        #endregion
    }
}
