using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class EmployeeOfOrganization
    {
        public int IdEmployeeOfOrganization { get; set; }
        public int UserId { get; set; }
        public int OrganizationId { get; set; }

        public virtual Organization Organization { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
