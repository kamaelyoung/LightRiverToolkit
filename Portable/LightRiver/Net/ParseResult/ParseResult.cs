using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class ParseResult<T>
    {
        public int SerialNo { get; protected set; }

        public bool IsSuccess { get; protected set; }

        public ParseError Error { get; protected set; }

        public T Content { get; private set; }

        public ParseResult(T result)
            : this(-1, true, result, null)
        {
        }

        public ParseResult(ParseError error)
            : this(-1, false, default(T), error)
        {
        }

        public ParseResult(int serialNo, T content)
            : this(serialNo, true, content, null)
        {
        }

        public ParseResult(int serialNo, ParseError error)
            : this(serialNo, false, default(T), error)
        {
        }

        public ParseResult(int serialNo, bool isSuccess, T content, ParseError error)
        {
            SerialNo = serialNo;
            IsSuccess = isSuccess;
            Error = error;
            Content = content;
        }
    }
}
