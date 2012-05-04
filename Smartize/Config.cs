﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using System.IO;
using Newtonsoft.Json;

namespace SmarterFingers
{
    [Serializable]
    public class Words
    {
        public List<string> StupidWords;
        public List<string> SmartWords;

        public Words(List<string> StupidWords, List<string> SmartWords)
        {
            this.StupidWords = StupidWords;
            this.SmartWords = SmartWords;
        }

        public List<string> getOriginal()
        {
            return StupidWords;
        }

        public List<string> getReplacement()
        {
            return SmartWords;
        }
    }

    public class WordList
    {
        public List<Words> SmarterFingers;

        public WordList()
        {
            SmarterFingers = new List<Words>();
        }

        public void AddItem(Words k)
        {
            SmarterFingers.Add(k);
        }
    }

    public class WordReader
    {
        public WordList writeFile(String file)
        {
            TextWriter tw = new StreamWriter(file);

            WordList OrigWordList = new WordList();

            OrigWordList.AddItem(new Words(new List<string> { "hai", "hullo", "ello", "halo", "hallo" }, new List<string> { "greetings", "hi", "hello" }));
            OrigWordList.AddItem(new Words(new List<string> { "em" }, new List<string> { "am" }));
            OrigWordList.AddItem(new Words(new List<string> { "y", "wy", "wai" }, new List<string> { "why" }));
            OrigWordList.AddItem(new Words(new List<string> { "pooter", "comptooter", "comp", "putter", "macintosh", "compooter", "cpu" }, new List<string> { "computer" }));
            OrigWordList.AddItem(new Words(new List<string> { "r" }, new List<string> { "are" }));
            OrigWordList.AddItem(new Words(new List<string> { "teh", "tuh", "duh", "d", "de", "le" }, new List<string> { "the" }));
            OrigWordList.AddItem(new Words(new List<string> { "donot", "dont", "dnt" }, new List<string> { "don't" }));
            OrigWordList.AddItem(new Words(new List<string> { "yea", "yep", "yeh", "yuh", "yah", "ya" }, new List<string> { "yes" }));
            OrigWordList.AddItem(new Words(new List<string> { "wut", "wat", "whut", "wt" }, new List<string> { "what" }));
            OrigWordList.AddItem(new Words(new List<string> { "wuts", "wats", "whuts", "wts", "whats" }, new List<string> { "what's" }));
            OrigWordList.AddItem(new Words(new List<string> { "dis" }, new List<string> { "this" }));
            OrigWordList.AddItem(new Words(new List<string> { "meh" }, new List<string> { "me" }));
            OrigWordList.AddItem(new Words(new List<string> { "mah" }, new List<string> { "my" }));

            tw.Write(JsonConvert.SerializeObject(OrigWordList, Formatting.Indented));
            tw.Close();

            return OrigWordList;
        }

        public WordList readFile(String file)
        {
            TextReader tr = new StreamReader(file);
            String raw = tr.ReadToEnd();
            tr.Close();
            WordList wordList = JsonConvert.DeserializeObject<WordList>(raw);
            return wordList;
        }
    }
}