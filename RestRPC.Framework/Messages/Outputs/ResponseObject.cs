using Newtonsoft.Json;
using RestRPC.Framework.Messages.Inputs;

namespace RestRPC.Framework.Messages.Outputs
{
    class ResponseObject
    {
        [JsonProperty]
        public object Result { get; private set; }

        [JsonProperty]
        public ErrorObject Error { get; private set; }

        [JsonProperty]
        public string ID { get; private set; }

        [JsonProperty]
        public string RID { get; private set; }

        public ResponseObject(object Result, ErrorObject Error, RequestObject RequestObject)
        {
            this.Result = Result;
            this.Error = Error;
            this.ID = RequestObject?.ID;
            this.RID = RequestObject.RID;
        }
    }
}
