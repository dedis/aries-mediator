using System;
using Hyperledger.Aries.Agents;
using Newtonsoft.Json;

namespace MediatorAgent.Message
{
    public class AuthorizeResponseMessage : AgentMessage
    {
        public const string AuthorizeMessageType = "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/authorize/1.0/authorize_response";

        public AuthorizeResponseMessage()
        {
            Id = Guid.NewGuid().ToString();
            Type = AuthorizeMessageType;
        }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }
}
