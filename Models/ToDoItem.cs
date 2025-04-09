using System;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
//using K4os.Hash.xxHash;

public class ToDoItem
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [MinLength(4)]
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? Deadline { get; set; }
    [Required]
    public Status Status { get; set; }
    [Required]
    public Priority Priority { get; set; }
    [Required]
    public DateOnly CreateTime { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? EditTime { get; set; } 
}
