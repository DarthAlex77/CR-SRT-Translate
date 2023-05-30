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
            StringBuilder builder = new StringBuilder();
            foreach (Line line in message)
            {
                builder.AppendLine(line.Text.Replace('\n', ' ').Replace('\r', ' '));
            }
            OriginalText = builder.ToString();
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