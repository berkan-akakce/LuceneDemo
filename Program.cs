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
                new Product { Name = "Huawei MateBook X i5-10210U 16 GB 512 GB SSD UHD Graphics 13\" Notebook" },
                new Product { Name = "Samsung Galaxy S9 64 GB Gri" },
                new Product { Name = "SAMSUNG MZ7L3480HCHQ-00A07 PM893 480GB SATA ENTERPRISE SSD (KUTUSUZ)" },
                new Product { Name = "iPhone 8 Plus 128 GB Gold" },
                new Product { Name = "iPhone XR 128 GB Aksesuarsız Kutu Beyaz" },
                new Product { Name = "Huawei P40 Pro 256 GB Gümüş" },
                new Product { Name = "iPhone 12 Mini 256 GB Kırmızı" },
                new Product { Name = "Huawei P40 Pro 256 GB Gümüş" },
                new Product { Name = "Samsung Galaxy S10 Plus 128 GB Beyaz" },
                new Product { Name = "Huawei MateBook X i5-10210U 16 GB 512 GB SSD UHD Graphics 13\" Notebook" },
                new Product { Name = "Samsung Galaxy A20 32 GB Siyah" },
                new Product { Name = "iPhone 8 Plus 128 GB Gold" },
                new Product { Name = "Samsung Galaxy A30s 64 GB Beyaz" },
                new Product { Name = "Alcatel 1T 16 GB 7\" Tablet" },
                new Product { Name = "Oppo A5 2020 64 GB Siyah" },
                new Product { Name = "Philips EP2220/10 Tam Otomatik Espresso Makinesi (Teşhir Ürünü)" },
                new Product { Name = "Evk Evonline Large Luxury Gamer 3D Siyah/Yeşil Oyuncu Koltuğu (Teşhir Ürünü)" },
                new Product { Name = "Tefal Easy Fry Grill & Steam+ Az Yağlı Fritöz (Teşhir Ürünü)" },
                new Product { Name = "Csa-İnox 6 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Karacasan Endüstriyel Elektrikli Set Üstü Börek Teşhir Benmarisi" },
                new Product { Name = "Csa-İnox 8 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Karacasan Endüstriyel Elektrikli Set Üstü Börek Teşhir Benmarisi" },
                new Product { Name = "Csa-Inox 5 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Csa-İnox 8 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Csa-Inox 5 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Csa-İnox 10 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Karacasan Endüstriyel Elektrikli Set Üstü Börek Teşhir Benmarisi" },
                new Product { Name = "Csa-İnox 6 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Csa-İnox 10 Adet 1/3 Gastronorm Küvetli Set Üstü Teşhir Buzdolabı" },
                new Product { Name = "Vera SMG Temperli Doğrama Camı" },
                new Product { Name = "Franke Active Plus Doccia Bianco Mutfak Bataryası" },
                new Product { Name = "Miele Aqua 12.5 ml Kurutma Makinesi Koku Flakonu" },
                new Product { Name = "Miele Kurutma Makinesi Koku Flakonu Cocoon 12,5 ml (Yeni)" },
                new Product { Name = "Miele POWERDİSK 6" },
                new Product { Name = "Franke OLX 651 Sağ Çelik Evye (SS)" },
                new Product { Name = "NSK SİENA ANKASTRE BANYO BATARYASI (YUVARLAK)" },
                new Product { Name = "Portabianco ST- 155 Teşhir Buzdolabı, 130 Lt, 304 Kalite" },
                new Product { Name = "Portabianco ST- 210 Teşhir Buzdolabı, 140 Lt, 304 Kalite" },
                new Product { Name = "My Mob Betong Plus Marco Şifonyer Teşhir" },
                new Product { Name = "Tom Ford Lip Lacquer Liquid Tint - 05 Exhibitionis Teşhir" },
                new Product { Name = "My Mob Ottoke Dresuar Teşhir" },
                new Product { Name = "My Mob Betong Plus Prilika Konsol Teşhir" },
                new Product { Name = "My Mob Betong Plus Plamen Dresuar Teşhir" },
                new Product { Name = "My Mob Cross Orta Sehpa Teşhir" },
                new Product { Name = "My Mob Oseyo Dresuar Teşhir" },
                new Product { Name = "My Cosenza Dresuar Teşhir" },
                new Product { Name = "My Mob Nossa Konsol Teşhir" },
                new Product { Name = "My Mob Ticaba Tv Sehpası Teşhir" },
                new Product { Name = "Sandalie Grande / Efes Mutfak Masa Takımı - Cappucino Teşhir" },
                new Product { Name = "Ferro Fl21259a-C Çelik 36 Mm Rose Gold Kadın Saati+ Bileklik Teşhir" },
                new Product { Name = "My Mob Silla Orta Sehpa Teşhir" },
                new Product { Name = "My Mob Obsoyo Ayakkabılık Teşhir" },
                new Product { Name = "My Mob Betong Plus İndira Dresuar Teşhir" },
                new Product { Name = "My Mob Nuguso Orta Sehpa Teşhir" },
                new Product { Name = "My Mob Crotone Tekerlekli Geniş Kitaplık Teşhir" },
                new Product { Name = "Retro Kitaplık Dekoratif 9 Raflı 2 Kapaklı Suntalam Kitaplık - Ceviz/Koyu Lacivert Teşhir" },
                new Product { Name = "My Mob Lara Tv Sehpası Teşhir" },
                new Product { Name = "My Mob Betong Plus İllas Dresuar Teşhir" },
                new Product { Name = "Tom Ford Lip Lacquer Liquid Tint - 06 La Vie En Rouge Teşhir" },
                new Product { Name = "My Mob Betong Plus Dencia Konsol Teşhir" },
                new Product { Name = "My Mob Dowel Orta Sehpa Teşhir" },
                new Product { Name = "My Mob Seria Konsol Teşhir" },
                new Product { Name = "My Mob Betong Plus Spanze Teşhir" },
                new Product { Name = "My Mob Betong Plus Polla Komodin Teşhir" },
                new Product { Name = "My Mob Betong Plus Suwon 2 Kapaklı 2 Çekmeceli Gardırop - Meşe Gri Teşhir" },
                new Product { Name = "Taral Tp600 Piton Asılır (Tar60) Teşhir" },
                new Product { Name = "My Mob Via Orta Sehpa Teşhir" },
                new Product { Name = "My Mob Prava Konsol Teşhir" },
                new Product { Name = "My Mob Betong Plus Astava Yan Sehpa Teşhir" },
                new Product { Name = "My Mob Betong Plus Chunky 3 Kapaklı 3 Çekmeceli Gardırop - Meşe Gri Teşhir" },
                new Product { Name = "My Mob Salerno Dresuar Teşhir" },
                new Product { Name = "My Mob Betong Plus Rana Sürgü Gardırop Teşhir" },
                new Product { Name = "My Mob Kiogi Dresuar Teşhir" },
                new Product { Name = "My Mob Venezia Dresuar Teşhir" },
                new Product { Name = "My Mob Betong Plus Janna Vestiyer Teşhir" },
                new Product { Name = "My Mob Zipa Kitaplık Teşhir" },
                new Product { Name = "My Mob Yeppuga Komodin Teşhir" },
                new Product { Name = "Rüya Kitaplık Dekoratif 5 Raflı 2 Kapaklı Suntalam Kitaplık - Atlantik Çam Teşhir" },
                new Product { Name = "Kebapçı Tipi Buzdolabı Teşhir" },
                new Product { Name = "My Mob Springa Ayakkabılık Teşhir" },
                new Product { Name = "My Mob Potena Kitaplık Teşhir" },
                new Product { Name = "My Mob Dokka Dresuar Teşhir" },
                new Product { Name = "My Mob Betong Plus Witlof Teşhir" },
                new Product { Name = "Pasta Mankeni Teşhir" },
                new Product { Name = "My Potenza Kitaplık Teşhir" },
                new Product { Name = "Taral Tp100 Piton 100 Lt Benzinli İlaçlama Makinesi Teşhir" },
                new Product { Name = "Sandalie Eva 402 - 4 Kapaklı 2 Çekmeceli Gardırop Teşhir" },
                new Product { Name = "My Glasgow Tv Sehpası Teşhir" },
                new Product { Name = "Köfteciler İçin Camlı Buzdolabı Teşhir" },
                new Product { Name = "Tom Ford Lip Lacquer Liquid Tint - 04 In Ectasty Teşhir" },
                new Product { Name = "My Mob Betong Plus Olenj Komodin Teşhir" },
                new Product { Name = "Eva Plus Kitaplık Dekoratif Çok Raflı Suntalam Kitaplık - Beyaz Teşhir" },
                new Product { Name = "My Mob Betong Plus Seva Tv Sehpası Teşhir" },
                new Product { Name = "Betong Plus-Brida Orta Sehpa Teşhir" },
                new Product { Name = "My Venezia Dresuar Teşhir" },
                new Product { Name = "7 Katlı Pasta Mankeni Teşhir" },
                new Product { Name = "My Mob Maku Tv Sehpası Teşhir" },
                new Product { Name = "My Mob Arasso Ayakkabılık Teşhir" },
                new Product { Name = "Sara-Sae Fıg-100 4\" Dişli Çekiç Rakoru Teşhir" },
                new Product { Name = "My Mob Betong Plus Lawen Dresuar Teşhir" }
            ];
        }
    }
}
