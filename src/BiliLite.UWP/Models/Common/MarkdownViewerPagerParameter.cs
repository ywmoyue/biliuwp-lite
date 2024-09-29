namespace BiliLite.Models.Common
{
    public class MarkdownViewerPagerParameter
    {
        public MarkdownViewerPagerParameterType Type { get; set; }

        public string Value { get; set; }
    }

    public enum MarkdownViewerPagerParameterType
    {
        Link,
        Content,
    }
}
