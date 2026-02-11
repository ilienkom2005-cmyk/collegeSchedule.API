using collegeSchedule.API.DTO;
using collegeSchedule.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace collegeSchedule.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController: ControllerBase
    {
        private readonly IScheduleService _service;

        public ScheduleController(IScheduleService service)
        {
            _service = service;
        }

        [HttpGet("group/{groupName}")]
        public async Task<ActionResult<List<ScheduleByDateDto>>> GetScheduleForGroup(
            [FromRoute] string groupName,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            try
            {
                var schedule = await _service.GetScheduleForGroup(groupName, start, end);
                return Ok(schedule); // Возвращает 200 OK с данными в формате JSON
            }
            catch (Exception ex)
            {
                // Для тестирования возвращаем ошибку здесь
                // throw; // Бросает исключение дальше, где его поймает Middleware
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
