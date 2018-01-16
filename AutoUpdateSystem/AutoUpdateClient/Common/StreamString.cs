using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    internal class StreamString
    {
        private readonly Stream _ioStream;
        private readonly Encoding _streamEncoding = Encoding.UTF8;

        public StreamString(Stream ioStream)
        {
            _ioStream = ioStream;
        }

        public string ReadString()
        {
            var lenBytes = new byte[4];
            var n = _ioStream.Read(lenBytes, 0, lenBytes.Length);
            if (n < 4)
                return null;
            var len = BitConverter.ToInt32(lenBytes, 0);
            var inBuffer = new byte[len];
            _ioStream.Read(inBuffer, 0, len);
            return _streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            var outBuffer = _streamEncoding.GetBytes(outString);
            var len = outBuffer.Length;
            var lenBytes = BitConverter.GetBytes(len);
            _ioStream.Write(lenBytes, 0, lenBytes.Length);
            _ioStream.Write(outBuffer, 0, len);
            _ioStream.Flush();

            return lenBytes.Length + outBuffer.Length;
        }
    }
}
