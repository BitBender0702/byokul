﻿// <auto-generated />
using System;
using LMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LMS.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20221012051025_Add_ClassTeachers")]
    partial class Add_ClassTeachers
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("LMS.Data.Entity.Class", b =>
                {
                    b.Property<Guid>("ClassId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ClassName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("NoOfStudents")
                        .HasColumnType("int");

                    b.Property<Guid?>("SchoolId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ServiceTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ClassId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.HasIndex("SchoolId");

                    b.HasIndex("ServiceTypeId");

                    b.HasIndex("TeacherId");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("LMS.Data.Entity.ClassDiscipline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DisciplineId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("DisciplineId");

                    b.ToTable("ClassDiscipline");
                });

            modelBuilder.Entity("LMS.Data.Entity.ClassTeacher", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("TeacherId");

                    b.ToTable("ClassTeachers");
                });

            modelBuilder.Entity("LMS.Data.Entity.Country", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("LMS.Data.Entity.Course", b =>
                {
                    b.Property<Guid>("CourseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CourseName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CourseId");

                    b.HasIndex("ClassId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.HasIndex("TeacherId");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("LMS.Data.Entity.Discipline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.ToTable("Disciplines");
                });

            modelBuilder.Entity("LMS.Data.Entity.Language", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("LMS.Data.Entity.School", b =>
                {
                    b.Property<Guid>("SchoolId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("CountryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CoveredPhoto")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("SchoolName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SpecializationId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("SchoolId");

                    b.HasIndex("CountryId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.HasIndex("SpecializationId");

                    b.ToTable("Schools");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolCertificate", b =>
                {
                    b.Property<Guid>("CertificateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CertificateUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CertificateId");

                    b.HasIndex("SchoolId");

                    b.ToTable("SchoolCertificates");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolLanguage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("LanguageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SchoolId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LanguageId");

                    b.HasIndex("SchoolId");

                    b.ToTable("SchoolLanguages");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid?>("SchoolId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SchoolTagValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.HasIndex("SchoolId");

                    b.ToTable("SchoolTags");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolTeacher", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SchoolId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SchoolId");

                    b.HasIndex("TeacherId");

                    b.ToTable("SchoolTeachers");
                });

            modelBuilder.Entity("LMS.Data.Entity.ServiceType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ServiceTypes");
                });

            modelBuilder.Entity("LMS.Data.Entity.Specialization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Specializations");
                });

            modelBuilder.Entity("LMS.Data.Entity.Teacher", b =>
                {
                    b.Property<Guid>("TeacherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedById")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TeacherId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DeletedById");

                    b.ToTable("Teachers");
                });

            modelBuilder.Entity("LMS.Data.Entity.TeacherCertificate", b =>
                {
                    b.Property<Guid>("CertificateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CertificateUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CertificateId");

                    b.HasIndex("TeacherId");

                    b.ToTable("TeacherCertificates");
                });

            modelBuilder.Entity("LMS.Data.Entity.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DOB")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Gender")
                        .HasColumnType("int");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ResetTokenExirationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("TokenCreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UniqueToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("LMS.Data.Entity.Class", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("LMS.Data.Entity.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");

                    b.HasOne("LMS.Data.Entity.ServiceType", "ServiceType")
                        .WithMany()
                        .HasForeignKey("ServiceTypeId");

                    b.HasOne("LMS.Data.Entity.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");

                    b.Navigation("School");

                    b.Navigation("ServiceType");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("LMS.Data.Entity.ClassDiscipline", b =>
                {
                    b.HasOne("LMS.Data.Entity.Class", "Class")
                        .WithMany()
                        .HasForeignKey("ClassId");

                    b.HasOne("LMS.Data.Entity.Discipline", "Discipline")
                        .WithMany()
                        .HasForeignKey("DisciplineId");

                    b.Navigation("Class");

                    b.Navigation("Discipline");
                });

            modelBuilder.Entity("LMS.Data.Entity.ClassTeacher", b =>
                {
                    b.HasOne("LMS.Data.Entity.Class", "Class")
                        .WithMany()
                        .HasForeignKey("ClassId");

                    b.HasOne("LMS.Data.Entity.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId");

                    b.Navigation("Class");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("LMS.Data.Entity.Course", b =>
                {
                    b.HasOne("LMS.Data.Entity.Class", "Class")
                        .WithMany()
                        .HasForeignKey("ClassId");

                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("LMS.Data.Entity.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId");

                    b.Navigation("Class");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("LMS.Data.Entity.Discipline", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");
                });

            modelBuilder.Entity("LMS.Data.Entity.School", b =>
                {
                    b.HasOne("LMS.Data.Entity.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId");

                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("LMS.Data.Entity.Specialization", "Specialization")
                        .WithMany()
                        .HasForeignKey("SpecializationId");

                    b.Navigation("Country");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");

                    b.Navigation("Specialization");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolCertificate", b =>
                {
                    b.HasOne("LMS.Data.Entity.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolLanguage", b =>
                {
                    b.HasOne("LMS.Data.Entity.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId");

                    b.HasOne("LMS.Data.Entity.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");

                    b.Navigation("Language");

                    b.Navigation("School");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolTag", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.HasOne("LMS.Data.Entity.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");

                    b.Navigation("School");
                });

            modelBuilder.Entity("LMS.Data.Entity.SchoolTeacher", b =>
                {
                    b.HasOne("LMS.Data.Entity.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");

                    b.HasOne("LMS.Data.Entity.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId");

                    b.Navigation("School");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("LMS.Data.Entity.Teacher", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", "DeletedBy")
                        .WithMany()
                        .HasForeignKey("DeletedById");

                    b.Navigation("CreatedBy");

                    b.Navigation("DeletedBy");
                });

            modelBuilder.Entity("LMS.Data.Entity.TeacherCertificate", b =>
                {
                    b.HasOne("LMS.Data.Entity.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LMS.Data.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("LMS.Data.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
