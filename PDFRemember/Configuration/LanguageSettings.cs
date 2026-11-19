using Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace PDFViewLastReadedPage.Configuration
{
    internal class LanguageSettings
    {
        private static IReadOnlyDictionary<Enums.Langs, LanguageModel> dicLangs = new Dictionary<Enums.Langs, LanguageModel>() {
            { Enums.Langs.Portuguese_BR, new LanguageModel{ ELang = Enums.Langs.Portuguese_BR, ResourceFileIdentifier =  "pt-BR", ResourceValue = "LangPtBR" } },
            { Enums.Langs.English_US, new LanguageModel{ ELang = Enums.Langs.Portuguese_BR, ResourceFileIdentifier =  "en-US", ResourceValue = "LangEnUS" } }
        };


        public static IReadOnlyDictionary<Enums.Langs, LanguageModel> GetLangs
        {
            get { return dicLangs; }
        }

        public static void ChangeLanguage(Enums.Langs language)
        {
            var lang = dicLangs[language];
            ResourceDictionary dictionary = new ResourceDictionary
            {
                Source = new Uri(
                    $"Resources/Strings.{lang.ResourceFileIdentifier}.xaml",
                    UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources
                .MergedDictionaries
                .Add(dictionary);
        }

    }
}
