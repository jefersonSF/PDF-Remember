using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    internal class Connection
    {
        public static string ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "PdfViewLastReadedPage.db")}";
    }
}
