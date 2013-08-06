using System;

namespace HNTweetBot.Console
{
    public static class Program
    {
        public static void Main(String[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "auth")
                Service.TwitterAuth();
            else if (args != null && args.Length > 0 && args[0] == "test")
                Service.InnerWorker();
            else
                Service.Start();
        }

        public static void Stop() { Service.Running = false; }
    }
}