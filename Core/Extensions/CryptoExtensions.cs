using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace CKAN.Extensions
{
    /// <summary>
    /// Extensions for cryptographic operations
    /// </summary>
    public static class CryptoExtensions
    {

        /// <summary>
        /// A version of ComputeHash with progress updates.
        /// Based on https://stackoverflow.com/a/53966139 with lots of cleaning up.
        /// </summary>
        /// <param name="hashAlgo">The crypto object to use for the operation, like SHA1, SHA256, etc.</param>
        /// <param name="stream">Input stream to hash</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <param name="cancelToken">A cancellation token that can be used to abort the hash</param>
        /// <returns>The requested hash of the input stream</returns>
        public static byte[] ComputeHash(this HashAlgorithm hashAlgo,
                                         Stream             stream,
                                         IProgress<int>     progress,
                                         CancellationToken  cancelToken = default)
        {
            const int bufSize = 1024 * 1024;
            var buffer = new byte[bufSize];
            long totalBytesRead = 0;
            while (true)
            {
                var bytesRead = stream.Read(buffer, 0, bufSize);
                cancelToken.ThrowIfCancellationRequested();

                if (bytesRead < bufSize)
                {
                    // Done!
                    hashAlgo.TransformFinalBlock(buffer, 0, bytesRead);
                    progress.Report(100);
                    break;
                }
                else
                {
                    hashAlgo.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                totalBytesRead += bytesRead;
                progress.Report((int)(100 * totalBytesRead / stream.Length));
            }
            return hashAlgo.Hash;
        }
    }
}
