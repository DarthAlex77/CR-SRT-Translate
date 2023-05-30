using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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
        #region Bindable Properties

        public ObservableCollection<Line> Lines { get; set; }

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            _canTranslate           =  false;
            _canSave                =  false;
            Lines                   =  new ObservableCollection<Line>();
            Lines.CollectionChanged += Lines_CollectionChanged;
            OpenSrtCommand          =  new RelayCommand<Window>(OpenSrt!);
            OpenJsonCommand         =  new RelayCommand<Window>(OpenJson!);
            SaveSrtCommand          =  new RelayCommand<Window>(SaveSrt!,   _ => _canSave);
            SaveJsonCommand         =  new RelayCommand<Window>(SaveJson!,  _ => _canSave);
            TranslateCommand        =  new RelayCommand<Window>(Translate!, _ => _canTranslate);
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

        private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Lines.Any())
            {
                _canTranslate = true;
                TranslateCommand.NotifyCanExecuteChanged();
            }
        }

        private void Line_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TranslatedText" when Lines.Any(line => !line.TranslatedText.IsNullOrWhiteSpace()):
                    _canSave = true;
                    SaveSrtCommand.NotifyCanExecuteChanged();
                    SaveJsonCommand.NotifyCanExecuteChanged();
                    break;
                case "TranslatedText":
                    _canSave = false;
                    SaveSrtCommand.NotifyCanExecuteChanged();
                    SaveJsonCommand.NotifyCanExecuteChanged();
                    break;
            }
        }

        #endregion

        #region Fields

        private          bool                   _canTranslate;
        private          bool                   _canSave;
        private readonly List<FileDialogFilter> _srtFilter;
        private readonly List<FileDialogFilter> _jsonFilter;
        private          string?                _fileName;

        #endregion

        #region Commands

        #region OpenSrtCommand

        public RelayCommand<Window> OpenSrtCommand { get; set; }

        public async void OpenSrt(Window window)
        {
            Lines.Clear();
            OpenFileDialog dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters       = _srtFilter
            };
            string[]? path = await dialog.ShowAsync(window);

            if (path != null && path.Any())
            {
                _fileName = path[0].Split("\\").Last().Split('.').First();
                foreach (Line line in SrtEngine.ParseSrt(await File.ReadAllTextAsync(path[0])))
                {
                    Lines.Add(line);
                    line.PropertyChanged += Line_PropertyChanged;
                }
            }
        }

        #endregion

        #region OpenJsonCommand

        public RelayCommand<Window> OpenJsonCommand { get; set; }

        public async void OpenJson(Window window)
        {
            Lines.Clear();
            OpenFileDialog dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters       = _jsonFilter
            };
            string[]? path = await dialog.ShowAsync(window);
            if (path != null && path.Any())
            {
                string json = await File.ReadAllTextAsync(path[0]);
                foreach (Line line in JsonSerializer.Deserialize<IEnumerable<Line>>(json))
                {
                    Lines.Add(line);
                    line.PropertyChanged += Line_PropertyChanged;
                }
            }
        }

        #endregion

        #region SaveSrtCommand

        public RelayCommand<Window> SaveSrtCommand { get; set; }

        public async void SaveSrt(Window window)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filters          = _srtFilter,
                DefaultExtension = ".srt",
                InitialFileName  = _fileName
            };
            string path = await dialog.ShowAsync(window) ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                string srt = SrtEngine.WriteSrt(Lines);
                await File.WriteAllTextAsync(path!, srt);
            }
        }

        #endregion

        #region SaveJsonCommand

        public RelayCommand<Window> SaveJsonCommand { get; set; }

        public async void SaveJson(Window window)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filters          = _jsonFilter,
                DefaultExtension = ".json"
            };
            string path = await dialog.ShowAsync(window) ?? string.Empty;
            if (!path.IsNullOrWhiteSpace())
            {
                string json = JsonSerializer.Serialize(Lines,
                                                       new JsonSerializerOptions
                                                       {
                                                           Encoder       = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                                                           WriteIndented = true
                                                       });
                await File.WriteAllTextAsync(path, json);
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
            List<string?> translatedLines = message.Split('\n').ToList();
            for (int index = 0; index < Lines.Count; index++)
            {
                Lines[index].TranslatedText = translatedLines[index];
            }
        }

        #endregion

        #endregion
    }
}