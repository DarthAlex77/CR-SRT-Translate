using System.Collections.Generic;
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
            List<Line> lines = new List<Line>(matches.Count);
            foreach (Match match in matches)
            {
                lines.Add(new Line(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value));
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