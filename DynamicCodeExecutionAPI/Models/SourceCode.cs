using System.ComponentModel.DataAnnotations;

namespace DynamicCodeExecutionAPI.Models
{
    public class SourceCode
    {
        [Key] // we can add configuration class for models and update it 
        public int? Id { get; set; }
        public string? SegmentName { get; set; }
        public string? Code { get; set; }
    }
}
