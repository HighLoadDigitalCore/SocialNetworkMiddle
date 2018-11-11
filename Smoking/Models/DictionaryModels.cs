using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{

    public partial class LabelDictionary
    {

        public static string Translate(string key)
        {
            return Translate(key, CurrentDictionary, AccessHelper.CurrentLang.ID);
        }
        public static string Translate(string key, Dictionary<string, string> dictionary, int langID)
        {

            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else
            {
                var db = new DB();
                var test = db.LabelDictionaries.FirstOrDefault(x => x.TextKey == key);
                if (test != null)
                {
                    HttpRuntime.Cache.Remove("DictinaryTable_" + langID);
                    SynchronizeDicts();
                    var dict = GetDictionary(langID);
                    if(dict.ContainsKey(key))
                        return dict[key];
                }
                var langs = db.Languages.ToList();
                var dictKey = new LabelDictionary() { TextKey = key };
                db.LabelDictionaries.InsertOnSubmit(dictKey);
                db.LabelDictionaryLangs.InsertAllOnSubmit(
                    langs.Select(
                        x =>
                        new LabelDictionaryLang()
                            {
                                LabelDictionary = dictKey,
                                LanguageID = x.ID,
                                TranslatedLabel = x.ShortName == "ru" ? dictKey.TextKey : ""
                            }));
                db.SubmitChanges();
                SynchronizeDicts();
                HttpRuntime.Cache.Remove("DictinaryTable_" + langID);
                var translated = string.Format(AccessHelper.CurrentLang.ShortName == "ru" && AccessHelper.CurrentLang.ID == langID ? "{0}" : "[{0}]", key);
                return translated.Trim('[').Trim(']');
            }
        }

        public static void SynchronizeDicts()
        {
            var db = new DB();
            int langCount = db.Languages.Count();
            var notCorrect =
                db.LabelDictionaries.Where(
                    x => x.LabelDictionaryLangs.Select(z => z.LanguageID).Distinct().Count() != langCount).ToList();

            foreach (var dictionary in notCorrect)
            {
                var absent = db.Languages.Where(x => !dictionary.LabelDictionaryLangs.Any(z => z.LanguageID == x.ID));
                foreach (var language in absent)
                {
                    db.LabelDictionaryLangs.InsertOnSubmit(new LabelDictionaryLang()
                        {
                            LabelID = dictionary.ID,
                            LanguageID = language.ID,
                            TranslatedLabel = ""
                        });
                }
            }
            db.SubmitChanges();
        }


        //private static Dictionary<string, string> _currentDictionary;
        public static Dictionary<string, string> CurrentDictionary
        {
            get
            {
                return GetDictionary(AccessHelper.CurrentLang.ID);
                //return _currentDictionary ?? (_currentDictionary = GetDictionary(AccessHelper.CurrentLang.ID));
            }
            set
            {
                if (value == null)
                {
                    HttpRuntime.Cache.Remove("DictinaryTable_" + AccessHelper.CurrentLang.ID);
                }
            }
        }

        public static Dictionary<string, string> GetDictionary(int langID)
        {
            
            var cached = HttpRuntime.Cache.Get("DictinaryTable_" + langID);
            if (cached is Dictionary<string, string>)
            {
                return cached as Dictionary<string, string>;
            }
            else
            {
                var db = new DB();
                var dict = db.LabelDictionaries.Distinct().ToList().Distinct(new DictionaryEqualityComparer()).ToDictionary(x => x.TextKey, z =>
                    {
                        var item =
                            z.LabelDictionaryLangs.FirstOrDefault(c => c.LanguageID == langID);
                        if (item == null || item.TranslatedLabel.IsNullOrEmpty())
                            return string.Format("[{0}]", z.TextKey).Trim('[').Trim(']');
                        return item.TranslatedLabel.Trim('[').Trim(']');
                    });

                HttpRuntime.Cache.Insert("DictinaryTable_" + langID,
                                         dict, null,
                                         DateTime.Now.AddDays(1D),
                                         Cache.NoSlidingExpiration);
                return dict;
            }
        }

        public class DictionaryEqualityComparer : IEqualityComparer<LabelDictionary>
        {
            public bool Equals(LabelDictionary x, LabelDictionary y)
            {
                return x.TextKey == y.TextKey;
            }

            public int GetHashCode(LabelDictionary obj)
            {
                return obj.TextKey.GetHashCode();
            }
        }
    }
}