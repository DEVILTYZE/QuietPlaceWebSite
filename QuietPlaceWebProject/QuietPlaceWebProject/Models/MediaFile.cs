using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    [Table("MediaFiles")]
    public class MediaFile : IMediaFile
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; init; }
        
        [Required]
        [HiddenInput(DisplayValue = false)]
        public string Path { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int PostId { get; set; }
    }
}