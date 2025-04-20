using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using To_Do_List_backend.Services;
using To_Do_List_backend.Models;
using To_Do_List_backend.DTO;

[Route("api")]
[ApiController]

public class TodoListController : ControllerBase
{
    private IToDoListService _toDoListService;
    public TodoListController(IToDoListService toDoListService)
    {
        _toDoListService = toDoListService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddTask([FromBody] NewItemDTO task)
    {
        try
        {
            Guid id = await _toDoListService.AddTask(task);
            return Ok(id);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Введите данные")
            {
                return BadRequest(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }
    [HttpPut("edit/{id}")]
    public async Task<IActionResult> EditTask(Guid id, [FromBody] EditTaskDTO task)
    {
        try
        {
            await _toDoListService.EditTask(id, task);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            if (ex.Message == "Введите данные")
            {
                return BadRequest(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }

    [HttpDelete("{id}")]//удаление дела
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            await _toDoListService.DeleteTask(id);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTaskDTO>> GetTask(Guid id)
    {
        try
        {
            var task = await _toDoListService.GetTask(id);
            return task;
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }

    [HttpPatch("complete/{id}")]//выполнение дела
    public async Task<IActionResult> CompleteTask(Guid id)
    {
        try
        {
            await _toDoListService.CompleteTask(id);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }
    [HttpPatch("incomplete/{id}")]//невыполнение дела
    public async Task<IActionResult> InCompleteTask(Guid id)
    {
        try
        {
            await _toDoListService.InCompleteTask(id);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }
    [HttpGet]//список всех задач
    public async Task<ActionResult<GetTasksDTO>> GetList(Sorting? sort)
    {
        try
        {
            return await _toDoListService.GetList(sort);
            //return Ok();
        }
        catch
        {
            return StatusCode(500, "InternalServerError");
        }
    }

    [HttpPatch("changeStatus/{id}")]//изменение статуса
    public async Task<IActionResult> ChangeStatus(Guid id)
    {
        try
        {
            await _toDoListService.ChangeStatus(id);
            return Ok();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Такой задачи нет")
            {
                return NotFound(ex.Message);
            }
            return StatusCode(500, "InternalServerError");
        }
    }
}
