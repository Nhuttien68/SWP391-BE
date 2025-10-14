using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class ResendOtpRequest
    {
        [Required(ErrorMessage = "Email lĂ  báº¯t buá»™c")]
        [EmailAddress(ErrorMessage = "Email khĂ´ng há»£p lá»‡")]
        public string Email { get; set; } = null!;
    }
}