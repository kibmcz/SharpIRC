using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Fry_2_0;
using PluginArchitecture;
using System.Web;

namespace FryQuotes
{
    public class FryQuotes : PluginInterface, Plugin
    {
        #region PluginInterface Members

        public static bool trustedon = false;
        public string WSDesc = "";
        public string WSLink = "";
        public string PluginName()
        {
            return "8ball";
        }

        public string PluginAuthor()
        {
            return "Alex Sørlie";
        }

        public string PluginPurpose()
        {
            return "8ball command.";
        }
        #endregion

        #region PluginInterface Members


        #endregion
        public override void ChanMsg(IRCConnection connection, string host, string nick, string channel, string message)
        {
            if (string.Equals(Functions.getcommand(message), "fryquote", StringComparison.OrdinalIgnoreCase) && Functions.GetMessage(message).Length > 0)
            {
                var random = new Random().Next(1, 66);
                if (random == 1) Commands.Send_PrivMsg(connection, channel, "Did everything just taste purple for a second?");
                if (random == 2) Commands.Send_PrivMsg(connection, channel, "But existing is practically all I do!");
                if (random == 3) Commands.Send_PrivMsg(connection, channel, "I did do the nasty in the pasty");
                if (random == 4) Commands.Send_PrivMsg(connection, channel, "In my time we didn't depend on hi-tech gadgets like you do, we didn't need a mechanical washing unit to wash our clothes, we just used a washing machine");
                if (random == 5) Commands.Send_PrivMsg(connection, channel, "The important thing is, we're all together for Xmas. And even though I'm surrounded by robots and monsters and old people, I've never felt more at home");
                if (random == 6) Commands.Send_PrivMsg(connection, channel, "Wow, you got that off the internet?! In my day the internet was only used to download pornography");
                if (random == 7) Commands.Send_PrivMsg(connection, channel, "Wow, there's a million aliens, i've never seen anything so mindblowing.....Oooh, a reception table with muffins");
                if (random == 8) Commands.Send_PrivMsg(connection, channel, "Wow Bender, are you and the ship an item? I mean i know you're both items but how can you date a ship anyway? It'd be like me dating a really fat lady and living inside her and she'd be all like neeau-weeeedd-wisisou");
                if (random == 9) Commands.Send_PrivMsg(connection, channel, "Space. It seems to go on and on forver. But then you get to the end and the gorilla starts throwing barrels at you.");
                if (random == 10) Commands.Send_PrivMsg(connection, channel, "Pizza delivery for.... I. C. Wiener! Oh crud! I always thought at this point in my life I'd be the one making the crank calls!");
                if (random == 11) Commands.Send_PrivMsg(connection, channel, "My God!! It's the future! My parents, my co-workers, my girlfriend. I'll never see any of them again. YAHOOO!!!!");
                if (random == 12) Commands.Send_PrivMsg(connection, channel, "This is awesome!! Are we gonna fly through space fighting monsters and teaching alien women to love? ");
                if (random == 13) Commands.Send_PrivMsg(connection, channel, "*eating a burger* Ahhhhhh, just like my Dad used to make, until McDonalds fired him.");
                if (random == 14) Commands.Send_PrivMsg(connection, channel, "Wait a second. I'm getting an idea. No, false alarm. No. Yes! No. Yep. Nope, waaiiit, no. Yes. Yes. No. YES!!!!");
                if (random == 15) Commands.Send_PrivMsg(connection, channel, "It's just like the story of the grasshopper and the octopus. All year long, the grasshopper kept burying acorns for the winter, while the octopus mooched off his girlfriend and watched TV. But then the winter came, and the grasshopper died, and the octopus ate all his acorns. And also he got a racecar. Is any of this getting through to you?");
                if (random == 16) Commands.Send_PrivMsg(connection, channel, "My folks were always on at me to groom myself and wear underpants. What am I, the Pope?!");
                if (random == 17) Commands.Send_PrivMsg(connection, channel, "If I ever want to go back to the year 2000, I'll just freeze myself again");
                if (random == 18) Commands.Send_PrivMsg(connection, channel, "Hey, my girlfriend had one of those. Actually, it wasnt hers it was her Dad's. And actually she wasn't my girlfriend, she just lived next door and never closed the curtains");
                if (random == 19) Commands.Send_PrivMsg(connection, channel, "I never told anybody this but a thousand years ago I used to look up at the moon and dream about being an astronaut. I just never had the grades. Nor the physical endurance. Plus I threw up a lot and nobody liked spending a week with me.");
                if (random == 20) Commands.Send_PrivMsg(connection, channel, "Man, it's a total sty! For the first time in a thousand years, I feel like I'm home! ");
                if (random == 21) Commands.Send_PrivMsg(connection, channel, "Oh, thanks. If you ever need a saviour again, just ask!");
                if (random == 22) Commands.Send_PrivMsg(connection, channel, "I'll be whatever I wanna do!");
                if (random == 23) Commands.Send_PrivMsg(connection, channel, "Fry: No I'm......doesn't!");
                if (random == 24) Commands.Send_PrivMsg(connection, channel, "Well first, everything went dark. Then this bright light appeared and it said GAME OVER");
                if (random == 25) Commands.Send_PrivMsg(connection, channel, "So Leela, do you wanna be like us? Or do you wanna be like Adelai, with no severe mental or social problems whatsoever?");
                if (random == 26) Commands.Send_PrivMsg(connection, channel, "..I'll show yee");
                if (random == 27) Commands.Send_PrivMsg(connection, channel, "I'm going to continue never washing this cheek again.");
                if (random == 28) Commands.Send_PrivMsg(connection, channel, "No knife can penetrate my skintanium armor! weeeeeooooeeeeooooeeeeooooeee eooooeeee");
                if (random == 29) Commands.Send_PrivMsg(connection, channel, "People said i was stupid, but i proved them!!");
                if (random == 30) Commands.Send_PrivMsg(connection, channel, "*sniff sniff sniff* what smells like blue?");
                if (random == 31) Commands.Send_PrivMsg(connection, channel, "I just saw something incredibly cool. A big floating ball that lit up with every colour of the rainbow, plus some new ones that were so beautiful I fell to my knees and cried");
                if (random == 32) Commands.Send_PrivMsg(connection, channel, "Yeah! A barbecue. I'll wear my hilairious apron! ");
                if (random == 33) Commands.Send_PrivMsg(connection, channel, "Wait, Mount Rushmore and The Leaning Tower of Piza. I didnt know they were both in New York.");
                if (random == 34) Commands.Send_PrivMsg(connection, channel, "You mean Bender is the evil Bender? Im shocked, *b*shocked*/b*, well, not that shocked.");
                if (random == 35) Commands.Send_PrivMsg(connection, channel, "Alright bird, you thought you could beat me in a game of wits. Well you just met your equal.");
                if (random == 36) Commands.Send_PrivMsg(connection, channel, "They're like sex, except I'm having them!");
                if (random == 37) Commands.Send_PrivMsg(connection, channel, "Thinking hurts him, maybe i can think of a way to use that");
                if (random == 38) Commands.Send_PrivMsg(connection, channel, "The Breakfast Club soundtrack. I can't wait until I'm old enough to feel ways about stuff.");
                if (random == 39) Commands.Send_PrivMsg(connection, channel, "Hey, you have no right to criticize the 20th century! We gave the world the light bulb, the steam boat and the cotton gin.");
                if (random == 40) Commands.Send_PrivMsg(connection, channel, "Take that one, and that one. This sentence I don’t understand, but take this one.");
                if (random == 41) Commands.Send_PrivMsg(connection, channel, "I love you' too conventional, 'You're my man' Ooh, so close");
                if (random == 42) Commands.Send_PrivMsg(connection, channel, "why am i sticky, did i fall asleep on the ground at the movie theater again?");
                if (random == 43) Commands.Send_PrivMsg(connection, channel, "One time, my friend Richie swore to me he wasn't taking drugs. Then he sold me my mom's VCR. Then I found out he was taking drugs!");
                if (random == 44) Commands.Send_PrivMsg(connection, channel, "Life on the road made me realise it's better without TV. Well, let's see what's on.");
                if (random == 45) Commands.Send_PrivMsg(connection, channel, "Drugs are for losers, and hypnosis is for losers with big weird eyebrows.");
                if (random == 46) Commands.Send_PrivMsg(connection, channel, "Why am I sticky and naked... did I miss something fun?");
                if (random == 47) Commands.Send_PrivMsg(connection, channel, "Is it this room that's making you sad? It's probably the room. Come on, lets go for a walk");
                if (random == 48) Commands.Send_PrivMsg(connection, channel, "Of course i've been up all night! Not because of caffeine because of insomnia. I couldn't stop thinking about coffee. I need a nap (Snore) Coffee time!");
                if (random == 49) Commands.Send_PrivMsg(connection, channel, "I feel like I was mauled by Jesus");
                if (random == 50) Commands.Send_PrivMsg(connection, channel, "wow it's like that drug trip i saw in that movie when i was on that drug trip");
                if (random == 51) Commands.Send_PrivMsg(connection, channel, "That's the saltiest thing I've ever tasted, and I once ate a big heaping bowl of salt! ");
                if (random == 52) Commands.Send_PrivMsg(connection, channel, "All This Prolonged Exposure to the Sun is Making Me Thirsty");
                if (random == 53) Commands.Send_PrivMsg(connection, channel, "I've always known TV will help me save the world someday!");
                if (random == 54) Commands.Send_PrivMsg(connection, channel, "Look! It's that guy you are!");
                if (random == 55) Commands.Send_PrivMsg(connection, channel, "Maybe we're all wearing magic rings, but they're invisible rings so we don't even realize it. Also, you can't feel the rings.");
                if (random == 56) Commands.Send_PrivMsg(connection, channel, "So? I'm just as important as him. *pause* It's just that, the kind of importance I have ... it doesn't matter if I ... don't do it.");
                if (random == 57) Commands.Send_PrivMsg(connection, channel, "I'm doing my job... there's Amy. I spend a few hours selecting a candy from the machine... there's Amy. I wake up the morning after sleeping with Amy... there's Amy!");
                if (random == 58) Commands.Send_PrivMsg(connection, channel, "Armstrong's footprint! Hey! My foot's bigger!");
                if (random == 59) Commands.Send_PrivMsg(connection, channel, "You're better than normal - you're abnormal!!!");
                if (random == 60) Commands.Send_PrivMsg(connection, channel, "Wait, i cant find my pockets! i know, ill just take off my pants and give you those!");
                if (random == 61) Commands.Send_PrivMsg(connection, channel, "Because baby, when we're together time will stop.");
                if (random == 62) Commands.Send_PrivMsg(connection, channel, "Everytime something good happens you say it's some kind of madness , or i'm drunk , or I ate too much candy.");
                if (random == 63) Commands.Send_PrivMsg(connection, channel, "I always thought it was real like pro wrestling but it's fixed like boxing.");
                if (random == 64) Commands.Send_PrivMsg(connection, channel, "Now that's what I call a thousand years of progress: a Bavarian Cream dog that's self-microwaving!");
                if (random == 65) Commands.Send_PrivMsg(connection, channel, "Poor Bender, you're seeing things. You've been drinking too much. Or too little. I forget how it works with you. Anyway, you haven't drunk exactly  the right amount.");
                if (random == 66) Commands.Send_PrivMsg(connection, channel, "All right, all right, if it will make you happy, I will overthrow society.");
            }
        }
    }
}