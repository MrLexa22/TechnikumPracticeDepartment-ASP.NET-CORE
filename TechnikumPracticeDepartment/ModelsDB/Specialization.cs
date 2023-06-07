using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Specialization
    {
        public Specialization()
        {
            Groups = new HashSet<Group>();
            PracticeSpecializations = new HashSet<PracticeSpecialization>();
        }

        public int IdSpecialization { get; set; }
        public string SpecializationCode { get; set; } = null!;
        public string SpecializationName { get; set; } = null!;
        public string NameQualification { get; set; } = null!;

        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<PracticeSpecialization> PracticeSpecializations { get; set; }
    }
}
