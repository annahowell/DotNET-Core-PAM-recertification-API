using System;

namespace PAMrecert.DTOs.RecertCycleController
{
    public class RecertCycleDTO
    {
        public int RecertCycleId { get; set; }
        public string RecertCycleTitle { get; set; }
        public DateTime RecertStartedDate { get; set; }
        public DateTime? RecertEndedDate { get; set; }
        public bool RecertEnabled { get; set; }
        public int RecertCount { get; set; }
    }
}
