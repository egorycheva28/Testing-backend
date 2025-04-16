using System;
using System.ComponentModel.DataAnnotations;
using To_Do_List_backend.Models;

namespace To_Do_List_backend.DTO
{
    public class EditTaskDTO
    {
        [Required]
        [MinLength(4)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? Deadline { get; set; }
        public Priority? Priority { get; set; }
    }
}
