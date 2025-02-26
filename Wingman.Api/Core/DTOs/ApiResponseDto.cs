namespace Wingman.Api.Core.DTOs;

public class ApiResponseDto<T>
{
    public int StatusCode { get; set; }
    public bool Success => StatusCode >= StatusCodes.Status200OK && StatusCode < StatusCodes.Status300MultipleChoices;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
