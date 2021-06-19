using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class User : IUser
    {
        public string AddressOfUser { get; set; }
        public int PasscodeId { get; set; }
    }
}