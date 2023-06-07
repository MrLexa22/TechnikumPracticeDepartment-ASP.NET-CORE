using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment
{
    public partial class TechnikumPracticeDepartmentContext : DbContext
    {
        public TechnikumPracticeDepartmentContext()
        {
        }

        public TechnikumPracticeDepartmentContext(DbContextOptions<TechnikumPracticeDepartmentContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmployeeOfOrganization> EmployeeOfOrganizations { get; set; } = null!;
        public virtual DbSet<Group> Groups { get; set; } = null!;
        public virtual DbSet<Organization> Organizations { get; set; } = null!;
        public virtual DbSet<Practice> Practices { get; set; } = null!;
        public virtual DbSet<PracticeChart> PracticeCharts { get; set; } = null!;
        public virtual DbSet<PracticeChartDistibution> PracticeChartDistibutions { get; set; } = null!;
        public virtual DbSet<PracticeSpecialization> PracticeSpecializations { get; set; } = null!;
        public virtual DbSet<PracticesChartDate> PracticesChartDates { get; set; } = null!;
        public virtual DbSet<PracticesChartGroup> PracticesChartGroups { get; set; } = null!;
        public virtual DbSet<RequestToDistributuion> RequestToDistributuions { get; set; } = null!;
        public virtual DbSet<ResponseFromOrganization> ResponseFromOrganizations { get; set; } = null!;
        public virtual DbSet<ResponseFromStudent> ResponseFromStudents { get; set; } = null!;
        public virtual DbSet<Resume> Resumes { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Specialization> Specializations { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UsersRole> UsersRoles { get; set; } = null!;
        public virtual DbSet<Vacancy> Vacancies { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            modelBuilder.Entity<EmployeeOfOrganization>(entity =>
            {
                entity.HasKey(e => e.IdEmployeeOfOrganization)
                    .HasName("PRIMARY");

                entity.ToTable("EmployeeOfOrganization");

                entity.HasIndex(e => e.OrganizationId, "EmployeeOfOrganization_ibfk_2");

                entity.HasIndex(e => e.UserId, "User_ID_UQ")
                    .IsUnique();

                entity.Property(e => e.IdEmployeeOfOrganization).HasColumnName("ID_EmployeeOfOrganization");

                entity.Property(e => e.OrganizationId).HasColumnName("Organization_ID");

                entity.Property(e => e.UserId).HasColumnName("User_ID");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.EmployeeOfOrganizations)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EmployeeOfOrganization_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.EmployeeOfOrganization)
                    .HasForeignKey<EmployeeOfOrganization>(d => d.UserId)
                    .HasConstraintName("EmployeeOfOrganization_ibfk_1");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.IdGroup)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.NameGroup, "NameGroup_UQ")
                    .IsUnique();

                entity.HasIndex(e => e.SpecializationId, "Specialization_Groups");

                entity.Property(e => e.IdGroup).HasColumnName("ID_Group");

                entity.Property(e => e.NameGroup).HasMaxLength(50);

                entity.Property(e => e.SpecializationId).HasColumnName("Specialization_ID");

                entity.Property(e => e.YearOfGraduation).HasColumnType("year");

                entity.Property(e => e.YearStartEducation).HasColumnType("year");

                entity.HasOne(d => d.Specialization)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Specialization_Groups");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.IdOrganization)
                    .HasName("PRIMARY");

                entity.ToTable("Organization");

                entity.HasIndex(e => e.InnOrganization, "INN_OrganizationUQ")
                    .IsUnique();

                entity.HasIndex(e => e.FullNameOrganization, "NameOrganizationUQ")
                    .IsUnique();

                entity.HasIndex(e => e.NotFullNameOrganization, "NotFullNameOrganizationUQ")
                    .IsUnique();

                entity.Property(e => e.IdOrganization).HasColumnName("ID_Organization");

                entity.Property(e => e.AddressOrganization).HasMaxLength(250);

                entity.Property(e => e.InnOrganization)
                    .HasMaxLength(15)
                    .HasColumnName("INN_Organization");

                entity.Property(e => e.NameContactOfOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("Name_ContactOfOrganization");

                entity.Property(e => e.PatronymicContactOfOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("Patronymic_ContactOfOrganization");

                entity.Property(e => e.PhoneNumberContactOfOrganization)
                    .HasMaxLength(25)
                    .HasColumnName("PhoneNumber_ContactOfOrganization");

                entity.Property(e => e.SurnameContactOfOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("Surname_ContactOfOrganization");
            });

            modelBuilder.Entity<Practice>(entity =>
            {
                entity.HasKey(e => e.IdPractice)
                    .HasName("PRIMARY");

                entity.ToTable("Practice");

                entity.HasIndex(e => e.NamePractice, "NamePractice_UQ");

                entity.HasIndex(e => e.NameProfModule, "NameProfModule_UQ");

                entity.Property(e => e.IdPractice).HasColumnName("ID_Practice");

                entity.Property(e => e.NamePractice).HasMaxLength(250);

                entity.Property(e => e.NameProfModule).HasMaxLength(250);
            });

            modelBuilder.Entity<PracticeChart>(entity =>
            {
                entity.HasKey(e => e.IdChart)
                    .HasName("PRIMARY");

                entity.ToTable("PracticeChart");

                entity.HasIndex(e => e.PracticeId, "Practice_ID");

                entity.Property(e => e.IdChart).HasColumnName("ID_Chart");

                entity.Property(e => e.DaysPractice).HasMaxLength(50);

                entity.Property(e => e.PracticeId).HasColumnName("Practice_ID");

                entity.HasOne(d => d.Practice)
                    .WithMany(p => p.PracticeCharts)
                    .HasForeignKey(d => d.PracticeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PracticeChart_ibfk_1");
            });

            modelBuilder.Entity<PracticeChartDistibution>(entity =>
            {
                entity.HasKey(e => e.IdPcdist)
                    .HasName("PRIMARY");

                entity.ToTable("PracticeChart_Distibution");

                entity.HasIndex(e => e.OrganizationId, "Organization_ID");

                entity.HasIndex(e => e.PracticeId, "Practice_ID");

                entity.HasIndex(e => e.StudentId, "Student_ID");

                entity.Property(e => e.IdPcdist).HasColumnName("ID_PCDist");

                entity.Property(e => e.OrganizationId).HasColumnName("Organization_ID");

                entity.Property(e => e.PracticeId).HasColumnName("Practice_ID");

                entity.Property(e => e.StudentId).HasColumnName("Student_ID");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.PracticeChartDistibutions)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("PracticeChart_Distibution_ibfk_1");

                entity.HasOne(d => d.Practice)
                    .WithMany(p => p.PracticeChartDistibutions)
                    .HasForeignKey(d => d.PracticeId)
                    .HasConstraintName("PracticeChart_Distibution_ibfk_2");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.PracticeChartDistibutions)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PracticeChart_Distibution_ibfk_3");
            });

            modelBuilder.Entity<PracticeSpecialization>(entity =>
            {
                entity.HasKey(e => e.IdPracticeSpecialization)
                    .HasName("PRIMARY");

                entity.ToTable("Practice_Specialization");

                entity.HasIndex(e => e.PracticeId, "Practice_ID_FK");

                entity.HasIndex(e => e.SpecializationId, "SpecID_FK");

                entity.Property(e => e.IdPracticeSpecialization).HasColumnName("ID_PracticeSpecialization");

                entity.Property(e => e.PracticeId).HasColumnName("Practice_ID");

                entity.Property(e => e.SpecializationId).HasColumnName("Specialization_ID");

                entity.HasOne(d => d.Practice)
                    .WithMany(p => p.PracticeSpecializations)
                    .HasForeignKey(d => d.PracticeId)
                    .HasConstraintName("Practice_Specialization_ibfk_1");

                entity.HasOne(d => d.Specialization)
                    .WithMany(p => p.PracticeSpecializations)
                    .HasForeignKey(d => d.SpecializationId)
                    .HasConstraintName("Practice_Specialization_ibfk_2");
            });

            modelBuilder.Entity<PracticesChartDate>(entity =>
            {
                entity.HasKey(e => e.IdDatePracticeChart)
                    .HasName("PRIMARY");

                entity.ToTable("PracticesChart_Dates");

                entity.HasIndex(e => e.PracticeChartId, "PracticeChart_ID");

                entity.Property(e => e.IdDatePracticeChart).HasColumnName("ID_DatePracticeChart");

                entity.Property(e => e.PracticeChartId).HasColumnName("PracticeChart_ID");

                entity.HasOne(d => d.PracticeChart)
                    .WithMany(p => p.PracticesChartDates)
                    .HasForeignKey(d => d.PracticeChartId)
                    .HasConstraintName("PracticesChart_Dates_ibfk_1");
            });

            modelBuilder.Entity<PracticesChartGroup>(entity =>
            {
                entity.HasKey(e => e.IdPracticesGroups)
                    .HasName("PRIMARY");

                entity.ToTable("PracticesChart_Groups");

                entity.HasIndex(e => e.GroupId, "Group_ID");

                entity.HasIndex(e => e.PracticeChartId, "PracticeChart_ID");

                entity.Property(e => e.IdPracticesGroups).HasColumnName("ID_PracticesGroups");

                entity.Property(e => e.GroupId).HasColumnName("Group_ID");

                entity.Property(e => e.PracticeChartId).HasColumnName("PracticeChart_ID");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.PracticesChartGroups)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("PracticesChart_Groups_ibfk_1");

                entity.HasOne(d => d.PracticeChart)
                    .WithMany(p => p.PracticesChartGroups)
                    .HasForeignKey(d => d.PracticeChartId)
                    .HasConstraintName("PracticesChart_Groups_ibfk_2");
            });

            modelBuilder.Entity<RequestToDistributuion>(entity =>
            {
                entity.HasKey(e => e.IdRequest)
                    .HasName("PRIMARY");

                entity.ToTable("RequestToDistributuion");

                entity.HasIndex(e => e.EmployeeOfTechnikumId, "EmployeeOfTechnikum_ID");

                entity.HasIndex(e => e.StudentId, "Student_ID")
                    .IsUnique();

                entity.Property(e => e.IdRequest).HasColumnName("ID_Request");

                entity.Property(e => e.AddressOrganization).HasMaxLength(255);

                entity.Property(e => e.EmailContactNameOrganization)
                    .HasMaxLength(250)
                    .HasColumnName("Email_ContactNameOrganization");

                entity.Property(e => e.EmployeeOfTechnikumId).HasColumnName("EmployeeOfTechnikum_ID");

                entity.Property(e => e.FullNameOrganization).HasMaxLength(255);

                entity.Property(e => e.InnOrganization)
                    .HasMaxLength(20)
                    .HasColumnName("INN_Organization");

                entity.Property(e => e.NameContactNameOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("Name_ContactNameOrganization");

                entity.Property(e => e.NotFullNameOrganization).HasMaxLength(250);

                entity.Property(e => e.PatronymicNameContactNameOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("PatronymicName_ContactNameOrganization");

                entity.Property(e => e.PhoneNumberContactNameOrganization)
                    .HasMaxLength(25)
                    .HasColumnName("PhoneNumber_ContactNameOrganization");

                entity.Property(e => e.StudentId).HasColumnName("Student_ID");

                entity.Property(e => e.SurnameContactNameOrganization)
                    .HasMaxLength(150)
                    .HasColumnName("Surname_ContactNameOrganization");

                entity.HasOne(d => d.EmployeeOfTechnikum)
                    .WithMany(p => p.RequestToDistributuions)
                    .HasForeignKey(d => d.EmployeeOfTechnikumId)
                    .HasConstraintName("RequestToDistributuion_ibfk_2");

                entity.HasOne(d => d.Student)
                    .WithOne(p => p.RequestToDistributuion)
                    .HasForeignKey<RequestToDistributuion>(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RequestToDistributuion_ibfk_1");
            });

            modelBuilder.Entity<ResponseFromOrganization>(entity =>
            {
                entity.HasKey(e => e.IdResponse)
                    .HasName("PRIMARY");

                entity.ToTable("ResponseFromOrganization");

                entity.HasIndex(e => e.ResumeId, "Resume_ID");

                entity.HasIndex(e => e.VacancyId, "Vacancy_ID");

                entity.Property(e => e.IdResponse).HasColumnName("ID_Response");

                entity.Property(e => e.CommentOrganization).HasColumnType("text");

                entity.Property(e => e.CommentStudent).HasColumnType("text");

                entity.Property(e => e.DateTimeCreate)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Create");

                entity.Property(e => e.ResumeId).HasColumnName("Resume_ID");

                entity.Property(e => e.VacancyId).HasColumnName("Vacancy_ID");

                entity.HasOne(d => d.Resume)
                    .WithMany(p => p.ResponseFromOrganizations)
                    .HasForeignKey(d => d.ResumeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ResponseFromOrganization_ibfk_2");

                entity.HasOne(d => d.Vacancy)
                    .WithMany(p => p.ResponseFromOrganizations)
                    .HasForeignKey(d => d.VacancyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ResponseFromOrganization_ibfk_1");
            });

            modelBuilder.Entity<ResponseFromStudent>(entity =>
            {
                entity.HasKey(e => e.IdResponse)
                    .HasName("PRIMARY");

                entity.ToTable("ResponseFromStudent");

                entity.HasIndex(e => e.StudentId, "Student_ID");

                entity.HasIndex(e => e.VacancyId, "Vacancy_ID");

                entity.Property(e => e.IdResponse).HasColumnName("ID_Response");

                entity.Property(e => e.CommentOrganization).HasColumnType("text");

                entity.Property(e => e.CommentStudent).HasColumnType("text");

                entity.Property(e => e.DateTimeCreate)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Create");

                entity.Property(e => e.StudentId).HasColumnName("Student_ID");

                entity.Property(e => e.VacancyId).HasColumnName("Vacancy_ID");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ResponseFromStudents)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ResponseFromStudent_ibfk_1");

                entity.HasOne(d => d.Vacancy)
                    .WithMany(p => p.ResponseFromStudents)
                    .HasForeignKey(d => d.VacancyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ResponseFromStudent_ibfk_2");
            });

            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(e => e.IdResume)
                    .HasName("PRIMARY");

                entity.ToTable("Resume");

                entity.HasIndex(e => e.StudentId, "Student_ID")
                    .IsUnique();

                entity.Property(e => e.IdResume).HasColumnName("ID_Resume");

                entity.Property(e => e.AboutStudent).HasColumnType("text");

                entity.Property(e => e.AdditionalInformation).HasColumnType("text");

                entity.Property(e => e.DesiredPosition).HasMaxLength(150);

                entity.Property(e => e.Education).HasColumnType("text");

                entity.Property(e => e.FileWithResume).HasMaxLength(250);

                entity.Property(e => e.ProfessionalSkills).HasColumnType("text");

                entity.Property(e => e.StudentId).HasColumnName("Student_ID");

                entity.Property(e => e.TagsSkills).HasColumnType("text");

                entity.Property(e => e.WorkExperience).HasColumnType("text");

                entity.HasOne(d => d.Student)
                    .WithOne(p => p.Resume)
                    .HasForeignKey<Resume>(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Resume_Students");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.IdRole)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.NameRole, "NameRoleUQ")
                    .IsUnique();

                entity.Property(e => e.IdRole).HasColumnName("ID_Role");

                entity.Property(e => e.NameRole).HasMaxLength(100);
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.IdSpecialization)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SpecializationCode, "speccode_UQ")
                    .IsUnique();

                entity.Property(e => e.IdSpecialization).HasColumnName("ID_Specialization");

                entity.Property(e => e.NameQualification).HasMaxLength(100);

                entity.Property(e => e.SpecializationCode).HasMaxLength(20);

                entity.Property(e => e.SpecializationName).HasMaxLength(200);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.IdStudent)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.GroupId, "Groups_Students");

                entity.HasIndex(e => e.UserId, "Us_UQ")
                    .IsUnique();

                entity.Property(e => e.IdStudent).HasColumnName("ID_Student");

                entity.Property(e => e.GroupId).HasColumnName("Group_ID");

                entity.Property(e => e.ImageStudent).HasMaxLength(200);

                entity.Property(e => e.PhoneNumber).HasMaxLength(25);

                entity.Property(e => e.UserId).HasColumnName("User_ID");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Groups_Students");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Student)
                    .HasForeignKey<Student>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Users_Students");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.IdUser)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Email, "Email_UQ")
                    .IsUnique();

                entity.Property(e => e.IdUser).HasColumnName("ID_User");

                entity.Property(e => e.Fz152).HasColumnName("FZ152");

                entity.Property(e => e.NameUser).HasMaxLength(250);

                entity.Property(e => e.Password).HasColumnType("text");

                entity.Property(e => e.PatronymicNameUser).HasMaxLength(250);

                entity.Property(e => e.SurnameUser).HasMaxLength(250);
            });

            modelBuilder.Entity<UsersRole>(entity =>
            {
                entity.HasKey(e => e.IdUsersRoles)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.RoleId, "RoleID_Roles_ForeignKey");

                entity.HasIndex(e => e.UserId, "UserID_Roles_ForeignKey");

                entity.Property(e => e.IdUsersRoles).HasColumnName("ID_UsersRoles");

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.Property(e => e.UserId).HasColumnName("User_ID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UsersRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UsersRoles_ibfk_1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UsersRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UsersRoles_ibfk_2");
            });

            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.HasKey(e => e.IdVacancy)
                    .HasName("PRIMARY");

                entity.ToTable("Vacancy");

                entity.HasIndex(e => e.OrganizationId, "Organization_ID");

                entity.Property(e => e.IdVacancy).HasColumnName("ID_Vacancy");

                entity.Property(e => e.AdditionalInformation).HasColumnType("text");

                entity.Property(e => e.Conditions).HasColumnType("text");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.Duties).HasColumnType("text");

                entity.Property(e => e.NameVacancy).HasMaxLength(50);

                entity.Property(e => e.OrganizationId).HasColumnName("Organization_ID");

                entity.Property(e => e.Requirements).HasColumnType("text");

                entity.Property(e => e.TagsSkills).HasColumnType("text");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Vacancies)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Vacancy_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
