using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("Posts")]
    public class Post : IPost
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; init; }
        
        [Required]
        [StringLength(5000, ErrorMessage = "Текст поста не должен превышать 5000 символов.")]
        [Display(Name = "Текст: ")]
        public string Text { get; set; }
        
        [Display(Name = "Дата создания: ")]
        public DateTime DateOfCreation { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int PosterId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool IsOriginalPoster { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int ThreadId { get; set; }
    }
}