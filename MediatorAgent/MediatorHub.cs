using System;
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
        private IAgentProvider _agentProvider;
        private IConnectionService _connectionService;
        private IWalletRecordService _walletRecordService;
        private IWalletService _walletService;

        public MediatorHub(IAgentProvider agentProvider, IConnectionService connectionService, IWalletRecordService walletRecordService, IWalletService walletService)
        {
            _agentProvider = agentProvider;
            _connectionService = connectionService;
            _walletRecordService = walletRecordService;
            _walletService = walletService;
        }

        public override Task OnConnectedAsync()
        {
            UserManager.addConnection(Context.ConnectionId);
            Clients.Caller.SendAsync("Authorize", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserManager.removeConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task AuthorizeResponse(string message)
        {
            System.Diagnostics.Debug.WriteLine("Received AuthorizeResponse");

            var (authorizeResponse, connection) = await GetMessage<AuthorizeResponseMessage>(message);
            System.Diagnostics.Debug.WriteLine("Nonce Received: " + authorizeResponse.Nonce + " Expected " + Context.ConnectionId);
            if (authorizeResponse.Nonce == Context.ConnectionId)
            {
                UserManager.associateConnection(authorizeResponse.Nonce, connection.TheirDid);
                UserManager.Print();
            }
            System.Diagnostics.Debug.WriteLine("Processed AuthorizeResponse");
        }

        public async Task Acknowledge(string message)
        {
            System.Diagnostics.Debug.WriteLine("Received Acknowledgement");
            var agentContext = await _agentProvider.GetContextAsync();
            var (acknowledgement, connection) = await GetMessage<AcknowledgeMessage>(message);

            var inboxId = connection.GetTag("InboxId");
            var inboxRecord = await _walletRecordService.GetAsync<InboxRecord>(agentContext.Wallet, inboxId);
            var edgeWallet = await _walletService.GetWalletAsync(inboxRecord.WalletConfiguration, inboxRecord.WalletCredentials);
            try
            {
                await _walletRecordService.DeleteAsync<InboxItemRecord>(edgeWallet, acknowledgement.ItemId);
            } catch(Exception)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't delete inbox item with id: " + acknowledgement.ItemId);
            }
            System.Diagnostics.Debug.WriteLine("Processed Acknowledgement");
        }

        private async Task<(T, ConnectionRecord)> GetMessage<T>(string message) where T : AgentMessage, new() {
            var agentContext = await _agentProvider.GetContextAsync();
            var unpackedMessage = await Crypto.UnpackMessageAsync(agentContext.Wallet, message.GetUTF8Bytes());
            var unpackResult = unpackedMessage.ToObject<UnpackResult>();
            var connection = await _connectionService.ResolveByMyKeyAsync(agentContext, unpackResult.RecipientVerkey);
            var unpackedMessageContext = new UnpackedMessageContext(unpackResult.Message, connection);

            return (unpackedMessageContext.GetMessage<T>(), connection);
        }

    }
}
