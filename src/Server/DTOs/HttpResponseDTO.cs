namespace ServerSample.DTOs;

public class HttpResponseDTO
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息内容
    /// </summary>
    public string Message { get; set; }

    public static HttpResponseDTO Success(string message = "Success")
    {
        return new HttpResponseDTO
        {
            Code = 200,
            Message = message
        };
    }

    public static HttpResponseDTO InnerException(string message)
    {
        return new HttpResponseDTO
        {
            Code = 500,
            Message = message
        };
    }
}

public sealed class HttpResponseDTO<T> : HttpResponseDTO
{
    /// <summary>
    /// 数据内容
    /// </summary>
    public T? Body { get; set; }

    private HttpResponseDTO()
    {
    }

    /// <summary>
    /// 成功
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static HttpResponseDTO<T> Success(T data, string message = "Success")
    {
        return new HttpResponseDTO<T>
        {
            Code = 200,
            Body = data,
            Message = message
        };
    }

    /// <summary>
    /// 失败
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static HttpResponseDTO<T> Failure(string message, T? data = default)
    {
        return new HttpResponseDTO<T>
        {
            Code = 400,
            Body = data,
            Message = message
        };
    }

    /// <summary>
    /// 服务器内部异常
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static HttpResponseDTO<T> InnerException(string message, T? data = default)
    {
        return new HttpResponseDTO<T>
        {
            Code = 500,
            Body = data,
            Message = message
        };
    }
}