namespace EVMarketPlace.Repositories.Options
{
    public class PaginationOptions
    {
        public int DefaultPage { get; set; } = 1;
        public int DefaultPageSize { get; set; } = 10;
        public int MaxPageSize { get; set; } = 100;
    }
}
