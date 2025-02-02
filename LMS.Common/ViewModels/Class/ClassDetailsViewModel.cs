﻿using LMS.Common.ViewModels.Accessibility;
using LMS.Common.ViewModels.Common;
using LMS.Common.ViewModels.Post;
using LMS.Common.ViewModels.School;
using LMS.Common.ViewModels.ServiceType;
using LMS.Common.ViewModels.Student;
using LMS.Common.ViewModels.Teacher;
using LMS.Data.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Common.ViewModels.Class
{
    public class ClassDetailsViewModel
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public int NoOfStudents { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? SchoolId { get; set; }
        public SchoolViewModel School { get; set; }
        public Guid? ServiceTypeId { get; set; }
        public ServiceTypeViewModel ServiceType { get; set; }
        public Guid? AccessibilityId { get; set; }
        public string ClassUrl { get; set; }
        public AccessibilityViewModel Accessibility { get; set; }
        public IEnumerable<LanguageViewModel> Languages { get; set; }
        public IEnumerable<TeacherViewModel> Teachers { get; set; }
        public int Students { get; set; }
        public IEnumerable<DisciplineViewModel> Disciplines { get; set; }
        public IEnumerable<PostDetailsViewModel> Posts { get; set; }
        public IEnumerable<PostDetailsViewModel> Reels { get; set; }
        public IEnumerable<CertificateViewModel> ClassCertificates { get; set; }
        public string? Description { get; set; }
        public long? Price { get; set; }
        public string? Currency { get; set; }
        public Double? Rating { get; set; }
        public string? Avatar { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsDisableByOwner { get; set; }
        public bool IsCommentsDisabled { get; set; }
        public bool IsClassAccessable { get; set; }
        public bool IsRatedByUser { get; set; }
        public bool IsBannedFromClassCourse { get; set; }
        public bool IsClassStudent { get; set; }
        //public bool IsFileStorageAccessible { get; set; }
        public bool IsEnable { get; set; }
    }
}
