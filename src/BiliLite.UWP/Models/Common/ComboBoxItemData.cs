namespace BiliLite.Models.Common
{
    public class ComboBoxItemData
    {
        public string Text { get; set; }

        public int Value { get; set; }
    }

    public class ComboBoxItemData<T>
    {
        public string Text { get; set; }

        public T Value { get; set; }
    }
}
