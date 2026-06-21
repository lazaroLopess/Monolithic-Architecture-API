namespace Monolithic_Architecture_API.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }
    }
}
