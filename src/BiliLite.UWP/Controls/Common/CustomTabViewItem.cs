using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace BiliLite.Controls.Common;

public class CustomTabViewItem : TabViewItem
{
    private bool m_isLoaded;

    public static readonly DependencyProperty IsPlayButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsPlayButtonVisible), typeof(bool), typeof(CustomTabViewItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(CustomTabViewItem),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public CustomTabViewItem() : base()
    {
        Loaded += CustomTabViewItem_Loaded;
    }

    private void CustomTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (m_isLoaded) return;
        m_isLoaded = true;
        var playButton = this.FindDescendant("PlayButton") as Button;
        if (playButton != null)
        {
            playButton.Click += PlayButton_Click; 
        }
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        PlayButtonClick?.Invoke(sender, e);
    }

    public event RoutedEventHandler PlayButtonClick;

    public bool IsPlayButtonVisible
    {
        get => (bool)GetValue(IsPlayButtonVisibleProperty);
        set => SetValue(IsPlayButtonVisibleProperty, value);
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTabViewItem tabItem)
        {
            // 更新视觉状态
            VisualStateManager.GoToState(tabItem, (bool)e.NewValue ? "PauseState" : "PlayState", true);
        }
    }
}
