using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

namespace BiliLite.Extensions
{
    public static class FileExtensions
    {
        public static FileOpenPicker GetFileOpenPicker()
        {
            var filePicker = new FileOpenPicker();
            var hwnd = App.MainWindow.GetWindowHandle();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            return filePicker;
        }

        public static FileSavePicker GetFileSavePicker()
        {
            var filePicker = new FileSavePicker();
            var hwnd = App.MainWindow.GetWindowHandle();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            return filePicker;
        }

        public static FolderPicker GetFolderPicker()
        {
            var folderPicker = new FolderPicker();
            var hwnd = App.MainWindow.GetWindowHandle();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            return folderPicker;
        }

        public static async Task<StorageFile> GetExportFile(string fileTypeChoiceKey, string fileTypeChoiceValue,
            string suggestedFileName,
            PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary)
        {
            var savePicker = GetFileSavePicker();
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
