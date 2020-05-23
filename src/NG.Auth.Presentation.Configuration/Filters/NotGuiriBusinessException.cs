using System;

namespace NG.Auth.Presentation.Configuration.Filters
{
    public class NotGuiriBusinessException : Exception
    {
        public int ErrorCode { get; set; }

        public NotGuiriBusinessException(string message,
                                        int errorCode = 0) : base(message)
        {
            ErrorCode = errorCode;
        }

        public NotGuiriBusinessException(string message,
                                        Exception innerException,
                                        int errorCode = 0) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}