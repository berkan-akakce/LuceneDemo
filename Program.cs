using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LuceneDemo
{
    internal class Program
    {
        private static readonly Regex NonNewRegex = new Regex(
            pattern: @"kutusuz|outlet|revizyonlu|te[sş]h?[ıi]r\b|ürünü|yen[ıi]lenmi[sş]|[oö]l[uü] ?p[ıi](?:ks|x)el|refurb[ıi]sh\w*|(?:teshir.*ürünü|[ıi]k[ıi]nc[ıi].*el)|(?:nakliye|ambalaj|paket|kutu)\w* hasarl[ıi]|hasarl[ıi] (?:nakliye|ambalaj|paket|kutu)\w*|kutu\w* deforme|deforme kutu\w*",
            options: RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static void Main()
        {
            const string inputFilePath = "Start.json";
            const string outputFilePath = "matchedProducts.txt";
            var matchedProducts = new List<string>();

            //var json = File.ReadAllText(inputFilePath);
            //var products = JsonConvert.DeserializeObject<List<Product>>(json);
            //var matchedProducts = 
            //    products
            //        .Where(product => NonNewRegex.IsMatch(product.Name))
            //        .Select(product => product.Name)
            //        .ToList();

            //File.WriteAllLines(outputFilePath, matchedProducts);

            using (StreamReader file = File.OpenText(inputFilePath))
            using (var reader = new JsonTextReader(file))
            {
                var serializer = new JsonSerializer();
                reader.Read();

                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType != JsonToken.StartObject)
                        continue;

                    var product = serializer.Deserialize<Product>(reader);

                    if (NonNewRegex.IsMatch(product.Name))
                        matchedProducts.Add(product.Name);
                }

                if (matchedProducts.Count != 0)
                    File.WriteAllLines(outputFilePath, matchedProducts);
            }
        }
    }
}