using HtmlAgilityPack;
using OpenBitly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using TweetSharp;

namespace HNTweetBot.Console
{
    public static class Service
    {
        public static Boolean Running { get; set; }
        private static String ServiceName { get { return "HNTweetBot"; } }
        private const String ConsumerKey = "xxx";
        private const String ConsumerSecret = "xxx";
        private static readonly Uri TargetUri = new Uri(@"https://news.ycombinator.com/news");

        private static String GetCommentsURL(HtmlNode node)
        {
            String toReturn = null;
            if (node != null && node.ParentNode != null && node.ParentNode.ParentNode != null && node.ParentNode.ParentNode.NextSibling != null)
            {
                var targetAnchor = node.ParentNode.ParentNode.NextSibling.SelectSingleNode(@"td/a[2]");
                if (targetAnchor != null)
                {
                    toReturn = targetAnchor.Attributes["href"].Value;
                    if (!String.IsNullOrEmpty(toReturn))
                        try
                        {
                            toReturn = (new Uri(TargetUri, toReturn)).AbsoluteUri;
                            toReturn = Shorten(toReturn);
                        }
                        catch (Exception)
                        { }
                }
            }

            if (!String.IsNullOrEmpty(toReturn)) toReturn = " Comments: " + toReturn;

            return toReturn;
        }

        public static void InnerWorker()
        {
            Log("Checking");
            var html = GetHtmlRaw();
            if (!String.IsNullOrEmpty(html))
            {
                var htmlDocument = AsHtml(html);
                if (htmlDocument != null)
                {
                    var anchorNodes = htmlDocument.DocumentNode.SelectNodes(@"//center/table/tr[3]/td/table/tr/td[3]/a");
                    if (anchorNodes == null)
                        anchorNodes = htmlDocument.DocumentNode.SelectNodes(@"//center/table/tr[4]/td/table/tr/td[3]/a");

                    if (anchorNodes != null)
                    {
                        var state = ReadState();
                        Log("Found " + anchorNodes.Count + " stories");

                        for (var i = 0; i < Math.Min(anchorNodes.Count, 5); i++)
                        {
                            var anchorNode = anchorNodes[i];

                            if (anchorNode != null)
                            {
                                var verbatim = anchorNode.InnerText;
                                var canonical = Canonicalise(verbatim);

                                if (!state.Contains(verbatim) && !state.Contains(canonical))
                                {
                                    Log("Going to Tweet: " + verbatim);
                                    var link = System.Web.HttpUtility.HtmlDecode(anchorNode.Attributes["href"].Value);

                                    if (!String.IsNullOrEmpty(link) && !state.Contains(link))
                                    {
                                        state.Add(link);

                                        if (!link.ToLowerInvariant().Trim().StartsWith("http")) link = "http://news.ycombinator.com" + ("/" + link).Replace(@"//", @"/");
                                        link = Shorten(link);

                                        Log("Upodating state");
                                        state.Add(canonical);

                                        verbatim = verbatim.Trim();
                                        var toTweet = (verbatim + " " + link);
                                        var comments = GetCommentsURL(anchorNode);

                                        if (toTweet.Length > 140)
                                        {
                                            verbatim = verbatim.Substring(0, (140 - link.Length) - 4);
                                            toTweet = verbatim + "... " + link;
                                        }
                                        else if (comments != null && toTweet.Length + comments.Length < 135)
                                        {
                                            toTweet += comments;
                                        }

                                        Log("At the point of Tweeting");
                                        if (!String.IsNullOrEmpty(toTweet)) Tweet(toTweet);
                                        else Log("Failed: No Tweet");
                                    }
                                }
                                else Log("Failed: Already posted");
                            }
                            else Log("Failed: No Anchor Node");
                        }
                        WriteState(state);
                    }
                    else Log("Failed: No Anchor Nodes");
                }
                else Log("Failed: No HTML Document");
            }
            else Log("Failed: No HTML");
        }

        private static String Canonicalise(String input)
        {
            var charArray = (input ?? "").ToLowerInvariant().ToCharArray();
            input = "";

            foreach (var character in charArray)
                if (Char.IsLetterOrDigit(character)) input += character;
                else input += " ";

            return input.Replace("  ", " ").Trim();
        }

        public static void Tweet(String status)
        {
            try
            {
                var twitterCredentials = ReadCredentials();
                Log("Getting credentials");
                if (twitterCredentials != null)
                {
                    Log("Got Credentials");
                    var service = new TwitterService(ConsumerKey, ConsumerSecret, twitterCredentials.Item1, twitterCredentials.Item2);
                    service.SendTweet(new SendTweetOptions { Status = status });
                    Log("Success!");
                }
                else Log("NO CREDENTIALS");
            }
            catch (Exception exception)
            {
                Error(exception);
            }
        }

        public static void TwitterAuth()
        {
            var service = new TwitterService(ConsumerKey, ConsumerSecret);
            var requestToken = service.GetRequestToken();
            var uri = service.GetAuthorizationUri(requestToken);
            Process.Start(uri.ToString());
            System.Console.WriteLine("Enter Verifier");
            var verifier = System.Console.ReadLine();
            var access = service.GetAccessToken(requestToken, verifier);
            WriteCredentials(access.Token, access.TokenSecret);
            System.Console.WriteLine("All good in the hood");
            System.Console.ReadLine();
        }

        public static void Start()
        {
            StartThread(Worker, null, "MAIN:" + ServiceName);
        }

        public static void Worker(Object state)
        {
            Running = true;
            while (Running)
                try
                {
                    var startTime = DateTime.UtcNow.AddHours(-1);

                    while (Running)
                    {
                        var minutesSinceLastStart = DateTime.UtcNow.Subtract(startTime).TotalMinutes;
                        if (minutesSinceLastStart >= 10)
                        {
                            startTime = DateTime.UtcNow;
                            InnerWorker();
                        }
                        Thread.Sleep(500);
                    }
                }
                catch (Exception exception)
                {
                    Error(exception);
                }
        }

        public static void StartThread(ParameterizedThreadStart start, Object arg, String threadName)
        {
            var thread = new Thread(start) { Name = threadName };
            thread.Start(arg);
        }

        private static String Path(String filename)
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            Directory.SetCurrentDirectory(path);
            return (path + "\\" + filename).Replace(@"\\", @"\");
        }

        private static void WriteState(List<String> state)
        {
            File.WriteAllLines(Path("state.txt"), state);
        }

        private static List<String> ReadState()
        {
            var state = new List<String>();
            var path = Path("state.txt");
            Log("Reading state from " + path);
            if (File.Exists(path))
                state.AddRange(File.ReadAllLines(path));
            else Log("State doesn't exist at " + path);
            return state;
        }

        private static void WriteCredentials(String token, String tokenSecret)
        {
            File.WriteAllText(Path(@"credentials.txt"), token + "£" + tokenSecret);
        }

        private static Tuple<String, String> ReadCredentials()
        {
            var path = Path("credentials.txt");
            Log("Reading credentials from " + path);
            if (File.Exists(path))
            {
                var contents = File.ReadAllText(path).Split('£');
                return new Tuple<String, String>(contents[0], contents[1]);
            }
            Log("Credentials doesn't exist at " + Environment.CurrentDirectory);
            return null;
        }

        private static String GetHtmlRaw()
        {
            var contents = "";
            try
            {
                var httpRequest = HttpWebRequest.CreateHttp(TargetUri);
                httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0";
                httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5");
                httpRequest.UseDefaultCredentials = true;

                var response = (httpRequest.GetResponse() as HttpWebResponse);
                if (response != null)
                    using (var streamReader = new StreamReader(response.GetResponseStream())) contents = streamReader.ReadToEnd();
            }
            catch (Exception exception)
            {
                Error(exception);
            }
            return contents;
        }

        private static HtmlDocument AsHtml(String input)
        {
            try
            {
                var x = new HtmlDocument();
                x.LoadHtml(input);
                return x;
            }
            catch (Exception exception)
            {
                Error(exception);
            }
            return null;
        }

        public static String Shorten(String url)
        {
            try
            {
                var bitlyService = new BitlyService("xxx", "xxx");
                bitlyService.AuthenticateWith("xxx");
                var response = bitlyService.ShortenUrl(new ShortenUrlOptions { Longurl = url });
                return response.Data.Url;
            }
            catch (Exception exception)
            {
                Error(exception);
            }
            return url;
        }

        public static void Log(String message)
        {
            try
            {
                System.Console.WriteLine(message);
            }
            catch (Exception) { }

            try
            {
                Debug.WriteLine(message);
            }
            catch (Exception) { }

            try
            {
                if (!EventLog.SourceExists(ServiceName))
                    EventLog.CreateEventSource(ServiceName, "Application");
                EventLog.WriteEntry(ServiceName, message, EventLogEntryType.Information, 234);
            }
            catch (Exception) { }
        }

        public static void Error(Exception message)
        {
            if (message != null)
                Error(message.ToString());
        }

        public static void Error(String message)
        {
            try
            {
                System.Console.WriteLine(message);
            }
            catch (Exception) { }

            try
            {
                Debug.WriteLine(message);
            }
            catch (Exception) { }

            try
            {
                if (!EventLog.SourceExists(ServiceName))
                    EventLog.CreateEventSource(ServiceName, "Application");
                EventLog.WriteEntry(ServiceName, message, EventLogEntryType.Error, 234);
            }
            catch (Exception) { }
        }
    }
}