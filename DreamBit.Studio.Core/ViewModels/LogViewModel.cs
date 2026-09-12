using System;
using System.Collections.ObjectModel;
using System.Threading;
using DreamBit.Engine.Diagnostics;
using DreamBit.Engine.Notification;
using DreamBit.Studio.Mvvm;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Console do editor: espelha o <see cref="EngineLog"/> numa coleção bindável,
    /// marshalando para a thread de UI (funciona igual em WPF e Avalonia). Mostra
    /// erros de compilação/execução de scripts e qualquer mensagem do motor.
    /// </summary>
    public sealed class LogViewModel : NotificationObject, IDisposable
    {
        private readonly SynchronizationContext? _sync = SynchronizationContext.Current;
        private int _errorCount;

        public LogViewModel()
        {
            foreach (var entry in EngineLog.Entries)
                Add(entry);

            EngineLog.Logged += OnLogged;
            EngineLog.Cleared += OnCleared;

            ClearCommand = new RelayCommand(EngineLog.Clear);
        }

        public ObservableCollection<LogEntry> Entries { get; } = new();

        public RelayCommand ClearCommand { get; }

        /// <summary>Quantidade de erros no buffer atual (para destacar o console).</summary>
        public int ErrorCount
        {
            get => _errorCount;
            private set => Set(ref _errorCount, value);
        }

        public bool HasErrors => _errorCount > 0;

        private void OnLogged(LogEntry entry)
        {
            if (_sync != null)
                _sync.Post(_ => Add(entry), null);
            else
                Add(entry);
        }

        private void OnCleared()
        {
            if (_sync != null)
                _sync.Post(_ => ClearLocal(), null);
            else
                ClearLocal();
        }

        private void Add(LogEntry entry)
        {
            Entries.Add(entry);
            if (entry.Level == LogLevel.Error)
            {
                ErrorCount++;
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        private void ClearLocal()
        {
            Entries.Clear();
            ErrorCount = 0;
            OnPropertyChanged(nameof(HasErrors));
        }

        public void Dispose()
        {
            EngineLog.Logged -= OnLogged;
            EngineLog.Cleared -= OnCleared;
        }
    }
}
