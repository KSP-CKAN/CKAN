using System;
using System.Collections.Generic;

using ChinhDo.Transactions.FileManager;

namespace CKAN
{
    public partial class NetAsyncDownloader
    {
        public class DownloadTarget
        {
            public List<Uri> urls     { get; private set; }
            public string    filename { get; private set; }
            public long      size     { get; set; }
            public string    mimeType { get; private set; }

            public DownloadTarget(List<Uri> urls,
                                  string    filename = null,
                                  long      size     = 0,
                                  string    mimeType = "")
            {
                var FileTransaction = new TxFileManager();

                this.urls     = urls;
                this.filename = string.IsNullOrEmpty(filename)
                                    ? FileTransaction.GetTempFileName()
                                    : filename;
                this.size     = size;
                this.mimeType = mimeType;
            }

            public DownloadTarget(Uri    url,
                                  string filename = null,
                                  long   size     = 0,
                                  string mimeType = "")
                : this(new List<Uri> { url },
                       filename, size, mimeType)
            {
            }
        }
    }
}
