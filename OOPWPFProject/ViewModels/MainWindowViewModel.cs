using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using OOPWPFProject.Data;
using OOPWPFProject.Helpers;
using OOPWPFProject.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace OOPWPFProject.ViewModels
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        private readonly EntityManager _entityManager = new EntityManager();
        private readonly UserRepository _userRepository = new();

        public ObservableCollection<ReservationViewModel> Reservations { get; } = new();

        // -----------
        // ВЛАСТИВОСТІ
        // -----------

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadReservations(); // Оновлюємо список при виборі нової дати
            }
        }

        // Пошук
        private string searchText = string.Empty;
        public string SearchText 
        {
            get => searchText;
            set 
            {
                searchText = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            } 
        }

        // Фільтри
        private string activeFilter = "Всі";
        public string ActiveFilter
        {
            get => activeFilter;
            set
            {
                activeFilter = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        // Сортування
        private int sortIndex = 0; // 0=час, 1=назва, 2=місце
        public int SortIndex
        {
            get => sortIndex;
            set
            {
                sortIndex = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        // Обрані записи
        private ReservationViewModel? selected;
        public ReservationViewModel? Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(HasNoSelection)); 
            }
        }
        public bool HasSelection => selected != null;
        public bool HasNoSelection => selected == null; 


        // Нижня панель статусу
        private string statusMessage = "Готово";
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }


        // Виведення сьогоднішньої дати
        public string TodayLabel => SelectedDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("uk-UA"));

        // Статистичні
        private AdminStats? _stats;
        public AdminStats? Stats
        {
            get => _stats;
            private set { _stats = value; OnPropertyChanged(); }
        }



        //--------------
        //   КОМАНДИ
        //--------------

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ApplySortCommand { get; }
        public ICommand GroupCommand { get; }
        public ICommand CompareEqualityCommand { get; }
        public ICommand CompareTimeCommand { get; }
        public ICommand ExportGroupedCommand { get; }


        //-------------
        // Конструктор
        //-------------

        public MainWindowViewModel()
        {
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            DeleteCommand = new RelayCommand(_ => ExecuteDelete(), _ => HasSelection && Selected?.IsCanceled == false);
            CancelCommand = new RelayCommand(_ => ExecuteCancel(), _ => HasSelection && Selected?.IsCanceled == false);
            ApplySortCommand = new RelayCommand(_ => ApplyFilterAndSort());

            GroupCommand = new RelayCommand(_ => ExecuteGroup(), _ => HasSelection);
            CompareTimeCommand = new RelayCommand(_ => ExecuteCompareTime());
            ExportGroupedCommand = new RelayCommand(_ => ExecuteExportGrouped());

            LoadReservations();
        }

        //--------------------
        // Завантаження даних
        //--------------------

        public void LoadReservations()
        {
            var users = _userRepository.GetAll();
            var raw = _entityManager.GetAllReservations(SelectedDate);

            var vms = raw.Select(r => 
            {
                var vm = new ReservationViewModel(r);
                var user = users.FirstOrDefault(u => u.Id == r.UserId);
                vm.SetUser(user);
                return vm;
            }).ToList();

            RebuildCollection(vms);
            Stats = _entityManager.GetAdminStats(SelectedDate);
            StatusMessage = $"Завантажено: {Reservations.Count} записів за {TodayLabel}";
        }

        private void ApplyFilterAndSort()
        {
            var users = _userRepository.GetAll();

            ReservationViewModel ToVM(Reservation r)
            {
                var vm = new ReservationViewModel(r);
                var user = users.FirstOrDefault(u => u.Id == r.UserId);
                vm.SetUser(user);
                return vm;
            }

            var raw = _entityManager.GetAllReservations(SelectedDate.Date).Select(ToVM);

            IEnumerable<ReservationViewModel> filtered;

            if (ActiveFilter == "Скасовані")
            {
                filtered = _entityManager.GetAllReservations(SelectedDate.Date).Select(ToVM).Where(r => r.IsCanceled);
            }
            else
            {
                filtered = ActiveFilter switch
                {
                    "VIP" => raw.Where(r => r.IsVip && !r.IsCanceled),
                    "IMAX" => raw.Where(r => r.Format == "IMAX" && !r.IsCanceled),
                    _ => raw.Where(r => !r.IsCanceled) // "Всі"
                };
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(r => r.MovieTitle.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            var sorted = SortIndex switch
            {
                1 => filtered.OrderBy(r => r.MovieTitle),
                2 => filtered.OrderBy(r => r.SeatNumber),
                _ => filtered.OrderBy(r => r.ShowTime)
            };

            RebuildCollection(sorted.ToList());
            Stats = _entityManager.GetAdminStats(SelectedDate);
            StatusMessage = $"Показано: {Reservations.Count} записів";
        }

        private void RebuildCollection(List<ReservationViewModel> list)
        {
            Reservations.Clear();
            foreach (var r in list) Reservations.Add(r);
        }



        //----------------
        //   Дії команд
        //----------------

        private void ExecuteAdd()
        {
            AddRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteDelete() 
        {
            if (selected == null) return;
            var result = MessageBox.Show(
                $"Видалити бронювання?\n{selected.MovieTitle}, місце {selected.SeatNumber}",
                "Видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;
            _entityManager.RemoveReservation(selected.Source);
            Logger.Log("Видалено", $"Запис #{selected.Id}: {selected.MovieTitle}");
            LoadReservations();
            StatusMessage = "Запис видалено.";
            ConfirmationRequested?.Invoke(this, "Бронювання успішно видалено!");
        }

        private void ExecuteCancel()
        {
            if (selected == null) return;
            _entityManager.CancelReservation(selected.Source);
            Logger.Log("Скасовано", $"Запис #{selected.Id}: {selected.MovieTitle}");
            LoadReservations();
            StatusMessage = "Запис скасовано.";
            ConfirmationRequested?.Invoke(this, "Бронювання успішно скасовано!");
        }

        private void ExecuteGroup()
        {
            if (selected == null) return;
            var grouped = Reservations.Where(r => r.MovieTitle == selected.MovieTitle && !r.IsCanceled).ToList();

            string info = string.Join("\n", grouped.Select(r => $"  • {r.ShowTime:hh\\:mm} — Місце {r.SeatNumber} ({r.Format})"));

            MessageRequested?.Invoke(this, $"Усі записи для «{selected.MovieTitle}»:\n{info}\n\nЗагалом: {grouped.Count} броню.");
        }

        private void ExecuteCompareTime()
        {
            var items = Reservations.Where(r => !r.IsCanceled).ToList();
            if (items.Count < 2)
            {
                MessageRequested?.Invoke(this, "Потрібно щонайменше 2 активних записи.");
                return;
            }

            var earliest = items.OrderBy(r => r.ShowTime).First(); 
            MessageRequested?.Invoke(this, $"Найбільш ранній сеанс:\n«{earliest.MovieTitle}» о {earliest.ShowTime:hh\\:mm}  (Місце {earliest.SeatNumber})");
        }

        private void ExecuteExportGrouped()
        {
            var groups = Reservations.Where(r => !r.IsCanceled).GroupBy(r => r.MovieTitle).OrderBy(g => g.Key);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Експорт груп — {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"Сеанси на {SelectedDate:yyyy-MM-dd}");

            foreach (var group in groups)
            {
                sb.AppendLine($"\n{group.Key} ({group.Count()} бронь):");
                foreach (var r in group.OrderBy(r => r.ShowTime))
                    sb.AppendLine($"  {r.ShowTime:hh\\:mm}  Місце {r.SeatNumber}  [{r.Format}]{(r.IsVip ? " VIP" : "")}");

                string path = System.IO.Path.Combine("Data", $"AllReservations{SelectedDate:yyyy-MM-dd}.txt");
                Directory.CreateDirectory("Data");
                File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);

                Logger.Log("Експорт", $"Файл: {path}");
                StatusMessage = $"Експортовано → {path}";
            }
            MessageRequested?.Invoke(this, $"Файл збережено:\n");
        }


        public event EventHandler? AddRequested;
        public event EventHandler<string>? ConfirmationRequested;
        public event EventHandler<string>? MessageRequested;

        // -----------------------
        // INotifyPropertyChanged
        // -----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

