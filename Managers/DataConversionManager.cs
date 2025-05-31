namespace Quicker.Managers
{
    internal class DataConversionManager:IDisposable
    {
        private bool isDisposed = false; // 是否已释放资源

        /// <summary>
        /// 转换数据
        /// </summary>
        /// <param name="data"> 数据大小 </param>
        /// <returns> 转换后的数据 </returns>
        public int ConversionData(int data)
        {
            if (data < 1024)
                return data; // 字节
            else if (data < 1024 * 1024)
                return data / 1024; // 千字节
            else if (data < 1024 * 1024 * 1024)
                return data / (1024 * 1024); // 兆字节
            return data / (1024 * 1024 * 1024); // 吉字节
        }

        /// <summary>
        /// 转换单位
        /// </summary>
        /// <param name="unit"> 数据大小 </param>
        /// <returns> 大小单位 </returns>
        public string ConversionUnits(int unit)
        {
            if (unit < 1024)
                return "B"; // 字节
            else if (unit < 1024 * 1024)
                return "KB"; // 千字节
            else if (unit < 1024 * 1024 * 1024)
                return "MB"; // 兆字节
            return "GB"; // 吉字节
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
        /// <param name="disposing"> 是否释放 </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed) isDisposed = true;
        }

        // 析构函数
        ~DataConversionManager()
        {
            Dispose(false);
        }
    }
}