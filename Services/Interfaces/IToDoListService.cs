using Microsoft.AspNetCore.Mvc;
using System;
using To_Do_List_backend.DTO;

public interface IToDoListService
{
	public Task<Guid> AddTask(NewItemDTO task);
	public Task EditTask(Guid id, EditTaskDTO editTask);
	public Task DeleteTask(Guid id);
	public Task<GetTaskDTO> GetTask(Guid id);
    public Task CompleteTask(Guid id);
    public Task InCompleteTask(Guid id);
	public Task<GetTasksDTO> GetList(Sorting? sort);
	public Task ChangeStatus(Guid id);

}
