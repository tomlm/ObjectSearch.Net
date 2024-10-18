namespace ObjectSearch
{
    public class SearchResult
    {
        public float Score { get; set;  }
        public object? Value { get; set; }
    }

    public class SearchResult<T>
    {
        public float Score { get; set; }
        public T? Value { get; set; }
    }
}
