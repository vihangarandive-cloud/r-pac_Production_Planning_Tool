namespace RPACProductionPlanner.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ServiceResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
