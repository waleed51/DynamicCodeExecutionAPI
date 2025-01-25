using System.ComponentModel.DataAnnotations;

namespace DynamicCodeExecutionAPI.Models
{
    public class SourceCode
    {
        [Key]
        public int? Id { get; set; } // Nullable Id for flexibility
        public string? SegmentName { get; set; } // Nullable SegmentName
        public string? Code { get; set; } // Nullable Code
    }
}
