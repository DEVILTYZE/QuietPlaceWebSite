using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("Users")]
    public class User : IUser
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; init; }

        [Required]
        [Display(Name = "IP-адрес пользователя: ")]
        public string IpAddress { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public string Passcode { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int RoleId { get; set; }
    }
}