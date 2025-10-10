using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        public class TodoItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public bool IsDone { get; set; }
        }

        private static readonly List<TodoItem> _todos = new()
        {
            new TodoItem { Id = 1, Title = "Learn ASP.NET Core", IsDone = true },
            new TodoItem { Id = 2, Title = "Learn Angular", IsDone = false },
            new TodoItem { Id = 3, Title = "Build full-stack app", IsDone = false }
        };

        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetAll()
        {
            return Ok(_todos);
        }
    }
}
