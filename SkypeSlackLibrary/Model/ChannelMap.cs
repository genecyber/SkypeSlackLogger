using System.Collections.Generic;

namespace SkypeSlackLibrary.Model
{
    public class ChannelMap : SkypeChat
    {
        public ChannelMap()
        {
        }

        public ChannelMap(ChannelMap map)
        {
            SkypeChannelId = map.SkypeChannelId;
            SkypeFriendlyName = map.SkypeFriendlyName;
            SlackChannelId = map.SlackChannelId;
            SlackFriendlyName = map.SlackFriendlyName;
            UriBlob = map.UriBlob;
        }
        public string SkypeChannelId { get; set; }
        public string SkypeFriendlyName { get; set; }
        public string SlackChannelId { get; set; }
        public string SlackFriendlyName { get; set; }
        internal List<Chat> Said { get; set; }
    }
}