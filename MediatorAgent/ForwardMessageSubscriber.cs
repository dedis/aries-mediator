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

        public ForwardMessageSubscriber(IEventAggregator eventAggregator, IAgentProvider agentProvider, IWalletService walletService, IWalletRecordService walletRecordService, IConnectionService connectionService, IHubContext<MediatorHub> hubContext)
        {
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
                    wallet: agentContext.Wallet,
                    e.InboxId
                );
                var edgeWallet = await _walletService.GetWalletAsync(record.WalletConfiguration, record.WalletCredentials);

                var item = await _walletRecordService.GetAsync<InboxItemRecord>(edgeWallet, e.ItemId);
                var connection = await _connectionService.ListAsync(agentContext, SearchQuery.Equal("InboxId", e.InboxId));
                // Should only get one connection-id since connection and inbox have 1:1 relation
                if (connection.Count == 1)
                {
                    var did = connection[0].TheirDid;
                    if (UserManager.didConnectionMap.ContainsKey(did))
                    {
                        // User is connected
                        System.Diagnostics.Debug.WriteLine("User is connected, sending message");
                        var connectionId = UserManager.didConnectionMap[did];
                        await _hubContext.Clients.Client(connectionId).SendAsync("HandleMessage", item.ItemData);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("User isn't online " + did);
                        UserManager.Print();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Got " + connection.Count + " connections for this inbox id. Something's wrong.");
                }

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
