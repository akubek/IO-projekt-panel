using System;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationRepository _automationRepository;

        public AutomationController(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var automations = await _automationRepository.GetAllAsync();
            return Ok(automations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var automation = await _automationRepository.GetByIdAsync(id);
            if (automation == null)
            {
                return NotFound();
            }
            return Ok(automation);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Automation automation)
        {
            if (automation == null)
            {
                return BadRequest();
            }
            await _automationRepository.AddAsync(automation);
            return CreatedAtAction(nameof(GetById), new { id = automation.Id }, automation);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Automation automation)
        {
            if (automation == null || automation.Id != id)
            {
                return BadRequest();
            }

            var existingAutomation = await _automationRepository.GetByIdAsync(id);
            if (existingAutomation == null)
            {
                return NotFound();
            }

            await _automationRepository.UpdateAsync(automation);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingAutomation = await _automationRepository.GetByIdAsync(id);
            if (existingAutomation == null)
            {
                return NotFound();
            }

            await _automationRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}