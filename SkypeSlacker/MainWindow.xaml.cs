using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SkypeSlacker.Extensions;
using SkypeSlackLibrary;
using SkypeSlackLibrary.Model;

namespace SkypeSlacker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly SkypeApplication SkypeLib = SkypeApplication.Instance;
        static readonly SlackApplication SlackLib = SlackApplication.Instance;
        private static readonly GroupsOfInterest GroupsOfInterest = new GroupsOfInterest();
        private Thread _pollingThread;
        bool _browserloading;

        public delegate void SkypeDelegate();

        public MainWindow()
        {
            InitializeComponent();
            LoadLiveGroups();
            SetUpSelectionChangedEvent();
            OnLoad();
        }

        private void SetUpSelectionChangedEvent()
        {
            MainTabs.SelectionChanged +=
                delegate
                {
                    AddToInterestButton.IsEnabled = LiveGroupsTab.IsSelected;
                    JoinGroupButton.IsEnabled = ((LiveGroupsTab.IsSelected &&
                                                  LiveGroupTable.SelectedItems.Count > 0) ||
                                                 (InterestGroupsTab.IsSelected &&
                                                  InterestGroupTable.SelectedItems.Count > 0));
                    RemoveFromInterestButton.IsEnabled = (InterestGroupsTab.IsSelected &&
                                                          InterestGroupTable.SelectedItems
                                                              .Count >
                                                          0);
                };
        }

        private void LoadLiveGroups()
        {
            browser1.LoadCompleted += delegate { _browserloading = false; };

            var groups = SkypeLib.GetGroupCollection();
            LiveGroupTable.ItemsSource = groups;
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            OnLoad();
        }

        private void OnLoad()
        {
            var instance = ApplicationSettings.Instance;
            if (String.IsNullOrEmpty(instance.SlackREST_EndpointBase))
                EndpointTextbox.Text = "https://slack.com/api/";
            else
                EndpointTextbox.Text = instance.SlackREST_EndpointBase;
            TokenTextbox.Text = instance.Slack_API_Token;
            FilenameTextbox.Text = instance.GroupsOfInterest_FileName;

            if (String.IsNullOrEmpty(instance.Slack_API_Token) || String.IsNullOrEmpty(instance.GroupsOfInterest_FileName) || String.IsNullOrEmpty(instance.Slack_API_Token))
            {
                LiveGroupsTab.IsEnabled = false;
                InterestGroupsTab.IsEnabled = false;
                SettingsTab.Focus();
            }
            else
            {
                SkypeLoggerWindow.Loaded += (sender, args) => JoinAllChats();
                GroupsOfInterest.Refresh();
                var map = GroupsOfInterest.Map;
                UpdateSlackChannelIds();
                InterestGroupTable.ItemsSource = map;
                SaveButton.IsEnabled = true;
                StartButton.IsEnabled = true;
                LiveGroupsTab.IsEnabled = true;
                InterestGroupsTab.IsEnabled = true;
                UpdateSlackChannelIdButton.IsEnabled = true;
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateSlackChannelIds();
            }
            catch (Exception ex){}
            GroupsOfInterest.SaveMap();
        }


        private void PopulateSlackChannelIdButtonClick(object sender, RoutedEventArgs e)
        {
            new Thread(UpdateSlackChannelIds).Start();
        }

        private void UpdateSlackChannelIds()
        {
            foreach (var channelMap in GroupsOfInterest.Map)
            {
                if (!String.IsNullOrEmpty(channelMap.SlackFriendlyName))
                {
                    dynamic channelDetails = SlackLib.GetOrJoinChannel(channelMap.SlackFriendlyName);
                    channelMap.SlackChannelId = channelDetails.channel.id;
                }
            }
            if (InterestGroupTable.Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                InterestGroupTable.ItemsSource = GroupsOfInterest.Map;
            }
            else
            {
                // Invokation required
                InterestGroupTable.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                                                                                               {
                                                                                                   InterestGroupTable
                                                                                                       .ItemsSource
                                                                                                       = null;
                                                                                                   InterestGroupTable
                                                                                                       .ItemsSource
                                                                                                       =
                                                                                                       GroupsOfInterest
                                                                                                           .Map;
                                                                                               }));
            }

        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            new Thread(StartPoll).Start();
            Title = "Skype to Slack Logger : RUNNING";
            StopButton.Toggle();
            StartButton.Toggle();
        }

        private static void RefreshPollingTargets()
        {
            SkypeLib.RefreshInterest(GroupsOfInterest);
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            StartButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SkypeDelegate(StopPoll));
            Title = "Skype to Slack Logger";
            StopButton.Toggle();
            StartButton.Toggle();
        }

        private static void StartPoll()
        {
            SkypeLib.StartPoll(GroupsOfInterest);
        }

        private void StopPoll()
        {
            SkypeLib.StopPoll();
        }

        public void JoinAllChats()
        {
            foreach (ChannelMap item in GroupsOfInterest.Map)
            {
                if (!String.IsNullOrEmpty(item.UriBlob))
                {
                    OpenChat(item.UriBlob);
                }
            }
        }

        private void JoinChatButtonClick(object sender, RoutedEventArgs e)
        {
            var selected = InterestGroupsTab.IsSelected ? InterestGroupTable : LiveGroupTable;
            if (selected.SelectedItems.Count <= 0) return;
            foreach (SkypeChat selectedItem in selected.SelectedItems)
            {
                if (!String.IsNullOrEmpty(selectedItem.UriBlob))
                {
                    OpenChat(selectedItem.UriBlob);
                }
            }
        }

        private void OpenChat(string blob)
        {
            browser1.Navigate(SkypeLib.GenerateJoinUri(blob));
            _browserloading = true;
            while (_browserloading)
            {
                DoEvents();
            }
        }

        private void AddInterestButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var map in from LiveChat selectedItem in LiveGroupTable.SelectedItems
                select
                    new ChannelMap
                    {
                        SkypeChannelId = selectedItem.ChatName,
                        SkypeFriendlyName = selectedItem.ChatTopic,
                        UriBlob = selectedItem.UriBlob
                    })
            {
                InterestGroupTable.ItemsSource = null;
                ChannelMap _map = map;
                foreach (ChannelMap channelMap in GroupsOfInterest.Map.Where(c => c.SkypeChannelId.Equals(_map.SkypeChannelId) && String.IsNullOrEmpty(c.UriBlob)))
                {
                    channelMap.UriBlob = _map.UriBlob;
                }

                if (! GroupsOfInterest.Map.Any(c => c.SkypeChannelId.Equals(map.SkypeChannelId)))
                    GroupsOfInterest.Map.Add(map);
                InterestGroupTable.ItemsSource = GroupsOfInterest.Map;
                new Thread(RefreshPollingTargets).Start();
            }
        }

        private void RemoveFromInterestButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = (from ChannelMap selectedItem in InterestGroupTable.SelectedItems select new ChannelMap(selectedItem)).ToList();
                foreach (ChannelMap selectedItem in selected)
                {
                    InterestGroupTable.ItemsSource = null;
                    GroupsOfInterest.Map.Remove(GroupsOfInterest.Map.FirstOrDefault(c => c.SkypeChannelId == selectedItem.SkypeChannelId));
                    InterestGroupTable.ItemsSource = GroupsOfInterest.Map;
                }
            }
            catch (Exception)
            {
                return;
            }
            new Thread(RefreshPollingTargets).Start();
        }

        /// <summary>
        /// Event Pump
        /// </summary>
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private void SaveSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var instance = ApplicationSettings.Instance;
            instance.Slack_API_Token = TokenTextbox.Text;
            instance.SlackREST_EndpointBase = EndpointTextbox.Text;
            instance.GroupsOfInterest_FileName = FilenameTextbox.Text;
            ApplicationSettings.Save();

            if (!String.IsNullOrEmpty(instance.GroupsOfInterest_FileName) &&
                !String.IsNullOrEmpty(instance.GroupsOfInterest_FileName) &&
                !String.IsNullOrEmpty(instance.Slack_API_Token))
            {
                OnLoad();
                LiveGroupsTab.Focus();
            }
        }

    }
}
