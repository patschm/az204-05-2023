using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LiveDemo
{
    public static class Duurzaam
    {
        [FunctionName("Duurzaam")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<Task<string>>();

            // Replace "hello" with the name of your Durable Activity Function.
            var r1 = context.CallActivityAsync<string>(nameof(SayHello), "Tokyo");
            outputs.Add(r1);
            //bool isOk = await context.WaitForExternalEvent<bool>("ok");
            var r2 = context.CallActivityAsync<string>(nameof(SayHello), "Seattle");
            outputs.Add(r2);
            var r3 = context.CallActivityAsync<string>(nameof(SayHello), "London");
            outputs.Add(r3);

            await Task.WhenAll(outputs);
            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs.Select(o=>o.Result).ToList();
        }

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }

        [FunctionName("Duurzaam_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Duurzaam", "hallo");

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}