using System.Collections.Generic;

namespace BiliLite.Models.Common.Msg
{
    public class BiliSendMessageResultCode : Dictionary<int,string>
    {
        public BiliSendMessageResultCode()
        {
            Add(0, "成功");
            Add(-3, "系统错误");
            Add(-101, "账号未登录");
            Add(-400, "请求错误");
            Add(10005, "msgkey不存在");
            Add(21007, "消息过长，无法发送");
            Add(21020, "你发送消息频率过快，请稍后再发~");
            Add(21026, "不能给自己发送消息哦~");
            Add(21028, "由于系统升级，暂无法发送，敬请谅解");
            Add(21035, "该类消息暂时无法发送");
            Add(21037, "图片格式不合法，不要调戏接口啦");
            Add(21041, "消息已超期，不能撤回了哦");
            Add(21042, "消息已经撤回了哦");
            Add(21046, "你发消息的频率太高了，请在24小时后再发吧~");
            Add(21047, "对方主动回复或关注你前，最多发送1条消息~");
            Add(25003, "因对方隐私设置，暂无法给他发送聊天消息");
            Add(25005, "你已拉黑了对方，请先将对方移出黑名单后才能聊天");
            Add(700013, "已解散QAQ，无法执行此操作");
            Add(700014, "你已不在此同萌中QAQ，无法执行此操作");
        }
    }
}
