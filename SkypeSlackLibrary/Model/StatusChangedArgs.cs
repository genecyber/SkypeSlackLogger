using System;

namespace SkypeSlackLibrary.Model
{
    public class StatusChangedArgs : EventArgs
    {
        public Status Status { get; set; }
        public DateTime Time { get; set; }
    }
}