using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models.Exceptions;
using BiliLite.Services;

namespace BiliLite.Models.Functions
{
    public class DebugThrowErrorFunction : IShortcutFunction
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public string Name { get; } = "debug";

        public async Task Action(object param)
        {
            _logger.Debug("debug throw error");
            var errList = new List<Exception>();
            errList.FirstOrDefault().ToString();
        }
    }
}
