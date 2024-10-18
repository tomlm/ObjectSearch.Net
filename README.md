# ObjectSearch.NET
ObjectSearch is a lightweight in memory search engine using Lucene. 

It allows you to use Lucene search engine query semantics over a collection of in-memory objects.

```csharp
Console.WriteLine("Objects match: +big cat");
foreach(var result in items.Search("+big cat"))
{
    Console.WriteLine($"{result.Score} {JsonConvert.SerializeObject(result.Value)}");
}
```

# ObjectSearchEngine 
ObjectSearchEngine represents a search engine over a collection of objects. You can call multiple times without penalty as
the lifetime of the search index is managed by ObjectSearchEngine.

Example
```csharp
// search over items collection.
var searchEngine = new ObjectSearchEngine()
    .AddObjects(items);

var results1 = searchEngine.Search("foo bar");
var results2 = searchEngine.Search("big cat");
```


# Linq style .Search() operator 
The Linq style **.Search()** operator will build an index on the fly over the collection of objects and then do a full text search over the collection. 

You can do further Linq operations on it like .Select(), .Where(), etc.

```csharp
// find adults named joe 
foreach(var item in items.Where(item => item.Age >= 18)
                        .Search("joe")
                        .Select(searchResult => searchResult.Value)
                        .Take(5))
{
    ...
}
```

## Issuing multiple queries with Linq

All **SearchResults&lt;T&gt;** and **SearchResult&lt;T&gt;** have a reference to the ObjectSearchEngine used to create the results
allowing you to issue multiple queries and it will all be backed by the same search index built over the
original collection of items.

```csharp
var bigDogs = items.Search("big dogs"); // creates a new index over items
var littleDogs = results.SearchEngine.Search("little dogs"); // uses the same engine (GOOD)

var bigCats = items.Search("big cats"); // creates a new index over items
var littleCats = items.Search("little cats"); // creates a duplicate index over items (BAD)
```

# Controlling indexing

## Default content
By default the object is serialized as JSON object stored as default field of "content".

You can override this by simply providing a object=>content selector function to **AddObjects()** or **Search** linq operator.
```csharp
// defining using AddObjects()
var engine = new ObjectSearchEngine()
    .AddObjects(items, (item) => item.Title); // <-- this will build index on title content only.

// You can also do this for Linq .Search operator
foreach(var item in items.Search("christmas", (item) => item.Title))
    ...
```


## Additional Custom fields
You can add additional fields by implementing a doc selector action.  

```csharp
// index "title" as additional field
foreach(var item in items.Search("title:christmas", (record, doc) => doc.AddTextField("title", record.Title, Field.Store.NO))
{
    ...
}
```

> If you add a "content" field you will override the normal behavior of searching over the JSON respresentation of the object.
