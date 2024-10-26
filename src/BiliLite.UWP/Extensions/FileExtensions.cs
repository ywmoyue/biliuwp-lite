using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BiliLite.Extensions
{
    public static class FileExtensions
    {
        public static async Task<StorageFile> GetExportFile(string fileTypeChoiceKey, string fileTypeChoiceValue,
            string suggestedFileName,
            PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = suggestedStartLocation;

            savePicker.FileTypeChoices.Add(fileTypeChoiceKey, new List<string>() { fileTypeChoiceValue });
            var fileName = suggestedFileName;
            savePicker.SuggestedFileName = fileName;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        public static async Task<bool> CheckFileExist(this string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return file != null;
            }
            catch (Exception)
            {
                // 如果文件不存在或者路径无效，将捕获异常
                return false;
            }
        }
    }
}
