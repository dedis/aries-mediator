using System;
using System.Collections.Concurrent;

namespace MediatorAgent
{
    public static class UserManager
    {
        // TODO: protect this with a mutex?
        public static ConcurrentDictionary<string, User> onlineConnections = new ConcurrentDictionary<string, User>();
        public static ConcurrentDictionary<string, string> didConnectionMap = new ConcurrentDictionary<string, string>();

        public static void addConnection(string connectionId)
        {
            onlineConnections.TryAdd(connectionId, new User());
        }

        public static void removeConnection(string connectionId)
        {
            User user;
            onlineConnections.TryRemove(connectionId, out user);
        }

        public static void associateConnection(string connectionId, string did)
        {
            onlineConnections[connectionId].state = User.UserState.Identified;
            onlineConnections[connectionId].DID = did;
            didConnectionMap.TryAdd(did, connectionId);
        }

        public static void Print()
        {
            System.Diagnostics.Debug.WriteLine("onlineConnections:");
            foreach (var key in onlineConnections.Keys)
            {
                var u = onlineConnections[key];
                System.Diagnostics.Debug.WriteLine(key + " => " + u.state + "," + u.DID);
            }
            System.Diagnostics.Debug.WriteLine("didConnectionMap:");
            foreach (var key in didConnectionMap.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key + " => " + didConnectionMap[key]);
            }
        }
    }

    public class User
    {
        public enum UserState
        {
            Connected,
            Identified
        }

        public UserState state { get; set; }

        public string DID { get; set; }

        public User()
        {
            state = UserState.Connected;
        }
    }
}
