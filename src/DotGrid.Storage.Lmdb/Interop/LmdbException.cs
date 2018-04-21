using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace DotGrid.Storage.Lmdb.Interop
{
    public class LmdbException : Exception
    {
        private static readonly IReadOnlyDictionary<LmdbErrorCode, Func<LmdbErrorCode, LmdbException,Exception>> exceptionTranslators
            =
            new Dictionary<LmdbErrorCode, Func<LmdbErrorCode,LmdbException, Exception>>()
            {
                {LmdbErrorCode.EFileExists,((code, exception) => new IOException("Database files already present",exception))}
            };
        
        public LmdbException()
        {
        }

        public LmdbException(string message) : base(message)
        {
        }

        public LmdbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static unsafe void ThrowOnError(int errorCode)
        {
            if (errorCode != 0)
            {
                var errorCodeType = (LmdbErrorCode) errorCode;
                var sourceException = new LmdbException($"ErrorCode: {errorCode}:{errorCodeType.ToString()}: {errorCodeType.ToErrorMessage()}");

                if (exceptionTranslators.TryGetValue(errorCodeType, out var translator))
                {
                    throw translator.Invoke(errorCodeType, sourceException);
                }
                else
                {
                    throw sourceException;
                }
            }
        }

        private static unsafe string BytesToString(byte* ptr)
        {
            var strLength = StrLen(ptr);

            return Encoding.ASCII.GetString(ptr, strLength);
        }
        
        private static unsafe int StrLen(byte* ptr)
        {
            int index = 0;

            while (ptr[index] != 0)
            {
                index++;
            }

            return index;
        }
    }
}