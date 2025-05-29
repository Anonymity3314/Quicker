using System.Reflection;
using Quicker.Managers;
using System.IO;

namespace Quicker.Interface
{
    internal class ModuleLoader
    {
        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="modulesDirectory"> 模块目录 </param>
        public void LoadModules(string modulesDirectory)
        {
            using var toast = new ToastManager(); // 消息提醒管理器
            if (!Directory.Exists(modulesDirectory))
            {
                Directory.CreateDirectory(modulesDirectory); // 创建模块目录
                toast.Show($"模块目录 {modulesDirectory} 不存在，已创建。", "Error"); // 显示消息提醒
                return; // 退出
            }

            try
            {
                string[] moduleFiles = Directory.GetFiles(modulesDirectory, "*.dll"); // 获取模块文件列表
                if (moduleFiles.Length == 0)
                {
                    toast.Show($"模块目录 {modulesDirectory} 中没有找到模块。", "Error"); // 显示消息提醒
                    return; // 退出
                }
                foreach (string moduleFile in moduleFiles)
                {
                    try
                    {
                        Assembly moduleAssembly = Assembly.LoadFrom(moduleFile); // 加载模块
                        Type[] types = moduleAssembly.GetTypes(); // 获取模块类型列表
                        foreach (Type type in types) // 遍历模块类型列表
                        {
                            if (typeof(IExtensionModule).IsAssignableFrom(type) && !type.IsInterface)
                            {
                                IExtensionModule module = (IExtensionModule)Activator.CreateInstance(type); // 创建模块实例
                                module.Initialize(); // 初始化模块
                                module.ShowWindow(); // 显示模块窗口
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        toast.Show($"加载模块 {moduleFile} 时出错：{ex.Message}", "Error"); // 显示消息提醒
                    }
                }
            }
            catch (Exception ex)
            {
                toast.Show($"加载模块时出错：{ex.Message}", "Error"); // 显示消息提醒
            }
        }
    }
}