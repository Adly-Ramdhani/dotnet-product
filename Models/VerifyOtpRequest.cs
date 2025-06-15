using System.ComponentModel.DataAnnotations;

public class VerifyOtpRequest
{
    public required string Email { get; set; }
    public required string OtpCode { get; set; }
}
