﻿using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Comment;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliLite.ViewModels.Comment
{
    public class CommentViewModel : BaseViewModel
    {
        private readonly int m_commentShrinkLength;
        private readonly bool m_enableCommentShrink;

        public CommentViewModel()
        {
            LaunchUrlCommand = new RelayCommand<object>(LaunchUrl_Click);
            SeekCommand = new RelayCommand<string>(Seek_Click);
            CommentExpandCommand = new RelayCommand(CommentExpand_Click);
            m_commentShrinkLength = SettingService.GetValue(SettingConstants.UI.COMMENT_SHRINK_LENGTH,
                SettingConstants.UI.COMMENT_SHRINK_DEFAULT_LENGTH);
            m_enableCommentShrink = SettingService.GetValue(SettingConstants.UI.ENABLE_COMMENT_SHRINK, true);
        }

        public RelayCommand CommentExpandCommand { get; private set; }

        public int Action { get; set; }

        [DependsOn(nameof(Action))]
        public Brush LikeColor => Action == 0 ? new SolidColorBrush(Colors.Gray) :
            new SolidColorBrush((Color)App.ServiceProvider.GetRequiredService<ThemeService>().AccentThemeResource["SystemAccentColor"]);

        public long RpId { get; set; }

        public long Oid { get; set; }

        public int Type { get; set; }

        public long Mid { get; set; }

        public long Root { get; set; }

        public long Parent { get; set; }

        public int Count { get; set; }

        public int Rcount { get; set; }

        public int Like { get; set; }

        public string RcountStr
        {
            get
            {
                if (Rcount > 1000)
                {
                    return ((double)Rcount / 1000).ToString("0.0") + "千";
                }
                else if (Rcount > 10000)
                {
                    return ((double)Rcount / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return Rcount.ToString();
                }
            }
        }

        [DependsOn(nameof(Like))]
        public string LikeStr
        {
            get
            {
                if (Like > 10000)
                {
                    return ((double)Like / 10000).ToString("0.0") + "万";
                }
                else if (Like > 1000)
                {
                    return ((double)Like / 1000).ToString("0.0") + "千";
                }
                else
                {
                    return Like.ToString();
                }
            }
        }

        public int Floor { get; set; }

        public int State { get; set; }

        public long Ctime { get; set; }

        public string Time
        {
            get
            {
                var dtStart = new DateTime(1970, 1, 1);
                var lTime = long.Parse(Ctime + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                var toNow = TimeSpan.FromSeconds(Ctime);
                var dt = dtStart.Add(toNow).ToLocalTime();
                var span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                }
                else
                if (span.TotalHours > 1)
                {
                    return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                }
                else
                if (span.TotalMinutes > 1)
                {
                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                }
                else
                if (span.TotalSeconds >= 1)
                {
                    return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                }
                else
                {
                    return "1秒前";
                }
            }
        }

        public string RpidStr { get; set; }

        public CommentMemberModel Member { get; set; }

        public CommentContentViewModel Content { get; set; }

        public CommentUPActionModel UpAction { get; set; }

        public CommentReplyControlModel ReplyControl { get; set; }

        public ObservableCollection<CommentViewModel> Replies { get; set; }

        public List<HotReply> HotReplies { get; set; }

        [DependsOn(nameof(ShowReplies))]
        public bool ShowHotReplies
        {
            get
            {
                if (!SettingService.GetValue(SettingConstants.UI.SHOW_HOT_REPLIES, SettingConstants.UI.DEFAULT_SHOW_HOT_REPLIES)) return false;
                return !ShowReplies;
            }
        }

        //public ObservableCollection<CommentModel> replies { get; set; }

        public bool ShowReplies { get; set; } = false;

        public bool ShowReplyBtn { get; set; } = false;

        public bool ShowReplyBox { get; set; } = false;

        public bool ShowReplyMore { get; set; } = false;

        public bool ShowLoading { get; set; } = false;

        public bool ShowDelete
        {
            get
            {
                if (SettingService.Account.Logined && Mid.ToString() == SettingService.Account.UserID.ToString())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int LoadPage { get; set; } = 1;

        public string ReplyAt => "回复 @" + Member.Uname;

        public string ReplyText { get; set; }

        public bool ShowTop { get; set; } = false;

        public RelayCommand<object> LaunchUrlCommand { get; private set; }

        public RelayCommand<string> SeekCommand { get; private set; }

        public bool ShowPics => Content.Pictures.Count > 0;

        public bool IsContentNeedExpand => m_enableCommentShrink && Content.Message.CalculateCommentTextLength() > m_commentShrinkLength;

        [DependsOn(nameof(IsExpanded))]
        public RichTextBlock CommentText
        {
            get
            {
                if (!IsContentNeedExpand || IsContentNeedExpand && IsExpanded)
                    return Content.Text;
                return $"{Content.Message.SubstringCommentText(m_commentShrinkLength)}...".ToRichTextBlock(
                    Content.Emote, enableVideoSeekTime: true);
            }
        }

        public bool IsExpanded { get; set; } = false;

        [DependsOn(nameof(IsExpanded))]
        public string ExtendBtnText => IsExpanded ? "收起" : "展开";

        private async void LaunchUrl_Click(object paramenter)
        {
            await MessageCenter.HandelUrl(paramenter.ToString());
        }

        private void Seek_Click(string timeText)
        {
            timeText = timeText.Replace('：', ':');
            var timeSplitCount = timeText.Count(x => x == ':');
            var text = timeText;
            // "mm:ss"格式，补全
            if (timeSplitCount == 1)
            {
                text = "00:" + timeText;
            }
            // 不是"hh:mm:ss"格式，不继续执行
            else if (timeSplitCount != 2)
            {
                return;
            }

            var time = TimeSpan.Parse(text);
            MessageCenter.HandleSeek(time.TotalSeconds);
        }

        private void CommentExpand_Click()
        {
            IsExpanded = !IsExpanded;
        }
    }
}