namespace ObjectSearch
{

    public interface IObjectSearchEngine
    {
        public ObjectSearchEngine SearchEngine { get; }
    }

    public class SearchResult<T> : IObjectSearchEngine
    {
        public SearchResult(ObjectSearchEngine engine, float score, T? value)
        {
            SearchEngine = engine;
            Score = score;
            Value = value;
        }

        public ObjectSearchEngine SearchEngine { get; }
        
        public float Score { get; }

        public T? Value { get; }
    }

    /// <summary>
    /// SearchResults<typeparamref name="T"/>
    /// </summary>
    /// <remarks>This is a List(T) that also has the SearchEngine on it so you can do additional searches.</remarks>
    /// <typeparam name="T"></typeparam>
    public class SearchResults<T> : List<SearchResult<T>>, IObjectSearchEngine
    {
        public SearchResults(ObjectSearchEngine engine)
        {
            SearchEngine = engine;
        }

        public ObjectSearchEngine SearchEngine { get; }
    }

}
