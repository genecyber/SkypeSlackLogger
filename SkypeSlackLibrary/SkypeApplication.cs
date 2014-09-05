using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SKYPE4COMLib;
using SkypeSlackLibrary.Model;
using Chat = SkypeSlackLibrary.Model.Chat;

namespace SkypeSlackLibrary
{
    public class SkypeApplication
    {
        private static SkypeApplication _theInstance;
        private static readonly SlackApplication Slack = SlackApplication.Instance;
        private readonly Skype _skype = new Skype();
        private bool _polling;
        private List<ChannelMap> _map = new List<ChannelMap>();
        int _timer = 550;

        private SkypeApplication()
        {
            Start();
            _skype.Cache = false;
        }

        public static SkypeApplication Instance
        {
            get { return _theInstance ?? (_theInstance = new SkypeApplication()); }
        }

        public bool Running
        {
            get { return _skype.Client.IsRunning; }
        }

        public string Information
        {
            get { return String.Format("v{0}", _skype.Version); }
        }

        public void Start()
        {
            if (!_skype.Client.IsRunning)
            {
                _skype.Client.Start(true, true);
            }
        }

        public void Shutdown()
        {
            if (_skype.Client.IsRunning)
            {
                _skype.Client.Shutdown();
            }
        }
        
        public bool PollChats(ChannelMap channel)
        {
            //_skype.get_Messages();
            var skypeCom = _skype.Chat[channel.SkypeChannelId];
            var collection = skypeCom.Messages;

            //make a list for fast compare
            List<Chat> usableCollection = null;
            try
            {
                usableCollection =
                    (from ChatMessage msg in collection
                     where !String.IsNullOrEmpty(msg.Body)
                     select new Chat { Body = msg.Body, Guid = msg.Guid, Name = msg.FromDisplayName }).ToList();
            }
            catch (OutOfMemoryException e)
            {
                return false;
            }
            var toSay = new List<Chat>();

            //Loop over potentials
            foreach (var msg in usableCollection)
            {
                //If has not been said on slack, add it to queue
                if (channel.Said.Count(n => (n.Body == msg.Body && n.Name == msg.Name)) < 1)
                {
                    toSay.Add(new Chat { Body = msg.Body, Guid = msg.Guid, Name = msg.Name });
                }
                //If it's been said we are all caught up
                else
                    break;
            }
            //Reverse the order so it flows correctly
            toSay.Reverse();

            foreach (var msg in toSay)
            {
                dynamic response = Slack.SaySomething(msg.Body, msg.Name, channel);
                if (response.error != null && response.error.Value == "rate_limited")
                {
                    //Oops we got ratelimited. Luckily the server tells us when we can try again
                    _timer = ~response.retry_after + 1;
                    Thread.Sleep(_timer);
                    response = Slack.SaySomething(msg.Body, msg.Name, channel);
                }

                msg.SlackId = response.ts;
                //Add to our list of items said.
                channel.Said.Add(msg);
                Thread.Sleep(_timer);
            }
            return true;
        }
        public void StartPoll(GroupsOfInterest groupsOfInterest)
        {
            _polling = true;
            CopyGroupLocally(groupsOfInterest);
            _map.ForEach(c => c.Said = Slack.AlreadySaid(c.SlackChannelId)); //Get all previously said items and add to object
            while (_polling)
            {
                foreach (ChannelMap channelMap in _map)
                {
                    PollChats(channelMap);
                }
            }
        }

        public void RefreshInterest(GroupsOfInterest groupsOfInterest)
        {
            CopyGroupLocally(groupsOfInterest);
        }

        private void CopyGroupLocally(GroupsOfInterest groupsOfInterest)
        {
            _map = new List<ChannelMap>();
            foreach (
                ChannelMap channel in
                    groupsOfInterest.Map.Where(
                        channel =>
                            channel.SkypeChannelId != null && channel.SkypeFriendlyName != null &&
                            channel.SlackChannelId != null && channel.SlackFriendlyName != null))
            {
                _map.Add(new ChannelMap
                         {
                             SkypeChannelId = channel.SkypeChannelId,
                             SkypeFriendlyName = channel.SkypeFriendlyName,
                             SlackChannelId = channel.SlackChannelId,
                             SlackFriendlyName = channel.SlackFriendlyName
                         });
            }
        }

        public void StopPoll()
        {
            _polling = false;
        }

        public List<LiveChat> GetGroupCollection(bool hideEmptyTopics= true)
        {
            var chats = _skype.Chats;
            var usableGroupCollection =
                (from SKYPE4COMLib.Chat c in chats
                    where c.Type.Equals(TChatType.chatTypeMultiChat)
                    select
                        new LiveChat
                        {
                            ChatTopic = c.Topic,
                            Friendly = c.FriendlyName,
                            UriBlob = c.Blob,
                            ChatName = c.Name
                        });
            if (hideEmptyTopics)
                usableGroupCollection = usableGroupCollection.Where(s => s.ChatTopic != "");

            return usableGroupCollection.ToList(); ;
        }
        public string GenerateJoinUri(string blob)
        {
            return "skype:?chat&blob="+blob;
        }
    }
}