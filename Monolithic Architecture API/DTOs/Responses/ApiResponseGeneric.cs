namespace Monolithic_Architecture_API.DTOs.Responses
{
    public class ApiResponseGeneric<T> : ApiResponse
    {
        public T? Data { get; set; } 
    }
}
