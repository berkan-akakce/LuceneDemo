using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneDemo;
using System.Globalization;

internal class Program
{
    private const string Field = "name";

    private static void Main()
    {
        // Specify the compatibility version we want.
        const LuceneVersion matchVersion = LuceneVersion.LUCENE_48;

        // Open the Directory using a Lucene Directory class.
        //string indexName = "lucene_index";
        //string path = Path.Combine(Environment.CurrentDirectory, indexName);
        //using FSDirectory indexDir = FSDirectory.Open(path);
        using RAMDirectory indexDir = new();

        // Create an analyzer to process the text.
        using Analyzer analyzer = new StandardAnalyzer(matchVersion);

        // Create an index config.
        IndexWriterConfig indexConfig = new(matchVersion, analyzer)
        {
            OpenMode = OpenMode.CREATE // create/overwrite index
        };

        // Create an index writer.
        using (IndexWriter writer = new(d: indexDir, conf: indexConfig))
        {
            Product[] documents = CreateExampleData();

            foreach (var document in documents)
            {
                // Create a new document.
                Document doc =
                [
                    // Add fields to the document.
                    new TextField(
                        name: Field,
                        value: document.Name.ToLower(culture: new CultureInfo(name: "tr-TR")),
                        store: Lucene.Net.Documents.Field.Store.YES
                    ),
                ];

                // Add the document to the index.
                writer.AddDocument(doc);
            }

            // Commit changes and close the writer.
            writer.Commit();
        }

        using (IndexReader reader = DirectoryReader.Open(directory: indexDir))
        {
            BooleanQuery booleanQuery = CreateBooleanQuery(analyzer);

            // Searching the index
            IndexSearcher searcher = new(r: reader);
            var hits = searcher.Search(query: booleanQuery, n: int.MaxValue).ScoreDocs;

            // Store the document ids of the matched documents.
            HashSet<int> matchedDocIds = [];

            foreach (var hit in hits)
                matchedDocIds.Add(item: hit.Doc);

            // Iterate over all documents in the index to find unmatched documents.
            for (int i = 0; i < reader.NumDocs; ++i)
            {
                // Skip matched documents.
                if (matchedDocIds.Contains(item: i))
                    continue;

                // Get the unmatched document and print it.
                var doc = searcher.Doc(docID: i);

                Console.WriteLine(doc.Get(name: Field));
            }
        }

        static BooleanQuery CreateBooleanQuery(Analyzer analyzer)
        {
            const Occur occur = Occur.SHOULD;

            BooleanQuery booleanQuery =
            [
                // Add OR conditions as SHOULD clauses.
                BooleanClauseFactory(text: "kutusuz", occur),
                BooleanClauseFactory(text: "outlet", occur),
                BooleanClauseFactory(text: "revizyonlu", occur),
                BooleanClauseFactory(text: "teşır", occur),
                BooleanClauseFactory(text: "teşir", occur),
                BooleanClauseFactory(text: "teshir", occur),
                BooleanClauseFactory(text: "teşhır", occur),
                BooleanClauseFactory(text: "teşhir", occur),
                BooleanClauseFactory(text: "ürünü", occur),
                BooleanClauseFactory(text: "yenılenmış", occur),
                BooleanClauseFactory(text: "yenilenmiş", occur),
                BooleanClauseFactory(text: "ölü pixel", occur),
                BooleanClauseFactory(text: "ölüpixel", occur),
                BooleanClauseFactory(text: "olu pixel", occur),
                BooleanClauseFactory(text: "olupixel", occur),
                BooleanClauseFactory(text: "ölü piksel", occur),
                BooleanClauseFactory(text: "ölüpiksel", occur),
                BooleanClauseFactory(text: "olu piksel", occur),
                BooleanClauseFactory(text: "olupiksel", occur)
            ];

            // Wildcard searches
            StandardQueryParser queryParser = new(analyzer);
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "Refurbish*", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "Refurbısh*", occur));

            // Add proximity searches (NEAR) using PhraseQuery with slop.
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "\"teshir ürünü\"~50", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "\"ikinci el\"~50", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "\"ıkıncı el\"~50", occur));

            // NEAR(("kutu*", deforme), 0)
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "kutu* deforme", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "deforme kutu*", occur));

            // NEAR(("kutu*", hasarlı), 0)
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "kutu* hasarlı", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "hasarlı kutu*", occur));

            // NEAR(("paket*", hasarlı), 0)
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "paket* hasarlı", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "hasarlı paket*", occur));

            // NEAR(("ambalaj*", hasarlı), 0)
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "ambalaj* hasarlı", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "hasarlı ambalaj*", occur));

            // NEAR(("nakliye*", hasarlı), 0)
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "nakliye* hasarlı", occur));
            booleanQuery.Add(clause: GetBooleanClause(queryParser, query: "hasarlı nakliye*", occur));

            return booleanQuery;
        }

        static BooleanClause BooleanClauseFactory(string text, Occur occur)
        {
            return new(query: new TermQuery(t: new Term(fld: Field, text)), occur);
        }

        static BooleanClause GetBooleanClause(StandardQueryParser queryParser, string query, Occur occur)
        {
            return new BooleanClause(
                query: queryParser.Parse(query, defaultField: Field),
                occur
            );
        }

        static Product[] CreateExampleData()
        {
            //TODO: prpgInof tablosundan ürün isimleri alınacak.

            // Example data
            return
            [
                new Product { Name = "Teshir Ürün Outlet", },
                new Product { Name = "Teshir Olmayan Ürün Outlet", },
                new Product { Name = "Yenilenmiş İkinci El" },
                new Product { Name = "İkinci Üçüncü Dördüncü El" },
                new Product { Name = "Test Product 1" },
                new Product { Name = "Refurbish Test Outlet" },
                new Product { Name = "Refurbısh Test Outlet" },
                new Product { Name = "Test Product 2" },
                new Product { Name = "iPhone Ambalaj Hasarlı" },
                new Product { Name = "Refurbished Product" },
                new Product { Name = "Kutusu Deforme Ürün" },
                new Product { Name = "Deforme Kutulu Ürün" }
            ];
        }
    }
}
