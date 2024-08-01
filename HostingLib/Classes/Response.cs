using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Classes
{
    public class Response
    {
        public Responses ResponseType { get; set; }
        public Payloads PayloadType { get; set; }
        public string? Payload { get; set; } = null;

        public Response(Responses responseType, Payloads payloadType, string payload) 
        {
            ResponseType = responseType;
            PayloadType = payloadType;
            Payload = payload;
        }

        public Response() { }
    }

    public enum Responses
    {
        Success, Fail, Pending
    }
}
