using System.Collections.Generic;
using OWCE.DependencyInterfaces;
using OWCE.PropertyChangeHandlers;
using WatchConnectivity;
namespace OWCE.iOS.DependencyImplementations
{
    // Implementation of IWatch on iOS.
    // Messages are received on InterfaceController.cs on the watch side.
    public class Watch : IWatch
    {
        private OWBoard _board;

        public void SendWatchMessages(Dictionary<WatchMessage, object> messages)
        {
            WCSessionManager.SharedManager.SendMessage(messages);
        }

        void IWatch.ListenForWatchMessages(OWBoard board)
        {
            _board = board;
            WCSessionManager.SharedManager.MessageReceived += DidReceiveMessage;
        }

        void IWatch.StopListeningForWatchMessages()
        {
            WCSessionManager.SharedManager.MessageReceived -= DidReceiveMessage;
        }

        public void DidReceiveMessage(WCSession session, Dictionary<WatchMessage, object> message)
        {
            WatchSyncEventHandler.HandleWatchMessage(message, _board);
        }
    }
}
