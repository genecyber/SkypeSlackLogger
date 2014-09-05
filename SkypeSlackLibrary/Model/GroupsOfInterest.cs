using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SkypeSlackLibrary.Model
{
    public class GroupsOfInterest
    {
        private List<ChannelMap> _map = new List<ChannelMap>();

        public List<ChannelMap> Map
        {
            get { return _map ?? (_map = PopulateChannelMap()); }
            set { _map = value; SaveMap(); }
        }

        private List<ChannelMap> PopulateChannelMap()
        {
            if (!File.Exists(ApplicationSettings.Instance.GroupsOfInterest_FileName))
                SaveMap();
            var node = XElement.Load(ApplicationSettings.Instance.GroupsOfInterest_FileName);
            return _map = node.FromXElement<List<ChannelMap>>();
        }

        public void Refresh()
        {
            PopulateChannelMap();
        }

        public void SaveMap()
        {
            XElement element = _map.ToXElement<List<ChannelMap>>();
            element.Save(ApplicationSettings.Instance.GroupsOfInterest_FileName);
        }
    }
}
