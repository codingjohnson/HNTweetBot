HNTweetBot
==========
There are many Hacker News bots out there, this is my attempt which is currently live and running at https://twitter.com/HackerNewsYCBot

It's something of a proof-of-a-concept, so isn't perhaps the most stable code ever.

What's in the box?
------------------
Well it's a C# based Windows service and console that uses OpenBitly and TweetSharp to post updates to Twitter every ten minutes. I confess it's not the best code I've ever written but I wrote it in four hours and it hasn't needed much updating since I wrote it.

You'll also find a copy of the OpenBitly library in there, the default project from Nuget isn't strongly named and that's the **only** reason its here

Does it need a DB?
------------------
Nope, it uses good old text files to store the progress. I may one day embed SQL-lite or some other lite database to handle data-storage.

How To Use
==========
* Add your Bitly key(s) to \HNTweetBot.Console\Service.cs
* Add your Twitter key(s) to \HNTweetBot.Console\Service.cs
* Build it
* Run the console with an "auth" switch to test the Twitter authentication process
* Run the console with an "test" to test it
* Install using the C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe (or whatever framework version you want)
* Run the service
* Pump fists with joy as you have your own TweetBot!