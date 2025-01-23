using DynamicCodeExecutionAPI.Models;

namespace DynamicCodeExecutionAPI.interfaces
{
    public interface IExecuseCodeService
    {
        Task<object?> ExecuteCode(string code, string methodName, object[] parameters);
    }
}
