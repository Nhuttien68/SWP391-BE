using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;   // [NotMapped]
using System.Text.Json.Serialization;                // [JsonIgnore]

namespace EVMarketPlace.Repositories.Entity
{
    public partial class Post
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation props: tạm bỏ qua để tránh EF map quan hệ
        [NotMapped, JsonIgnore] public virtual ICollection<Auction>? Auctions { get; set; }
        [NotMapped, JsonIgnore] public virtual ICollection<Battery>? Batteries { get; set; }
        [NotMapped, JsonIgnore] public virtual ICollection<CartItem>? CartItems { get; set; }
        [NotMapped, JsonIgnore] public virtual ICollection<Favorite>? Favorites { get; set; }
        [NotMapped, JsonIgnore] public virtual ICollection<Transaction>? Transactions { get; set; }
        [NotMapped, JsonIgnore] public virtual User? User { get; set; }
        [NotMapped, JsonIgnore] public virtual ICollection<Vehicle>? Vehicles { get; set; }
    }
}
