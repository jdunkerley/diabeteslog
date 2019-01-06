using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

namespace DiabetesLog
{
    public static class Handler
    {
        public static async Task EntryPoint(ILambdaContext context) 
        {
            context.Logger.Log($"Started Invocation - {context.AwsRequestId }");
            await Task.Delay(100);
            context.Logger.Log($"Finished Invocation - {context.AwsRequestId }");
        }
    }
}
