using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            const string inputFilePath = "Start216.json";
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

                    if (BannedRegexIsMatch(product.Name))
                        matchedProducts.Add(product.Name);
                }

                if (matchedProducts.Count != 0)
                    File.WriteAllLines(outputFilePath, matchedProducts);
            }
        }

        private static bool IsMatch(string input, string[] patterns)
        {
            return patterns.All(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        private static bool BannedRegexIsMatch(string input)
        {
            return
                IsMatch(input, patterns: new[] { @"m[ıi]nox[ıi]l\w*|minoxidil\w*|bioxinin\w*|prom[ıi]nox[ıi]l|testogel|delay 48000\w*|xdelay 48000\w*|maflor*|sonda\w*.*idrar|idrar.*sonda\w*|opti-?free|klitoral|d[ıi]ldo|mast[uü]rbat[oö]r|an[uü]s|azd[ıi]r[ıi][cç][ıi]|pen[ıi]s(?:li)?|lovetoy|pretty ?love|endotrakeal|tenek[uü]l[uü]m|reflor|Gaspass\w*|alopexy\w*|stag 9000|tracoe|spirometre|riester|contractubex|jockstrap|umca|strath|proxar|legalon|mt cosmet[ıi]cs|makeuptime|convatec|umkaled|polybactum\w*|elfbar|perwill|vozol|globbie\w*|joypara|paysafe|erox|acornbella|cialis\w*|cs13-85|nicorette\w*|özel bölgesi aç[ıi]k|propanthol|noskar|biohira|zinco[- ]c|eoprotin|loprox|pregomin|sadece (?:ankara|[ıi]stanbul)|[ıi]stanbula özel|remifemin|arkopharma harpadol|zade vital corvital|strath cold öksürük|su (?:maymunu|boncuğu|jeli)|büyüyen su topları|[ıi]nsektisit|akarisit|herbisit|bakırsülfat|g[oö]zta[sş][ıi]|nematisit|fungusit|antiparazit|fumigant|allicin|varroa" }) ||
                IsMatch(input, patterns: new[] { "uraw", "mavi|blue" }) ||
                IsMatch(input, patterns: new[] { @"avene\w*", @"tr[ıi]acneal\w*" }) ||
                IsMatch(input, patterns: new[] { "opti", "free" }) ||
                IsMatch(input, patterns: new[] { "istek", "damla" }) ||
                IsMatch(input, patterns: new[] { "anal", "top|tıkaç|vakum|plug" }) ||
                IsMatch(input, patterns: new[] { "vibrat[oö]r", @"testis\w*|belden|teknolojik|tıkaç|cm|külot\w*|anal|uyarıcı|seks|dokulu|büyük|gerçekçi|mini|kıvrımlı|duyarlı|prostat" }) ||
                IsMatch(input, patterns: new[] { "nikotin", "(?:sakız|band)[ıi]" }) ||
                IsMatch(input, patterns: new[] { "medika", "sarf" }) ||
                IsMatch(input, patterns: new[] { "keton", "[oö]l[cç][uü]m" }) ||
                IsMatch(input, patterns: new[] { "idrar", "stribi" }) ||
                IsMatch(input, patterns: new[] { "enjekt[oö]r", "ucu" }) ||
                IsMatch(input, patterns: new[] { "pansuman", "arabas[ıi]" }) ||
                IsMatch(input, patterns: new[] { "cerrahi", "dikiş" }) ||
                IsMatch(input, patterns: new[] { "jinekolojik", "masa" }) ||
                IsMatch(input, patterns: new[] { "kan", "alma", "koltuğu"}) ||
                IsMatch(input, patterns: new[] { "hasta", "muayene", "masası"}) ||
                IsMatch(input, patterns: new[] { "muayene", "masas[ıi]|lambas[ıi]"}) ||
                IsMatch(input, patterns: new[] { "m[uü]dehale", "sedye"}) ||
                IsMatch(input, patterns: new[] { "serum", "ask[ıi]s[ıi]"}) ||
                IsMatch(input, patterns: new[] { "stim[uü]lat[oö]r", "cihaz[ıi]" }) ||
                IsMatch(input, patterns: new[] { "mueller", "diz"}) ||
                IsMatch(input, patterns: new[] { "b[oö]brek", "k[uü]veti" }) ||
                IsMatch(input, patterns: new[] { "göz", "eşeli" }) ||
                IsMatch(input, patterns: new[] { "aspiratör", "portatif" }) ||
                IsMatch(input, patterns: new[] { "ambu", "cihaz[ıi]" }) ||
                IsMatch(input, patterns: new[] { "l[ıi]fe", "tea", "9" }) ||
                IsMatch(input, patterns: new[] { @"(?:zayıflama|hayat|esila) çay\w*", "9" }) ||
                IsMatch(input, patterns: new[] { "demo", "apple|samsung|iphone" }) ||
                IsMatch(input, patterns: new[] { "merry see", "fant[ae]z[ıi]|jartiyerli deri|özel bölgesi açık|nefes kesen|kışkırtıcı|arkası açık|göz alıcı|seksi|alttan açık|fantazi slip|deri boxer|kostümü|deri takım|gelin kız|göğüs ucu" }) ||
                IsMatch(input, patterns: new[] { "fant[ae]z[ıi]", "mite love|eldiven|duvaklı|kostüm|emay|gecel[ıi]k|elbise|mayokini|jartiyerli|mel bee|bacio|vixson|deri|seksi|lablinque|kelepçe|kelepçesi|k[ıi]rba[cç]|nightlight|moonlight|erkek|liona" }) ||
                IsMatch(input, patterns: new[] { "mite Love", "seksi|deri|jartiyer" }) ||
                IsMatch(input, patterns: new[] { "seksi", "bacio|erkek|redhotbest" }) ||
                IsMatch(input, patterns: new[] { "erkek", "g[- ]string" }) ||
                IsMatch(input, patterns: new[] { "açık", "babydoll" }) ||
                IsMatch(input, patterns: new[] { "vernel", "oda", @"koku\w*" }) ||
                IsMatch(input, patterns: new[] { "kamagra", "jel" }) ||
                IsMatch(input, patterns: new[] { "hn 25", "anti-ishal" }) ||
                IsMatch(input, patterns: new[] { "aptamil", "ar", "anti-reflü" }) ||
                IsMatch(input, patterns: new[] { "rosenna", "gül suyu" }) ||
                IsMatch(input, patterns: new[] { "demir", "şurubu", "[ıi]ron ?bis|pediatr[ıi]k" }) ||
                IsMatch(input, patterns: new[] { "xbox", "ay", "3" }) ||
                IsMatch(input, patterns: new[] { "steam", "key|c[uü]zdan" }) ||
                IsMatch(input, patterns: new[] { "apple", "store", "itunes" }) ||
                IsMatch(input, patterns: new[] { "origin", "cd" }) ||
                IsMatch(input, patterns: new[] { "google|itunes", "hediye|[ck]ar[dt]ı?|gift" }) ||
                IsMatch(input, patterns: new[] { "psn", "network|card|key" }) ||
                IsMatch(input, patterns: new[] { "pubg", "uc" }) ||
                IsMatch(input, patterns: new[] { "valorant", "vp" }) ||
                IsMatch(input, patterns: new[] { "hemen", "üyelik" }) ||
                IsMatch(input, patterns: new[] { "anında", "teslim", "key" }) ||
                IsMatch(input, patterns: new[] { "knight", "cash" }) ||
                IsMatch(input, patterns: new[] { "fast", "delivery" }) ||
                IsMatch(input, patterns: new[] { "netflix", "gift" }) ||
                IsMatch(input, patterns: new[] { "netflix", "hediye", "kartı" }) ||
                IsMatch(input, patterns: new[] { "dijital", "abonelik" }) ||
                IsMatch(input, patterns: new[] { "ejder", "paras[ıi]" }) ||
                IsMatch(input, patterns: new[] { "legends", "riot" }) ||
                IsMatch(input, patterns: new[] { "game", "pass", "ay" }) ||
                IsMatch(input, patterns: new[] { "bein", "connect", @"ay\w*" }) ||
                IsMatch(input, patterns: new[] { "mobile", "legends", "elmas" }) ||
                IsMatch(input, patterns: new[] { "razer", "gold", "tr" }) ||
                IsMatch(input, patterns: new[] { "point", "blank", "tg" }) ||
                IsMatch(input, patterns: new[] { "pc", "cd", "key" }) ||
                IsMatch(input, patterns: new[] { "cd", "key", "hemen|uplay" }) ||
                IsMatch(input, patterns: new[] { "origin", "key" }) ||
                IsMatch(input, patterns: new[] { "ön", "ödemeli", "kart" }) ||
                IsMatch(input, patterns: new[] { "tinder", @"abone\w*" }) ||
                IsMatch(input, patterns: new[] { "hediye", "kartı" }) ||
                IsMatch(input, patterns: new[] { "varino", "varis" }) ||
                IsMatch(input, patterns: new[] { "henkel", "maske" }) ||
                IsMatch(input, patterns: new[] { "aspgemix", "sarı", "köpük" }) ||
                IsMatch(input, patterns: new[] { "proles", "yangın", "köpük" }) ||
                IsMatch(input, patterns: new[] { "feti[sş]", @"(?:kemer|set)\w*" }) ||
                IsMatch(input, patterns: new[] { "seks", "salıncağı" }) ||
                IsMatch(input, patterns: new[] { "titreşim(?:li)?", "vibrat[oö]r" }) ||
                IsMatch(input, patterns: new[] { "erotik", @"(?:denge|oyun)\w*|film" }) ||
                IsMatch(input, patterns: new[] { "yaşam", @"çift\w*", "battaniye" }) ||
                IsMatch(input, patterns: new[] { "babydoll", "fant[ae]z[ıi]|jartiyer|dantel|gecelik" }) ||
                IsMatch(input, patterns: new[] { "almera", "golyat" }) ||
                IsMatch(input, patterns: new[] { "elektronik", "sigara" }) ||
                IsMatch(input, patterns: new[] { "era", @"aroma\w*" }) ||
                IsMatch(input, patterns: new[] { @"theravet\w*", "tablet|solüsyon|krem|sıvı" }) ||
                IsMatch(input, patterns: new[] { "serum fizyolojik|flakon", "wee|weebaby|miraderm|bebsi|fizyoser|b[ıi]of[ıi]z|thomson|minich|ime dc|fizyoes|babysoin|septomer|fizyonaz|aquaser|nasalmer|fizyo|ccmed|rinomer|nasobaby|gogove|opti|[ıi]me|babyright|iyon|nasomer|aquanose|fizyosol|doctormed|sente|bronsept|megafiz|ankamarin|besnim" }) ||
                IsMatch(input, patterns: new[] { "d[ıi]j[ıi]tsu|onvo|axen", @"(ov(?:32150|42250|43250|50350|65500|75F500)|ax(?:32DAB04|32DAB13|50f[ıi]l242|43d[ıi]L13)|32DS8500|32DS9800|43DS9800|50DS8800|50DS9800|55DS8500|65DS8500|65DS8800)\w*" }) ||
                IsMatch(input, patterns: new[] { "rüzgar", "ocak", "([234]|(?:iki|üç|dört)) gözlü|set üstü(?: lpg)?|[234]'l[iü]|set ütü doğalgaz" }) ||
                IsMatch(input, patterns: new[] { @"lescon\w*", "[ck]ampus" }) ||
                IsMatch(input, patterns: new[] { "tarım", "k[uü]k[uü]rd[uü]|ilacı" }) ||
                IsMatch(input, patterns: new[] { "sıvı|toz|zirai", "k[uü]k[uü]rt" }) ||
                IsMatch(input, patterns: new[] { "bitki", "gelişim", "düzenleyicisi|[uü]yar[ıi]c[ıi]" }) ||
                IsMatch(input, patterns: new[] { "bordo", "bulamac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "yabancı|kurutma", "ot", "ilac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "kırmızı", "örümcek", "ilac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "salyangoz|külleme|akar|çekirge|mildiyö", "ilac[ıi]|yem[ıi]" }) ||
                IsMatch(input, patterns: new[] { "ha[sş]ere", "k[uü]k[uü]rd[uü]|[ıi]lac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "salyongoz", "yem[ıi]" }) ||
                IsMatch(input, patterns: new[] { "sümüklü", "böcek", "yem[ıi]" }) ||
                IsMatch(input, patterns: new[] { "ot", "kurutucu" }) ||
                IsMatch(input, patterns: new[] { "yaprak", "biti", "ilac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "patates", "böceği", "ilac[ıi]" }) ||
                IsMatch(input, patterns: new[] { "antifungal", "ila[cç]" }) ||
                IsMatch(input, patterns: new[] { "antibiyotik", "toz|oregano" }) ||
                IsMatch(input, patterns: new[] { "r[oö]hnfried", "usnegano" }) ||
                IsMatch(input, patterns: new[] { "veteriner", "[uü]r[uü]n[uü]" }) ||
                IsMatch(input, patterns: new[] { "hayvan", "sa[gğ]l[ıi][gğ][ıi]", "[uü]r[uü]n[uü]" }) ||
                (
                    Regex.IsMatch(input, pattern: "ereksiyon|vajina", RegexOptions.IgnoreCase) &&
                    !Regex.IsMatch(input, pattern: "jel|solüsyon", RegexOptions.IgnoreCase)
                ) ||
                (
                    Regex.IsMatch(input, pattern: "silver shell", RegexOptions.IgnoreCase) &&
                    Regex.IsMatch(input, pattern: "ahcc", RegexOptions.IgnoreCase) &&
                    !Regex.IsMatch(input, pattern: "shiitake", RegexOptions.IgnoreCase) &&
                    !Regex.IsMatch(input, pattern: "hexose", RegexOptions.IgnoreCase)
                );
        }
    }
}