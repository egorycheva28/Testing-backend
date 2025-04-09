using System;
using System.ComponentModel.DataAnnotations;

public class GetTaskDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [MinLength(4)]
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? Deadline { get; set; }
    [Required]
    public string Status { get; set; }
    [Required]
    public string Priority { get; set; }
    [Required]
    public DateOnly CreateTime { get; set; }
    [Required]
    public DateOnly EditTime { get; set; }
}
