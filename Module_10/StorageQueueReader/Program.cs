using Azure;
using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;

namespace StorageQueueReader
{
    class Program
    {
        static string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=psqueueueueueues;AccountKey=SI0VBVPB4vyfQyHAK8V6u5gBojOlakubd/DN57hC1HbFa+ldbMQIEoDeKmedl4tCv8bdmj7onJCu+ASt5SiFKA==;EndpointSuffix=core.windows.net";
        static string QueueName = "kassa";
        static string sasToken = "sv=2022-11-02&ss=q&srt=so&sp=rdlp&se=2023-05-12T16:00:24Z&st=2023-05-12T08:00:24Z&spr=https&sig=3swYtNBjxj%2BXFWYVotou9syoUWqUptK9sZFts6tqMmI%3D";
        static Uri queueUri = new Uri("https://psstoring.queue.core.windows.net/myqueue");

        static async Task Main(string[] args)
        {
            var t1 = ReadFromQueueAsync(true);
            var t2 = ReadFromQueueAsync();
            await Task.WhenAll(t1, t2);

            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }

        private static async Task ReadFromQueueAsync(bool fout = false)
        {
            var cnt = 0;
            var creep = new AzureSasCredential(sasToken);
            var client = new QueueClient(queueUri, creep);
            //var client = new QueueClient(ConnectionString, QueueName);
            do
            {
                // 10 seconds "lease" time
                try
                {
                    var response = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                   
                    if (response.Value == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }
                   // await Task.Delay(200);
                    var msg = response.Value;
                    Console.WriteLine($"[{++cnt}] {msg.Body}");

                    // We need more time to process
                    //await client.UpdateMessageAsync(msg.MessageId, msg.PopReceipt, msg.Body, TimeSpan.FromSeconds(30));
                    // Don't forget to remove
                    if (fout) throw new Exception("oops");
                    await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            while (true);
        }
    }
}
