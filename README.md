### Usage

Please edit `MediatorAgent/configuration.json` and update `agentPublicEndpoint`
to be the internet facing URL where this agent can receive requests from.

If you're developing locally, [ngrok](https://ngrok.com/) can be used to
create a tunnel to localhost :)

### Caution

This repository is meant only for demonstration and is no where near production ready.
There are hacks around in UserManager.cs for instance. User state should be managed
in a more robust way.