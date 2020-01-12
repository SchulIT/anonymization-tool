using System;

namespace AnonymizationTool.Messages
{
    public class ErrorDialogMessage : DialogMessage
    {
        public Exception Exception { get; set; }
    }
}
