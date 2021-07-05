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
        public string Url { get; set; }
        
        [Required]
        [HiddenInput(DisplayValue = false)]
        public string Type { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int PostId { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int ThreadId { get; set; }
    }
}