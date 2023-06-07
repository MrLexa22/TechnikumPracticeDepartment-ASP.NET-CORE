using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class PracticeChart
    {
        public PracticeChart()
        {
            PracticesChartDates = new HashSet<PracticesChartDate>();
            PracticesChartGroups = new HashSet<PracticesChartGroup>();
        }

        public int IdChart { get; set; }
        public int PracticeId { get; set; }
        public string DaysPractice { get; set; } = null!;
        public double Hours { get; set; }

        public virtual Practice Practice { get; set; } = null!;
        public virtual ICollection<PracticesChartDate> PracticesChartDates { get; set; }
        public virtual ICollection<PracticesChartGroup> PracticesChartGroups { get; set; }
    }
}
