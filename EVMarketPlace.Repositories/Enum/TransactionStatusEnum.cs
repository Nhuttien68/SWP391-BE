namespace EVMarketPlace.Repositories.Enums
{
    /// <summary>
    /// Tr?ng thái giao d?ch (theo database schema)
    /// </summary>
    public enum TransactionStatusEnum
    {
        PENDING,      // ?ang ch? x? lý
        COMPLETED,    // ?ã hoàn thành
        CANCELLED     // ?ã h?y
    }
}