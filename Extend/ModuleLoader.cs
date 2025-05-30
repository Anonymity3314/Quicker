using System.Reflection;
using Quicker.Managers;
using System.IO;

namespace Quicker.Extend
{
    public class ModuleLoader
    {
        private readonly Dictionary<string, IExtensionModule> _loadedModules = new(); // 已加载的模块
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new(); // 已加载的程序集
        private readonly ToastManager _toast = new(); // 通知管理器

        // 获取已加载的模块列表
        public IReadOnlyDictionary<string, IExtensionModule> LoadedModules => _loadedModules;

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="modulesDirectory"> 模块目录 </param>
        public void LoadModules(string modulesDirectory)
        {
            if (!Directory.Exists(modulesDirectory))
            {
                Directory.CreateDirectory(modulesDirectory);
                _toast.Show($"模块目录 {modulesDirectory} 不存在，已创建。", "Common");
                return;
            }

            try
            {
                // 第一步：加载所有程序集
                LoadAllAssemblies(modulesDirectory);

                // 第二步：发现所有模块
                DiscoverModules();

                // 第三步：按依赖关系排序并初始化模块
                InitializeModulesInOrder();
            }
            catch (Exception ex)
            {
                _toast.Show($"加载模块时出错：{ex.Message}", "Error");
            }
        }

        /// <summary>
        /// 加载目录中的所有程序集
        /// </summary>
        private void LoadAllAssemblies(string modulesDirectory)
        {
            string[] moduleFiles = Directory.GetFiles(modulesDirectory, "*.dll");
            if (moduleFiles.Length == 0)
            {
                _toast.Show($"模块目录 {modulesDirectory} 中没有找到模块。", "Error");
                return;
            }

            foreach (string moduleFile in moduleFiles)
            {
                try
                {
                    string assemblyName = Path.GetFileNameWithoutExtension(moduleFile);
                    Assembly moduleAssembly = Assembly.LoadFrom(moduleFile);
                    _loadedAssemblies[assemblyName] = moduleAssembly;
                }
                catch (Exception ex)
                {
                    _toast.Show($"加载程序集 {moduleFile} 时出错：{ex.Message}", "Error");
                }
            }
        }

        /// <summary>
        /// 发现所有模块
        /// </summary>
        private void DiscoverModules()
        {
            foreach (var assembly in _loadedAssemblies.Values)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (typeof(IExtensionModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            IExtensionModule module = (IExtensionModule)Activator.CreateInstance(type);
                            _loadedModules[module.Name] = module;
                            _toast.Show($"发现模块：{module.Name} v{module.Version} by {module.Author}", "Common");
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
                        module.Initialize();
                        module.Start();
                        
                        if (module.HasUI)
                        {
                            module.ShowWindow();
                        }
                        
                        _toast.Show($"模块 {module.Name} 已成功加载", "Success");
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
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Dictionary<string, bool> inProgress = new Dictionary<string, bool>();
            List<string> sortedModules = new List<string>();

            foreach (var moduleName in _loadedModules.Keys)
            {
                if (!visited.ContainsKey(moduleName))
                {
                    VisitModule(moduleName, visited, inProgress, sortedModules);
                }
            }

            return sortedModules;
        }

        private void VisitModule(string moduleName, Dictionary<string, bool> visited, Dictionary<string, bool> inProgress, List<string> sortedModules)
        {
            if (inProgress.ContainsKey(moduleName) && inProgress[moduleName])
            {
                throw new Exception($"检测到循环依赖：{moduleName}");
            }

            if (visited.ContainsKey(moduleName) && visited[moduleName])
            {
                return;
            }

            if (!_loadedModules.ContainsKey(moduleName))
            {
                _toast.Show($"找不到依赖的模块：{moduleName}", "Error");
                return;
            }

            inProgress[moduleName] = true;

            // 处理依赖
            var dependencies = _loadedModules[moduleName].Dependencies;
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    VisitModule(dependency, visited, inProgress, sortedModules);
                }
            }

            visited[moduleName] = true;
            inProgress[moduleName] = false;
            sortedModules.Add(moduleName);
        }

        // 卸载所有模块
        public void UnloadAllModules()
        {
            // 按加载的相反顺序卸载
            foreach (var module in _loadedModules.Values.Reverse())
            {
                try
                {
                    module.Stop();
                    _toast.Show($"模块 {module.Name} 已停止", "Common");
                }
                catch (Exception ex)
                {
                    _toast.Show($"停止模块 {module.Name} 时出错：{ex.Message}", "Error");
                }
            }
            
            _loadedModules.Clear();
            _loadedAssemblies.Clear();
        }
    }
}