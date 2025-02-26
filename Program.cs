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
        private static readonly string Packaging = "(nakliye|ambalaj|paket|kutu)\\w*";
        private static readonly string Damaged = "hasarl[ıi]";
        private static readonly string Box = @"kutu\w*";
        private static readonly string Deformed = "deforme";

        private static readonly Regex NonNewRegex = new Regex(
            pattern: $@"kutusuz|outlet|re(vizyonlu|furb[ıi]sh\w*)|te[sş]h?[ıi]r\b|{Refurbished}|{DeadPixel}|{Packaging} {Damaged}|{Damaged} {Packaging}|{Box} {Deformed}|{Deformed} {Box}",
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
            pattern: @"([ıi]nsekti|akari|herbi|nemati|fungu|antipara)[sz]it|\b(an[uü]s|erox)|a(cornbella|llicin|lopexy\w*|rkopharma harpadol|zd[ıi]r[ıi][cç][ıi])|b(akırsülfat|iohira|ioxinin\w*|üyüyen su topları)|c(ialis\w*|ontractubex|onvatec|s13-85)|d[ıi]ldo|e(lfbar|ndotrakeal|oprotin)|fumigant|g([oö]zta[sş][ıi]|aspass\w*|lobbie\w*)|j(ockstrap|oypara)|klitoral|l(egalon|oprox|ovetoy)|m([ıi]nox[ıi](d[ıi])?l\w*|aflor\w*|akeuptime|ast[uü]rbat[oö]r|t cosmet[ıi]cs)|n(icorette\w*|oskar)|opti-?free|p(aysafe|en[ıi]s(li)?|erwill|olybactum\w*|regomin|retty ?love|rom[ıi]nox[ıi]l|ropanthol|roxar)|r(eflor|emifemin|iester)|s(adece (ankara|[ıi]stanbul)|pirometre|tag 9000|trath( cold öksürük)?|u (maymunu|boncuğu|jeli))|t(enek[uü]l[uü]m|estogel|racoe)|um(ca|kaled)|v(arroa|ozol)|x?delay 48000\w*|z(ade vital corvital|inco[- ]c)|özel bölgesi aç[ıi]k|[ıi]stanbula özel",
            options: options | RegexOptions.Compiled
        );

        private static readonly string[][] BannedProductPatterns =
        {
            new[] { @"\banal\b", "t(op|ıkaç)|vakum|plug" },
            new[] { "vibrat[oö]r", @"anal|b(elden|üyük)|cm|dokulu|gerçekçi|külot|mini|prostat|seks|t(eknolojik|estis\w*|ıkaç)|uyarıcı|(kıvrım|duyar)lı" },
            new[] { "almera", "golyat" },
            new[] { "ambu", "cihaz[ıi]" },
            new[] { "antibiyotik", "toz|oregano" },
            new[] { "antifungal", "ila[cç]" },
            new[] { "aspiratör", "portatif" },
            new[] { "b[oö]brek", "k[uü]veti" },
            new[] { "babydoll", "açık|dantel|fant[ae]z[ıi]|jartiyer|gecelik" },
            new[] { "bein", "connect", @"ay\w*" },
            new[] { "bordo", "bulamac[ıi]" },
            new[] { "cerrahi", "dikiş" },
            new[] { "d[ıi]j[ıi]tsu|onvo|axen", "(ov(3215|4(2|3)25)|5035|(65|75F)50)0|ax(32dab(04|13)|50f[ıi]l242|43d[ıi]l13)|(32|43|50|55|65)ds((85|(8|9)8)00)" },
            new[] { "demo", "apple|samsung|iphone" },
            new[] { "dijital", "abonelik" },
            new[] { "ejder", "paras[ıi]" },
            new[] { "elektronik", "sigara" },
            new[] { "enjekt[oö]r", "ucu" },
            new[] { @"\bera\b", "aroma" },
            new[] { "erkek", "g[- ]string" },
            new[] { "erotik", "(denge|oyun)|film" },
            new[] { "fant[ae]z[ıi]", "bacio|d(eri|uvaklı)|e(lbise|ldiven|may|rkek)|gecel[ıi]k|jartiyerli|k([ıi]rba[cç]|elepçe(:?si)?|ostüm)|l(ablinque|iona)|m(ayokini|el bee|ite love)|seksi|vixson|(moon|night)light" },
            new[] { "fast", "delivery" },
            new[] { "feti[sş]", "(kemer|set)" },
            new[] { "game", "pass", @"\bay\b" },
            new[] { "google|itunes", "hediye|[ck]ar[dt]ı?|gift" },
            new[] { "göz", "eşeli" },
            new[] { "ha[sş]ere", "k[uü]k[uü]rd[uü]|[ıi]lac[ıi]" },
            new[] { "hediye", "kartı" },
            new[] { "hemen", "üyelik" },
            new[] { "henkel", "maske" },
            new[] { "hn 25", "anti-ishal" },
            new[] { "idrar", "s(onda|tribi)" },
            new[] { "istek", "damla" },
            new[] { "jinekolojik", "masa" },
            new[] { "kamagra", "jel" },
            new[] { "keton", "[oö]l[cç][uü]m" },
            new[] { "knight", "cash" },
            new[] { "l[ıi]fe", "tea", "9" },
            new[] { "legends", "riot" },
            new[] { "m[uü]d[ae]hale", "sedye"},
            new[] { "medika", "sarf" },
            new[] { "merry see", "a(lttan açık|rkası açık)|deri (boxer|takım)|fant[ae]z[ıi]( slip)?|g(elin kız|ö(z alıcı|ğüs ucu))|jartiyerli deri|k(ostümü|ışkırtıcı)|nefes kesen|seksi" },
            new[] { "mite Love", "deri|jartiyer|seksi" },
            new[] { "mobile", "legends", "elmas" },
            new[] { "muayene", "masas[ıi]|lambas[ıi]"},
            new[] { "mueller", "diz"},
            new[] { "netflix", "gift" },
            new[] { "netflix", "hediye", "kartı" },
            new[] { "nikotin", "(sakız|band)[ıi]" },
            new[] { "opti", "free" },
            new[] { "origin", @"\b(cd|key)\b" },
            new[] { "ot", "kurutucu" },
            new[] { "pansuman", "arabas[ıi]" },
            new[] { "patates", "böceği", "ilac[ıi]" },
            new[] { @"\bpsn\b", "network|card|key" },
            new[] { "pubg", @"\buc\b" },
            new[] { "r[oö]hnfried", "usnegano" },
            new[] { "rosenna", "gül suyu" },
            new[] { "rüzgar", "ocak", "([234]|(iki|üç|dört)) gözlü|set üstü( lpg)?|[234]'l[iü]|set ütü doğalgaz" },
            new[] { "salyongoz", "yem[ıi]" },
            new[] { "salyangoz|külleme|akar|çekirge|mildiyö", "ilac[ıi]|yem[ıi]" },
            new[] { "seks", "salıncağı" },
            new[] { "seksi", "bacio|erkek|redhotbest" },
            new[] { "serum", "ask[ıi]s[ıi]"},
            new[] { "serum fizyolojik|flakon", "a(nkamarin|qua(nose|ser))|b([ıi]of[ıi]z|aby(right|soin)|e(bsi|snim)|ronsept)|ccmed|doctormed|fizyo(es|ser|naz|sol)?|gogove|[ıi]me (dc)?|iyon|m(egafiz|(i(nich|raderm)))|nas(obaby|almer|omer)|opti|rinomer|se(nte|ptomer)|thomson|wee(baby)?" },
            new[] { "steam", "key|c[uü]zdan" },
            new[] { "stim[uü]lat[oö]r", "cihaz[ıi]" },
            new[] { "sıvı|toz|zirai", "k[uü]k[uü]rt" },
            new[] { "tarım", "k[uü]k[uü]rd[uü]|ilacı" },
            new[] { "tinder", @"abone\w*" },
            new[] { "titreşim(li)?", "vibrat[oö]r" },
            new[] { "uraw", "mavi|blue" },
            new[] { "valorant", "vp" },
            new[] { "varino", "varis" },
            new[] { "veteriner", "[uü]r[uü]n[uü]" },
            new[] { "anında", "teslim", "key" },
            new[] { "apple", "store", "itunes" },
            new[] { "aptamil", @"\bar\b", "anti-reflü" },
            new[] { "aspgemix", "sarı", "köpük" },
            new[] { "avene", @"tr[ıi]acneal" },
            new[] { "bitki", "gelişim", "düzenleyicisi|[uü]yar[ıi]c[ıi]" },
            new[] { "demir", "şurubu", "[ıi]ron ?bis|pediatr[ıi]k" },
            new[] { "hasta", "muayene", "masası"},
            new[] { "hayvan", "sa[gğ]l[ıi][gğ][ıi]", "[uü]r[uü]n[uü]" },
            new[] { "kan", "alma", "koltuğu"},
            new[] { "kırmızı", "örümcek", "ilac[ıi]" },
            new[] { "lescon", "[ck]ampus" },
            new[] { "point", "blank", @"\btg\b" },
            new[] { "proles", "yangın", "köpük" },
            new[] { "razer", "gold", @"\btr\b" },
            new[] { "sümüklü", "böcek", "yem[ıi]" },
            new[] { "theravet", "krem|s(ıvı|olüsyon)|tablet" },
            new[] { "vernel", "oda", @"koku\w*" },
            new[] { "xbox", "ay", "3" },
            new[] { "yabancı|kurutma", "ot", "ilac[ıi]" },
            new[] { "yaprak", "biti", "ilac[ıi]" },
            new[] { "yaşam", "çift", "battaniye" },
            new[] { "ön", "ödemeli", "kart" },
            new[] { @"hemen|uplay|\bpc\b", @"\bcd\b", "key" },
            new[] { "(zayıflama|hayat|esila) çay", "9" }
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