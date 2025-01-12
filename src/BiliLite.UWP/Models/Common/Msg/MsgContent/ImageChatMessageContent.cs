namespace BiliLite.Models.Common.Msg.MsgContent;

public class ImageChatMessageContent : IChatMsgContent
{
    public string Url { get; set; }

    public double Height { get; set; }

    public double Width { get; set; }

    public string ImageType { get; set; }

    public int Original { get; set; }

    public double Size { get; set; }
}