namespace Quicker.Extend
{
    public interface IExtensionModule
    {
        // 模块元数据
        string Name { get; } // 模块名称
        string Version { get; } // 版本号
        string Author { get; } // 作者
        string Description { get; } // 描述
        
        // 生命周期方法
        void Initialize(); // 初始化
        void Start(); // 启动
        void Stop(); // 停止
        
        // UI相关方法
        bool HasUI { get; } // 是否具有UI
        void ShowWindow(); // 显示窗口
        
        // 依赖关系
        string[] Dependencies { get; } // 依赖的模块
    }
}