using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Resume
    {
        public Resume()
        {
            ResponseFromOrganizations = new HashSet<ResponseFromOrganization>();
        }

        public int IdResume { get; set; }
        public string DesiredPosition { get; set; } = null!;
        public string WorkExperience { get; set; } = null!;
        public string Education { get; set; } = null!;
        public string ProfessionalSkills { get; set; } = null!;
        public string AdditionalInformation { get; set; } = null!;
        public string AboutStudent { get; set; } = null!;
        public string TagsSkills { get; set; } = null!;
        public string? FileWithResume { get; set; }
        public int StudentId { get; set; }
        public bool IsAvaliable { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual ICollection<ResponseFromOrganization> ResponseFromOrganizations { get; set; }
    }
}
