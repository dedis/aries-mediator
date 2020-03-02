# Usage

Please edit `MediatorAgent/configuration.json` and update `agentPublicEndpoint`
to be the internet facing URL where this agent can receive requests from.

If you're developing locally, [ngrok](https://ngrok.com/) can be used to
create a tunnel to localhost.

# Caution

This repository is meant only for demonstration and is nowhere
near production ready. There are hacks in UserManager.cs for
instance. User state should be managed in a more robust way.

# License

[Apache License Version 2.0](https://github.com/hyperledger/aries-cloudagent-python/blob/master/LICENSE)

