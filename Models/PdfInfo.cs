namespace Models
{
    public class PdfInfo
    {
        public long Id { get; set; }
        public string FileHash { get; set; } = null!;
        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;
        public int NumPage { get; set; }

        public DateTime DthRegister { get; set; }

        public string GetFullPath
        {
            get { return FilePath + Path.DirectorySeparatorChar + FileName; }
        }
    }
}
