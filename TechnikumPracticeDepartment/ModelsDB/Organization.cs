using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Organization
    {
        public Organization()
        {
            EmployeeOfOrganizations = new HashSet<EmployeeOfOrganization>();
            PracticeChartDistibutions = new HashSet<PracticeChartDistibution>();
            Vacancies = new HashSet<Vacancy>();
        }

        public int IdOrganization { get; set; }
        public string FullNameOrganization { get; set; } = null!;
        public string NotFullNameOrganization { get; set; } = null!;
        public string AddressOrganization { get; set; } = null!;
        public string? InnOrganization { get; set; }
        public string SurnameContactOfOrganization { get; set; } = null!;
        public string NameContactOfOrganization { get; set; } = null!;
        public string? PatronymicContactOfOrganization { get; set; }
        public string? PhoneNumberContactOfOrganization { get; set; }
        public bool? IsAvaliable { get; set; }

        public virtual ICollection<EmployeeOfOrganization> EmployeeOfOrganizations { get; set; }
        public virtual ICollection<PracticeChartDistibution> PracticeChartDistibutions { get; set; }
        public virtual ICollection<Vacancy> Vacancies { get; set; }
    }
}
