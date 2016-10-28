using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Outputs
{
    class OutMessage : Message
    {
        [JsonConstructor]
        public OutMessage(string Header, object Data)
            : base(Header, Data)
        { }
    }
}
