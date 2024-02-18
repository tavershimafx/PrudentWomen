namespace Monochrome.Module.Core.Services
{
    public class PaginatedTable<T>
    {
        /// <summary>
        /// An array of the pages to be displayed in the pagination tab
        /// </summary>
        public int[] Pages { get; set; }

        /// <summary>
        /// current page or page to return
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// total pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The count of the total items in the collection
        /// </summary>
        public int TotalItems { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
