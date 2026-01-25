using System;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    /// <summary>
    /// CRUD API for automations. Execution is performed by the background automation evaluator/runner.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationRepository _automationRepository;

        public AutomationController(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        /// <summary>
        /// Returns all configured automations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var automations = await _automationRepository.GetAllAsync();
            return Ok(automations);
        }

        /// <summary>
        /// Returns a single automation by id.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Creates an automation definition.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Updates an automation definition.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Deletes an automation definition.
        /// </summary>
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