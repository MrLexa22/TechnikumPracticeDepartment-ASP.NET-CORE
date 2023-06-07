using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class PracticesChartGroup
    {
        public int IdPracticesGroups { get; set; }
        public int GroupId { get; set; }
        public int PracticeChartId { get; set; }

        public virtual Group Group { get; set; } = null!;
        public virtual PracticeChart PracticeChart { get; set; } = null!;
    }
}
