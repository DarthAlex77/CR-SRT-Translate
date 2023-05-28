using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CR_SRT_Translate.Models
{
    public class Line : ObservableObject
    {
        private int    _numberOfWords;
        private string _translatedText;

        public Line()
        { }

        public Line(string lineIndex, int sentenceIndex, string starTime, string endTime, string text)
        {
            IsOneLine      = true;
            LineIndex      = int.Parse(lineIndex);
            SentenceIndex  = sentenceIndex;
            StartTime      = TimeSpan.Parse(starTime);
            EndTime        = TimeSpan.Parse(endTime);
            Text           = text;
            NumberOfWords  = text.ReplaceLineEndings(" ").Split(' ').Length;
            TranslatedText = " ";
        }

        public bool     IsOneLine     { get; set; }
        public int      LineIndex     { get; set; }
        public int      SentenceIndex { get; set; }
        public TimeSpan StartTime     { get; set; }
        public TimeSpan EndTime       { get; set; }
        public string   Text          { get; set; }
        public string TranslatedText
        {
            get => _translatedText;
            set => SetProperty(ref _translatedText, value);
        }
        public int NumberOfWords
        {
            get => _numberOfWords;
            set => SetProperty(ref _numberOfWords, value);
        }
    }
}