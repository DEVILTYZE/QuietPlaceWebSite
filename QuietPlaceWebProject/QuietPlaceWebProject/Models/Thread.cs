using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class Thread : IThread
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasBumpLimit { get; set; }
        public int BoardId { get; set; }
        public int PosterId { get; set; }
    }
}