using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace BiliLite.Controls
{
    public class MyFrame : Frame
    {
        public string PageId { get; set; }

        public event EventHandler ClosedPage;
        public void Close()
        {
            ClosedPage?.Invoke(this, null);
        }

    }
}
