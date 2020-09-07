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
    public class HubMethods
    {
        private readonly IHubContext<MediatorHub> _hubContext;
        private IAgentProvider _agentProvider;
        private IConnectionService _connectionService;
        private IWalletRecordService _walletRecordService;
        private IWalletService _walletService;
        private MessageQueue<InboxItemRecord> _messageQueue;
        private HubConnectionSubscriberManager _hubConnectionSubscriberManager;

        public HubMethods(
            IAgentProvider agentProvider,
            IConnectionService connectionService,
            IWalletRecordService walletRecordService,
            IWalletService walletService,
            MessageQueue<InboxItemRecord> messageQueue,
            IHubContext<MediatorHub> hubContext,
            HubConnectionSubscriberManager hubConnectionSubscriberManager)
        {
            _agentProvider = agentProvider;
            _connectionService = connectionService;
            _walletRecordService = walletRecordService;
            _walletService = walletService;
            _hubContext = hubContext;
            _messageQueue = messageQueue;
            _hubConnectionSubscriberManager = hubConnectionSubscriberManager;
        }

        // Subscribes the client to the queue of messages associated with it
        public async Task HandleAuthorizeResponse(string message, string connectionId)
        {
            var (authorizeResponse, connection) = await GetMessage<AuthorizeResponseMessage>(message);
            System.Diagnostics.Debug.WriteLine("Nonce Received: " + authorizeResponse.Nonce + " Expected " + connectionId);
            if (authorizeResponse.Nonce == connectionId)
            {

                var inboxId = connection.GetTag("InboxId");
                var observable = _messageQueue.GetObservableForInbox(inboxId);
                System.Diagnostics.Debug.WriteLine("Got observable for connectionId " + connectionId);
                var disposable = observable.SubscribeOn(TaskPoolScheduler.Default)
                    .Select(item => Observable.Defer(() =>
                    {
			            System.Diagnostics.Debug.WriteLine("Sending HandleMessage to" + connectionId + "for event in inbox " + inboxId);
                        return _hubContext.Clients.Client(connectionId)
                        .SendAsync("HandleMessage", item.ItemData, item.Id)
                        .ToObservable();
                    }))
                    .Concat()
                    .Subscribe();
                _hubConnectionSubscriberManager.AssociateDisposable(connectionId, disposable);
            }
        }

        // An ack would remove the item-record from the wallet 
        public async Task HandleAcknowledge(string message)
        {
            var agentContext = await _agentProvider.GetContextAsync();
            var (acknowledgement, connection) = await GetMessage<AcknowledgeMessage>(message);

            var inboxId = connection.GetTag("InboxId");
            var inboxRecord = await _walletRecordService.GetAsync<InboxRecord>(agentContext.Wallet, inboxId);
            var edgeWallet = await _walletService.GetWalletAsync(inboxRecord.WalletConfiguration, inboxRecord.WalletCredentials);
            try
            {
                await _walletRecordService.DeleteAsync<InboxItemRecord>(edgeWallet, acknowledgement.ItemId);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't delete inbox item with id: {0}", acknowledgement.ItemId);
            }
        }

        private async Task<(T, ConnectionRecord)> GetMessage<T>(string message) where T : AgentMessage, new()
        {
            var agentContext = await _agentProvider.GetContextAsync();
            var unpackedMessage = await Crypto.UnpackMessageAsync(agentContext.Wallet, message.GetUTF8Bytes());
            var unpackResult = unpackedMessage.ToObject<UnpackResult>();
            var connection = await _connectionService.ResolveByMyKeyAsync(agentContext, unpackResult.RecipientVerkey);
            var unpackedMessageContext = new UnpackedMessageContext(unpackResult.Message, connection);

            return (unpackedMessageContext.GetMessage<T>(), connection);
        }
    }
}
