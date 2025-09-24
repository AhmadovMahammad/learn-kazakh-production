namespace LearnKazakh.Shared.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(Exception exception, string message)
    {
        List<string> errors = [];
        for (Exception? cur = exception; cur != null; cur = cur.InnerException)
        {
            errors.Add(cur.Message);
        }

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public class ApiPagedResponse<T>
{
    public bool Success { get; set; }
    public PagedData<T>? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiPagedResponse<T> SuccessResult(PagedData<T> data, string message = "")
    {
        return new ApiPagedResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiPagedResponse<T> ErrorResult(Exception exception, string message = "")
    {
        List<string> errors = [];
        for (Exception? cur = exception; cur != null; cur = cur.InnerException)
        {
            errors.Add(cur.Message);
        }

        return new ApiPagedResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public class PagedData<T>
{
    public List<T> Items { get; set; } = [];
    public int NextOffset { get; set; }
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}