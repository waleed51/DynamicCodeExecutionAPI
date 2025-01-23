using DynamicCodeExecutionAPI.Contexts;
using DynamicCodeExecutionAPI.interfaces;
using DynamicCodeExecutionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamicCodeExecutionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeEditorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IExecuseCodeService _executionService;

        public CodeEditorController(AppDbContext context, IExecuseCodeService executionService)
        {
            _context = context;
            _executionService = executionService;
        }

        [HttpGet("snippets")]
        public async Task<IActionResult> GetSnippets()
        {
            var snippets = await _context.SourceCodes.ToListAsync();
            return Ok(snippets);
        }

        [HttpPost("snippet")]
        public async Task<IActionResult> AddSnippet([FromBody] SourceCode snippet)
        {
            _context.SourceCodes.Add(snippet);
            await _context.SaveChangesAsync();
            return Ok(snippet);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSnippet([FromBody] dynamic payload)
        {
            int snippetId = payload.Id;
            string methodName = payload.Name;
            object[] parameters = ((Newtonsoft.Json.Linq.JArray)payload.parameters).ToObject<object[]>();

            var snippet = await _context.SourceCodes.FindAsync(snippetId);
            if (snippet == null)
            {
                return NotFound("Snippet not found.");
            }

            var result = _executionService.ExecuteCode(snippet.Code, methodName, parameters);
            return Ok(result);
        }
    }
}
