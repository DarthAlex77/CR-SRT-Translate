using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CR_SRT_Translate.Models;
using CR_SRT_Translate.Views;

namespace CR_SRT_Translate.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Constructor

        public MainWindowViewModel()
        {
            _canTranslate        =  false;
            _canSave             =  false;
            Lines                =  new ObservableCollection<Line>();
            View                 =  new DataGridCollectionView(Lines);
            View.PropertyChanged += View_PropertyChanged;
            OpenSrtCommand       =  new RelayCommand<Window>(OpenSrt!);
            OpenJsonCommand      =  new RelayCommand<Window>(OpenJson!);
            SaveSrtCommand       =  new RelayCommand<Window>(SaveSrt!,   _ => _canSave);
            SaveJsonCommand      =  new RelayCommand<Window>(SaveJson!,  _ => _canSave);
            TranslateCommand     =  new RelayCommand<Window>(Translate!, _ => _canTranslate);
            WeakReferenceMessenger.Default.Register<string>(this, Receive);

            #region InitFileDialogFilters

            _srtFilter  = new List<FileDialogFilter>();
            _jsonFilter = new List<FileDialogFilter>();
            FileDialogFilter srt  = new FileDialogFilter();
            FileDialogFilter json = new FileDialogFilter();
            srt.Extensions = new List<string>
            {
                "srt"
            };
            srt.Name = "SubRip video subtitle";
            json.Extensions = new List<string>
            {
                "json"
            };
            json.Name = "JSON file";
            _srtFilter.Add(srt);
            _jsonFilter.Add(json);

            #endregion
        }

        #endregion

        #region Methods

        private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (Lines.Any())
            {
                _canTranslate = true;
                TranslateCommand.NotifyCanExecuteChanged();
                if (Lines.Any(line => !line.TranslatedText.IsNullOrWhiteSpace()))
                {
                    _canSave = true;
                    SaveSrtCommand.NotifyCanExecuteChanged();
                    SaveJsonCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    _canSave = false;
                    SaveSrtCommand.NotifyCanExecuteChanged();
                    SaveJsonCommand.NotifyCanExecuteChanged();
                }
            }

            if (View.CurrentEditItem != null)
            {
                _editingStarted = true;
            }
            if (_editingStarted && View.CurrentEditItem == null)
            {
                Recalculate((Line) View.CurrentItem);
                _editingStarted = false;
            }
        }

        private void Recalculate(Line line)
        {
            if (!line.TranslatedText.IsNullOrWhiteSpace())
            {
                List<Line>   sentenceLines = Lines.Where(x => x.SentenceIndex == line.SentenceIndex).ToList();
                List<string> words         = string.Join(' ', sentenceLines.Select(x=>x.TranslatedText)).Split(' ').ToList();
                foreach (Line line1 in sentenceLines)
                {
                    line1.TranslatedText = string.Join(' ', words.Take(line1.NumberOfWords));
                    try
                    {
                        words.RemoveRange(0, line1.NumberOfWords);
                    }
                    catch (ArgumentException)
                    {
                        words.Clear();
                    }
                }
            }
        }

        #endregion

        #region Bindable Properties

        public DataGridCollectionView View { get; set; }

        public ObservableCollection<Line> Lines
        {
            get => _lines;
            set => SetProperty(ref _lines, value);
        }

        #endregion

        #region Fields

        private          bool                       _canTranslate;
        private          bool                       _canSave;
        private          bool                       _editingStarted;
        private readonly List<FileDialogFilter>     _srtFilter;
        private readonly List<FileDialogFilter>     _jsonFilter;
        private          ObservableCollection<Line> _lines = null!;
        private          string?                    _fileName;

        #endregion

        #region Commands

        #region OpenSrtCommand

        public RelayCommand<Window> OpenSrtCommand { get; set; }

        public void OpenSrt(Window window)
        {
            Lines.Clear();
            OpenFileDialog dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters       = _srtFilter
            };
            string path = dialog.ShowAsync(window).Result?[0] ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                _fileName = path.Split("\\").Last().Split('.').First();
                foreach (Line line in SrtEngine.ParseSrt(File.ReadAllText(path)))
                {
                    Lines.Add(line);
                }
            }
        }

        #endregion

        #region OpenJsonCommand

        public RelayCommand<Window> OpenJsonCommand { get; set; }

        public void OpenJson(Window window)
        {
            Lines.Clear();
            OpenFileDialog dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters       = _jsonFilter
            };
            string path = dialog.ShowAsync(window).Result?[0] ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                string json = File.ReadAllText(path);
                Lines = (ObservableCollection<Line>) (JsonSerializer.Deserialize<ObservableCollection<Line>>(json) ?? Enumerable.Empty<Line>());
            }
        }

        #endregion

        #region SaveSrtCommand

        public RelayCommand<Window> SaveSrtCommand { get; set; }

        public void SaveSrt(Window window)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filters          = _srtFilter,
                DefaultExtension = ".srt",
                InitialFileName  = _fileName
            };
            string path = dialog.ShowAsync(window).Result ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                string srt = SrtEngine.WriteSrt(Lines);
                File.WriteAllText(path!, srt);
            }
        }

        #endregion

        #region SaveJsonCommand

        public RelayCommand<Window> SaveJsonCommand { get; set; }

        public void SaveJson(Window window)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filters          = _jsonFilter,
                DefaultExtension = ".json"
            };
            string path = dialog.ShowAsync(window).Result ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                string json = JsonSerializer.Serialize(Lines,
                                                       new JsonSerializerOptions
                                                       {
                                                           Encoder       = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                                                           WriteIndented = true
                                                       });
                File.WriteAllText(path, json);
            }
        }

        #endregion

        #region TranslateCommand

        public RelayCommand<Window> TranslateCommand { get; set; }

        public void Translate(Window window)
        {
            new TranslateWindow().ShowDialog(window);
            WeakReferenceMessenger.Default.Send(Lines);
        }

        private void Receive(object recipient, string message)
        {
            List<string>     sentences = message.Replace('\r', ' ').Split('\n').ToList();
            List<List<Line>> lines     = Lines.GroupBy(x => x.SentenceIndex).Select(x => x.ToList()).ToList();
            foreach (List<Line> line in lines)
            {
                List<string> words = sentences[line.First().SentenceIndex - 1].Split(' ').ToList();
                foreach (Line line1 in line)
                {
                    line1.TranslatedText = string.Join(' ', words.Take(line1.NumberOfWords));
                    try
                    {
                        words.RemoveRange(0, line1.NumberOfWords);
                    }
                    catch (ArgumentException)
                    {
                        words.Clear();
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}