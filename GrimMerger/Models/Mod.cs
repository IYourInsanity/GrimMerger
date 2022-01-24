namespace GrimMerger.Models
{
    public class Mod
    {
        public string Path { get; }
        public string Name { get; }
        public bool IsActive { get; set; }

        public Mod(string path, string name, bool isActive)
        {
            Path = path;
            Name = name;
            IsActive = isActive;
        }
    }
}
