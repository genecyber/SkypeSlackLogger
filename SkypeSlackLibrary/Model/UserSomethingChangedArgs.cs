using System;

namespace SkypeSlackLibrary.Model
{
    public class UserSomethingChangedArgs : EventArgs
    {
        public Contact Contact { get; set; }
        public DateTime Time { get; set; }
    }
}