# RestRPC
RestRPC (RRPC) is a *client-to-client* remote procedure call (RPC) platform. RRPC supports both **REST** and **WebSocket** clients. The specification is similar to [JSON-RPC 2.0](http://www.jsonrpc.org/specification), with additional fields to identify the requester/responder.

There are 3 roles in the RRPC specification:
- RRPC server (*"router"*): The request/response router. Server is responsible to deliver a request (submitted via either WebSocket or REST) to its intended service, and return the response back to the requester.
- RRPC client (*"requester"*): The requester of a remote procedure. The origin of Request objects and the handler of Response objects.
- RRPC service (*"responder"*): The origin of Response objects and the handler of Request objects.

## Features
- Client-to-client RPC: Procedures are stored on clients (services) and can be called by other clients (requesters)
- Modular: RestRPC allows registering procedures on services at runtime
- Supports a network of RPC services
- Service management on a web dashboard
- Supports both REST and WebSocket clients
- RRPC is designed for games!
