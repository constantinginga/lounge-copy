using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Services;
using ScSoMe.EF;

namespace ScSoMe.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NpsController : ControllerBase
    {
        private readonly ILogger<NpsController> _logger;
        private readonly NpsService npsService;

        public NpsController(ILogger<NpsController> logger)
        {
            _logger = logger;
            npsService = new NpsService();
        }

        [HttpGet("NpsData")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<List<NpsResult>> NpsTable()
        {
            return await npsService.RunNPS();
        }

    }
}
