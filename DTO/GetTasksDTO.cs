using System;
using To_Do_List_backend.Models;

namespace To_Do_List_backend.DTO
{

	public class GetTasksDTO
	{
		public List<ToDoItem> Tasks { get; set; }
	}
}
