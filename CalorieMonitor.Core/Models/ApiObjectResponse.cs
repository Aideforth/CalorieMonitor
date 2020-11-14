namespace CalorieMonitor.Core.Models
{
    public class ApiObjectResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}
