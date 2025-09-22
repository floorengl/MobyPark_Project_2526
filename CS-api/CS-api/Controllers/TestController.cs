using CS_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly MobyParkDBContext _dbContext;

        public TestController(MobyParkDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetTests")]
        public async Task<IActionResult> GetTest()
        {
            var result = await _dbContext.Test.Select(x => new Test
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToListAsync();

            return Ok(result);
        }

        [HttpPost("CreateTest")]
        public async Task<IActionResult> CreateTest([FromBody] Test test)
        {
            _dbContext.Test.Add(test);
            await _dbContext.SaveChangesAsync();

            return Ok(test);
        }

        [HttpPut("EditTest")]
        public async Task<IActionResult> EditTest([FromBody] Test test)
        {
            var rows = await _dbContext.Test.Where(x => x.Id == test.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.Name, test.Name));

            return Ok(test);
        }

        [HttpDelete("DeleteTest")]
        public async Task<IActionResult> DeleteTest(Test test)
        {
            var rows = await _dbContext.Test.Where(x => x.Id == test.Id).ExecuteDeleteAsync();

            return Ok(true);
        }
    }
}
