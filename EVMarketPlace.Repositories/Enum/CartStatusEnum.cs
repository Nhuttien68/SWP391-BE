namespace EVMarketPlace.Repositories.Enum
{
    /// <summary>
    /// Trạng thái giỏ hàng 
    /// </summary>
    public enum CartStatusEnum
    {
        ACTIVE,   // Giỏ hàng đang hoạt động
        DELETED   // Giỏ hàng đã bị xóa (soft delete)
    }
}