using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SkypeSlackLibrary.Model;

namespace SkypeSlackLibrary
{
    public class SlackApplication
    {
        private static SlackApplication _theInstance;

        public readonly string Token = ApplicationSettings.Instance.Slack_API_Token;
        private readonly RestClient _client = new RestClient(ApplicationSettings.Instance.SlackREST_EndpointBase);

        public static SlackApplication Instance { get { if (_theInstance == null ) _theInstance = new SlackApplication(); return _theInstance; } }

        public JObject SaySomething(string say, string who, ChannelMap where)
        {
            var request = new RestRequest("chat.postMessage", Method.POST);
            request.AddParameter("token", Token);
            request.AddParameter("channel", where.SlackChannelId);
            request.AddParameter("username", who);
            request.AddParameter("text", say);
            request.AddParameter("icon_url", "https://cdn4.iconfinder.com/data/icons/Colourful_Social_Media_Icons/64x64/skype.png");

            IRestResponse response = _client.Execute(request);
            return (JObject)JsonConvert.DeserializeObject(response.Content);
        }

        public JObject GetOrJoinChannel(string name)
        {
            var request = new RestRequest("channels.join", Method.POST);
            request.AddParameter("token", Token);
            request.AddParameter("name", name);

            IRestResponse response = _client.Execute(request);
            return (JObject)JsonConvert.DeserializeObject(response.Content);
        }

        public List<Chat> AlreadySaid(string slackChannelId)
        {
            var said = new List<Chat>();
            var request = new RestRequest("channels.history", Method.POST);
            request.AddParameter("token", Token);
            request.AddParameter("channel", slackChannelId);
            request.AddParameter("count", 1000);
            IRestResponse response = _client.Execute(request);

            dynamic responseJson = JsonConvert.DeserializeObject(response.Content);
            foreach (dynamic message in responseJson.messages)
            {
                if (said.Count(f => f.SlackId.Equals(message.ts)) < 1 && message.subtype != null)
                    said.Add(new Chat { Body = message.text, SlackId = message.ts, Name = message.username });
            }

            return said;
        }
    }
}
