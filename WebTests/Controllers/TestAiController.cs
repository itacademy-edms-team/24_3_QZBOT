using Microsoft.AspNetCore.Mvc;
using WebTests.Services;

namespace WebTests.Controllers
{
    [ApiController]
    [Route("api/test-ai")]
    public class TestAiController : Controller
    {
        private readonly YandexGptService _gptService;
        public TestAiController(YandexGptService gptService)
        {
            _gptService = gptService;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var result = await _gptService.Test();
            return Ok(result);
        }
    }
}
