using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using To_Do_List_backend.Models;
using To_Do_List_backend.DTO;
using To_Do_List_backend.Data;

namespace To_Do_List_backend.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly Context _context;
        public ToDoListService(Context context)
        {
            _context = context;
        }

        public async Task<Guid> AddTask([FromBody] NewItemDTO task)
        {
            if (task == null)
            {
                throw new Exception("Введите данные");
            }

            var newTask = new ToDoItem
            {
                Name = task.Name,
                Description = task.Description
            };

            AddDeadline(newTask, task);
            AddPriority(newTask, task);

            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            if (newTask.Deadline != null)
            {
                if (date > newTask.Deadline)
                {
                    newTask.Status = Status.Overdue;
                }
                else
                {
                    newTask.Status = Status.Active;
                }
            }

            await _context.ToDoItems.AddAsync(newTask);
            await _context.SaveChangesAsync();

            return newTask.Id;
        }
        public async Task AddDeadline(ToDoItem newTask, NewItemDTO task)
        {
            var regex = new Regex(@"!before\s*(\d{2}[.-]\d{2}[.-]\d{4})", RegexOptions.IgnoreCase);
            var match = regex.Match(task.Name);

            if (task.Deadline != null)
            {
                newTask.Deadline = task.Deadline;
                if (match.Success)
                {
                    task.Name = task.Name.Replace(match.Value, string.Empty).Trim();
                }
            }
            else
            {
                if (match.Success)
                {
                    string dateString = match.Groups[1].Value;
                    if (DateOnly.TryParseExact(dateString, new[] { "dd.MM.yyyy", "dd-MM-yyyy" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateOnly parsedDate))
                    {
                        newTask.Deadline = parsedDate;
                        task.Name = task.Name.Replace(match.Value, string.Empty).Trim();
                    }
                }
            }
            newTask.Name = task.Name;
        }
        public async Task AddPriority(ToDoItem newTask, NewItemDTO task)
        {
            string pattern = @"!(\d)";

            if (task.Priority != null)
            {
                newTask.Priority = (Priority)task.Priority;
                newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
            }
            else
            {
                if (newTask.Name.Contains("!1"))
                {
                    newTask.Priority = Priority.Critical;
                }
                else if (newTask.Name.Contains("!2"))
                {
                    newTask.Priority = Priority.High;
                }
                else if (newTask.Name.Contains("!3"))
                {
                    newTask.Priority = Priority.Medium;
                }
                else if (newTask.Name.Contains("!4"))
                {
                    newTask.Priority = Priority.Low;
                }
                else
                {
                    newTask.Priority = Priority.Medium;
                }
                newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
            }
        }
        public async Task EditTask(Guid id, [FromBody] EditTaskDTO editTask)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }
            if (editTask == null)
            {
                throw new Exception("Введите данные");
            }

            task.Name = editTask.Name;
            task.Description = editTask.Description;
            task.Deadline = editTask.Deadline;
            task.Priority = (Priority)editTask.Priority;

            EditDeadline(task, editTask);
            EditPriority(task, editTask);

            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            if (task.Deadline != null)
            {
                if (task.Status == Status.Active && date > task.Deadline)
                {
                    task.Status = Status.Overdue;
                }
                if (task.Status == Status.Overdue && date <= task.Deadline)
                {
                    task.Status = Status.Active;
                }
            }

            task.EditTime = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();
        }
        public async Task EditDeadline(ToDoItem newTask, EditTaskDTO task)
        {
            var regex = new Regex(@"!before\s*(\d{2}[.-]\d{2}[.-]\d{4})", RegexOptions.IgnoreCase);
            var match = regex.Match(task.Name);

            if (newTask.Deadline != null)
            {
                newTask.Deadline = task.Deadline;
                if (match.Success)
                {
                    newTask.Name = task.Name.Replace(match.Value, string.Empty).Trim();
                }
            }
            else
            {
                if (match.Success)
                {
                    string dateString = match.Groups[1].Value;
                    if (DateOnly.TryParseExact(dateString, new[] { "dd.MM.yyyy", "dd-MM-yyyy" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateOnly parsedDate))
                    {
                        newTask.Deadline = parsedDate;
                        newTask.Name = task.Name.Replace(match.Value, string.Empty).Trim();
                    }
                }
            }
        }
        public async Task EditPriority(ToDoItem newTask, EditTaskDTO task)
        {
            string pattern = @"!\d";

            if (newTask.Priority != null)
            {
                newTask.Priority = (Priority)task.Priority;
                if (newTask.Name.Contains("!1") || newTask.Name.Contains("!2") || newTask.Name.Contains("!3") || newTask.Name.Contains("!4"))
                {
                    newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
                }
            }
            else
            {
                if (newTask.Name.Contains("!1"))
                {
                    newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
                    newTask.Priority = Priority.Critical;
                }
                else if (newTask.Name.Contains("!2"))
                {
                    newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
                    newTask.Priority = Priority.High;
                }
                else if (newTask.Name.Contains("!3"))
                {
                    newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
                    newTask.Priority = Priority.Medium;
                }
                else if (newTask.Name.Contains("!4"))
                {
                    newTask.Name = Regex.Replace(task.Name, pattern, "").Trim();
                    newTask.Priority = Priority.Low;
                }
                else
                {
                    newTask.Priority = Priority.Medium;
                }
            }
        }
        public async Task DeleteTask(Guid id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }

            _context.ToDoItems.Remove(task);
            await _context.SaveChangesAsync();
        }
        public async Task<GetTaskDTO> GetTask(Guid id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }

            var getTask = new GetTaskDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                CreateTime = task.CreateTime,
                EditTime = (DateOnly)task.EditTime
            };

            return getTask;
        }
        public async Task CompleteTask(Guid id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }

            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            if (task.Deadline != null)
            {
                if (date <= task.Deadline)
                {
                    task.Status = Status.Completed;
                }

                else
                {
                    task.Status = Status.Late;
                }
            }
            else
            {
                task.Status = Status.Completed;
            }


            task.EditTime = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();
        }
        public async Task InCompleteTask(Guid id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }

            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            if (task.Deadline != null)
            {
                if (date <= task.Deadline)
                {
                    task.Status = Status.Active;
                }

                else
                {
                    task.Status = Status.Overdue;
                }
            }
            else
            {
                task.Status = Status.Active;
            }

            task.EditTime = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();
        }
        public async Task<GetTasksDTO> GetList(Sorting? sort)
        {
            var tasks = await _context.ToDoItems.ToListAsync();

            //сортировка по статусу
            if (sort == Sorting.StatusAsc)
            {
                tasks = tasks.OrderBy(p => p.Status).ToList();
            }
            else if (sort == Sorting.StatusDesc)
            {
                tasks = tasks.OrderByDescending(p => p.Status).ToList();
            }
            //сортировка по приоритету
            else if (sort == Sorting.PriorityAsc)
            {
                tasks = tasks.OrderBy(p => p.Priority).ToList();
            }
            else if (sort == Sorting.PriorityDesc)
            {
                tasks = tasks.OrderByDescending(p => p.Priority).ToList();
            }

            //сортировка по дедлайну
            else if (sort.ToString() == Sorting.DeadlineAsc.ToString())
            {
                tasks = tasks.OrderBy(p => p.Deadline).ToList();
            }
            else if (sort == Sorting.DeadlineDesc)
            {
                tasks = tasks.OrderByDescending(p => p.Deadline).ToList();
            }
            //сортировка по дате создания
            else if (sort == Sorting.CreateTimeAsc)
            {
                tasks = tasks.OrderBy(p => p.CreateTime).ToList();
            }
            else if (sort == Sorting.CreateTimeDesc)
            {
                tasks = tasks.OrderByDescending(p => p.CreateTime).ToList();
            }

            var result = new GetTasksDTO
            {
                Tasks = tasks
            };


            return result;
        }

        public async Task ChangeStatus(Guid id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Такой задачи нет");
            }

            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            if (task.Deadline != null)
            {
                if (date > task.Deadline)
                {
                    task.Status = Status.Overdue;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
