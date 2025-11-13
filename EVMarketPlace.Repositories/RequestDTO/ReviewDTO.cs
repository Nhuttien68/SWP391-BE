using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    internal class ReviewDTO
    {
    }
    public class UpdateReviewDTO
    {
        [Required(ErrorMessage ="reviewID not null")]
        public Guid ReviewId { get; set; }
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
    public class ReviewCreateDTO
    {
        [Required(ErrorMessage = "TransactionId not null")]
        public Guid TransactionId { get; set; }
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class ReviewPostResponseDTO
    {
        public Guid ReviewId { get; set; }
        public string ReviewTargetType { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReviewerName { get; set; }
        public PostResponseDto detailDto { get; set; }
    }
    public class ReviewSellerResponseDTO
    {
        public Guid ReviewId { get; set; }
        public string ReviewTargetType { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReviewerName { get; set; }

        public UserinformationResponse SellerInfor { get; set; }
    }
}
