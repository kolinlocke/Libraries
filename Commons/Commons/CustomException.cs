using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable()]
    public class CustomException : Exception, ISerializable
    {
        public CustomException() { }

        public CustomException(String Message) : base(Message) { }

        public CustomException(String Message, Exception Ex) : base(Message, Ex) { }

        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable()]
    public class SimpleException
    {
        public String Message { get; set; }
        public String StackTrace { get; set; }
        public SimpleException InnerException { get; set; }

        public SimpleException() { }

        public SimpleException(String Message, String StackTrace)
        {
            this.Message = Message;
            this.StackTrace = StackTrace;
        }

        public SimpleException(Exception Ex)
        {            
            this.Message = Ex.Message;
            this.StackTrace = Ex.StackTrace;

            if (Ex.InnerException != null)
            { ConvertFromException(this, Ex.InnerException); }
        }

        public static SimpleException ConvertFromException(Exception Ex)
        {
            SimpleException SimpleEx = new SimpleException(Ex.Message, Ex.StackTrace);
            if (Ex.InnerException != null)
            { ConvertFromException(SimpleEx, Ex.InnerException); }

            return SimpleEx;
        }

        static void ConvertFromException(SimpleException SimpleEx, Exception Ex)
        {
            SimpleEx.InnerException = new SimpleException(Ex.Message, Ex.StackTrace);
            if (Ex.InnerException != null)
            { ConvertFromException(SimpleEx.InnerException, Ex.InnerException); }
        }

    }
}
