using System;
using System.Windows.Threading;

namespace AnonymizationTool.UI
{
    public class UIDispatcher : IDispatcher
    {

        private Dispatcher dispatcher;

        public UIDispatcher()
        {
            dispatcher = App.Current.Dispatcher;
        }

        public void RunOnUI(Action action)
        {
            dispatcher.BeginInvoke(action, DispatcherPriority.Background);
        }
    }
}
