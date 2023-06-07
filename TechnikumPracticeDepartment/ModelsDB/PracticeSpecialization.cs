using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class PracticeSpecialization
    {
        public int IdPracticeSpecialization { get; set; }
        public int PracticeId { get; set; }
        public int SpecializationId { get; set; }

        public virtual Practice Practice { get; set; } = null!;
        public virtual Specialization Specialization { get; set; } = null!;
    }
}
