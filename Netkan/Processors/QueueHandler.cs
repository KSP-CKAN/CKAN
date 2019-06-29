using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using log4net;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Processors
{
    public class QueueHandler
    {
        public QueueHandler(string inputQueueName, string outputQueueName, string cacheDir, bool overwriteCache, string githubToken, bool prerelease)
        {
            log.Debug("Initializing SQS queue handler");
            inflator = new Inflator(cacheDir, overwriteCache, githubToken, prerelease);

            inputQueueURL  = getQueueUrl(inputQueueName);
            outputQueueURL = getQueueUrl(outputQueueName);
            log.DebugFormat("Queue URLs: {0}, {1}", inputQueueURL, outputQueueURL);
        }

        public void Process()
        {
            while (true)
            {
                // 10 messages, 30 minutes to allow time to handle them all
                handleMessages(inputQueueURL, 10, 30);
            }
        }

        private string getQueueUrl(string name)
        {
            log.DebugFormat("Looking up URL for queue {0}", name);
            return client.GetQueueUrl(new GetQueueUrlRequest() { QueueName = name }).QueueUrl;
        }

        private void handleMessages(string url, int howMany, int timeoutMinutes)
        {
            log.DebugFormat("Looking for messages from {0}", url);
            var resp = client.ReceiveMessage(new ReceiveMessageRequest()
            {
                QueueUrl              = url,
                MaxNumberOfMessages   = howMany,
                VisibilityTimeout     = (int)TimeSpan.FromMinutes(timeoutMinutes).TotalSeconds,
                MessageAttributeNames = new List<string>() { "All" },
            });
            if (!resp.Messages.Any())
            {
                log.Debug("No metadata in queue");
            }
            else foreach (var msg in resp.Messages)
            {
                log.DebugFormat("Metadata returned: {0}", msg.Body);
                var netkan = new Metadata(JObject.Parse(msg.Body));

                int releases = 1;
                MessageAttributeValue releasesAttr;
                if (msg.MessageAttributes.TryGetValue("Releases", out releasesAttr))
                {
                    releases = int.Parse(releasesAttr.StringValue);
                }

                log.InfoFormat("Inflating {0}", netkan.Identifier);
                IEnumerable<Metadata> ckans = null;
                try
                {
                    ckans = inflator.Inflate($"{netkan.Identifier}.netkan", netkan, releases);
                }
                catch (Exception e)
                {
                    e = e.GetBaseException() ?? e;
                    log.InfoFormat("Inflation failed, sending error: {0}", e.Message);
                    sendCkan(null, netkan, false, e.Message);
                }
                if (ckans != null)
                {
                    foreach (Metadata ckan in ckans)
                    {
                        log.InfoFormat("Sending {0}-{1}", ckan.Identifier, ckan.Version);
                        sendCkan(ckan, netkan, true);
                    }
                }

                // Delete message after processing
                log.Debug("Deleting message");
                client.DeleteMessage(new DeleteMessageRequest()
                {
                    QueueUrl      = url,
                    ReceiptHandle = msg.ReceiptHandle,
                });
            }
        }

        private void sendCkan(Metadata ckan, Metadata netkan, bool success, string err = null)
        {
            var attribs = new Dictionary<string, MessageAttributeValue>()
            {
                {
                    "ModIdentifier",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = netkan.Identifier
                    }
                },
                {
                    "Staged",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = netkan.Staged.ToString()
                    }
                },
                {
                    "Success",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = success.ToString()
                    }
                },
                {
                    "CheckTime",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture)
                    }
                },
                {
                    "FileName",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = Program.CkanFileName(ckan)
                    }
                }
            };
            if (!string.IsNullOrEmpty(err))
            {
                attribs.Add(
                    "ErrorMessage",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = err
                    }
                );
            }

            SendMessageRequest msg = new SendMessageRequest()
            {
                QueueUrl               = outputQueueURL,
                MessageGroupId         = "1",
                MessageDeduplicationId = Path.GetRandomFileName(),
                MessageBody            = serializeCkan(ckan),
                MessageAttributes      = attribs,
            };
            try
            {
                var resp = client.SendMessage(msg);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Send failed: {0}\r\n{1}", e.Message, e.StackTrace);
            }
        }

        internal static string serializeCkan(Metadata ckan)
        {
            if (ckan == null)
            {
                // SendMessage doesn't like empty bodies, so send an empty JSON object
                return "{}";
            }
            var sw = new StringWriter(new StringBuilder());
            using (var writer = new JsonTextWriter(sw)
                {
                    Formatting  = Formatting.Indented,
                    Indentation = 4,
                    IndentChar  = ' ',
                })
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, ckan.Json());
            }
            return sw + Environment.NewLine;
        }

        private Inflator        inflator;
        private AmazonSQSClient client = new AmazonSQSClient();

        private readonly string inputQueueURL;
        private readonly string outputQueueURL;

        private static readonly ILog log = LogManager.GetLogger(typeof(QueueHandler));
    }
}
