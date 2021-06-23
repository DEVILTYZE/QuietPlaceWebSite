using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("Threads")]
    public class Thread : IThread
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; init; }
        
        [Required]
        [StringLength(50, ErrorMessage = "Тема треда должна быть не более 50 символов.")]
        [Display(Name = "Тема")]
        public string Name { get; set; }
        
        [Display(Name = "Бамп-лимит")]
        public bool HasBumpLimit { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int BoardId { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int PosterId { get; set; }
    }
}