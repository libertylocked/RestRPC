using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Inputs
{
    class RequestObject
    {
        public string Method { get; private set; }

        public object[] Params { get; private set; }

        public string ID { get; private set; }

        public string RID { get; private set; }

        [JsonConstructor]
        public RequestObject(string Method, object[] Params, string ID, string RID)
        {
            this.Method = Method;
            this.Params = Params;
            this.ID = ID;
            this.RID = RID;
        }

        public override string ToString()
        {
            return "Method: " + Method + ", Params: [" + 
                ((Params == null) ? "" : string.Join(",", Params)) +
                "], ID: " + ID + ", RID: " + RID;
        }
    }
}
