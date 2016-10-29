using RestRPC.Framework.Messages.Outputs;
using System;

namespace RestRPC.Framework.Exceptions
{
    abstract class RrpcException : Exception
    {
        public int Code { get; private set; }

        public RrpcException(int code, string message)
            : this(code, message, null)
        { }

        public RrpcException(int code, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Code = code;
        }

        internal ErrorObject ToErrorObject()
        {
            return new ErrorObject(Code, Message, ToString());
        }
    }
}
