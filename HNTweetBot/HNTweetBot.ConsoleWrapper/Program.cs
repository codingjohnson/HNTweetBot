using System.ServiceProcess;

namespace HNTweetBot.ConsoleWrapper
{
    public static class Program
    {
        public static void Main()
        {
            var ServicesToRun = new ServiceBase[] { new HNTweetBotService() };
            ServiceBase.Run(ServicesToRun);
        }
    }
}