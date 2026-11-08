namespace Models
{
    public class Recents
    {
        public Recents(string fullPathFile)
        {
            FileFullPath = fullPathFile;
        }
        public int Id { get; set; }
        public string FileFullPath { get; } = null!;
        public DateTime DthRegister { get; set; }
    }
}
