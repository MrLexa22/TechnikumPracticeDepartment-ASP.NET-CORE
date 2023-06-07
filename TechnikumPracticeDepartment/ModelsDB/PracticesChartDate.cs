using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class PracticesChartDate
    {
        public int IdDatePracticeChart { get; set; }
        public DateOnly DateStart { get; set; }
        public DateOnly DateEnd { get; set; }
        public int PracticeChartId { get; set; }

        public virtual PracticeChart PracticeChart { get; set; } = null!;
    }
}
