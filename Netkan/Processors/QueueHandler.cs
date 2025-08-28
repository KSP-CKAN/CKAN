using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

using Amazon.SQS;
using Amazon.SQS.Model;
using log4net;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

using CKAN.Versioning;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Model;
using CKAN.Extensions;
using CKAN.Games;

namespace CKAN.NetKAN.Processors
{
    [ExcludeFromCodeCoverage]
    public class QueueHandler
    {
        public QueueHandler(string  inputQueueName,
                            string  outputQueueName,
                            string? cacheDir,
                            string? outputDir,
                            bool    overwriteCache,
                            string? githubToken,
                            string? gitlabToken,
                            string? userAgent,
                            bool?   prerelease,
                            IGame   game)
        {
            this.outputDir = outputDir;
            warningAppender = GetQueueLogAppender();
            (LogManager.GetRepository() as Hierarchy)?.Root.AddAppender(warningAppender);
            this.game = game;

            log.Debug("Initializing SQS queue handler");
            inflator = new Inflator(cacheDir, overwriteCache, githubToken, gitlabToken, userAgent, prerelease, game);

            inputQueueURL  = getQueueUrl(inputQueueName);
            outputQueueURL = getQueueUrl(outputQueueName);
            log.DebugFormat("Queue URLs: {0}, {1}", inputQueueURL, outputQueueURL);
        }

        ~QueueHandler()
        {
            (LogManager.GetRepository() as Hierarchy)?.Root.RemoveAppender(warningAppender);
        }

        public void Process()
        {
            while (true)
            {
                // 10 messages, 30 minutes to allow time to handle them all
                handleMessages(inputQueueURL, 10, 30);
            }
        }

        private static QueueAppender GetQueueLogAppender()
        {
            var qap = new QueueAppender()
            {
                Name = "QueueAppender",
            };
            qap.AddFilter(new LevelMatchFilter()
            {
                LevelToMatch  = Level.Warn,
                AcceptOnMatch = true,
            });
            qap.AddFilter(new DenyAllFilter());
            return qap;
        }

        private string getQueueUrl(string name)
        {
            log.DebugFormat("Looking up URL for queue {0}", name);
            return client.GetQueueUrlAsync(new GetQueueUrlRequest() { QueueName = name }).Result.QueueUrl;
        }

        private void handleMessages(string url, int howMany, int timeoutMinutes)
        {
            log.DebugFormat("Looking for messages from {0}", url);
            var resp = client.ReceiveMessageAsync(new ReceiveMessageRequest()
            {
                QueueUrl              = url,
                MaxNumberOfMessages   = howMany,
                VisibilityTimeout     = (int)TimeSpan.FromMinutes(timeoutMinutes).TotalSeconds,
                MessageAttributeNames = new List<string>() { "All" },
            }).Result;
            if (resp.Messages.Count == 0)
            {
                log.Debug("No metadata in queue");
            }
            else
            {
                try
                {
                    // Reset the ids between batches
                    responseId = 0;
                    // Might be >10 if Releases>1
                    var responses = resp.Messages.SelectMany(Inflate).ToList();
                    for (int i = 0; i < responses.Count; i += howMany)
                    {
                        client.SendMessageBatchAsync(new SendMessageBatchRequest()
                        {
                            QueueUrl = outputQueueURL,
                            Entries  = responses.GetRange(i, Math.Min(howMany, responses.Count - i)),
                        });
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Send failed: {0}\r\n{1}", e.Message, e.StackTrace);
                }
                try
                {
                    log.Debug("Deleting messages");
                    client.DeleteMessageBatchAsync(new DeleteMessageBatchRequest()
                    {
                        QueueUrl = url,
                        Entries  = resp.Messages.Select(Delete).ToList(),
                    });
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Delete failed: {0}\r\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        private IEnumerable<SendMessageBatchRequestEntry> Inflate(Message msg)
        {
            log.DebugFormat("Metadata returned: {0}", msg.Body);
            var netkans = YamlExtensions.Parse(msg.Body)
                                        .Select(ymap => new Metadata(ymap))
                                        .ToArray();

            int releases = 1;
            if (msg.MessageAttributes.TryGetValue("Releases", out MessageAttributeValue? releasesAttr))
            {
                releases = int.Parse(releasesAttr.StringValue);
            }

            ModuleVersion? highVer = null;
            if (msg.MessageAttributes.TryGetValue("HighestVersion", out MessageAttributeValue? highVerAttr))
            {
                highVer = new ModuleVersion(highVerAttr.StringValue);
            }

            ModuleVersion? highVerPre = null;
            if (msg.MessageAttributes.TryGetValue("HighestVersionPrerelease", out MessageAttributeValue? highVerPreAttr))
            {
                highVerPre = new ModuleVersion(highVerPreAttr.StringValue);
            }

            log.InfoFormat("Inflating {0}", netkans.First().Identifier);
            IEnumerable<Metadata>? ckans = null;
            bool    caught        = false;
            string? caughtMessage = null;
            var     opts          = new TransformOptions(releases,
                                                         null,
                                                         highVer,
                                                         highVerPre,
                                                         netkans.First().Staged,
                                                         netkans.First().StagingReason);
            try
            {
                ckans = inflator.Inflate($"{netkans.First().Identifier}.netkan", netkans, opts)
                    .ToArray();
            }
            catch (Exception e)
            {
                log.InfoFormat("Inflation failed, sending error: {0}", e.Message);
                // If you do this the sensible way, the C# compiler throws:
                // error CS1631: Cannot yield a value in the body of a catch clause
                caught        = true;
                caughtMessage = e switch
                {
                    // Short error message for problems with mods
                    Kraken k => k.Message,
                    // Full message with stack trace for problems with code
                    _ => e.ToString(),
                };
                if (e is RequestThrottledKraken { retryTime: DateTime dt })
                {
                    // Let the API credits recharge
                    var span = dt.Subtract(DateTime.UtcNow);
                    caughtMessage += $@"; sleeping {Math.Floor(span.TotalMinutes):0}m {span.Seconds}s...";
                    Thread.Sleep(span);
                }
            }
            if (caught)
            {
                yield return inflationMessage(null, netkans.First(), opts, false, outputDir, caughtMessage);
            }
            if (ckans != null)
            {
                foreach (Metadata ckan in ckans)
                {
                    log.InfoFormat("Sending {0}-{1}", ckan.Identifier, ckan.Version);
                    yield return inflationMessage(ckan, netkans.First(), opts, true, outputDir);
                }
            }
        }

        private SendMessageBatchRequestEntry inflationMessage(Metadata?        ckan,
                                                              Metadata         netkan,
                                                              TransformOptions opts,
                                                              bool             success,
                                                              string?          outDir,
                                                              string?          err = null)
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
                        StringValue = opts.Staged.ToString()
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
                    "GameId",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = game.ShortName
                    }
                }
            };
            if (ckan != null)
            {
                attribs.Add(
                    "FileName",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = Program.CkanFileName(outDir, ckan.AllJson)
                    }
                );
            }
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
            if (warningAppender.Warnings.Count != 0)
            {
                attribs.Add(
                    "WarningMessages",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = string.Join("\r\n", warningAppender.Warnings),
                    }
                );
                warningAppender.Warnings.Clear();
            }
            if (opts.Staged && opts.StagingReasons.Count > 0)
            {
                attribs.Add(
                    "StagingReason",
                    new MessageAttributeValue()
                    {
                        DataType    = "String",
                        StringValue = string.Join("\r\n\r\n", opts.StagingReasons),
                    }
                );
            }
            return new SendMessageBatchRequestEntry()
            {
                Id                     = (responseId++).ToString(),
                MessageGroupId         = "1",
                MessageDeduplicationId = Path.GetRandomFileName(),
                MessageBody            = serializeCkan(ckan),
                MessageAttributes      = attribs,
            };
        }

        internal static string serializeCkan(Metadata? ckan)
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
                serializer.Serialize(writer, ckan.AllJson);
            }
            return sw + Environment.NewLine;
        }

        private DeleteMessageBatchRequestEntry Delete(Message msg)
        {
            return new DeleteMessageBatchRequestEntry()
            {
                Id            = msg.MessageId,
                ReceiptHandle = msg.ReceiptHandle,
            };
        }

        private readonly string?         outputDir;
        private readonly IGame           game;
        private readonly Inflator        inflator;
        private readonly AmazonSQSClient client = new AmazonSQSClient();

        private readonly string inputQueueURL;
        private readonly string outputQueueURL;

        private int responseId = 0;

        private static readonly ILog log = LogManager.GetLogger(typeof(QueueHandler));
        private readonly QueueAppender warningAppender;
    }
}
