using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class RecommendAPI : BaseApi
    {
        public ApiModel Recommend(string idx = "0")
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/feed/index",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&flush=0&idx={idx}&login_event=2&network=wifi&open_event=&pull={(idx == "0").ToString().ToLower()}&qn=32&style=2",
                headers = ApiHelper.GetAuroraHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel Dislike(RecommondFeedbackParams feedbackParams)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.biliapi.net/x/feed/dislike",
                parameter = ApiHelper.MustParameter(AppKey, true) 
                            + $"&goto={feedbackParams.GoTo}&id={feedbackParams.Id}&mid={feedbackParams.Mid}&reason_id={feedbackParams.ReasonId}&rid={feedbackParams.Rid}&tag_id={feedbackParams.TagId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel Feedback(RecommondFeedbackParams feedbackParams)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.biliapi.net/x/feed/dislike",
                parameter = ApiHelper.MustParameter(AppKey, true) + 
                            $"&goto={feedbackParams.GoTo}&id={feedbackParams.Id}&mid={feedbackParams.Mid}&feedback_id={feedbackParams.ReasonId}&rid={feedbackParams.Rid}&tag_id={feedbackParams.TagId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
