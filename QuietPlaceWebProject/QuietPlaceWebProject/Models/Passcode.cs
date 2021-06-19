using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class Passcode : IPasscode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int RoleId { get; set; }
    }
}