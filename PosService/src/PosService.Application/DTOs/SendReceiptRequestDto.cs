using System.ComponentModel.DataAnnotations;

namespace PosService.Application.DTOs;

public class SendReceiptRequestDto
{
    [Required]
    [RegularExpression("^(EMAIL|SMS)$", ErrorMessage = "Method must be EMAIL or SMS")]
    public string Method { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Destination { get; set; } = string.Empty;
}
