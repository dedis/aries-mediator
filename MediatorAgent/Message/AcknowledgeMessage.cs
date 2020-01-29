using System;
using Hyperledger.Aries.Agents;
using Newtonsoft.Json;

namespace MediatorAgent.Message
{
    public class AcknowledgeMessage : AgentMessage
    {
        public AcknowledgeMessage()
        {
            Id = Guid.NewGuid().ToString();
            Type = "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/acknowledge/1.0/acknowledge";
        }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }
    }
}
