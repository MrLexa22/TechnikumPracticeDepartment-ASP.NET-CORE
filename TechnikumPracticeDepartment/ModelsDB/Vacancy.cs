using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Vacancy
    {
        public Vacancy()
        {
            ResponseFromOrganizations = new HashSet<ResponseFromOrganization>();
            ResponseFromStudents = new HashSet<ResponseFromStudent>();
        }

        public int IdVacancy { get; set; }
        public string NameVacancy { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Duties { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string Conditions { get; set; } = null!;
        public string AdditionalInformation { get; set; } = null!;
        public string TagsSkills { get; set; } = null!;
        public int OrganizationId { get; set; }

        public virtual Organization Organization { get; set; } = null!;
        public virtual ICollection<ResponseFromOrganization> ResponseFromOrganizations { get; set; }
        public virtual ICollection<ResponseFromStudent> ResponseFromStudents { get; set; }
    }
}
