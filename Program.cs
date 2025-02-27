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
            new[] { "teşh?[ıi]r", @"\b((aya[gğ]|dolab|[km]asas|[uü]n[ıi]tes)[ıi]|k(ase|ol[ıi]s[ıi]|utusu)|(panos|reyon)u|s(et|tand)[ıi]|t(aba(k|[gğ]ı)|eps[ıi]\w*))\b" },
            new[] { @"\b(air outlet version|m[ıi]n[ıi]mal|melam[ıi]n|plast[ıi]k|akr[ıi]l[ıi]k|pol[ıi]karbon|(açık)?büfe|ayna)\b" },
            new[] { $"{Refurbished} {Edition}" },
            new[] { $"{Edition} {Refurbished}" },
        };

        private static readonly Regex BannedProductRegex = new Regex(
            pattern: @"([ıi]nsekti|akari|herbi|nemati|fungu|antipara)[sz]it|\b(an[uü]s|erox)|a(cornbella|l(licin|opexy)|rkopharma harpadol|zd[ıi]r[ıi][cç][ıi])|b(akırsülfat|io(hira|xinin)|üyüyen su topları)|c(ialis|on(tractubex|vatec)|s13-85)|d[ıi]ldo|e(lfbar|ndotrakeal|oprotin)|fumigant|g([oö]zta[sş][ıi]|aspass|lobbie)|jo(ckstrap|ypara)|klitoral|l(egal|o(pr|vet))o[nxy]|m([ıi]nox[ıi](d[ıi])?l|aflor|akeuptime|ast[uü]rbat[oö]r|t cosmet[ıi]cs)|n(icorette|oskar)|opti-?free|p(aysafe|en[ıi]s(li)?|erwill|olybactum|regomin|retty ?love|rom[ıi]nox[ıi]l|ropanthol|roxar)|r(eflor|emifemin|iester)|s(adece (ankara|[ıi]stanbul)|pirometre|tag 9000|trath( cold öksürük)?|u (maymunu|boncuğu|jeli))|t(enek[uü]l[uü]m|estogel|racoe)|um(ca|kaled)|v(arroa|ozol)|x?delay 48000|z(ade vital corvital|inco[- ]c)|özel bölgesi aç[ıi]k|[ıi]stanbula özel",
            options: options | RegexOptions.Compiled
        );

        private static readonly Regex VibratorRegex = new Regex(
            pattern: @"anal|b(elden|üyük)|cm|dokulu|gerçekçi|külot|mini|prostat|seks|t(e(knolojik|stis)|ıkaç)|(uyarıc|(kıvrım|duyar)l)ı",
            options: options | RegexOptions.Compiled
        );

        private static readonly Regex FantasyRegex = new Regex(
            pattern: @"bacio|d(eri|uvaklı)|e(lbise|ldiven|may|rkek)|gecel[ıi]k|jartiyerli|k([ıi]rba[cç]|elepçe(si)?|ostüm)|l(ablinque|iona)|m(ayokini|(el be|ite lov)e)|seksi|vixson|(moon|n[ıi]ght)l[ıi]ght",
            options: options | RegexOptions.Compiled
        );

        private static readonly Regex MerrySeeRegex = new Regex(
            pattern: @"a(lttan|rkası) açık|deri (boxer|takım)|fant[ae]z[ıi]( slip)?|g(elin kız|ö(z alıcı|ğüs ucu))|jartiyerli deri|k(ostümü|ışkırtıcı)|nefes kesen|seksi",
            options: options | RegexOptions.Compiled
        );

        private static readonly Regex DijitsuRegex = new Regex(
            pattern: @"(ov(3215|4(2|3)25)|5035|(65|75F)50)0|ax(32dab(04|13)|50f[ıi]l242|43d[ıi]l13)|(32|43|50|55|65)ds((85|(8|9)8)00)",
            options: options | RegexOptions.Compiled
        );

        private static readonly Regex SerumRegex = new Regex(
            pattern: "a(nkamarin|qua(nose|ser))|b([ıi]of[ıi]z|aby(right|soin)|e(bs|sn)im?|ronsept)|ccmed|doctormed|fizyo(es|naz|s(er|ol))?|gogove|[ıi]me (dc)?|iyon|m(egafiz|(i(nich|raderm)))|nas(obaby|almer|omer)|opti|rinomer|se(nte|ptomer)|thomson|wee(baby)?",
            options: options | RegexOptions.Compiled
        );

        private static readonly string[][] BannedProductPatterns =
        {
            new[] { "babydoll", "açık|dantel|fant[ae]z[ıi]|jartiyer|gecelik" },
            new[] { "seksi", "bacio|erkek|redhotbest" },
            new[] { @"\banal\b", "t(op|ıkaç)|vakum|plug" },
            new[] { "titreşim(li)?", "vibrat[oö]r" },
            new[] { "enjekt[oö]r", "ucu" },
            new[] { "muayene", "masas[ıi]|lambas[ıi]"},
            new[] { "pansuman", "arabas[ıi]" },
            new[] { "erkek", "g[- ]string" },
            new[] { "cerrahi", "dikiş" },
            new[] { "valorant", @"\bvp\b" },
            new[] { "steam", "key|c[uü]zdan" },
            new[] { "elektronik", "sigara" },
            new[] { "pubg", @"\buc\b" },
            new[] { "serum", "ask[ıi]s[ıi]"},
            new[] { "idrar", "s(onda|tribi)" },
            new[] { "sıvı|toz|zirai", "k[uü]k[uü]rt" },
            new[] { "legends", "riot" },
            new[] { "feti[sş]", "(kemer|set)" },
            new[] { "mobile", "legends", "elmas" },
            new[] { "salyangoz|külleme|akar|çekirge|mildiyö", "ilac[ıi]|yem[ıi]" },
            new[] { "bordo", "bulamac[ıi]" },
            new[] { "keton", "[oö]l[cç][uü]m" },
            new[] { "istek", "damla" },
            new[] { "origin", @"\b(cd|key)\b" },
            new[] { "erotik", "denge|oyun|film" },
            new[] { @"\bpsn\b", "network|card|key" },
            new[] { "lescon", "[ck]ampus" },
            new[] { "hediye", "kartı" },
            new[] { "b[oö]brek", "k[uü]veti" },
            new[] { "ha[sş]ere", "k[uü]k[uü]rd[uü]|[ıi]lac[ıi]" },
            new[] { "point", "blank", @"\btg\b" },
            new[] { "aspiratör", "portatif" },
            new[] { "ejder", "paras[ıi]" },
            new[] { "stim[uü]lat[oö]r", "cihaz[ıi]" },
            new[] { "göz", "eşeli" },
            new[] { "knight", "cash" },
            new[] { "demo", "apple|samsung|iphone" },
            new[] { "kan", "alma", "koltuğu"},
            new[] { "google|itunes", "hediye|[ck]ar[dt]ı?|gift" },
            new[] { "tarım", "k[uü]k[uü]rd[uü]|ilacı" },
            new[] { "rüzgar", "ocak", "([234]|(iki|üç|dört)) gözlü|set üstü( lpg)?|[234]'l[iü]|set ütü doğalgaz" },
            new[] { "r[oö]hnfried", "usnegano" },
            new[] { "uraw", "mavi|blue" },
            new[] { "dijital", "abonelik" },
            new[] { "veteriner", "[uü]r[uü]n[uü]" },
            new[] { "seks", "salıncağı" },
            new[] { "kırmızı", "örümcek", "ilac[ıi]" },
            new[] { "ot", "kurutucu" },
            new[] { "tinder", "abone" },
            new[] { "theravet", "krem|s(ıvı|olüsyon)|tablet" },
            new[] { "yaprak", "biti", "ilac[ıi]" },
            new[] { "l[ıi]fe", "tea", "9" },
            new[] { "bitki", "gelişim", "düzenleyicisi|[uü]yar[ıi]c[ıi]" },
            new[] { "jinekolojik", "masa" },
            new[] { "antibiyotik", "toz|oregano" },
            new[] { "yaşam", "çift", "battaniye" },
            new[] { "yabancı|kurutma", "ot", "ilac[ıi]" },
            new[] { @"hemen|uplay|\bpc\b", @"\bcd\b", "key" },
            new[] { "(zayıflama|hayat|esila) çay", "9" },
            new[] { "almera", "golyat" },
            new[] { "ambu", "cihaz[ıi]" },
            new[] { "antifungal", "ila[cç]" },
            new[] { "anında", "teslim", "key" },
            new[] { "apple", "store", "itunes" },
            new[] { "aptamil", @"\bar\b", "anti-reflü" },
            new[] { "aspgemix", "sarı", "köpük" },
            new[] { "avene", @"tr[ıi]acneal" },
            new[] { "bein", "connect", @"\bay" },
            new[] { "demir", "şurubu", "[ıi]ron ?bis|pediatr[ıi]k" },
            new[] { "fast", "delivery" },
            new[] { "game", "pass", @"\bay\b" },
            new[] { "hasta", "muayene", "masası"},
            new[] { "hayvan", "sa[gğ]l[ıi][gğ][ıi]", "[uü]r[uü]n[uü]" },
            new[] { "hemen", "üyelik" },
            new[] { "henkel", "maske" },
            new[] { "hn 25", "anti-ishal" },
            new[] { "kamagra", "jel" },
            new[] { "m[uü]d[ae]hale", "sedye"},
            new[] { "medikal", "sarf" },
            new[] { "mite Love", "deri|jartiyer|seksi" },
            new[] { "mueller", "diz"},
            new[] { "netflix", "gift" },
            new[] { "netflix", "hediye", "kartı" },
            new[] { "nikotin", "(sakız|band)[ıi]" },
            new[] { "opti", "free" },
            new[] { "patates", "böceği", "ilac[ıi]" },
            new[] { "proles", "yangın", "köpük" },
            new[] { "razer", "gold", @"\btr\b" },
            new[] { "rosenna", "gül suyu" },
            new[] { "salyongoz", "yem[ıi]" },
            new[] { "sümüklü", "böcek", "yem[ıi]" },
            new[] { "varino", "varis" },
            new[] { "vernel", "oda", "koku" },
            new[] { "xbox", "ay", "3" },
            new[] { "ön", "ödemeli", "kart" },
            new[] { @"\bera\b", "aroma" }
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
                        Regex.IsMatch(input, pattern: @"\bel\b", options)
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
                (
                    Regex.IsMatch(input, pattern: "vibrat[oö]r", options) &&
                    VibratorRegex.IsMatch(input)
                ) ||
                (
                    Regex.IsMatch(input, pattern: "fant[ae]z[ıi]", options) &&
                    FantasyRegex.IsMatch(input)
                ) ||
                DoesMatchPatternGroup(input, patterns: BannedProductPatterns) ||
                (
                    Regex.IsMatch(input, pattern: "merry see", options) &&
                    MerrySeeRegex.IsMatch(input)
                ) ||
                (
                    Regex.IsMatch(input, pattern: "d[ıi]j[ıi]tsu|onvo|axen", options) &&
                    DijitsuRegex.IsMatch(input)
                ) ||
                (
                    Regex.IsMatch(input, pattern: "serum fizyolojik|flakon", options) &&
                    SerumRegex.IsMatch(input)
                ) ||
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