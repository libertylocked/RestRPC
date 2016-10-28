using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestRPC.Framework.Messages.Inputs;
using System;

namespace RestRPC.Framework.Serialization
{
    class InMessageSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(InMessage).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var header = (string)jObject["Header"];

            InMessage inMsg = null;
            switch (header)
            {
                case "":
                    inMsg = new RrpcInput(jObject["Data"].ToObject<RequestObject>());
                break;

                default:
                    // TODO: Should return a response with an error object
                    throw new FormatException("Unrecognized header format");
            }

            return inMsg;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // We never need to serialize an InMessage
            throw new NotImplementedException();
        }
    }
}
