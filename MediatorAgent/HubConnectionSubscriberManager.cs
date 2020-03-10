using System;
using System.Collections.Concurrent;

namespace MediatorAgent
{
    public class HubConnectionSubscriberManager
    {
        /**
         * Stores a map from SignalR connection id to reference of IDisposables
         * that stop the subscription to the corresponding inbox
         */
        public ConcurrentDictionary<string, IDisposable> connectionDisposableMap = new ConcurrentDictionary<string, IDisposable>();

        /**
         * Associates a SignalR connection id to a IDisposable
         */
        public void AssociateDisposable(string connectionId, IDisposable disposable)
        {
            connectionDisposableMap.TryAdd(connectionId, disposable);
        }

        /**
         * Stops the subscriber associated with the connection id
         */
        public void StopMonitoringMessages(string connectionId)
        {
            connectionDisposableMap.TryRemove(connectionId, out IDisposable value);
            value.Dispose();
            System.Diagnostics.Debug.WriteLine("Disposed subscriber for " + connectionId);
        }
    }
}
