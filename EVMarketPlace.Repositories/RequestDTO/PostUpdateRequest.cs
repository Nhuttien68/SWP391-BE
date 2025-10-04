namespace EVMarketPlace.Repositories.RequestDTO.Posts
{

    //DTO cập nhật Post (partial update): gửi gì sửa nấy.
    //PostId sẽ được controller gán từ route {id}.

    public class PostUpdateRequest
    {
        public Guid PostId { get; set; }            // set từ route

        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
    }
}
