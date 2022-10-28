﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Data.Entity
{
    public class SchoolLanguage
    {
        public Guid Id { get; set; }
        public Guid? SchoolId { get; set; }
        public School School { get; set; }
        public Guid? LanguageId { get; set; }
        public Language Language { get; set; }
    }
}
