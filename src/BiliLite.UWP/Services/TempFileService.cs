using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Models.Attributes;

namespace BiliLite.Services
{
    [RegisterTransientService]
    public class TempFileService
    {
        public async Task<StorageFile> CreateTempFile(string fileName = null)
        {
            // 如果没有提供文件名，则生成一个随机文件名
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetRandomFileName();
            }

            // 在临时文件夹中创建文件并返回
            return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                fileName,
                CreationCollisionOption.GenerateUniqueName);
        }

        public async Task ClearTempFiles()
        {
            try
            {
                // 获取临时文件夹中的所有文件
                var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();

                // 删除每个文件
                foreach (var file in files)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                System.Diagnostics.Debug.WriteLine($"清理临时文件时出错: {ex.Message}");
                throw;
            }
        }
    }
}
