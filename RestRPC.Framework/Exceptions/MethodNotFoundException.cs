namespace RestRPC.Framework.Exceptions
{
    class MethodNotFoundException : RrpcException
    {
        public MethodNotFoundException(string Method)
            : base((int)ErrorCode.MethodNotFound, "Method not found", new System.MissingMethodException(Method))
        { }
    }
}
