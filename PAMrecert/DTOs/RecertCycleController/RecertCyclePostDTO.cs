using System;
using System.ComponentModel.DataAnnotations;

namespace PAMrecert.DTOs.RecertCycleController
{
    public class RecertCyclePostDTO
    {
        [Required]
        public string RecertCycleTitle { get; set; }

        public bool RecertEnabled { get; set; }
    }
}
