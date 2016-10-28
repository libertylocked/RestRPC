using Newtonsoft.Json;
using RestRPC.Framework.Exceptions;
using RestRPC.Framework.Messages.Outputs;
using System;

namespace RestRPC.Framework.Messages.Inputs
{
    class RrpcInput : InMessage
    {
        [JsonConstructor]
        public RrpcInput(RequestObject RequestObject)
            : base("", RequestObject)
        { }

        public override string ToString()
        {
            return "Header: " + Header + ", Data: {" + ((RequestObject)Data).ToString() + "}";
        }

        internal override void Evaluate(RrpcComponent component)
        {
            Logger.Log("Executing " + ToString(), LogType.Debug);

            RequestObject requestObject = (RequestObject)Data;
            object retVal = null;
            RrpcException exception = null;
            try
            {
                retVal = component.PluginManager.Dispatch(requestObject.Method, requestObject.Params);
            }
            catch (RrpcException rrpcExc)
            {
                exception = rrpcExc;
            }
            catch (Exception exc)
            {
                exception = new InternalErrorException(exc);
            }

            if (exception != null)
            {
                Logger.Log(exception.ToString(), LogType.Error);
            }

            // If ID is null or empty, this is a notification, therefore do not respond
            // Otherwise, return the procedure result
            if (!string.IsNullOrEmpty(requestObject.ID))
            {
                var responseMessage = new RrpcResponse(new ResponseObject(retVal, exception?.ToErrorObject(), requestObject));
                component.EnqueueOutMessage(responseMessage);
            }
        }
    }
}
