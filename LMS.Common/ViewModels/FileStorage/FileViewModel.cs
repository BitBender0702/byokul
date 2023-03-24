﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Common.ViewModels.FileStorage
{
    public class FileViewModel
    {
        public Guid? Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public int? FileType { get; set; }
        public Guid? FolderId { get; set; }
        public Guid? ParentId { get; set; }
    }
}
