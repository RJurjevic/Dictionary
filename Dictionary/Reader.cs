using System;
using System.IO;
using System.Text;

namespace Webster
{
    /// <summary>
    /// Thin wrapper around a StreamReader for the Gutenberg dictionary text file (29765-8.txt).
    ///
    /// Important: the file encoding is ISO-8859-1 (Latin-1), as stated in the Gutenberg header.
    /// </summary>
    class Reader : IDisposable
    {
        private static readonly Encoding WebsterEncoding = Encoding.GetEncoding("ISO-8859-1");

        private readonly FileStream _filestream;
        private readonly StreamReader _reader;

        private bool _disposed;

        public Reader()
        {
            // Original behavior: open 29765-8.txt from the current working directory.
            _filestream = new FileStream(
                @"29765-8.txt",
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            // Original behavior: no BOM detection, 1KB buffer.
            _reader = new StreamReader(_filestream, WebsterEncoding, detectEncodingFromByteOrderMarks: false, bufferSize: 1024);
        }

        /// <summary>
        /// Rewind the stream back to the start of the file.
        /// Because StreamReader buffers data internally, we must also discard its buffer.
        /// </summary>
        public void Rewind()
        {
            ThrowIfDisposed();

            _filestream.Position = 0;
            _reader.DiscardBufferedData();
        }

        /// <summary>Read the next line (or null at EOF).</summary>
        public string ReadLine()
        {
            ThrowIfDisposed();
            return _reader.ReadLine();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // StreamReader will dispose the underlying stream as well,
            // but we keep it explicit/ordered for clarity.
            _reader.Dispose();
            _filestream.Dispose();

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Reader));
        }
    }
}
