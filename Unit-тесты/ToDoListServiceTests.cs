using System;
using Moq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using To_Do_List_backend.Services; 
using To_Do_List_backend.Models; 
using To_Do_List_backend.DTO;
using To_Do_List_backend.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using FluentAssertions;
using Microsoft.Build.Framework;

public class ToDoListServiceTests
{
    private readonly Context _context;
    private readonly ToDoListService _service;
    public ToDoListServiceTests()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new Context(options);
        _service = new ToDoListService(_context);
    }

    /// <summary>
    /// Тест для проверки обработки null task
    /// </summary>
    [Fact]
    public async Task AddTaskNullTaskThrowsException()
    {
        NewItemDTO task = null;

        await Assert.ThrowsAsync<Exception>(() => _service.AddTask(task));
    }

    /// <summary>
    /// Тест для проверки добавления задач с корректными входными данными
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>

    [Theory]
    [InlineData("Task 1", "Description 1", null, Status.Active)]
    [InlineData("Task 2", "Description 3", "2025-04-15", Status.Overdue)]
    [InlineData("Task 3", "Description 4", "2025-04-16", Status.Active)]
    [InlineData("Task 4", "Description 4", "2025-04-17", Status.Active)]
    public async Task AddTaskValidInputsAddsTask(string name, string description, string deadlineInput, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
        };

        var resultId = await _service.AddTask(task);

        var addedTask = await _context.ToDoItems.FindAsync(resultId);
        Assert.NotNull(addedTask);
        Assert.Equal(name, addedTask.Name);
        Assert.Equal(description, addedTask.Description);
        Assert.Equal(expectedStatus, addedTask.Status);
    }

    /// <summary>
    /// Тест для проверки приоритета из заголовка и удаление макроса при добавлении задачи 
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedPriority">Ожидаемый приоритет задачи.</param>
    /// <param name="expectedName">Ожидаемый название задачи.</param>

    [Theory]
    [InlineData("Sample Task !1", "Description1", null, Priority.Critical, "Sample Task")]
    [InlineData("Sample Task !2", "Description2", null, Priority.High, "Sample Task")]
    [InlineData("Sample Task !3", "Description3", null, Priority.Medium, "Sample Task")]
    [InlineData("Sample Task !4", "Description4", null, Priority.Low, "Sample Task")]
    [InlineData("Sample Task", "Description2", null, Priority.Medium, "Sample Task")]
    public async Task AddTask_PriorityInName_SetsCorrectPriority1(string name, string description, Priority priority, Priority expectedPriority, string expectedName)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task = new NewItemDTO
        {
            Name = name,
            Description = description
        };
       
        var newTask = new ToDoItem
        {
            Name = task.Name,
            Description = task.Description
        };

        await _context.ToDoItems.AddAsync(newTask);
        await _context.SaveChangesAsync();

        await _service.AddPriority(newTask, task);

        var addedTask = await _context.ToDoItems.FindAsync(newTask.Id);
        Assert.Equal(expectedPriority, addedTask.Priority);
        Assert.Equal(expectedName, addedTask.Name);
    }

    /// <summary>
    /// Тест для проверки приоритета из заголовка и формы одновременно и удаление макроса при добавлении задачи 
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedPriority">Ожидаемый приоритет задачи.</param>
    /// <param name="expectedName">Ожидаемый название задачи.</param>

    [Theory]
    [InlineData("Sample Task !1", "Description5", Priority.Low, Priority.Low, "Sample Task")]
    [InlineData("Sample Task !2", "Description6", Priority.Medium, Priority.Medium, "Sample Task")]
    [InlineData("Sample Task !3", "Description7", Priority.High, Priority.High, "Sample Task")]
    [InlineData("Sample Task !4", "Description8", Priority.Critical, Priority.Critical, "Sample Task")]
    public async Task AddTask_PriorityInName_SetsCorrectPriority2(string name, string description, Priority priority, Priority expectedPriority, string expectedName)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task = new NewItemDTO
        {
            Name = name,
            Description = description,
            Priority = priority
        };

        var newTask = new ToDoItem
        {
            Name = task.Name,
            Description = task.Description
        };

        await _context.ToDoItems.AddAsync(newTask);
        await _context.SaveChangesAsync();

        await _service.AddPriority(newTask, task);

        var addedTask = await _context.ToDoItems.FindAsync(newTask.Id);
        Assert.Equal(expectedPriority, addedTask.Priority);
        Assert.Equal(expectedName, addedTask.Name);
    }

    /// <summary>
    /// Тест для проверки дедлайна и удаления макроса  при добавлении задачи 
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedDeadline">Ожидаемый дедлайн задачи.</param>
    /// <param name="expectedName">Ожидаемый название задачи.</param>
     
    [Theory]
    [InlineData("Task with deadline", "Description", null, null, "Task with deadline")]
    [InlineData("Task with deadline !before 17-04-2025", "Description", "2025-04-16", "2025-04-16", "Task with deadline")]
    [InlineData("Task with deadline !before 17-04-2025", "Description", null, "2025-04-17", "Task with deadline")]

    public async Task AddTask_DeadlineInName_SetsCorrectDeadline(string name, string description, string deadlineInput, string expectedDeadline, string expectedName)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline1) ? deadline1 : null
        };

        var newTask = new ToDoItem
        {
            Name = task.Name,
            Description = task.Description
        };

        await _context.ToDoItems.AddAsync(newTask);
        await _context.SaveChangesAsync();

        await _service.AddDeadline(newTask, task);

        var addedTask = await _context.ToDoItems.FindAsync(newTask.Id);
        Assert.NotNull(addedTask);
        Assert.Equal(DateOnly.TryParse(expectedDeadline, out var deadline2) ? deadline2 : null, addedTask.Deadline);
        Assert.Equal(expectedName, addedTask.Name);
    }

    /// <summary>
    /// Тест для проверки обработки неправильного id при редактировании задачи
    /// </summary>
    [Fact]
    public async Task NotFoundEditTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.EditTask(taskId, null));
    }

    /// <summary>
    /// Тест для проверки редактирования задач с корректными данными
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    [Theory]
    [InlineData("Task 1", "Description 1", null, Status.Active)]
    [InlineData("Task 2", "Description 3", "2025-04-15", Status.Overdue)]
    [InlineData("Task 3", "Description 4", "2025-04-16", Status.Active)]
    [InlineData("Task 4", "Description 4", "2025-04-17", Status.Active)]
    public async Task EditTaskValidInputsEditsTask(string name, string description, string deadlineInput, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var taskId = Guid.NewGuid();

        var existingTask = new ToDoItem
        {
            Id = taskId,
            Name = "Task",
            Description = "Description",
            Status = Status.Active,
            Deadline = DateOnly.Parse("2025-04-16"),
            Priority = Priority.Medium
        };

        await _context.ToDoItems.AddAsync(existingTask);
        await _context.SaveChangesAsync();

        var editTask = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline1) ? deadline1 : null,
            Priority = Priority.Critical 
        };

        await _service.EditTask(taskId, editTask);

        var updatedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(name, updatedTask.Name);
        Assert.Equal(description, updatedTask.Description);
        Assert.Equal(expectedStatus, updatedTask.Status);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), updatedTask.EditTime);
    }

    /// <summary>
    /// Тест для проверки приоритета и удаления макроса при редактировании задачи 
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedPriority">Ожидаемый приоритет задачи.</param>
    /// <param name="expectedName">Ожидаемое название задачи.</param>

    [Theory]
    [InlineData("Sample Task !1", "Description5", Priority.Low, Priority.Low, "Sample Task")]
    [InlineData("Sample Task !2", "Description6", Priority.Medium, Priority.Medium, "Sample Task")]
    [InlineData("Sample Task !3", "Description7", Priority.High, Priority.High, "Sample Task")]
    [InlineData("Sample Task !4", "Description8", Priority.Critical, Priority.Critical, "Sample Task")]
    public async Task EditTask_PriorityInName_SetsCorrectPriority(string name, string description, Priority priority, Priority expectedPriority, string expectedName)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var taskId = Guid.NewGuid();

        var existingTask = new ToDoItem
        {
            Id = taskId,
            Name = "Task",
            Description = "Description",
            Status = Status.Active,
            Deadline = DateOnly.Parse("2025-04-16"),
            Priority = Priority.Medium
        };

        await _context.ToDoItems.AddAsync(existingTask);
        await _context.SaveChangesAsync();

        var editTask = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.Parse("2025-04-16"),
            Priority = priority
        };

        await _service.EditTask(taskId, editTask);

        var updatedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(expectedName, updatedTask.Name);
        Assert.Equal(expectedPriority, updatedTask.Priority);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), updatedTask.EditTime);
    }

    /// <summary>
    /// Тест для проверки дедлайна и удаления макроса при редактировании задачи 
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedDeadline">Ожидаемый дедлайн задачи.</param>
    /// <param name="expectedName">Ожидаемое название задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>

    [Theory]
    [InlineData("Task with deadline", "Description 1", null, null, "Task with deadline", Status.Active)]
    [InlineData("Task with deadline !before 17-04-2025", "Description 2", "2025-04-15", "2025-04-15", "Task with deadline", Status.Overdue)]
    [InlineData("Task with deadline !before 17-04-2025", "Description 3", null, "2025-04-17", "Task with deadline", Status.Active)]

    public async Task Editask_DeadlineInName_SetsCorrectDeadline(string name, string description, string deadlineInput, string expectedDeadline, string expectedName, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var taskId = Guid.NewGuid();

        var existingTask = new ToDoItem
        {
            Id = taskId,
            Name = "Task",
            Description = "Description",
            Status = Status.Active,
            Deadline = DateOnly.Parse("2025-04-16"),
            Priority = Priority.Medium
        };

        await _context.ToDoItems.AddAsync(existingTask);
        await _context.SaveChangesAsync();

        var editTask = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline1) ? deadline1 : null,
            Priority = Priority.Critical
        };

        await _service.EditTask(taskId, editTask);

        var updatedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(expectedName, updatedTask.Name);
        Assert.Equal(expectedStatus, updatedTask.Status);
        Assert.Equal(DateOnly.TryParse(expectedDeadline, out var deadline2) ? deadline2 : null, updatedTask.Deadline);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), updatedTask.EditTime);
    }
    /// <summary>
    /// Тест для проверки обработки неправильного id при удалении задачи
    /// </summary>
    [Fact]
    public async Task NotFoundDeleteTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.DeleteTask(taskId));
    }

    /// <summary>
    /// Тест для проверки удаления задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>

    [Theory]
    [InlineData("Task 1", "Description 1", null)]
    [InlineData("Task 2", "Description 2", "2025-04-16")]
    public async Task DeleteTaskCorrectly(string name, string description, string deadlineInput)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        var taskToDelete = new ToDoItem
        {
            Id = taskId,
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null
        };

        await _context.ToDoItems.AddAsync(taskToDelete);
        await _context.SaveChangesAsync();

        await _service.DeleteTask(taskId);

        var deletedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.Null(deletedTask);
    }

    /// <summary>
    /// Тест для проверки обработки неправильного id при смене статуса задачи на выполнено
    /// </summary>
    [Fact]
    public async Task NotFoundCompleteTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.CompleteTask(taskId));
    }

    /// <summary>
    /// Тест для проверки обработки смены статуса задачи на выполнено
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    
    [Theory]
    [InlineData("Task 1", "Description 1", null, Status.Completed)]
    [InlineData("Task 2", "Description 2", "2025-04-15", Status.Late)]
    [InlineData("Task 3", "Description 3", "2025-04-16", Status.Completed)]
    [InlineData("Task 4", "Description 4", "2025-04-17", Status.Completed)]
    public async Task CompleteTaskCorrectly(string name, string description, string deadlineInput, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        var taskToComplete = new ToDoItem
        {
            Id = taskId,
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Status = Status.Active,
            Priority = Priority.Medium,
            CreateTime = DateOnly.FromDateTime(DateTime.UtcNow),
            EditTime = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _context.ToDoItems.AddAsync(taskToComplete);
        await _context.SaveChangesAsync();

        await _service.CompleteTask(taskId);

        var completedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(completedTask);
        Assert.Equal(expectedStatus, completedTask.Status);
        
    }

    /// <summary>
    /// Тест для проверки обработки неправильного id при смене статуса задачи на невыполнено
    /// </summary>

    [Fact]
    public async Task NotFoundIncompleteTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.InCompleteTask(taskId));
    }

    /// <summary>
    /// Тест для проверки обработки смены статуса задачи на невыполнено
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    
    [Theory]
    [InlineData("Task 1", "Description 1", null, Status.Active)]
    [InlineData("Task 2", "Description 2", "2025-04-15", Status.Overdue)]
    [InlineData("Task 3", "Description 3", "2025-04-16", Status.Active)]
    [InlineData("Task 4", "Description 4", "2025-04-17", Status.Active)]
    public async Task IncompleteTaskCorrectly(string name, string description, string deadlineInput, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        var taskToIncomplete = new ToDoItem
        {
            Id = taskId,
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Status = Status.Completed,
            Priority = Priority.Medium,
            CreateTime = DateOnly.FromDateTime(DateTime.UtcNow),
            EditTime = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _context.ToDoItems.AddAsync(taskToIncomplete);
        await _context.SaveChangesAsync();

        await _service.InCompleteTask(taskId);

        var incompletedTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(incompletedTask);
        Assert.Equal(expectedStatus, incompletedTask.Status);

    }

    /// <summary>
    /// Тест для проверки обработки неправильного id при получении задачи
    /// </summary>
    [Fact]
    public async Task NotFoundGetTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.GetTask(taskId));
    }

    /// <summary>
    /// Тест для проверки получения задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    
    [Theory]
    [InlineData("Task 1", "Description 1", null)]
    [InlineData("Task 2", "Description 2", "2025-04-17")]
    public async Task GetTaskCorrectly(string name, string description, string deadlineInput)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        var taskToGet = new ToDoItem
        {
            Id = taskId,
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Status = Status.Active,
            Priority = Priority.Medium,
            CreateTime = DateOnly.FromDateTime(DateTime.UtcNow),
            EditTime = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _context.ToDoItems.AddAsync(taskToGet);
        await _context.SaveChangesAsync();

        var result = await _service.GetTask(taskId);

        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.Equal(Status.Active.ToString(), result.Status);
        Assert.Equal(Priority.Medium.ToString(), result.Priority);
        Assert.Equal(DateOnly.Parse("2025-04-16"), result.CreateTime);
        Assert.Equal(DateOnly.Parse("2025-04-16"), result.EditTime);
    }

    /// <summary>
    /// Тест для проверки обработки неправильного id при автоматической смене статуса задачи
    /// </summary>

    [Fact]
    public async Task NotFoundChangeStatusTaskThrowsException()
    {
        Guid taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<Exception>(() => _service.ChangeStatus(taskId));
    }

    /// <summary>
    /// Тест для проверки автоматичексой смены статуса задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    
    [Theory]
    [InlineData("Task 1", "Description 1", null, Status.Active)]
    [InlineData("Task 2", "Description 2", "2025-04-15", Status.Overdue)]
    [InlineData("Task 3", "Description 3", "2025-04-16", Status.Active)]
    [InlineData("Task 4", "Description 4", "2025-04-17", Status.Active)]
    public async Task ChangeStatusTaskCorrectly(string name, string description, string deadlineInput, Status expectedStatus)
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        var taskToComplete = new ToDoItem
        {
            Id = taskId,
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Status = Status.Active,
            Priority = Priority.Medium,
            CreateTime = DateOnly.FromDateTime(DateTime.UtcNow),
            EditTime = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _context.ToDoItems.AddAsync(taskToComplete);
        await _context.SaveChangesAsync();

        await _service.ChangeStatus(taskId);

        var changeStatusTask = await _context.ToDoItems.FindAsync(taskId);
        Assert.NotNull(changeStatusTask);
        Assert.Equal(expectedStatus, changeStatusTask.Status);

    }

    /// <summary>
    /// Тест для проверки получения списка задач
    /// </summary>
    [Fact]
    public async Task GetListCorrectly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();


        await _context.ToDoItems.AddRangeAsync(new List<ToDoItem>
        {
            new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Description = "Description 1", Status = Status.Active, Priority = Priority.Critical },
            new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2",  Description = "Description 2", Status = Status.Completed, Priority = Priority.Low }
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetList(null);

        Assert.NotNull(result.Tasks);
        Assert.Equal(2, result.Tasks.Count);
    }

    /// <summary>
    /// Тест для проверки сортировки по возрастанию статуса
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByStatusAsc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Status = Status.Completed };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Status = Status.Active };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Status = Status.Overdue };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4", Status = Status.Late };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.StatusAsc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task2.Status, result.Tasks[0].Status);
        Assert.Equal(task1.Status, result.Tasks[1].Status);
        Assert.Equal(task3.Status, result.Tasks[2].Status);
        Assert.Equal(task4.Status, result.Tasks[3].Status);
    }

    /// <summary>
    /// Тест для проверки сортировки по убыванию статуса
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByStatusDesc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Status = Status.Completed };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Status = Status.Active };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Status = Status.Overdue };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4", Status = Status.Late };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.StatusDesc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task4.Status, result.Tasks[0].Status);
        Assert.Equal(task3.Status, result.Tasks[1].Status);
        Assert.Equal(task1.Status, result.Tasks[2].Status);
        Assert.Equal(task2.Status, result.Tasks[3].Status);
    }

    /// <summary>
    /// Тест для проверки сортировки по возрастанию приоритета
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByPriorityAsc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Priority = Priority.Low };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Priority = Priority.High };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Priority = Priority.Critical };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4", Priority = Priority.Medium };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.PriorityAsc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task1.Priority, result.Tasks[0].Priority);
        Assert.Equal(task4.Priority, result.Tasks[1].Priority);
        Assert.Equal(task2.Priority, result.Tasks[2].Priority);
        Assert.Equal(task3.Priority, result.Tasks[3].Priority);
    }

    /// <summary>
    /// Тест для проверки сортировки по убыванию приоритета
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByPriorityDesc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Priority = Priority.Low };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Priority = Priority.High };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Priority = Priority.Critical };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4", Priority = Priority.Medium };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.PriorityDesc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task3.Priority, result.Tasks[0].Priority);
        Assert.Equal(task2.Priority, result.Tasks[1].Priority);
        Assert.Equal(task4.Priority, result.Tasks[2].Priority);
        Assert.Equal(task1.Priority, result.Tasks[3].Priority);
    }

    /// <summary>
    /// Тест для проверки сортировки по возрастанию дедлайна
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByDeadlineAsc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Deadline = DateOnly.TryParse("2025-04-17", out var deadline1) ? deadline1 : null };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Deadline = DateOnly.TryParse("2025-04-15", out var deadline2) ? deadline2 : null };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Deadline = DateOnly.TryParse("2025-04-16", out var deadline3) ? deadline3 : null };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4"};

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.DeadlineAsc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task4.Deadline, result.Tasks[0].Deadline);
        Assert.Equal(task2.Deadline, result.Tasks[1].Deadline);
        Assert.Equal(task3.Deadline, result.Tasks[2].Deadline);
        Assert.Equal(task1.Deadline, result.Tasks[3].Deadline);
    }

    /// <summary>
    /// Тест для проверки сортировки по убыванию дедлайна
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByDeadlineDesc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1", Deadline = DateOnly.TryParse("2025-04-17", out var deadline1) ? deadline1 : null };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2", Deadline = DateOnly.TryParse("2025-04-15", out var deadline2) ? deadline2 : null };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3", Deadline = DateOnly.TryParse("2025-04-16", out var deadline3) ? deadline3 : null };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4"};

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.DeadlineDesc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task1.Deadline, result.Tasks[0].Deadline);
        Assert.Equal(task3.Deadline, result.Tasks[1].Deadline);
        Assert.Equal(task2.Deadline, result.Tasks[2].Deadline);
        Assert.Equal(task4.Deadline, result.Tasks[3].Deadline);
    }

    /// <summary>
    /// Тест для проверки сортировки по возрастанию даты создания
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByCreateTimeAsc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1" };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2" };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3" };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4" };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.CreateTimeAsc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task1.CreateTime, result.Tasks[0].CreateTime);
        Assert.Equal(task2.CreateTime, result.Tasks[1].CreateTime);
        Assert.Equal(task3.CreateTime, result.Tasks[2].CreateTime);
        Assert.Equal(task4.CreateTime, result.Tasks[3].CreateTime);
    }

    /// <summary>
    /// Тест для проверки сортировки по убыванию даты создания
    /// </summary>
    [Fact]
    public async Task GetList_SortsTasksByCreateTimeDesc_Correctly()
    {
        _context.ToDoItems.RemoveRange(await _context.ToDoItems.ToListAsync());
        await _context.SaveChangesAsync();

        var task1 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 1" };
        var task2 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 2" };
        var task3 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 3" };
        var task4 = new ToDoItem { Id = Guid.NewGuid(), Name = "Task 4" };

        await _context.ToDoItems.AddRangeAsync(task1, task2, task3, task4);
        await _context.SaveChangesAsync();

        var result = await _service.GetList(Sorting.CreateTimeDesc);

        Assert.NotNull(result.Tasks);
        Assert.Equal(4, result.Tasks.Count);
        Assert.Equal(task1.CreateTime, result.Tasks[0].CreateTime);
        Assert.Equal(task3.CreateTime, result.Tasks[1].CreateTime);
        Assert.Equal(task2.CreateTime, result.Tasks[2].CreateTime);
        Assert.Equal(task4.CreateTime, result.Tasks[3].CreateTime);
    }
}
