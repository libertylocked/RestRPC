using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Outputs
{
    class ErrorObject
    {
        [JsonProperty]
        public int Code { get; private set; }

        [JsonProperty]
        public string Message { get; private set; }

        [JsonProperty]
        public object Data { get; private set; }

        public ErrorObject(int Code, string Message, object Data)
        {
            this.Code = Code;
            this.Message = Message;
            this.Data = Data;
        }
    }
}
