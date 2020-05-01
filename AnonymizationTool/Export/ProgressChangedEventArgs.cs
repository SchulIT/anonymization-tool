using System;

namespace AnonymizationTool.Export
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public int? Completed { get; private set; }

        public int? Total { get; private set; }

        public string Details { get; private set; }

        public ProgressChangedEventArgs(int? completed, int? total, string details = null)
        {
            Completed = completed;
            Total = total;
            Details = details;
        }
    }
}
