using System;
using System.ComponentModel.DataAnnotations;

public class EditTaskDTO
{
    [Required]
    [MinLength(4)]
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? Deadline { get; set; }
    public Priority? Priority { get; set; }
}
