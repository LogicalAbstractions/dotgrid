using System;

namespace DotGrid.Binary
{
    public class BinaryFormatException : Exception 
    {
        public BinaryFormatException(string message) 
            : base(message)
        {
        }

        public BinaryFormatException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}