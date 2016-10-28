using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Inputs
{
    class InMessage : Message
    {
        [JsonConstructor]
        public InMessage(string Header, object Data)
            : base(Header, Data)
        { }

        internal virtual void Evaluate(RrpcComponent component)
        { }
    }
}
