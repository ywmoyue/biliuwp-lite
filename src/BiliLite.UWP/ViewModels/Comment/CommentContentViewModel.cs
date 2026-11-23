using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Comment
{
    public class CommentContentViewModel : BaseViewModel
    {
        private const double PictureDisplayLimit = 280d;
        private const int PicturePageSize = 9;

        public List<NotePicture> Pictures { get; set; }
        public string Message { get; set; }
        public int Plat { get; set; }

        [DependsOn(nameof(Pictures))]
        public int PictureCount => Pictures?.Count ?? 0;

        [DependsOn(nameof(Pictures))]
        public bool HasSinglePicture => PictureCount == 1;

        [DependsOn(nameof(Pictures))]
        public bool HasGridPictures => PictureCount >= 2 && PictureCount <= PicturePageSize;

        [DependsOn(nameof(Pictures))]
        public bool HasQuadGridPictures => PictureCount >= 2 && PictureCount <= 4;

        [DependsOn(nameof(Pictures))]
        public bool HasNineGridPictures => PictureCount >= 5 && PictureCount <= PicturePageSize;

        [DependsOn(nameof(Pictures))]
        public bool HasPagedPictures => PictureCount > PicturePageSize;

        [DependsOn(nameof(Pictures))]
        public NotePicture SinglePicture => Pictures?.FirstOrDefault();

        [DependsOn(nameof(Pictures))]
        public double SinglePictureDisplayWidth => GetSinglePictureDisplaySize().width;

        [DependsOn(nameof(Pictures))]
        public double SinglePictureDisplayHeight => GetSinglePictureDisplaySize().height;

        [DependsOn(nameof(Pictures))]
        public int GridPictureColumns => PictureCount <= 4 ? 2 : 3;

        [DependsOn(nameof(Pictures))]
        public int GridPictureRows
        {
            get
            {
                if (!HasGridPictures)
                {
                    return 0;
                }

                return (PictureCount + GridPictureColumns - 1) / GridPictureColumns;
            }
        }

        [DependsOn(nameof(Pictures))]
        public double GridPictureTileSize => GridPictureColumns == 0 ? PictureDisplayLimit : PictureDisplayLimit / GridPictureColumns;

        [DependsOn(nameof(Pictures))]
        public double GridPictureContainerWidth => HasGridPictures ? PictureDisplayLimit : 0;

        [DependsOn(nameof(Pictures))]
        public double GridPictureContainerHeight => GridPictureRows == 0 ? 0 : GridPictureTileSize * GridPictureRows;

        [DependsOn(nameof(Pictures))]
        public double PagedPictureTileSize => PictureDisplayLimit / 3;

        [DependsOn(nameof(Pictures))]
        public List<CommentPicturePageViewModel> PicturePages
        {
            get
            {
                if (!HasPagedPictures)
                {
                    return null;
                }

                var pages = new List<CommentPicturePageViewModel>();
                for (var index = 0; index < Pictures.Count; index += PicturePageSize)
                {
                    pages.Add(new CommentPicturePageViewModel
                    {
                        AllPictures = Pictures,
                        Pictures = Pictures.Skip(index).Take(PicturePageSize).ToList()
                    });
                }

                return pages;
            }
        }

        [DependsOn(nameof(Plat))]
        public string PlatStr
        {
            get
            {
                return Plat switch
                {
                    2 => "来自 Android",
                    3 => "来自 IOS",
                    4 => "来自 WindowsPhone",
                    6 => "来自 Windows",
                    _ => ""
                };
            }
        }
        public string Device { get; set; }

        [DependsOn(nameof(Message))]
        public RichTextBlock Text => Message.ToRichTextBlock(Emote, enableVideoSeekTime: true);

        public JObject Emote { get; set; }

        private (double width, double height) GetSinglePictureDisplaySize()
        {
            if (SinglePicture == null)
            {
                return (PictureDisplayLimit, PictureDisplayLimit);
            }

            var width = SinglePicture.ImgWidth;
            var height = SinglePicture.ImgHeight;

            if (width <= 0 || height <= 0)
            {
                return (PictureDisplayLimit, PictureDisplayLimit);
            }

            var scale = System.Math.Min(1d, System.Math.Min(PictureDisplayLimit / width, PictureDisplayLimit / height));
            return (width * scale, height * scale);
        }
    }

    public class CommentPicturePageViewModel
    {
        public List<NotePicture> AllPictures { get; set; }

        public List<NotePicture> Pictures { get; set; }
    }
}
