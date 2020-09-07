using System;
using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace MediatorAgent
{
    public class ForwardMessageSubscriber : IHostedService
    {
        private readonly IObservable<InboxItemEvent> observable;
        private IDisposable disposable;
        private readonly IAgentProvider _agentProvider;
        private readonly IWalletService _walletService;
        private readonly IWalletRecordService _walletRecordService;
        private readonly IConnectionService _connectionService;
        private readonly IHubContext<MediatorHub> _hubContext;
        private readonly MessageQueue<InboxItemRecord> _messageQueue;

        public ForwardMessageSubscriber(MessageQueue<InboxItemRecord> messageQueue, IEventAggregator eventAggregator, IAgentProvider agentProvider, IWalletService walletService, IWalletRecordService walletRecordService, IConnectionService connectionService, IHubContext<MediatorHub> hubContext)
        {
            _messageQueue = messageQueue;
            observable = eventAggregator.GetEventByType<InboxItemEvent>();
            _agentProvider = agentProvider;
            _walletService = walletService;
            _walletRecordService = walletRecordService;
            _connectionService = connectionService;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            disposable = observable.Subscribe(async (e) =>
            {
                
                var agentContext = await _agentProvider.GetContextAsync();

                var record = await _walletRecordService.GetAsync<InboxRecord>(
                    agentContext.Wallet,
                    e.InboxId
                );
                var edgeWallet = await _walletService.GetWalletAsync(record.WalletConfiguration, record.WalletCredentials);

                var item = await _walletRecordService.GetAsync<InboxItemRecord>(edgeWallet, e.ItemId);
		System.Diagnostics.Debug.WriteLine("Got an item for inboxId" + e.InboxId);
                _messageQueue.enqueue(e.InboxId, item);
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            disposable.Dispose();
            return Task.CompletedTask;
        }

    }
}
