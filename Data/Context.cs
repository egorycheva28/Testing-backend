using Microsoft.EntityFrameworkCore;
using To_Do_List_backend.Models;
namespace To_Do_List_backend.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        { }
        public  virtual DbSet<ToDoItem> ToDoItems { get; set; }

    }
}