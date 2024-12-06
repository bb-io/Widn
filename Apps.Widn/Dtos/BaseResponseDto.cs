namespace Apps.Widn.Dtos;
public class BaseResponseDto<T>
{
    public string NextPageToken { get; set; }
    public List<T> Results { get; set; }
}

