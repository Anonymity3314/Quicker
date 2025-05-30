namespace Quicker.Extend
{
    public interface IExtensionModule
    {
        // 模块元数据
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        
        // 生命周期方法
        void Initialize();
        void Start();
        void Stop();
        
        // UI相关方法
        bool HasUI { get; }
        void ShowWindow();
        
        // 依赖关系
        string[] Dependencies { get; }
    }
}