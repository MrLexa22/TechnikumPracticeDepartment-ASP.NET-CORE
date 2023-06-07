using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class PracticeChartDistibution
    {
        public int IdPcdist { get; set; }
        public int PracticeId { get; set; }
        public int StudentId { get; set; }
        public int OrganizationId { get; set; }

        public virtual Organization Organization { get; set; } = null!;
        public virtual Practice Practice { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
