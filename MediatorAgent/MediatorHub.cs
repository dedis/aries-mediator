using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Utils;
using Hyperledger.Indy.CryptoApi;
using MediatorAgent.Message;
using Microsoft.AspNetCore.SignalR;

namespace MediatorAgent
{
    public class MediatorHub : Hub
    {
        
        private HubMethods _hubMethods;
        private HubConnectionSubscriberManager _hubConnectionSubscriberManager;

        public MediatorHub(HubMethods hubMethods, HubConnectionSubscriberManager hubConnectionSubscriberManager)
        {
            
            _hubMethods = hubMethods;
            _hubConnectionSubscriberManager = hubConnectionSubscriberManager;
        }

        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("Authorize", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine("Disconnected. Removing observable...");
            _hubConnectionSubscriberManager.StopMonitoringMessages(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task AuthorizeResponse(string message)
        {
            System.Diagnostics.Debug.WriteLine("Received AuthorizeResponse");
            await _hubMethods.handleAuthorizeResponse(message, Context.ConnectionId);
            System.Diagnostics.Debug.WriteLine("Processed AuthorizeResponse");
        }

        public async Task Acknowledge(string message)
        {
            System.Diagnostics.Debug.WriteLine("Received Acknowledgement");
            await _hubMethods.handleAcknowledge(message);
            System.Diagnostics.Debug.WriteLine("Processed Acknowledgement");
        }

    }
}
