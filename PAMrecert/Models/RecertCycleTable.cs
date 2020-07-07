using System;
using System.Collections.Generic;

namespace PAMrecert.Models
{
    public partial class RecertCycleTable
    {
        public int RecertCycleId { get; set; }
        public string RecertCycleTitle { get; set; }
        public DateTime RecertStartedDate { get; set; }
        public DateTime? RecertEndedDate { get; set; }
        public bool RecertEnabled { get; set; }
    }
}
