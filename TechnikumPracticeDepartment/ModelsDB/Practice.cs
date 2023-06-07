using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Practice
    {
        public Practice()
        {
            PracticeChartDistibutions = new HashSet<PracticeChartDistibution>();
            PracticeCharts = new HashSet<PracticeChart>();
            PracticeSpecializations = new HashSet<PracticeSpecialization>();
        }

        public int IdPractice { get; set; }
        public string NamePractice { get; set; } = null!;
        public string NameProfModule { get; set; } = null!;

        public virtual ICollection<PracticeChartDistibution> PracticeChartDistibutions { get; set; }
        public virtual ICollection<PracticeChart> PracticeCharts { get; set; }
        public virtual ICollection<PracticeSpecialization> PracticeSpecializations { get; set; }
    }
}
