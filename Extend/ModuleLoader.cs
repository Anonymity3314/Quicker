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
        /// <param name="modulesDirectory"> 模块目录 </param>
        public void LoadModules(string modulesDirectory)
        {
            try
            {
                LoadAllAssemblies(modulesDirectory); // 加载所有程序集
                DiscoverModules(); // 发现所有模块
                InitializeModulesInOrder(); // 按依赖关系排序并初始化模块
            }
            catch (Exception ex)
            {
                _toast.Show($"加载模块时出错：{ex.Message}", "Error"); // 通知用户
            }
        }

        /// <summary>
        /// 加载目录中的所有程序集
        /// </summary>
        /// <param name="modulesDirectory"> 模块目录 </param>
        private void LoadAllAssemblies(string modulesDirectory)
        {
            string[] moduleFiles = Directory.GetFiles(modulesDirectory, "*.dll");
            if (moduleFiles.Length == 0)
            {
                _toast.Show($"模块目录 {modulesDirectory} 中没有找到模块。", "Error"); // 通知用户
                return;
            }

            foreach (string moduleFile in moduleFiles)
            {
                try
                {
                    string assemblyName = Path.GetFileNameWithoutExtension(moduleFile); // 获取程序集名称
                    Assembly moduleAssembly = Assembly.LoadFrom(moduleFile); // 加载程序集
                    _loadedAssemblies[assemblyName] = moduleAssembly; // 加入已加载的程序集列表
                }
                catch (Exception ex)
                {
                    _toast.Show($"加载程序集 {moduleFile} 时出错：{ex.Message}", "Error");
                }
            }
        }

        // 发现所有模块
        private void DiscoverModules()
        {
            foreach (var assembly in _loadedAssemblies.Values) // 遍历所有程序集
            {
                try
                {
                    Type[] types = assembly.GetTypes(); // 获取所有类型
                    foreach (Type type in types)
                    {
                        if (typeof(IExtensionModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            IExtensionModule module = (IExtensionModule)Activator.CreateInstance(type); // 创建模块实例
                            _loadedModules[module.Name] = module; // 加入已加载的模块列表
                        }
                    }
                }
                catch (Exception ex)
                {
                    _toast.Show($"发现模块时出错：{ex.Message}", "Error");
                }
            }
        }

        // 按依赖关系排序并初始化模块
        private void InitializeModulesInOrder()
        {
            List<string> sortedModules = SortModulesByDependency(); // 按依赖关系排序
            foreach (string moduleName in sortedModules) // 按顺序初始化模块
            {
                if (_loadedModules.TryGetValue(moduleName, out IExtensionModule module))
                {
                    try
                    {
                        module.Initialize(); // 初始化模块
                        module.Start(); // 启动模块
                        if (module.HasUI) // 如果模块有UI
                        {
                            module.ShowWindow(); // 显示模块UI
                        }
                    }
                    catch (Exception ex)
                    {
                        _toast.Show($"初始化模块 {module.Name} 时出错：{ex.Message}", "Error");
                    }
                }
            }
        }

        /// <summary>
        /// 使用拓扑排序处理模块依赖关系
        /// </summary>
        /// <returns> 已排序的模块名称列表 </returns>
        private List<string> SortModulesByDependency()
        {
            Dictionary<string, bool> visited = new Dictionary<string, bool>(); // 已访问的模块
            Dictionary<string, bool> inProgress = new Dictionary<string, bool>(); // 正在处理的模块
            List<string> sortedModules = new List<string>(); // 已排序的模块
            foreach (var moduleName in _loadedModules.Keys)
            {
                if (!visited.ContainsKey(moduleName))
                {
                    VisitModule(moduleName, visited, inProgress, sortedModules); // 处理模块
                }
            }
            return sortedModules; // 返回已排序的模块名称列表
        }

        /// <summary>
        /// 处理模块
        /// </summary>
        /// <param name="moduleName"> 模块名称 </param>
        /// <param name="visited"> 已访问的模块 </param>
        /// <param name="inProgress"> 正在处理的模块 </param>
        /// <param name="sortedModules"> 已排序的模块 </param>
        private void VisitModule(string moduleName, Dictionary<string, bool> visited, Dictionary<string, bool> inProgress, List<string> sortedModules)
        {
            if (inProgress.ContainsKey(moduleName) && inProgress[moduleName]) // 如果正在处理
            {
                throw new Exception($"检测到循环依赖：{moduleName}"); // 循环依赖
            }

            if (visited.ContainsKey(moduleName) && visited[moduleName]) // 如果已访问过
            {
                return; // 已访问过
            }

            if (!_loadedModules.ContainsKey(moduleName)) // 如果找不到模块
            {
                _toast.Show($"找不到依赖的模块：{moduleName}", "Error"); // 找不到依赖的模块
                return; // 退出
            }

            inProgress[moduleName] = true; // 标记正在处理

            // 处理依赖
            var dependencies = _loadedModules[moduleName].Dependencies; // 获取依赖
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    VisitModule(dependency, visited, inProgress, sortedModules); // 处理依赖
                }
            }

            visited[moduleName] = true; // 标记已访问
            inProgress[moduleName] = false; // 标记处理结束
            sortedModules.Add(moduleName); // 加入已排序的模块列表
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
                    _toast.Show($"停止模块 {module.Name} 时出错：{ex.Message}", "Error"); // 通知用户
                }
            }
            _loadedModules.Clear(); // 清空已加载的模块
            _loadedAssemblies.Clear(); // 清空已加载的程序集
        }
    }
}