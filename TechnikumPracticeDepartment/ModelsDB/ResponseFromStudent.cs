using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class ResponseFromStudent
    {
        public int IdResponse { get; set; }
        public int VacancyId { get; set; }
        public int StudentId { get; set; }
        public string? CommentStudent { get; set; }
        public string? CommentOrganization { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public int Status { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual Vacancy Vacancy { get; set; } = null!;
    }
}
