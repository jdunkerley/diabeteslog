using System;

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
}
