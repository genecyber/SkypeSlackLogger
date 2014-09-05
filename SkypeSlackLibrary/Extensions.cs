using System;
using SKYPE4COMLib;
using SkypeSlackLibrary.Model;

namespace SkypeSlackLibrary
{
    public static class Extensions
    {
        public static string GetSomeName(this User user)
        {
            if (!string.IsNullOrEmpty(user.FullName))
                return user.FullName;
            if (!string.IsNullOrEmpty(user.DisplayName))
                return user.DisplayName;
            if (!string.IsNullOrEmpty(user.Handle))
                return user.Handle;
            if (!string.IsNullOrEmpty(user.Aliases))
                return user.Aliases;
            return "unbekannt";
        }


        public static Sex GetUserInfoSexValue(this TUserSex sex)
        {
            return sex == TUserSex.usexMale ? Sex.Male : Sex.Female;
        }

        public static string ToLocalString(this Status status)
        {
            switch (status)
            {
                case Status.Online:
                    return "online";
                case Status.Away:
                    return "abwesend";
                case Status.Offline:
                    return "offline";
                case Status.Unknown:
                    return "unbekannt";
                case Status.DoNotDisturb:
                    return "nicht stören";
                case Status.NotAvailable:
                    return "nicht verfügbar";
                case Status.SkypeMe:
                    return "Skype Me!";
                case Status.SkypeOut:
                    return "Skype out";
                default:
                    return String.Empty;
            }
        }

        public static Status GetStatusValue(this TOnlineStatus status)
        {
            switch (status)
            {
                case TOnlineStatus.olsOnline:
                    return Status.Online;
                case TOnlineStatus.olsAway:
                    return Status.Away;
                case TOnlineStatus.olsOffline:
                    return Status.Offline;
                case TOnlineStatus.olsDoNotDisturb:
                    return Status.DoNotDisturb;
                case TOnlineStatus.olsNotAvailable:
                    return Status.NotAvailable;
                case TOnlineStatus.olsSkypeMe:
                    return Status.SkypeMe;
                case TOnlineStatus.olsSkypeOut:
                    return Status.SkypeOut;
            }
            return Status.Unknown;
        }

        public static TUserStatus GetTUserStatusValue(this Status status)
        {
            switch (status)
            {
                case Status.Online:
                    return TUserStatus.cusOnline;
                case Status.Away:
                    return TUserStatus.cusAway;
                case Status.Offline:
                    return TUserStatus.cusOffline;
                case Status.Unknown:
                    return TUserStatus.cusUnknown;
                case Status.DoNotDisturb:
                    return TUserStatus.cusDoNotDisturb;
                case Status.NotAvailable:
                    return TUserStatus.cusNotAvailable;
                case Status.SkypeMe:
                    return TUserStatus.cusSkypeMe;
            }
            return TUserStatus.cusUnknown;
        }

        public static Status GetStatusValue(this TUserStatus status)
        {
            switch (status)
            {
                case TUserStatus.cusOnline:
                    return Status.Online;
                case TUserStatus.cusAway:
                    return Status.Away;
                case TUserStatus.cusOffline:
                    return Status.Offline;
                case TUserStatus.cusDoNotDisturb:
                    return Status.DoNotDisturb;
                case TUserStatus.cusNotAvailable:
                    return Status.NotAvailable;
                case TUserStatus.cusSkypeMe:
                    return Status.SkypeMe;
            }
            return Status.Unknown;
        }

        public static int GetImageIndex(this Status status)
        {
            switch (status)
            {
                case Status.Online:
                    return 0;
                case Status.Away:
                    return 1;
                case Status.Offline:
                    return 4;
                case Status.Unknown:
                    return 4;
                case Status.DoNotDisturb:
                    return 2;
                case Status.NotAvailable:
                    return 1;
                case Status.SkypeMe:
                    return 0;
                case Status.SkypeOut:
                    return 3;
                default:
                    return 4;
            }
        }

        public static UserInfo GetUserInfo(this User user)
        {
            // TODO: Maybe take from some database!
            return new UserInfo { Name = user.GetSomeName(), Sex = user.Sex.GetUserInfoSexValue() };
        }
    }
}