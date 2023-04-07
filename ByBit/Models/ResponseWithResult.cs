using System.Collections.Generic;

namespace ByBit.Models;

public class ResponseWithResult<T> : BaseResponse
{
    public T result { get; set; }
}


public class ResponseWithListResult_V5<T> : BaseResponse_V5
{
    public ListResult_V5<T> result { get; set; }
}

public class ListResult_V5<T>
{
    public IEnumerable<T> list { get; set; }
}