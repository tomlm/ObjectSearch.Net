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

        #region IEnumerable/Text
        /// <summary>
        /// Search enumerable for objects that match text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, string text, int n = int.MaxValue)
            => source.OfType<T>().Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), n);


        /// <summary>
        /// Search enumerable for objects that match text with custom content selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, string text, Func<T, string> customContent, int n = int.MaxValue)
            => source.OfType<T>().Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), customContent, n);

        /// <summary>
        /// Search enumerable for objects that match text with customfields added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, string text, Action<T, Document> customField, int n = int.MaxValue)
            => source.OfType<T>().Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), customField, n);
        #endregion

        #region IEnumerable/Query
        /// <summary>
        /// Search enumerable for objects that match query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, Query query, int n = int.MaxValue)
            => source.OfType<T>().Search(query, n);

        /// <summary>
        /// Search enumerable for objects that match query with custom content selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="contentSelector">custom content selector</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, Query query, Func<T, string> contentSelector, int n = int.MaxValue)
            => source.OfType<T>().Search(query, contentSelector, n);

        /// <summary>
        /// Search enumerable for objects that match text with custom fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="customField">custom field selector</param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable source, Query query, Action<T, Document> customField, int n = int.MaxValue)
            => source.Search<T>(query, customField, n);
        #endregion

        #region IEnumerable_T/Text

        /// <summary>
        /// Search enumerable for objects that match query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text">query</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, string text, int n = int.MaxValue)
            => source.Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), n);

        /// <summary>
        /// Search enumerable for objects that match query with custom content selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text">query</param>
        /// <param name="contentSelector">custom content selector</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, string text, Func<T, string> contentSelector, int n = int.MaxValue)
            => source.Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), contentSelector, n);

        /// <summary>
        /// Search enumerable of T for objects that match text with custom fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <param name="customField">custom field func</param>
        /// <param name="n">number of results to get</param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, string text, Action<T, Document> customField, int n = int.MaxValue)
            => source.Search(QueryParser.Parse(text, ObjectSearchEngine.CONTENT), customField, n);
        #endregion

        #region IEnumerable_T/Query

        /// <summary>
        /// Search enumerable of T for objects that match query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, Query query, int n = int.MaxValue)
        {
            var searchEngine = new ObjectSearchEngine().AddObjects(source);
            return searchEngine.Search<T>(query, n);
        }


        /// <summary>
        /// Search enumerable of T for objects that match query with custom content selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="contentSelector">custom content selector</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, Query query, Func<T, string> contentSelector, int n = int.MaxValue)
        {
            var searchEngine = new ObjectSearchEngine()
                .AddObjects(source, contentSelector);
            return searchEngine.Search<T>(query, n);
        }

        /// <summary>
        /// Search enumerable of T for objects that match query with custom fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="query">query</param>
        /// <param name="customField">custom field selector</param>
        /// <param name="n">number of results to get </param>
        /// <returns></returns>
        public static SearchResults<T> Search<T>(this IEnumerable<T> source, Query query, Action<T, Document> customField, int n = int.MaxValue)
        {
            var searchEngine = new ObjectSearchEngine()
                .AddObjects(source, customField);
            return searchEngine.Search<T>(query, n);
        }
        #endregion

    }
}
