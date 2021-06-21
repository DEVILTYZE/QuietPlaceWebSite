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
        public int Id { get; set; }

        [Required]
        [Display(Name = "IP-адрес пользователя: ")]
        public string IpAddress { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int PasscodeId { get; set; }

        [Display(Name = "Забанен: ")]
        public bool IsBanned { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime TimeOfRemoving { get; set; }
    }
}