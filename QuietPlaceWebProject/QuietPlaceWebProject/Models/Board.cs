using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class Board : IBoard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DomainName { get; set; }
        public int MaxCountOfThreads { get; set; }
        public bool IsHidden { get; set; }
        public int AccessRoleId { get; set; }
    }
}