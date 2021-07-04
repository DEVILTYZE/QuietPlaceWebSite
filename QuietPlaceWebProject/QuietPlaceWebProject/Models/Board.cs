using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table(("Boards"))]
    public class Board : IBoard
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; init; }
        
        [Required]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 30 символов.")]
        [Display(Name = "Название: ")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Домен должен быть трёхсимвольным.")]
        [Display(Name = "Домен: ")]
        public string DomainName { get; set; }

        [Required]
        [HiddenInput(DisplayValue = false)]
        [DefaultValue(true)]
        public string ImageUrl { get; set; } = "question_pic.png";

        [Required]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "Длина описания должна быть от 10 до 300 символов")]
        [Display(Name = "Описание: ")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Максимальное количество тредов на доске: ")]
        [Range(2, 50, ErrorMessage = "Максимальное количество должно быть в диапазоне от 2 до 50.")]
        public int MaxCountOfThreads { get; set; }
        
        [Required]
        [Display(Name = "Скрыт: ")]
        public bool IsHidden { get; set; }
        
        [Required]
        [Display(Name = "Доступен для ролей: ")]
        public int AccessRoleId { get; set; }
    }
}