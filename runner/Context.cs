using System;
using System.Linq;
using Amazon.Lambda.Core;

namespace runner
{
    internal class Context : ILambdaContext 
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
}
