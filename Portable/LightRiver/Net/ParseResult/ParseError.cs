using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public sealed class ParseError
    {
        /// <summary>
        /// Error Code
        /// </summary>
        public int Code { get; private set;}

        /// <summary>
        /// Error Message
        /// </summary>
        public string Message { get; private set; }

        public ParseError(int code)
            : this(code, null)
        {
        }

        public ParseError(string message)
            : this(-1, message)
        {
        }

        public ParseError(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
