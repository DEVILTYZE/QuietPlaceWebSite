using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("Roles")]
    public class Role : IRole
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Имя роли должно быть от 3 до 30 символов.")]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}