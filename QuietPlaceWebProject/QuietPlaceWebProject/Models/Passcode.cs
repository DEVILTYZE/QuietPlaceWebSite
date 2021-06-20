using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("Passcodes")]
    public class Passcode : IPasscode
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }
        
        [Required]
        [HiddenInput(DisplayValue = false)]
        public string Code { get; set; }
        
        [Required]
        [HiddenInput(DisplayValue = false)]
        public int RoleId { get; set; }
    }
}