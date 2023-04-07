namespace ByBit.Models;

public class BaseResponse
{
    public int ret_code { get; set; }
    public string ret_msg { get; set; }
}

public class BaseResponse_V5
{
    public int retCode { get; set; }
    public string retMsg { get; set; }
}