using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System.Collections;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Documents;

namespace ObjectSearch.Net
{
    public static class SearchExtensions
    {
        /// <summary>
        /// The QueryParser which is used for search extensions.
        /// </summary>
        public static StandardQueryParser QueryParser { get; set; } = new StandardQueryParser(new StandardAnalyzer(LuceneVersion.LUCENE_48));

        /// <summary>
        /// Search enumerable for objects that match text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static IEnumerable<SearchResult<T>> Search<T>(this IEnumerable source, string text, Action<T, Document>? docSelector = null, int n = int.MaxValue)
            => source.Search<T>(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), docSelector, n);

        /// <summary>
        /// Search enumerable for objects that match query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static IEnumerable<SearchResult<T>> Search<T>(this IEnumerable source, Query query, Action<T, Document>? docSelector = null, int n = int.MaxValue)
            => source.OfType<T>().Search(query, docSelector, n);

        /// <summary>
        /// Search enumerableof T for objects that match text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static IEnumerable<SearchResult<T>> Search<T>(this IEnumerable<T> source, string text, Action<T, Document>? docSelector = null, int n = int.MaxValue)
            => source.Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), docSelector, n);


        /// <summary>
        /// Search enumerable of T for objects that match query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static IEnumerable<SearchResult<T>> Search<T>(this IEnumerable<T> source, Query query, Action<T, Document>? docSelector = null, int n = int.MaxValue)
            => new ObjectSearchEngine()
                .AddObjects(source, docSelector)
                .Search<T>(query, n);
    }
}
