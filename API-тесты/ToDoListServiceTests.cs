using Xunit;
using To_Do_List_backend.Models;
using To_Do_List_backend.DTO;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using To_Do_List_backend.Program;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

public class ToDoListServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ToDoListServiceTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Проверка данных при добавлении задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает id добавленной задачи.</returns>
    [Theory]
    [InlineData("Task 1", "Description 1", "2025-04-15", Priority.Low, HttpStatusCode.OK)]
    [InlineData("Task 2", null, null, null, HttpStatusCode.OK)]
    [InlineData("Task", "Description", "2025-04-15", Priority.Low, HttpStatusCode.OK)]
    [InlineData("Task3", "Description 3", "2025-04-19", Priority.Low, HttpStatusCode.OK)]
    public async Task AddTask(string name, string description, string deadlineInput, Priority priority, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Priority = priority
        };

        var request = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/add", request);

        var content = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(content);
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var taskId = JsonConvert.DeserializeObject<Guid>(content);
        await _client.DeleteAsync($"/api/{taskId}");
    }

    /// <summary>
    /// Проверка данных при добавлении задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(null, null, null, null, HttpStatusCode.BadRequest)]
    [InlineData(null, "Description 2", "2025-04-17", Priority.Low, HttpStatusCode.BadRequest)]
    [InlineData("abc", "abc", "2025-04-18", Priority.Low, HttpStatusCode.BadRequest)]
    public async Task AddWrongTask(string name, string description, string deadlineInput, Priority priority, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Priority = priority
        };

        var request = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/add", request);

        var content = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(content);
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка данных при редактировании задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает Ok.</returns>
    [Theory]
    [InlineData("Task 1", "Description 1", "2025-04-28", Priority.Medium, HttpStatusCode.OK)]
    [InlineData("Task 2", null, null, Priority.Low, HttpStatusCode.OK)]
    [InlineData("Task", "Description", "2025-04-25", Priority.Low, HttpStatusCode.OK)]
    [InlineData("Task3", "Description 3", "2025-04-29", Priority.Low, HttpStatusCode.OK)]
    public async Task EditTask(string name, string description, string deadlineInput, Priority priority, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = "NewTask",
            Description = "NewDescription",
            Deadline = DateOnly.TryParse("2025-04-30", out var deadline1) ? deadline1 : null,
            Priority = Priority.Critical
        };

        var request1 = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request1);

        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        var editTask = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Priority = priority
        };
        var request = new StringContent(JsonConvert.SerializeObject(editTask), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/edit/{_taskId}", request);

        Assert.Equal(expectedStatusCode, response.StatusCode);

        var response2 = await _client.GetAsync($"/api/{_taskId}");
        var content = await response2.Content.ReadAsStringAsync();
        var task = JsonConvert.DeserializeObject<GetTaskDTO>(content);
        Assert.Equal(name, task.Name);
        Assert.Equal(description, task.Description);
        Assert.Equal(DateOnly.TryParse(deadlineInput, out var deadline2) ? deadline2 : null, task.Deadline);
        Assert.Equal(priority.ToString(), task.Priority);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), task.EditTime);

        await _client.DeleteAsync($"/api/{_taskId}");
    }

    /// <summary>
    /// Проверка данных при редактировании задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(null, null, null, Priority.Low, HttpStatusCode.BadRequest)]
    [InlineData(null, "Description 2", "2025-04-27", Priority.Low, HttpStatusCode.BadRequest)]
    [InlineData("abc", "abc", "2025-04-28", Priority.Low, HttpStatusCode.BadRequest)]
    public async Task EditTaskWithWrongData(string name, string description, string deadlineInput, Priority priority, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = "NewTask",
            Description = "NewDescription",
            Deadline = DateOnly.TryParse("2025-04-30", out var deadline1) ? deadline1 : null,
            Priority = Priority.Critical
        };

        var request1 = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request1);

        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        var editTask = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Priority = priority
        };
        var request = new StringContent(JsonConvert.SerializeObject(editTask), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/edit/{_taskId}", request);

        Assert.Equal(expectedStatusCode, response.StatusCode);

        await _client.DeleteAsync($"/api/{_taskId}");
    }

    /// <summary>
    /// Проверка данных при редактировании задачи с неправильным id
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData("Task3", "Description 3", "2025-04-19", Priority.Low, HttpStatusCode.NotFound)]
    [InlineData("abc", "Description 3", "2025-04-19", Priority.Low, HttpStatusCode.BadRequest)]
    public async Task EditTaskWithWrongId(string name, string description, string deadlineInput, Priority priority, HttpStatusCode expectedStatusCode)
    {
        Guid taskId = new Guid();

        var task = new EditTaskDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline) ? deadline : null,
            Priority = priority
        };
        var request = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/edit/{taskId}", request);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка id при удалении задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает Ok.</returns>
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    public async Task DeleteTask(HttpStatusCode expectedStatusCode)
    {
        var task = new NewItemDTO
        {
            Name = "NewTask"
        };

        var request = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request);
        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        var response2 = await _client.DeleteAsync($"/api/{_taskId}");
        Assert.Equal(expectedStatusCode, response2.StatusCode);

        var deletedTask = await _client.GetAsync($"/api/{_taskId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedTask.StatusCode);
    }

    /// <summary>
    /// Проверка неправильного id при удалении задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task DeleteWrongTask(HttpStatusCode expectedStatusCode)
    {
        Guid taskId = new Guid();
        var response = await _client.DeleteAsync($"/api/{taskId}");

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка id при выполнении задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает Ok.</returns>
    [Theory]
    [InlineData("NewTask1", "NewDescription1", "2025-04-30", Priority.Low, Status.Completed, HttpStatusCode.OK)]
    [InlineData("NewTask2", "NewDescription2", "2025-04-24", Priority.Low, Status.Late, HttpStatusCode.OK)]
    public async Task CompleteTask(string name, string description, string deadlineInput, Priority priority, Status expectedStatus, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline1) ? deadline1 : null,
            Priority = priority
        };

        var request = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request);
        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        var response = await _client.PatchAsync($"/api/complete/{_taskId}", null);
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var response2 = await _client.GetAsync($"/api/{_taskId}");
        var content = await response2.Content.ReadAsStringAsync();
        var task = JsonConvert.DeserializeObject<ToDoItem>(content);
        Assert.Equal(expectedStatus, task.Status);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), task.EditTime);

        await _client.DeleteAsync($"/api/{_taskId}");
    }

    /// <summary>
    /// Проверка неправильного id при выполнении задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task CompleteWrongTask(HttpStatusCode expectedStatusCode)
    {
        Guid taskId = new Guid();

        var response = await _client.PatchAsync($"/api/complete/{taskId}", null);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка id при невыполнении задачи
    /// </summary>
    /// <param name="name">ЗНазвание задачи.</param>
    /// <param name="description">Описание задачи.</param>
    /// <param name="deadlineInput">Дедлайн задачи.</param>
    /// <param name="priority">Приоритет задачи.</param>
    /// <param name="expectedStatus">Ожидаемый статус задачи.</param>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает Ok.</returns>
    [Theory]
    [InlineData("NewTask1", "NewDescription1", "2025-04-30", Priority.Low, Status.Active, HttpStatusCode.OK)]
    [InlineData("NewTask2", "NewDescription2", "2025-04-24", Priority.Low, Status.Overdue, HttpStatusCode.OK)]
    public async Task IncompleteTask(string name, string description, string deadlineInput, Priority priority, Status expectedStatus, HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = name,
            Description = description,
            Deadline = DateOnly.TryParse(deadlineInput, out var deadline1) ? deadline1 : null,
            Priority = priority
        };

        var request = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request);
        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        await _client.PatchAsync($"/api/complete/{_taskId}", null);

        var response = await _client.PatchAsync($"/api/incomplete/{_taskId}", null);
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var response3 = await _client.GetAsync($"/api/{_taskId}");
        var content = await response3.Content.ReadAsStringAsync();
        var task = JsonConvert.DeserializeObject<ToDoItem>(content);
        Assert.Equal(expectedStatus, task.Status);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), task.EditTime);

        await _client.DeleteAsync($"/api/{_taskId}");
    }

    /// <summary>
    /// Проверка неправильного id при невыполнении задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task IncompleteWrongTask(HttpStatusCode expectedStatusCode)
    {
        Guid taskId = new Guid();

        var response = await _client.PatchAsync($"/api/incomplete/{taskId}", null);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка правильного получения списка задач
    /// </summary>
    /// <returns>Возвращает список задач или сообщение об ошибке.</returns>
    [Fact]
    public async Task GetListTasks()
    {
        var response = await _client.GetAsync($"/api");

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Проверка неправильного id при автоматическом редактировании задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает сообщение об ошибке.</returns>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task ChangeStatusWrongTask(HttpStatusCode expectedStatusCode)
    {
        Guid taskId = new Guid();

        var response = await _client.PatchAsync($"/api/changeStatus/{taskId}", null);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Проверка id при автоматическом редактировании задачи
    /// </summary>
    /// <param name="expectedStatusCode">Ожидаемый статус ответа на запрос.</param>
    /// <returns>Возвращает Ok.</returns>
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    public async Task ChangeStatus(HttpStatusCode expectedStatusCode)
    {
        var newTask = new NewItemDTO
        {
            Name = "NewTask",
            Description = "NewDescription",
            Deadline = DateOnly.TryParse("2025-04-30", out var deadline1) ? deadline1 : null,
            Priority = Priority.Critical
        };

        var request = new StringContent(JsonConvert.SerializeObject(newTask), Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/add", request);
        var taskId = await response1.Content.ReadAsStringAsync();
        var _taskId = JsonConvert.DeserializeObject<string>(taskId);

        var response = await _client.PatchAsync($"/api/changeStatus/{_taskId}", null);

        Assert.Equal(expectedStatusCode, response.StatusCode);

        await _client.DeleteAsync($"/api/{_taskId}");
    }
}