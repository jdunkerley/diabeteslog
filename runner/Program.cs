using System;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace runner
{
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

                System.Environment.SetEnvironmentVariable("_X_AMZN_TRACE_ID", invocation.Headers.Where(x => x.Key == "Lambda-Runtime-Trace-Id").FirstOrDefault().Value.FirstOrDefault());
                var context = new Context(invocation.Headers);
                var content = await invocation.Content.ReadAsStringAsync();

                // Run Handler
                try 
                {
                    var task = method.Invoke(null, new object[] {content, context}) as Task;
                    task.Start();
                    await task;

                    // Execute child process...
                    // Create Isolated app domain
)

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
