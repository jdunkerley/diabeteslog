using System;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace runner
{
    internal class Logger : Amazon.Lambda.Core.ILambdaLogger
    {
        public void Log(string message)
        {
            Console.Write(message);
        }

        public void LogLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class Context : Amazon.Lambda.Core.ILambdaContext 
    {
        DateTime endTime;

        public Context(System.Net.Http.Headers.HttpResponseHeaders headers) {
            this.AwsRequestId = headers.Where(x => x.Key == "Lambda-Runtime-Aws-Request-Id").FirstOrDefault().Value.FirstOrDefault();
            this.InvokedFunctionArn = headers.Where(x => x.Key == "Lambda-Runtime-Invoked-Function-Arn").FirstOrDefault().Value.FirstOrDefault();
            this.endTime = UnixTimeStampToDateTime(long.Parse(headers.Where(x => x.Key == "Lambda-Runtime-Deadline-Ms").FirstOrDefault().Value.FirstOrDefault()));
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
            => new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();

        public string AwsRequestId { get; }

        public string FunctionName => System.Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME");

        public string FunctionVersion => System.Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_VERSION");

        public string InvokedFunctionArn { get; }

        public int MemoryLimitInMB => int.Parse(System.Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_MEMORY_SIZE"));

        public TimeSpan RemainingTime => this.endTime.Subtract(DateTime.Now);

        public string LogGroupName => System.Environment.GetEnvironmentVariable("AWS_LAMBDA_LOG_GROUP_NAME");

        public string LogStreamName => System.Environment.GetEnvironmentVariable("AWS_LAMBDA_LOG_STREAM_NAME");

        public ILambdaLogger Logger => new Logger();

        public IClientContext ClientContext => throw new NotImplementedException();

        public ICognitoIdentity Identity => throw new NotImplementedException();
    }

    class Program
    {
        public static System.Reflection.Assembly LoadFile(string path)
        {
            System.Reflection.Assembly ass = null;
            ass = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            return ass;
        }

        static async Task Main(string[] args)
        {
            var assembly = LoadFile(args[0]);
            var type = assembly.GetType(args[1]);
            var method = type.GetMethod(args[2]);

            var root = System.Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API");
            var nextUrl = $"http://{root}/2018-06-01/runtime/invocation/next";
            var client = new System.Net.Http.HttpClient();

            while (true) 
            {
                var invocation = await client.GetAsync(nextUrl);

                //var requestId = invocation.Headers.Where(x => x.Key == "Lambda-Runtime-Aws-Request-Id").FirstOrDefault().Value;
                var context = new Context(invocation.Headers);
                var content = await invocation.Content.ReadAsStringAsync();

                // Run Handler
                try 
                {
                    var task = method.Invoke(null, new object[] {content, context}) as Task;
                    task.Start();
                    await task;

                    System.Net.Http.HttpContent response = null;
                    if (task is Task<string>) {
                        response = new System.Net.Http.StringContent(((Task<string>)task).Result);
                    } else {
                        response = new System.Net.Http.StringContent("SUCCESS");
                    }

                    var postUrl = $"http://{root}/2018-06-01/runtime/invocation/{context.AwsRequestId}/response";
                    await client.PostAsync(postUrl, response);
                }
                catch (Exception e) {
                    var postUrl = $"http://{root}/2018-06-01/runtime/invocation/{context.AwsRequestId}/error";
                    await client.PostAsync(postUrl, new System.Net.Http.StringContent("{\"errorMessage\" : \"" + e.Message + "\", \"errorType\" : \"" + e.GetType().FullName + "\"}"));
                }
            }
        }
    }
}
