using System;

namespace AnonymizationTool.UI
{
    public interface IDispatcher
    {
        void RunOnUI(Action action);
    }
}
