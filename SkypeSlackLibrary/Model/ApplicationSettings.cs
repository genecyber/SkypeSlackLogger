using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SkypeSlackLibrary.Model
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instanceApplicationSettings = new ApplicationSettings();
        private const string Filename = "Settings.txt";
        public string GroupsOfInterest_FileName { get; set; }
        public string Slack_API_Token { get; set; }
        public string SlackREST_EndpointBase { get; set; }

        public static ApplicationSettings Instance
        {
            get
            {
                if (String.IsNullOrEmpty(_instanceApplicationSettings.Slack_API_Token))
                    return _instanceApplicationSettings = Load();
                return _instanceApplicationSettings;
            }
            set { _instanceApplicationSettings = value; Save(); }
        }

        public static ApplicationSettings Load()
        {
            CheckExistence();
            var element = XElement.Load(Filename);
            return _instanceApplicationSettings = element.FromXElement<ApplicationSettings>();
        }

        private static void CheckExistence()
        {
            if (!File.Exists(Filename))
            {
                Save();
            }
        }

        public static void Save()
        {
            XElement element = _instanceApplicationSettings.ToXElement<ApplicationSettings>();
            element.Save(Filename);
        }
    }
}
