using System;

namespace RestRPC.Framework.Exceptions
{
    class InternalErrorException : RrpcException
    {
        public InternalErrorException(Exception innerException)
            : base((int)ErrorCode.InternalError, "Internal error", innerException)
        { }
    }
}
