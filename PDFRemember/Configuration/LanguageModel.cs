using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFViewLastReadedPage.Configuration
{
    internal class LanguageModel
    {
        public Enums.Langs ELang { get; set; }
        public string ResourceFileIdentifier { get; set; } = null!;
        public string ResourceValue { get; set; } = null!;
    }
}
