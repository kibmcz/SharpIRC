/*
   Copyright 2009-2012 Alex Sørlie Glomsaas, Adonis S. Deliannis, Kevin Crowston

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class QuoteSystem : PluginInterface {
        public static QuoteDatabase myquotes = ConfigurationAPI.LoadConfigurationFile<QuoteDatabase>("Quotes");

        public static List<SearchHistory> History = new List<SearchHistory>();
        private static readonly Regex _isNumber = new Regex(@"^\d+$");

        public override void ConfigurationChange(ConfigurationFile file) { if (file.Name == "Quotes") myquotes = ConfigurationAPI.LoadConfigurationFile<QuoteDatabase>("Quotes"); }

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("addquote") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "addquote")) {
                string qmessage = message.Message.GetMessageWithoutCommand();
                var newquote = new Quote {
                    Author = message.Sender.Nick,
                    Date = DateTime.Now.ToString(),
                    Location = message.Channel.Name,
                    Message = qmessage
                };
                myquotes.Quotes.Add(newquote);
                Commands.SendPrivMsg(message.Connection, message.Channel, "Your quote has been saved as quote number: " + myquotes.Quotes.Count);
                ConfigurationAPI.SaveConfigurationFile(myquotes, "Quotes");
            }
            if (message.Message.IsCommand("quote") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "quote")) {
                string qmessage = message.Message.GetMessageWithoutCommand();
                if (IsInteger(qmessage)) {
                    if (myquotes.Quotes.Count >= Convert.ToInt32(qmessage)) {
                        int quotenumber = 0;
                        foreach (Quote quote in myquotes.Quotes) {
                            quotenumber++;
                            if (quotenumber == Convert.ToInt32(qmessage)) Commands.SendPrivMsg(message.Connection, message.Channel, "~Quote~ [" + qmessage + "/" + myquotes.Quotes.Count + "]: " + quote.Message);
                        }
                    } else Commands.SendPrivMsg(message.Connection, message.Channel, "Quote " + qmessage + " does not exsist. The last quote is " + myquotes.Quotes.Count + ".");
                } else Commands.SendPrivMsg(message.Connection, message.Channel, "The quote you entered is not a number. Syntax: ~quote <number>.");
            }
            if (message.Message.IsCommand("fquote") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "fquote")) {
                if (IsInteger(message.Message.Split(' ')[1])) {
                    if (message.Message.Split(' ').Length >= 2) {
                        var wcard = new Connect.Wildcard(String.Format("*{0}*", message.Message.GetMessageWithoutSubCommand()), RegexOptions.IgnoreCase);
                        int qnum = Convert.ToInt32(message.Message.Split(' ')[1]);
                        List<Quote> qlist = myquotes.Quotes.Where(x => wcard.IsMatch(x.Message)).ToList();
                        if (qnum <= qlist.Count) {
                            Quote qmsg = qlist[qnum - 1];
                            Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("~Quote~ [{0}/{1}] Match: [{2}/{3}]: {4}",
                                    myquotes.Quotes.IndexOf(qmsg), myquotes.Quotes.Count, qnum, qlist.Count, qmsg.Message));
                            AddtoHistory(message.Message.GetMessageWithoutSubCommand(), qnum - 1);
                        } else Commands.SendPrivMsg(message.Connection, message.Channel, "No quotes matching your query was found.");
                    }
                } else {
                    if (message.Message.IsSubCommand("last")) {
                        var wcard = new Connect.Wildcard(String.Format("*{0}*", message.Message.GetMessageWithoutSubCommand()), RegexOptions.IgnoreCase);
                        List<Quote> qlist = myquotes.Quotes.Where(x => wcard.IsMatch(x.Message)).ToList();
                        int qnum = qlist.Count;
                        if (qlist.Count > 0) {
                            Quote qmsg = qlist[qnum - 1];
                            Commands.SendPrivMsg(message.Connection, message.Channel,
                                String.Format("~Quote~ [{0}/{1}] Match: [{2}/{3}]: {4}", myquotes.Quotes.IndexOf(qmsg), myquotes.Quotes.Count, qnum, qlist.Count, qmsg.Message));
                            AddtoHistory(message.Message.GetMessageWithoutSubCommand(), qnum);
                        } else Commands.SendPrivMsg(message.Connection, message.Channel, "No quotes matching your query was found.");
                    }
                    if (message.Message.IsSubCommand("next")) {
                        var wcard = new Connect.Wildcard(String.Format("*{0}*", message.Message.GetMessageWithoutSubCommand()), RegexOptions.IgnoreCase);
                        List<Quote> qlist = myquotes.Quotes.Where(x => wcard.IsMatch(x.Message)).ToList();
                        int qnum = GetCurrentHistory(message.Message.GetMessageWithoutSubCommand()) + 1;
                        if (qnum <= qlist.Count) {
                            Quote qmsg = qlist[qnum - 1];
                            Commands.SendPrivMsg(message.Connection, message.Channel, 
                                String.Format("~Quote~ [{0}/{1}] Match: [{2}/{3}]: {4}", myquotes.Quotes.IndexOf(qmsg), myquotes.Quotes.Count, qnum, qlist.Count, qmsg.Message));
                            AddtoHistory(message.Message.GetMessageWithoutSubCommand(), qnum);
                        } else Commands.SendPrivMsg(message.Connection, message.Channel, "No quotes matching your query was found.");
                    }
                    if (message.Message.IsSubCommand("previous")) {
                        var wcard = new Connect.Wildcard(String.Format("*{0}*", message.Message.GetMessageWithoutSubCommand()), RegexOptions.IgnoreCase);

                        List<Quote> qlist = myquotes.Quotes.Where(x => wcard.IsMatch(x.Message)).ToList();
                        int qnum = GetCurrentHistory(message.Message.GetMessageWithoutSubCommand()) - 1;
                        if (qlist.Count > 0) {
                            Quote qmsg = qlist[qnum - 1];
                            Commands.SendPrivMsg(message.Connection, message.Channel, 
                                String.Format("~Quote~ [{0}/{1}] Match: [{2}/{3}]: {4}", myquotes.Quotes.IndexOf(qmsg), myquotes.Quotes.Count, qnum, qlist.Count, qmsg.Message));

                            AddtoHistory(message.Message.GetMessageWithoutSubCommand(), qnum);
                        } else Commands.SendPrivMsg(message.Connection, message.Channel, "No quotes matching your query was found.");
                    }
                }
            }
            if (message.Message.IsCommand("delquote") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "delquote")) {
                string qmessage = message.Message.GetMessageWithoutCommand();
                if (IsInteger(qmessage)) {
                    if (myquotes.Quotes.Count >= Convert.ToInt32(qmessage)) {
                        int quotenumber = 0;
                        List<Quote> templist = myquotes.Quotes.ToList();
                        foreach (Quote quote in templist) {
                            quotenumber++;
                            if (quotenumber == Convert.ToInt32(qmessage)) myquotes.Quotes.Remove(quote);
                        }
                        ConfigurationAPI.SaveConfigurationFile(myquotes, "Quotes");
                        Commands.SendPrivMsg(message.Connection, message.Channel,
                                             "Quote " + qmessage + "has been removed.");
                    }
                    else
                        Commands.SendPrivMsg(message.Connection, message.Channel,
                                             "Quote " + qmessage + "does not exsist. The last quote is: " +
                                             myquotes.Quotes.Count);
                }
                else
                    Commands.SendPrivMsg(message.Connection, message.Channel,
                                         "That is not a number.. Syntax: ~delquote <quote number>");
            }
        }

        public static bool IsInteger(string theValue) {
            Match m = _isNumber.Match(theValue);
            return m.Success;
        }

        //IsInteger
        public static void AddtoHistory(string quote, int match) {
            SearchHistory histtorem = null;
            foreach (SearchHistory hist in History.Where(hist => hist.Quote == quote)) histtorem = hist;
            if (histtorem != null) History.Remove(histtorem);
            var newhist = new SearchHistory {Quote = quote, MatchNumber = match};
            History.Add(newhist);
        }

        public static int GetCurrentHistory(string quote) {
            int curhist = 0;
            foreach (SearchHistory hist in History.Where(hist => hist.Quote == quote)) curhist = hist.MatchNumber;
            return curhist;
        }

        public static int Getmatches(string searchterm) {
            var wcard = new Connect.Wildcard("*" + searchterm + "*", RegexOptions.IgnoreCase);
            return myquotes.Quotes.Count(quote => wcard.IsMatch(quote.Message));
        }

        #region Nested type: Quote
        public class Quote {
            public string Author { get; set; }
            public string Location { get; set; }
            public string Message { get; set; }
            public string Date { get; set; }
        }
        #endregion

        #region Nested type: QuoteDatabase
        public class QuoteDatabase {
            public List<Quote> Quotes = new List<Quote>();
        }
        #endregion

        #region Nested type: SearchHistory
        public class SearchHistory {
            public string Quote { get; set; }
            public int MatchNumber { get; set; }
        }
        #endregion
    }
}
