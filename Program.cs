using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LuceneDemo
{
    internal class Program
    {
        private static readonly RegexOptions options = RegexOptions.IgnoreCase;

        private static readonly string Edition = "bask[ıi]";
        private static readonly string Refurbished = "yen[ıi]lenm[ıi][sş]";
        private static readonly string DeadPixel = "[oö]l[uü] ?p[ıi](ks|x)el";
        private static readonly string Box = "(nakliye|ambalaj|paket|kutu)\\w*";
        private static readonly string Damaged = "hasarl[ıi]";

        private static readonly Regex NonNewRegex = new Regex(
            pattern: $@"kutusuz|outlet|revizyonlu|te[sş]h?[ıi]r\b|{Refurbished}|{DeadPixel}|refurb[ıi]sh\w*|{Box} {Damaged}|{Damaged} {Box}|kutu\w* deforme|deforme kutu\w*",
            options: options | RegexOptions.Compiled
        );

        private static readonly string[][] NonNewExcludedPatterns =
        {
            new[] { Refurbished, "perwoll" },
            new[] { "teşh?[ıi]r", @"\b(set[ıi]|kase|[km]asas[ıi]|teps[ıi]\w*|dolab[ıi]|panosu|taba[gğ]ı|reyonu|stand[ıi]|[uü]n[ıi]tesi|aya[gğ][ıi]|kol[ıi]s[ıi]|kutusu)\b" },
            new[] { "\b(air outlet version|m[ıi]n[ıi]mal|melam[ıi]n|plast[ıi]k|akr[ıi]l[ıi]k|pol[ıi]karbon|(açık)?büfe|ayna)\b" },
            new[] { $"{Refurbished} {Edition}" },
            new[] { $"{Edition} {Refurbished}" },
        };

        private static readonly Regex BannedProductRegex = new Regex(
            pattern: @"([ıi]nsekti|akari|herbi|nemati|fungu|antipara)[sz]it|[ıi]stanbula özel|\b(an[uü]s|erox)|a(cornbella|llicin|lopexy\w*|rkopharma harpadol|zd[ıi]r[ıi][cç][ıi])|b(akırsülfat|iohira|ioxinin\w*|üyüyen su topları)|c(ialis\w*|ontractubex|onvatec|s13-85)|d[ıi]ldo|e(lfbar|ndotrakeal|oprotin)|fumigant|g([oö]zta[sş][ıi]|aspass\w*|lobbie\w*)|j(ockstrap|oypara)|klitoral|l(egalon|oprox|ovetoy)|m([ıi]nox[ıi](d[ıi])?l\w*|aflor\w*|akeuptime|ast[uü]rbat[oö]r|t cosmet[ıi]cs)|n(icorette\w*|oskar)|opti-?free|p(aysafe|en[ıi]s(li)?|erwill|olybactum\w*|regomin|retty ?love|rom[ıi]nox[ıi]l|ropanthol|roxar)|r(eflor|emifemin|iester)|s(adece (ankara|[ıi]stanbul)|pirometre|tag 9000|trath( cold öksürük)?|u (maymunu|boncuğu|jeli))|t(enek[uü]l[uü]m|estogel|racoe)|um(ca|kaled)|v(arroa|ozol)|x?delay 48000\w*|z(ade vital corvital|inco[- ]c)|özel bölgesi aç[ıi]k",
            options: options | RegexOptions.Compiled
        );

        private static readonly string[][] BannedProductPatterns =
        {
            new[] { "uraw", "mavi|blue" },
            new[] { "idrar", @"sonda\w*" },
            new[] { @"avene\w*", @"tr[ıi]acneal\w*" },
            new[] { "opti", "free" },
            new[] { "istek", "damla" },
            new[] { @"\banal\b", "top|tıkaç|vakum|plug" },
            new[] { "vibrat[oö]r", @"testis\w*|belden|teknolojik|tıkaç|cm|külot\w*|anal|uyarıcı|seks|dokulu|büyük|gerçekçi|mini|(kıvrım|duyar)lı|prostat" },
            new[] { "nikotin", "(sakız|band)[ıi]" },
            new[] { "medika", "sarf" },
            new[] { "keton", "[oö]l[cç][uü]m" },
            new[] { "idrar", "stribi" },
            new[] { "enjekt[oö]r", "ucu" },
            new[] { "pansuman", "arabas[ıi]" },
            new[] { "cerrahi", "dikiş" },
            new[] { "jinekolojik", "masa" },
            new[] { "kan", "alma", "koltuğu"},
            new[] { "hasta", "muayene", "masası"},
            new[] { "muayene", "masas[ıi]|lambas[ıi]"},
            new[] { "m[uü]d[ae]hale", "sedye"},
            new[] { "serum", "ask[ıi]s[ıi]"},
            new[] { "stim[uü]lat[oö]r", "cihaz[ıi]" },
            new[] { "mueller", "diz"},
            new[] { "b[oö]brek", "k[uü]veti" },
            new[] { "göz", "eşeli" },
            new[] { "aspiratör", "portatif" },
            new[] { "ambu", "cihaz[ıi]" },
            new[] { "l[ıi]fe", "tea", "9" },
            new[] { @"(zayıflama|hayat|esila) çay\w*", "9" },
            new[] { "demo", "apple|samsung|iphone" },
            new[] { "merry see", "fant[ae]z[ıi]|jartiyerli deri|nefes kesen|kışkırtıcı|arkası açık|göz alıcı|seksi|alttan açık|fantazi slip|deri boxer|kostümü|deri takım|gelin kız|göğüs ucu" },
            new[] { "fant[ae]z[ıi]", "mite love|eldiven|duvaklı|kostüm|emay|gecel[ıi]k|elbise|mayokini|jartiyerli|mel bee|bacio|vixson|deri|seksi|lablinque|kelepçe(:?si)?|k[ıi]rba[cç]|(moon|night)light|erkek|liona" },
            new[] { "mite Love", "seksi|deri|jartiyer" },
            new[] { "seksi", "bacio|erkek|redhotbest" },
            new[] { "erkek", "g[- ]string" },
            new[] { "açık", "babydoll" },
            new[] { "vernel", "oda", @"koku\w*" },
            new[] { "kamagra", "jel" },
            new[] { "hn 25", "anti-ishal" },
            new[] { "aptamil", "ar", "anti-reflü" },
            new[] { "rosenna", "gül suyu" },
            new[] { "demir", "şurubu", "[ıi]ron ?bis|pediatr[ıi]k" },
            new[] { "xbox", "ay", "3" },
            new[] { "steam", "key|c[uü]zdan" },
            new[] { "apple", "store", "itunes" },
            new[] { "origin", "cd" },
            new[] { "google|itunes", "hediye|[ck]ar[dt]ı?|gift" },
            new[] { "psn", "network|card|key" },
            new[] { "pubg", "uc" },
            new[] { "valorant", "vp" },
            new[] { "hemen", "üyelik" },
            new[] { "anında", "teslim", "key" },
            new[] { "knight", "cash" },
            new[] { "fast", "delivery" },
            new[] { "netflix", "gift" },
            new[] { "netflix", "hediye", "kartı" },
            new[] { "dijital", "abonelik" },
            new[] { "ejder", "paras[ıi]" },
            new[] { "legends", "riot" },
            new[] { "game", "pass", "ay" },
            new[] { "bein", "connect", @"ay\w*" },
            new[] { "mobile", "legends", "elmas" },
            new[] { "razer", "gold", "tr" },
            new[] { "point", "blank", "tg" },
            new[] { "pc", "cd", "key" },
            new[] { "cd", "key", "hemen|uplay" },
            new[] { "origin", "key" },
            new[] { "ön", "ödemeli", "kart" },
            new[] { "tinder", @"abone\w*" },
            new[] { "hediye", "kartı" },
            new[] { "varino", "varis" },
            new[] { "henkel", "maske" },
            new[] { "aspgemix", "sarı", "köpük" },
            new[] { "proles", "yangın", "köpük" },
            new[] { "feti[sş]", @"(kemer|set)\w*" },
            new[] { "seks", "salıncağı" },
            new[] { "titreşim(li)?", "vibrat[oö]r" },
            new[] { "erotik", @"(denge|oyun)\w*|film" },
            new[] { "yaşam", @"çift\w*", "battaniye" },
            new[] { "babydoll", "fant[ae]z[ıi]|jartiyer|dantel|gecelik" },
            new[] { "almera", "golyat" },
            new[] { "elektronik", "sigara" },
            new[] { "era", @"aroma\w*" },
            new[] { @"theravet\w*", "tablet|solüsyon|krem|sıvı" },
            new[] { "serum fizyolojik|flakon", "wee(baby)?|miraderm|bebsi|fizyo(es|ser)|b[ıi]of[ıi]z|thomson|minich|ime dc|baby(right|soin)|septomer|fizyonaz|aquaser|nas(obaby|almer)|fizyo|ccmed|rinomer|gogove|opti|[ıi]me|iyon|nasomer|aquanose|fizyosol|doctormed|sente|bronsept|megafiz|ankamarin|besnim" },
            new[] { "d[ıi]j[ıi]tsu|onvo|axen", @"(ov(32150|42250|43250|50350|65500|75F500)|ax(32DAB04|32DAB13|50f[ıi]l242|43d[ıi]L13)|32DS8500|32DS9800|43DS9800|50DS8800|50DS9800|55DS8500|65DS8500|65DS8800)\w*" },
            new[] { "rüzgar", "ocak", "([234]|(iki|üç|dört)) gözlü|set üstü( lpg)?|[234]'l[iü]|set ütü doğalgaz" },
            new[] { @"lescon\w*", "[ck]ampus" },
            new[] { "tarım", "k[uü]k[uü]rd[uü]|ilacı" },
            new[] { "sıvı|toz|zirai", "k[uü]k[uü]rt" },
            new[] { "bitki", "gelişim", "düzenleyicisi|[uü]yar[ıi]c[ıi]" },
            new[] { "bordo", "bulamac[ıi]" },
            new[] { "yabancı|kurutma", "ot", "ilac[ıi]" },
            new[] { "kırmızı", "örümcek", "ilac[ıi]" },
            new[] { "salyangoz|külleme|akar|çekirge|mildiyö", "ilac[ıi]|yem[ıi]" },
            new[] { "ha[sş]ere", "k[uü]k[uü]rd[uü]|[ıi]lac[ıi]" },
            new[] { "salyongoz", "yem[ıi]" },
            new[] { "sümüklü", "böcek", "yem[ıi]" },
            new[] { "ot", "kurutucu" },
            new[] { "yaprak", "biti", "ilac[ıi]" },
            new[] { "patates", "böceği", "ilac[ıi]" },
            new[] { "antifungal", "ila[cç]" },
            new[] { "antibiyotik", "toz|oregano" },
            new[] { "r[oö]hnfried", "usnegano" },
            new[] { "veteriner", "[uü]r[uü]n[uü]" },
            new[] { "hayvan", "sa[gğ]l[ıi][gğ][ıi]", "[uü]r[uü]n[uü]" }
        };

        private static void Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            const string inputFilePath = "StartNoError.json";
            //const string outputFilePath = "matchedProducts.txt";
            var matchedProducts = new List<string>();
            var unMatchedProducts = new List<string>();

            using (StreamReader file = File.OpenText(path: inputFilePath))
            using (var reader = new JsonTextReader(reader: file))
            {
                if (!reader.Read())
                    return;

                var serializer = new JsonSerializer();

                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType != JsonToken.StartObject)
                        continue;

                    var product = serializer.Deserialize<Product>(reader);
                    string input = product.Name;

                    if (BannedRegexIsMatch(input) || NonNewRegexIsMatch(input))
                        matchedProducts.Add(item: input);
                    else
                        unMatchedProducts.Add(item: input);
                }

                if (matchedProducts.Count != 0)
                {
                    //File.WriteAllLines(path: outputFilePath, contents: matchedProducts);
                }

                foreach (var product in matchedProducts)
                    Console.WriteLine(value: product);

                Console.WriteLine();
                Console.WriteLine(value: stopwatch.ElapsedMilliseconds);
            }
        }

        private static bool DoesMatchPatternGroup(string input, IEnumerable<string[]> patterns)
        {
            return
                patterns.Any(
                    predicate: array => array.All(
                        predicate: pattern => Regex.IsMatch(input, pattern, options)
                    )
                );
        }

        private static bool NonNewRegexIsMatch(string input)
        {
            return
                (
                    NonNewRegex.IsMatch(input) ||
                    (
                        Regex.IsMatch(input, pattern: "[ıi]k[ıi]nc[ıi]", options) &&
                        Regex.IsMatch(input, pattern: "el", options)
                    ) ||
                    (
                        Regex.IsMatch(input, pattern: "te[sş]hir", options) &&
                        Regex.IsMatch(input, pattern: "[uü]r[uü]n[uü]", options)
                    )
                ) &&
                !DoesMatchPatternGroup(input, patterns: NonNewExcludedPatterns);
        }

        private static bool BannedRegexIsMatch(string input)
        {
            return
                BannedProductRegex.IsMatch(input) ||
                DoesMatchPatternGroup(input, patterns: BannedProductPatterns) ||
                (
                    Regex.IsMatch(input, pattern: "ereksiyon|vajina", options) &&
                    !Regex.IsMatch(input, pattern: "jel|solüsyon", options)
                ) ||
                (
                    Regex.IsMatch(input, pattern: "silver shell", options) &&
                    Regex.IsMatch(input, pattern: "ahcc", options) &&
                    !Regex.IsMatch(input, pattern: "shiitake", options) &&
                    !Regex.IsMatch(input, pattern: "hexose", options)
                );
        }
    }
}