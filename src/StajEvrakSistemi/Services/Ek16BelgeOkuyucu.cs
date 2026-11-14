using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace StajEvrakSistemi.Services;

public static class Ek16BelgeOkuyucu
{
    private static readonly (string Alan, string Etiket)[] Alanlar =
    [
        ("Ad Soyad", @"Ad[ıi]?\s*(?:ve\s*|/\s*)?Soyad[ıi]?"),
        ("E-posta", @"E\s*[- ]?\s*posta"),
        ("Öğrenci No", @"Öğrenci\s*(?:Numarası|No\.?)"),
        ("Telefon", @"Telefon(?:\s*No\.?)?"),
        ("Kimlik Numarası", @"(?:T\.?\s*C\.?\s*)?Kimlik\s*(?:Numarası|No\.?)"),
        ("Evrak Teslim Tarihi", @"(?:Evrak\s*)?Teslim\s*Tarihi"),
        ("Staj Başlangıç Tarihi", @"Staj\s*Başlangıç\s*Tarihi"),
        ("Staj Bitiş Tarihi", @"Staj\s*Bitiş\s*Tarihi")
    ];

    public static bool FormatGecerli(Stream akim, string uzanti)
    {
        Span<byte> baslik = stackalloc byte[8];
        if (akim.ReadAtLeast(baslik, baslik.Length, false) < baslik.Length)
        {
            return false;
        }

        return uzanti switch
        {
            ".pdf" => baslik.StartsWith("%PDF-"u8),
            ".png" => baslik.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => baslik.StartsWith(new byte[] { 255, 216, 255 }),
            ".doc" => baslik.SequenceEqual(new byte[] { 208, 207, 17, 224, 161, 177, 26, 225 }),
            ".docx" => baslik.StartsWith("PK\u0003\u0004"u8) && WordBelgesiMi(akim),
            _ => false
        };
    }

    private static bool WordBelgesiMi(Stream akim)
    {
        if (!akim.CanSeek)
        {
            return false;
        }

        akim.Position = 0;

        try
        {
            using var zip = new ZipArchive(akim, ZipArchiveMode.Read, leaveOpen: true);
            return zip.GetEntry("word/document.xml") is { Length: > 0 and <= 2_000_000 };
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    public static Dictionary<string, string> Oku(string yol)
    {
        string metin = Path.GetExtension(yol).ToLowerInvariant() switch
        {
            ".pdf" => PdfMetni(yol),
            ".docx" => WordMetni(yol),
            _ => ""
        };

        var degerler = new Dictionary<string, string>();
        var etiketler = string.Join("|", Alanlar.Select(a => a.Etiket));

        foreach (var (alan, etiket) in Alanlar)
        {
            var eslesme = Regex.Match(
                metin,
                $@"(?:^|\s)(?:{etiket})[ \t]*[:：]?[ \t]*([^\r\n]+?)(?=[ \t]+(?:{etiketler})[ \t]*[:：]|\r?\n|$)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline,
                TimeSpan.FromSeconds(1));

            if (eslesme.Success && !string.IsNullOrWhiteSpace(eslesme.Groups[1].Value))
            {
                degerler[alan] = eslesme.Groups[1].Value.Trim();
            }
        }

        return degerler;
    }

    private static string PdfMetni(string yol)
    {
        using var belge = PdfDocument.Open(yol);

        return string.Join("\n", belge.GetPages().Take(10)
            .Select(sayfa => ContentOrderTextExtractor.GetText(sayfa)));
    }

    private static string WordMetni(string yol)
    {
        using var zip = ZipFile.OpenRead(yol);
        var belge = zip.GetEntry("word/document.xml");

        if (belge is null || belge.Length > 2_000_000)
        {
            return "";
        }

        using var akim = belge.Open();
        using var okuyucu = XmlReader.Create(akim, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            MaxCharactersInDocument = 2_000_000
        });

        XNamespace word = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        var xml = XDocument.Load(okuyucu);

        return string.Join("\n", xml.Descendants(word + "p")
            .Select(p => string.Concat(p.Descendants(word + "t").Select(t => t.Value))));
    }
}