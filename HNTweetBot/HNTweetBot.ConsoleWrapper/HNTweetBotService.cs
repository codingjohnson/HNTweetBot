using System.ServiceProcess;

namespace HNTweetBot.ConsoleWrapper
{
    public partial class HNTweetBotService : ServiceBase
    {
        public HNTweetBotService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.Program.Main(args);
        }

        protected override void OnStop()
        {
            Console.Program.Stop();
        }
    }
}