using JiebaNet.Segmenter.Common;
using System;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BiliLite.Converters
{
    class SingleCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var noCorner = new CornerRadius(0);
            bool isWin10 = !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 14);
            if (isWin10 || parameter.ToString().IsEmpty() || value == null || parameter == null)
                return noCorner;

            var roundCorner = value.ToString();
            var radius = System.Convert.ToDouble(parameter);
            double topLeft = 0, topRight = 0, bottomRight = 0, bottomLeft = 0;
            if (roundCorner.Contains("1"))
                topLeft = radius;
            if (roundCorner.Contains("2"))
                topRight = radius;
            if (roundCorner.Contains("3"))
                bottomRight = radius;
            if (roundCorner.Contains("4"))
                bottomLeft = radius;

            return new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
