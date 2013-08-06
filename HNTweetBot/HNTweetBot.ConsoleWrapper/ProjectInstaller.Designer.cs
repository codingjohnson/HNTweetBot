namespace HNTweetBot.ConsoleWrapper
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.HNTweetBotServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.HNTweetBotServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // HNTweetBotServiceProcessInstaller
            // 
            this.HNTweetBotServiceProcessInstaller.Password = null;
            this.HNTweetBotServiceProcessInstaller.Username = null;
            // 
            // HNTweetBotServiceInstaller
            // 
            this.HNTweetBotServiceInstaller.ServiceName = "HNTweetBotService";
            this.HNTweetBotServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.HNTweetBotServiceProcessInstaller,
            this.HNTweetBotServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller HNTweetBotServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller HNTweetBotServiceInstaller;
    }
}