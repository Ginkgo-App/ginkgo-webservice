using System;
using System.Runtime.Serialization;

namespace APICore.Models
{
    public class ExceptionWithMessage: Exception
    {
        public ExceptionWithMessage()
        {
        }

        protected ExceptionWithMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExceptionWithMessage(string? message) : base(message)
        {
        }

        public ExceptionWithMessage(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}