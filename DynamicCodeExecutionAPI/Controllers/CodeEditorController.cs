using DynamicCodeExecutionAPI.Contexts;
using DynamicCodeExecutionAPI.interfaces;
using DynamicCodeExecutionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DynamicCodeExecutionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeEditorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IExecuseCodeService _executionService;

        //we can use CQRS or MediatR
        public CodeEditorController(AppDbContext context, IExecuseCodeService executionService)
        {
            //we can use repository/unit of work pattern or any suitable patterns to handle it
            _context = context;
            _executionService = executionService;
        }

        // GET: api/CodeEditor/snippets
        [HttpGet("snippets")]
        public async Task<IActionResult> GetSnippets()
        {
            try
            {
                var snippets = await _context.SourceCodes.ToListAsync();
                return Ok(snippets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching snippets.", error = ex.Message });
            }
        }

        // POST: api/CodeEditor/snippet
        [HttpPost("snippet")]
        public async Task<IActionResult> AddSnippet([FromBody] SourceCode snippet)
        {
            if (snippet == null)
            {
                return BadRequest(new { message = "Snippet cannot be null." });
            }

            // Validate required properties
            if (string.IsNullOrEmpty(snippet.SegmentName))
            {
                return BadRequest(new { message = "SegmentName is required." });
            }

            if (string.IsNullOrEmpty(snippet.Code))
            {
                return BadRequest(new { message = "Code is required." });
            }

            try
            {
                _context.SourceCodes.Add(snippet);
                await _context.SaveChangesAsync();
                return Ok(snippet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the snippet.", error = ex.Message });
            }
        }

        // POST: api/CodeEditor/execute
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSnippet([FromBody] JObject payload)
        {
            if (payload == null)
            {
                return BadRequest(new { message = "Invalid payload. Request body is required." });
            }

            try
            {
                // Extract payload parameters
                int snippetId = payload["id"]?.Value<int>() ?? 0;
                string methodName = payload["name"]?.Value<string>();
                object[] parameters = payload["parameters"]?.ToObject<object[]>() ?? Array.Empty<object>();

                // Validate input
                if (snippetId == 0 || string.IsNullOrEmpty(methodName))
                {
                    return BadRequest(new { message = "Snippet ID and method name are required." });
                }

                // Retrieve snippet from the database
                var snippet = await _context.SourceCodes.FindAsync(snippetId);
                if (snippet == null)
                {
                    return NotFound(new { message = $"Snippet with ID {snippetId} not found." });
                }

                // Validate snippet code
                if (string.IsNullOrEmpty(snippet.Code))
                {
                    return BadRequest(new { message = "Snippet code is empty or invalid." });
                }

                // Execute the code
                var result = _executionService.ExecuteCode(snippet.Code, methodName, parameters);

                return Ok(new
                {
                    snippetId = snippetId,
                    segmentName = snippet.SegmentName,
                    methodName = methodName,
                    parameters = parameters,
                    result = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while executing the snippet.", error = ex.Message });
            }
        }
    }
}
