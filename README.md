# Usage

Please edit `MediatorAgent/configuration.json` and update `agentPublicEndpoint`
to be the internet facing URL where this agent can receive requests from.

If you're developing locally, [ngrok](https://ngrok.com/) can be used to
create a tunnel to localhost:

```
$ ngrok http 5000
```

You can build and run the server on the commandline with `dotnet build`.

You need to edit configuration.json to know the external URL of the mediator, which
you get from ngrok. Make certain that the directory mentioned for the
wallet *does not exist* before running it for the first time.

Then you can run it like this: `dotnet run --project MediatorAgent`.

Instructions to integrate it with an edge-agent can be found [here](Usage.md).

# Dependencies

You will need to have libindy v1.14.2 available for dynamic linking.

On MacOS you do this to get it:

```
$ wget https://repo.sovrin.org/macos/libindy/stable/1.14.2/libindy_1.14.2.zip
$ mkdir libindy-1.14.2
$ cd libindy-1.14.2/
$ unzip ../libindy_1.14.2.zip 
$ cp lib/libindy.dylib /usr/local/lib
```

It needs libsodium, but with a hack:

```
$ brew install libsodium
$ brew info libsodium
# check that you have 1.0.18_1
$ cd /usr/local/Cellar/libsodium/1.0.18_1/lib
$ cp libsodium.23.dylib libsodium.18.dylib
```

# Caution

This repository is meant only for demonstration and is nowhere
near production ready. The current implementation maintains
in-memory queues for each identity it has to route messages for
and attaches subscribers to it when the corresponding identity
connects.

A more robust way for instance, would be to persist the in-memory
queues to be more resilient to failures and partitioning
based on identity ids.


# License

This software is derived from [the
MediatorAgentService](https://github.com/hyperledger/aries-framework-dotnet/tree/master/samples/routing/MediatorAgentService)
sample. It carries the same license.

[Apache License Version 2.0](https://github.com/hyperledger/aries-cloudagent-python/blob/master/LICENSE)

