using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Student
    {
        public Student()
        {
            PracticeChartDistibutions = new HashSet<PracticeChartDistibution>();
            ResponseFromStudents = new HashSet<ResponseFromStudent>();
        }

        public int IdStudent { get; set; }
        public int GroupId { get; set; }
        public DateOnly? DateOfBirthday { get; set; }
        public string? PhoneNumber { get; set; }
        public int UserId { get; set; }
        public bool? IsStudent { get; set; }
        public string? ImageStudent { get; set; }

        public virtual Group Group { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual RequestToDistributuion RequestToDistributuion { get; set; } = null!;
        public virtual Resume Resume { get; set; } = null!;
        public virtual ICollection<PracticeChartDistibution> PracticeChartDistibutions { get; set; }
        public virtual ICollection<ResponseFromStudent> ResponseFromStudents { get; set; }
    }
}
