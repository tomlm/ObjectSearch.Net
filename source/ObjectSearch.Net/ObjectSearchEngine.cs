using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace ObjectSearch
{
    public class ObjectSearchEngine
    {
        private const string TYPE = "_type";
        private const string OBJECTID = "_oid";
        public const string CONTENT = "content";

        private Dictionary<string, object> _objectCache = new Dictionary<string, object>();
        private Lucene.Net.Store.Directory _directory;

        public ObjectSearchEngine(Analyzer? analyzer = null)
        {
            _directory = new RAMDirectory();
            Analyzer = analyzer ?? new StandardAnalyzer(LuceneVersion.LUCENE_48);
            QueryParser = new StandardQueryParser(Analyzer);
        }

        public IndexSearcher? Searcher { get; private set; }

        public Analyzer Analyzer { get; private set; }

        public StandardQueryParser QueryParser { get; private set; }

        /// <summary>
        /// AddObjects to search engine 
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects(IEnumerable objects)
            => AddObjects(objects, (obj, doc) => { });

        /// <summary>
        /// AddObjects to search engine
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects<T>(IEnumerable<T> objects)
            => AddObjects<T>(objects, (obj, doc) => { });

        /// <summary>
        /// AddObjects to search engine with ability to select the default content
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="contentSelector"></param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects(IEnumerable objects, Func<object, string> contentSelector)
            => AddObjects(objects, (obj, doc) => doc.AddTextField("content", contentSelector(obj), Field.Store.NO));

        /// <summary>
        /// AddObjects to search engine with ability to select the default content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="contentSelector"></param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects<T>(IEnumerable<T> objects, Func<T, string> contentSelector)
            => AddObjects<T>(objects, (obj, doc) => doc.AddTextField("content", contentSelector(obj), Field.Store.NO));

        /// <summary>
        /// AddObjects to search engine with custom fields
        /// </summary>
        /// <param name="objects">objects to add</param>
        /// <param name="customFields">option action to allow you to add custom fields</param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects(IEnumerable objects, Action<object, Document> customFields)
            => AddObjects(objects.OfType<object>(), customFields);

        /// <summary>
        /// AddObjects to search engine with custom fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects">objects to add</param>
        /// <param name="customFields">optional action to allow you to add custom fields</param>
        /// <returns></returns>
        public ObjectSearchEngine AddObjects<T>(IEnumerable<T> objects, Action<T, Document> customFields)
        {
            if (typeof(T).Name.StartsWith("SearchResult`1"))
                throw new ArgumentException($"{typeof(T).Name} is already a searchresult. You should use .SearchEngine property to issue a secondary query");
            
            lock (_directory)
            {
                using (var writer = new IndexWriter(_directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer)))
                {
                    var docs = new List<Document>();
                    foreach (var obj in objects)
                    {
                        docs.Add(CreateDocument(obj, customFields));
                    }

                    writer.AddDocuments(docs);
                }

                Searcher = new IndexSearcher(DirectoryReader.Open(_directory));
            }
            return this;
        }


        /// <summary>
        /// Search object for text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public SearchResults<object> Search(string text, int n = int.MaxValue)
            => Search<object>(text, n);

        /// <summary>
        /// Search objects using query
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="n"></param>
        /// <returns></returns>
        public SearchResults<object> Search(Query query, int n = int.MaxValue)
            => Search<object>(query, n);

        /// <summary>
        /// Search objects of T using text
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public SearchResults<T> Search<T>(string text, int n = int.MaxValue)
            => Search<T>(QueryParser.Parse(text, CONTENT), n);

        /// <summary>
        /// Search objects of T using query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public SearchResults<T> Search<T>(Query query, int n = int.MaxValue)
        {
            ArgumentNullException.ThrowIfNull(Searcher);
            
            if (typeof(T).Name.StartsWith("SearchResult`1"))
                throw new ArgumentException($"{typeof(T).Name} is already a SearchResult. You should use .SearchEngine property to issue a secondary query.");

            if (typeof(T) != typeof(object))
            {
                query = new BooleanQuery
                {
                    { new TermQuery(new Term(TYPE, typeof(T).Name)), Occur.MUST },
                    { query, Occur.MUST }
                };
            }

            var topDocs = Searcher.Search(query, n);

            var searchResults = new SearchResults<T>(this);
            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = Searcher.Doc(scoreDoc.Doc);
                var oid = doc.GetField(OBJECTID).GetStringValue();

                searchResults.Add(new SearchResult<T>(this, scoreDoc.Score, (T)_objectCache[oid]!));
            }
            return searchResults;
        }

        private Document CreateDocument<T>(T obj, Action<T, Document>? docSelector = null)
        {
            ArgumentNullException.ThrowIfNull(obj);
            var id = Guid.NewGuid().ToString("n");
            _objectCache.Add(id, obj!);

            var doc = new Document();
            doc.AddStringField(OBJECTID, id, Field.Store.YES);

            // if there is a docSelector call them so they can add custom fields.
            if (docSelector != null)
                docSelector(obj, doc);


            if (!doc.Fields.Any(f => f.Name == CONTENT))
                doc.AddTextField(CONTENT, JToken.FromObject(obj).ToString(), Field.Store.NO);

            // add type hierachy so you can constrain on subtypes
            var type = obj.GetType();
            while (type != null)
            {
                doc.AddStringField(TYPE, type.Name, Field.Store.YES);
                type = type.BaseType;
            }

            return doc;
        }


    }
}
