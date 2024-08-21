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
    }
}
