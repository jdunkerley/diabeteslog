using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

namespace DiabetesLog
{
    public static class Handler
    {
        public static Action<string> Log { get; private set; }

        public static async Task EntryPoint(ILambdaContext context) 
        {
            Log = (m) => {
                context.Logger.Log(m);
                System.Console.WriteLine(m);
            };

            Log($"Started Invocation - {context.AwsRequestId }");
            await Task.Delay(100);
            Log($"Version: {System.Environment.Version} from: {System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()}");
            Log($"Finished Invocation - {context.AwsRequestId}");
        }
    }
}
