using System;
using Hyperledger.Aries.Routing;

namespace MediatorAgent
{
    public class CustomMediatorAgent : DefaultMediatorAgent
    {
        public CustomMediatorAgent(IServiceProvider provider) : base(provider)
        {
        }

        protected override void ConfigureHandlers()
        {
            base.ConfigureHandlers();
            AddTrustPingHandler();
        }
    }
}
