using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            return Ok("API is working!");
        }

        [Authorize]
        [HttpGet("method2")]
        public IActionResult Get()
        {
            return Ok("Method 2 is also working!");
        }
    }
}
