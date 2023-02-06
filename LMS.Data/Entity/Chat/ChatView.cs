﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Data.Entity.Chat
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ChatHeadId { get; set; }
        public virtual ChatHead ChatHead { get; set; }
        public string? Message { get; set; }
        public string? LikedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsEdited { get; set; }
        public bool IsRead { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public List<Attachment> Attachments { get; set; }
        
    }

     public class Attachment
    {
        [Key]   
        public Guid Id { get; set; }
        public Guid ChatMessageId { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public FileTypeEnum FileType { get; set; }    
        public string FileName { get; set; }
        public string FileURL{ get; set; }
    }

    public enum FileTypeEnum
    {
        Image = 1,
        Video = 2,
        Attachment = 3,
    }

}