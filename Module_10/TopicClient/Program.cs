﻿using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Threading.Tasks;

namespace TopicClient
{
    class Program
    {
        static string EndPoint = "psbusje.servicebus.windows.net";
        static (string Name, string Key) SasKeyManager = ("RootManageSharedAccessKey", "od1eTMFo6zCrw9YSdgbw5Xj0wvTcOxHZ4+ASbLBS+B4=");
        static (string Name, string Key) SasKeyWriter = ("Schrijvert", "ZfxnkHEDWhzW3kZZCQ8OU2D8eeuffayP1+ASbNgLJJ4=");
        static string TopicName = "onderwerp";

        static async Task Main(string[] args)
        {
            //await ManageTopicAsync();
            //await ManageSubscriptionsAsync();

            await WriteToTopicAsync();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
        private static async Task ManageTopicAsync()
        {
            var cred = new AzureNamedKeyCredential(SasKeyManager.Name, SasKeyManager.Key);
            var client = new ServiceBusAdministrationClient(EndPoint, cred);
            var topicInfo = new CreateTopicOptions(TopicName);

            var response = await client.CreateTopicAsync(topicInfo);
            if (response != null)
            {
                Console.WriteLine("Topic created!");
            }
        }
        private static async Task ManageSubscriptionsAsync()
        {
            var cred = new AzureNamedKeyCredential(SasKeyManager.Name, SasKeyManager.Key);
            var client = new ServiceBusAdministrationClient(EndPoint, cred);
            
            var salesInfo = new CreateSubscriptionOptions(TopicName, "Sales");
            var salesRule = new CreateRuleOptions("sales", new SqlRuleFilter("price > 100"));
            await client.CreateSubscriptionAsync(salesInfo, salesRule);

            var supportInfo = new CreateSubscriptionOptions(TopicName, "Support");
            var supportRule = new CreateRuleOptions("support", new TrueRuleFilter());
            await client.CreateSubscriptionAsync(supportInfo, supportRule);

            var ceoInfo = new CreateSubscriptionOptions(TopicName, "Management");
            var ceoRule = new CreateRuleOptions("management", new SqlRuleFilter("price > 1000"));
            await client.CreateSubscriptionAsync(ceoInfo, ceoRule);

            Console.WriteLine("Subscriptions created");

        }

        private static async Task WriteToTopicAsync()
        {
            var cred = new AzureNamedKeyCredential(SasKeyWriter.Name, SasKeyWriter.Key);
            var client = new ServiceBusClient(EndPoint, cred);
            var sender = client.CreateSender(TopicName);

            Random rnd = new Random(); 
            for(int i = 0; i < 100; i++)
            {
                var price = rnd.Next(10, 2000);
                var msg = new ServiceBusMessage(BinaryData.FromString($"Sold (${price})"));
                msg.ApplicationProperties.Add("price", price);
                msg.ContentType = "string";
                await sender.SendMessageAsync(msg);
            }
            
        }
    }
}
