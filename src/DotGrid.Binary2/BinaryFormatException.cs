using System;

namespace DotGrid.Binary2
{
    public class BinaryFormatException : Exception
    {
        public BinaryFormatException()
        {
        }

        public BinaryFormatException(string message) : base(message)
        {
        }

        public BinaryFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}