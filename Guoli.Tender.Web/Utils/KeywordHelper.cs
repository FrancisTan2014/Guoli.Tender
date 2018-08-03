using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Guoli.Tender.Web.Utils
{
    public static class KeywordHelper
    {
        private const string _keywordsAppKey = "keywordsTable";

        private static void LoadDictToMemory()
        {
            var filename = ConfigurationManager.AppSettings["IkExtDictFilename"];
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var reader = new StreamReader(fs))
                {
                    var str = reader.ReadToEnd();
                    var hashTable = new Hashtable();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var keywods = str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var k in keywods)
                        {
                            if (!hashTable.ContainsKey(k))
                            {
                                hashTable.Add(k, 0);
                            }
                        }
                    }

                    HttpContext.Current.Application[_keywordsAppKey] = hashTable;
                }
            }
        }

        private static Hashtable GetTable()
        {
            var application = HttpContext.Current.Application;
            var table = application[_keywordsAppKey] as Hashtable;
            if (table == null)
            {
                lock (_keywordsAppKey)
                {
                    LoadDictToMemory();
                    table = application[_keywordsAppKey] as Hashtable;
                }
            }

            return table;
        }

        public static bool Exists(string keyword)
        {
            var table = GetTable();
            return table.ContainsKey(keyword);
        }

        public static void Add(params string[] keywords)
        {
            var filename = ConfigurationManager.AppSettings["IkExtDictFilename"];
            using (var writer = File.AppendText(filename))
            {
                var txt = string.Join("\r\n", keywords);
                writer.WriteLine(txt);
            }
        }
    }
}