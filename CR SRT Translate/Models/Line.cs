using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CR_SRT_Translate.Models
{
    public class Line : ObservableObject
    {
        private string _translatedText = null!;

        public Line()
        { }

        public Line(string lineIndex, string starTime, string endTime, string text)
        {
            LineIndex      = int.Parse(lineIndex);
            StartTime      = TimeSpan.Parse(starTime);
            EndTime        = TimeSpan.Parse(endTime);
            Text           = text;
            TranslatedText = string.Empty;
        }

        public int      LineIndex { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime   { get; set; }
        public string   Text      { get; set; } = null!;
        public string TranslatedText
        {
            get => _translatedText;
            set => SetProperty(ref _translatedText, value);
        }
    }
}