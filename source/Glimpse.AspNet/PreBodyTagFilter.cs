using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Glimpse.Core.Extensibility;

namespace Glimpse.AspNet
{
    public class PreBodyTagFilter : Stream
    {
        public PreBodyTagFilter(string htmlSnippet, Stream outputStream, Encoding contentEncoding, ILogger logger)
        {
            HtmlSnippet = htmlSnippet + "</body>";
            OutputStream = outputStream;
            ContentEncoding = contentEncoding;
            BodyEnd = new Regex("</body>", RegexOptions.Compiled | RegexOptions.Multiline);
            Logger = logger;
        }

        public override bool CanRead
        {
            get { return OutputStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return OutputStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return OutputStream.CanWrite; }
        }

        public override long Length
        {
            get { return OutputStream.Length; }
        }

        public override long Position
        {
            get { return OutputStream.Position; }
            set { OutputStream.Position = value; }
        }

        private ILogger Logger { get; set; }

        private string HtmlSnippet { get; set; }

        private Stream OutputStream { get; set; }

        private Encoding ContentEncoding { get; set; }

        private Regex BodyEnd { get; set; }

        public override void Close()
        {
            OutputStream.Close();
        }

        public override void Flush()
        {
            OutputStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return OutputStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            OutputStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return OutputStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string contentInBuffer = ContentEncoding.GetString(buffer);

            if (BodyEnd.IsMatch(contentInBuffer))
            {
                string bodyCloseWithScript = BodyEnd.Replace(contentInBuffer, HtmlSnippet);

                byte[] outputBuffer = ContentEncoding.GetBytes(bodyCloseWithScript);

                OutputStream.Write(outputBuffer, 0, outputBuffer.Length);
            }
            else
            {
                Logger.Warn("Unable to locate '</body>' with content encoding '{0}'. Response may be compressed.", ContentEncoding.EncodingName);
                OutputStream.Write(buffer, offset, count);
            }
        }
    }
}