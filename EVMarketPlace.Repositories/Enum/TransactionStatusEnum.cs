namespace EVMarketPlace.Repositories.Enums
{
    /// <summary>
    /// Tr?ng th�i giao d?ch (theo database schema)
    /// </summary>
    public enum TransactionStatusEnum
    {
        PENDING,      // ?ang ch? x? l�
        COMPLETED,    // ?� ho�n th�nh
        CANCELLED     // ?� h?y
    }
}