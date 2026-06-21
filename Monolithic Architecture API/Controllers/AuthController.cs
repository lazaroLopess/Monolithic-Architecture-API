using Microsoft.AspNetCore.Mvc;

namespace Monolithic_Architecture_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        [Route("teste")]
        public async Task<IActionResult> Get()
        {
            return Ok(new
            {
                message = "teste rota 1"
            });
        }
    }
}
