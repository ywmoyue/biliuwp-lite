using System;
using System.ComponentModel.DataAnnotations;

namespace BiliLite.Models.Databases
{
    public class PageSavedDTO
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Url { get; set; }

        public string Parameters { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string Icon { get; set; }
    }
}
