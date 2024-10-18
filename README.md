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

# Linq style .Search() operator 
The **.Search()** operator will build an index on the fly over the collection of objects and then allow you to do a full text search over the collection. 

The results of a Search() call is a SearchResult with **.Score** and **.Value** property. You can do further Linq operations on it like .Select(), .Where(), etc.

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

> NOTE 1: This pattern builds a new index with each call.  If you want to perform multiple queries over a collection you should use **ObjectSearchEngine**.

> NOTE 2: You should apply linq constraints to collection before calling .Search() to minimize the amount of indexing needed.

# ObjectSearchEngine 
ObjectSearchEngine represents a search engine over a collection of objects which you can call multiple times without penalty.

Example
```csharp
// search over items collection.
var searchEngine = new ObjectSearchEngine()
    .AddObjects(items);

var results1 = searchEngine.Search("foo bar");
var results2 = searchEngine.Search("big cat");
```


# Custom fields
By default the object is serialized as JSON object stored as "content" and the search is performed against "content" as the default field to search over.

You can add additional fields by implementing a doc selector and then adding a field based constraint to the search query.
```csharp
// index "title" as additional field
foreach(var item in items.Search("title:christmas*", (record, doc) => doc.AddTextField("title", record.Title, Field.Store.NO))
{
    ...
}

// only match on title field for the object.
foreach(var item in items.Search("christmas*", (record, doc) => doc.AddTextField("content", record.Title, Field.Store.NO))
{
    ...
}
```

