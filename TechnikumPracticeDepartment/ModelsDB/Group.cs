using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Group
    {
        public Group()
        {
            PracticesChartGroups = new HashSet<PracticesChartGroup>();
            Students = new HashSet<Student>();
        }

        public int IdGroup { get; set; }
        public string NameGroup { get; set; } = null!;
        public short YearStartEducation { get; set; }
        public short YearOfGraduation { get; set; }
        public int SpecializationId { get; set; }

        public virtual Specialization Specialization { get; set; } = null!;
        public virtual ICollection<PracticesChartGroup> PracticesChartGroups { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
