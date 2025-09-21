using System.Reflection;
using Quicker.Managers;
using System.IO;

namespace Quicker.Extend
{
    public class ModuleLoader
    {
        public IReadOnlyDictionary<string, IExtensionModule> LoadedModules => _loadedModules; // 已加载的模块列表
        private readonly Dictionary<string, IExtensionModule> _loadedModules = new(); // 已加载的模块
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new(); // 已加载的程序集
        private readonly ToastManager _toast = new(); // 通知管理器

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="modulePath"> 模块路径 </param>
        public void LoadModule(string modulePath)
        {
            try
            {
                string assemblyName = Path.GetFileNameWithoutExtension(modulePath); // 获取文件名
                Assembly moduleAssembly = Assembly.LoadFrom(modulePath);
                _loadedAssemblies[assemblyName] = moduleAssembly;
                foreach (Type type in moduleAssembly.GetTypes()) // 只查找并加载指定 DLL 里的扩展模块
                {
                    if (typeof(IExtensionModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        IExtensionModule module = (IExtensionModule)Activator.CreateInstance(type);
                        _loadedModules[module.Name] = module;

                        module.Initialize();
                        module.Start();
                        if (module.HasUI)
                        {
                            module.ShowWindow();
                        }

                        break; // 找到后直接 break
                    }
                }
            }
            catch (Exception ex)
            {
                _toast.Show($"加载模块时出错：{ex.Message}", ToastType.Error); // 通知用户
            }
        }

        // 卸载所有模块
        public void UnloadAllModules()
        {
            // 按加载的相反顺序卸载
            foreach (var module in _loadedModules.Values.Reverse())
            {
                try
                {
                    module.Stop(); // 停止模块
                }
                catch (Exception ex)
                {
                    _toast.Show($"停止模块 {module.Name} 时出错：{ex.Message}", ToastType.Error); // 通知用户
                }
            }
            _loadedModules.Clear(); // 清空已加载的模块
            _loadedAssemblies.Clear(); // 清空已加载的程序集
        }
    }
}