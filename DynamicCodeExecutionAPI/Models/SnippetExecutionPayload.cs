using System.ComponentModel.DataAnnotations;

namespace DynamicCodeExecutionAPI.Models
{
    public class SnippetExecutionPayload
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Method name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required]
        public object[] Parameters { get; set; }
    }
}
