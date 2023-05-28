using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CR_SRT_Translate.Models
{
    public static class SrtEngine
    {
        public static IEnumerable<Line> ParseSrt(string srtLines)
        {
            MatchCollection matches = Regex.Matches(srtLines,
                                                    "([0-9]+)\n([0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3}) --> ([0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3})\n(.*?)\n\n",
                                                    RegexOptions.Singleline);
            List<Line> lines         = new List<Line>(matches.Count);
            int        sentenceIndex = 0;
            foreach (Match match in matches)
            {
                if (Regex.Match(match.Groups[4].Value, "^[^a-z]*:").Success || match.Groups[4].Value.StartsWith('(') || match.Groups[4].Value.StartsWith('['))
                {
                    sentenceIndex++;
                }
                lines.Add(new Line(match.Groups[1].Value, sentenceIndex, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value));
            }
            List<Line> groupBy = lines.GroupBy(x => x.SentenceIndex).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
            foreach (Line line in groupBy)
            {
                line.IsOneLine = false;
            }

            return lines;
        }

        public static string WriteSrt(IEnumerable<Line> lines)
        {
            string srt = string.Empty;
            foreach (Line srtLine in lines)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(srtLine.LineIndex);
                builder.Append('\n');
                builder.Append(srtLine.StartTime.ToString(@"hh\:mm\:ss\,fff"));
                builder.Append(" --> ");
                builder.Append(srtLine.EndTime.ToString(@"hh\:mm\:ss\,fff"));
                builder.Append('\n');
                builder.Append(srtLine.TranslatedText);
                builder.Append('\n');
                builder.Append('\n');
                srt += builder.ToString();
            }
            return srt;
        }
    }
}