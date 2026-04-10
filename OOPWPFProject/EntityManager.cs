using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OOPWPFProject
{
    public class EntityManager<T>
    {
        public ObservableCollection<T> Items = new ObservableCollection<T>();

        public void Add(T item) { Items.Add(item); }
        public void Remove(T item) { Items.Remove(item); }
        public void Clear() { Items.Clear(); }
        public int Count() => Items.Count; 

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Items.Count) { throw new IndexOutOfRangeException(); } else { return Items[index]; }
            }
            set
            {
                if (index < 0 || index >= Items.Count) { throw new IndexOutOfRangeException(); } else { Items[index] = value; }
            }
        }
        public void DisplayAll()
        {
            if (!Items.Any()) { MessageBox.Show("Записи відсутні.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            
            StringBuilder allMovies = new StringBuilder();

            foreach (var item in Items)
            {
                allMovies.AppendLine(item.ToString());
                allMovies.AppendLine("--------------------------");
            }
            MessageBox.Show(allMovies.ToString(), "Список сеансів", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
