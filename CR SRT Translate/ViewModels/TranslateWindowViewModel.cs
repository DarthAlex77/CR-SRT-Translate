using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CR_SRT_Translate.Models;

namespace CR_SRT_Translate.ViewModels
{
    internal class TranslateWindowViewModel : ObservableRecipient
    {
        #region Constructor

        public TranslateWindowViewModel()
        {
            SaveCommand = new RelayCommand<Window>(Save!, _ => _canSave);
            WeakReferenceMessenger.Default.Register<ObservableCollection<Line>>(this, Receive);
            OriginalText   = string.Empty;
            TranslatedText = string.Empty;

        }
        #endregion

        #region Methods

        private void Receive(object recipient, ObservableCollection<Line> message)
        {
            int           sentenceIndex = 1;
            List<string>  sentences     = new List<string>();
            StringBuilder builder       = new StringBuilder();
            foreach (Line line in message)
            {
                if (line.SentenceIndex != sentenceIndex)
                {
                    sentences.Add(builder.ToString());
                    sentenceIndex++;
                    builder.Clear();
                    builder.AppendJoin(' ', line.Text.ReplaceLineEndings(" "));
                }
                else
                {
                    builder.AppendJoin(' ', line.Text.ReplaceLineEndings(" "));
                }
            }
            sentences.Add(builder.ToString());
            OriginalText = string.Join('\n', sentences);
        }

        #endregion

        #region Commands

        #region SaveCommand

        public RelayCommand<Window> SaveCommand { get; set; }

        private void Save(Window window)
        {
            WeakReferenceMessenger.Default.Send(TranslatedText);
            window.Close();
        }

        #endregion

        #endregion

        #region Fields

        private bool   _canSave;
        private string _originalText   = null!;
        private string _translatedText = null!;

        #endregion

        #region Properties

        public string OriginalText
        {
            get => _originalText;
            set => SetProperty(ref _originalText, value);
        }
        public string TranslatedText
        {
            get => _translatedText;
            set
            {
                if (value == _translatedText)
                {
                    return;
                }
                _translatedText = value;
                OnPropertyChanged();
                _canSave = !TranslatedText.IsNullOrWhiteSpace();
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        #endregion
    }
}