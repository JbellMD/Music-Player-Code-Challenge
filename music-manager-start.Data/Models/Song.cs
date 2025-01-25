using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace music_manager_starter.Data.Models
{
    public sealed class Song
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Artist { get; set; } = string.Empty;
        [Required]
        public string Album { get; set; } = string.Empty;
        [Required]
        public string Genre { get; set; } = string.Empty;
    }
}
