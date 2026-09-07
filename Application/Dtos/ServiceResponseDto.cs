namespace Application.Dtos;

public class ServiceResponseDto<T>
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public static ServiceResponseDto<T> Success(T data, string message = "Operation completed successfully")
    {
        return new ServiceResponseDto<T>
        {
            Status = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResponseDto<T> Fail(string message)
    {
        return new ServiceResponseDto<T>
        {
            Status = false,
            Message = message,
            Data = default
        };
    }
}