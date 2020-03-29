# Usage

A mediator is supposed to relay DIDComm messages between two Aries agents. Essentially, a mediator acts
as a blind "middle-man" which knows how to route a DIDComm message without knowing the underlying
contents of the message. More detailed explanation of different scenarios for using a mediator
is available [here](https://github.com/hyperledger/aries-rfcs/tree/master/concepts/0046-mediators-and-relays).

The mediator agent in this repository was built keeping edge (mobile) agents in mind. Since mobile devices
cannot be assumed to be connected to be online 24/7, a mediator is essential to act as a bridge and allow
routing messages to the edge agent. The description below lists the steps required to use this mediator
with an edge agent

## Setup

Every edge agent that wants to use the mediator must "sign up" to use it. At its heart, signing up
requires establishing a DID Connection with the mediator.

The implementation makes a reusable DID Invitation available at `http://<mediator-endpoint>/.well-known/agent-configuration`,
under the `Invitation` key in the JSON object. Edge agents willing to sign up need to parse the invitation and establish
a DID connection with the mediator using a slightly modified version of the [Connection Protocol](https://github.com/hyperledger/aries-rfcs/tree/master/features/0160-connection-protocol).

The only change in the protocol is to modify the DIDDoc received from the mediator as a `Connection Response` message and remove the
`routingKey` since the messages between the edge agent and the mediator do not require any routing/indirection. Every thing else
in the protocol remains the same.

Next, the edge agent needs to create an "inbox" at the mediator by sending a `create-inbox` message. Behind the scenes, this creates a wallet for every edge-agent
that stores encrypted incoming messages for the edge-agent. The key for this wallet is randomly generated and is
returned in the response.

*Create Inbox Message Example*
```
{
  "@type": "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/basic-routing/1.0/create-inbox",
  "@id": "<uuid>",
  "metadata": {}
}
```

*Create Inbox Response Message Example*
```
{
  "@type": "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/basic-routing/1.0/create-inbox-response"
  "@id": "<uuid>",
  "inboxId": "<inbox-id>",
  "inboxKey": "<inbox-key>"
}
```

## Making the mediator known to other agents

Now that we've established a connection with the mediator, the edge agent may use the mediator
url as a service endpoint and use the mediator agent's verkey as its routing key.

Further, the edge agent must also associate its verkey generated for every DID connection
with their "inbox" in the mediator. This would allow the mediator to route the messages for
a given verkey to the correct edge agent. This may be done by sending an "AddRoute" message
before accepting/sending any invitation

*Add Route Message Example*
```
{
  "@type": "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/basic-routing/1.0/add-route",
  "@id": "<uuid>",
  "routeDestination": "<verkey for this connection>"
}
```

## Getting messages from the mediator

Now that your edge agent has a connection established with the mediator and other agents
are sending messages to you via the mediator, let's have a look at retrieving your messages
from it.

The mediator allows edge agents to connect to it over [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1)
and retrieve messages while it's connected to it in real time.

To get started, the edge agent must establish a SignalR connection at `http://<mediator-url>/hub`.
Next the mediator would then send in an `Authorize` message to the edge agent with a `nonce`.
The edge-agent must reply back with an `AuthorizeResponse` message that contains a JSON encoded
*packed* (using indy.packMessage) message. The call to indy.packMessage should use the mediator
agent's verkey as the `receiverKey` and the edgeAgent-mediator connection verkey as the `senderVk`
with the following raw message:

```
{
  "@type": "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/authorize/1.0/authorize_response",
  "@id": "<uuid>",
  "nonce": <nonce>,
}
```

The mediator would now send messages recieved (including those which were sent while the edge agent was offline)
for this particular connection over `HandleMessage` with the `packedMessage` and the `itemId` as
the parameters. Edge agents must implement a handler for it, and process the
`packedMessage` as they would for any aries packed message.

On successful processing, the edge agent should send back an `Acknowledge` message to the mediator
over SignalR with the following message's indy packed representation as the argument:

```
{
  "@type": "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/authorize/1.0/acknowledge",
  "@id": "<uuid>",
  "itemId": "<itemId>",
}
```

Doing so would remove the message from the mediator's persistence layer.
