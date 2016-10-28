namespace RestRPC.Framework.Exceptions
{
    enum ErrorCode
    {
        ParseError = -32700,
        InvalidRequest = -32600,
        MethodNotFound = -32601,
        InvalidParams = -32602,
        InternalError = -32603,
        // -32000 to -32099: Reserved for implementation-defined server-errors
        // The remainder of the space is available for application defined errors
    }
}
