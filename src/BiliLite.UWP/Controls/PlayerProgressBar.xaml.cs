using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls;

public sealed partial class PlayerProgressBar : UserControl
{
    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register("Position", typeof(double), typeof(PlayerProgressBar), new PropertyMetadata(0.0, OnPositionChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register("Duration", typeof(double), typeof(PlayerProgressBar), new PropertyMetadata(0.0));

    public static readonly DependencyProperty PlayedColorProperty =
        DependencyProperty.Register("PlayedColor", typeof(Brush), typeof(PlayerProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Blue)));

    public static readonly DependencyProperty ThumbIconProperty =
        DependencyProperty.Register("ThumbIcon", typeof(ImageSource), typeof(PlayerProgressBar), new PropertyMetadata(null));

    private bool _isDragging; // 标记是否正在拖拽

    public double Position
    {
        get => (double)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public Brush PlayedColor
    {
        get => (Brush)GetValue(PlayedColorProperty);
        set => SetValue(PlayedColorProperty, value);
    }

    public ImageSource ThumbIcon
    {
        get => (ImageSource)GetValue(ThumbIconProperty);
        set => SetValue(ThumbIconProperty, value);
    }

    public event EventHandler<double> PositionChanged;

    public PlayerProgressBar()
    {
        this.InitializeComponent();
        Loaded += PlayerProgressBar_Loaded;
    }

    private void PlayerProgressBar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateProgress();
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as PlayerProgressBar;
        if (!control._isDragging) // 只有在非拖拽状态下才更新进度
        {
            control?.UpdateProgress();
        }
    }

    private void UpdateProgress()
    {
        // 更新进度条显示
        if (Duration > 0)
        {
            double progress = Position / Duration;
            ProgressRectangle.Width = progress * TrackGrid.ActualWidth;
            Canvas.SetLeft(ProgressThumb, progress * (TrackGrid.ActualWidth - ProgressThumb.ActualWidth));
        }
        else
        {
            ProgressRectangle.Width = 0;
            Canvas.SetLeft(ProgressThumb, 0);
        }
    }

    private void ProgressThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        _isDragging = true; // 标记正在拖拽
        double newX = Canvas.GetLeft(ProgressThumb) + e.HorizontalChange;
        newX = Math.Max(0, Math.Min(TrackGrid.ActualWidth - ProgressThumb.ActualWidth, newX));
        Canvas.SetLeft(ProgressThumb, newX);

        double progress = newX / (TrackGrid.ActualWidth - ProgressThumb.ActualWidth);
        PositionChanged?.Invoke(this, progress * Duration);
    }

    private void ProgressThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        _isDragging = false; // 拖拽结束
        double progress = Canvas.GetLeft(ProgressThumb) / (TrackGrid.ActualWidth - ProgressThumb.ActualWidth);
        PositionChanged?.Invoke(this, progress * Duration);
    }

    private void TrackGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(TrackGrid);
        var progress = (long) ((point.Position.X / TrackGrid.ActualWidth) * Duration);
        var displayTime = progress.ProgressToTime();
        ProgressToolTip.Content = displayTime;
        // 动态更新 ToolTip 的位置
        var transform = TrackGrid.TransformToVisual(Window.Current.Content);
        var position = transform.TransformPoint(new Point(point.Position.X, 0));
        ProgressToolTip.HorizontalOffset = position.X;
    }

    private void TrackGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(TrackGrid);
        double newX = point.Position.X - ProgressThumb.ActualWidth / 2;
        newX = Math.Max(0, Math.Min(TrackGrid.ActualWidth - ProgressThumb.ActualWidth, newX));
        Canvas.SetLeft(ProgressThumb, newX);

        double progress = newX / (TrackGrid.ActualWidth - ProgressThumb.ActualWidth);
        PositionChanged?.Invoke(this, progress * Duration);
    }
}
