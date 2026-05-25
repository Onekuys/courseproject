using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using OOPWPFProject.Models;

namespace OOPWPFProject.Data
{
    public class TicketRepository
    {
        private static readonly string FilePath = Path.Combine("Data", "tickets.json");
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public record TicketRecord(
            int ReservationId, int UserId, string UserFullName, string UserPhone, string UserEmail,
            string MovieTitle, string ShowDate, string ShowTime, string Format, int SeatNumber,
            bool IsVip, decimal Price, string PaidAt
        );

        private List<TicketRecord> ReadAll()
        {
            if (!File.Exists(FilePath)) return new();

            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<TicketRecord>>(json) ?? new();
            }
            catch { return new(); }
        }

        private void WriteAll(List<TicketRecord> tickets)
        {
            Directory.CreateDirectory("Data");
            File.WriteAllText(FilePath, JsonSerializer.Serialize(tickets, JsonOpts), Encoding.UTF8);
        }

        public void SaveTicket(User user, Reservation r)
        {
            var all = ReadAll();
            all.Add(new TicketRecord(
                ReservationId: r.Id,
                UserId: user.Id,
                UserFullName: user.FullName,
                UserPhone: user.Phone,
                UserEmail: user.Email,
                MovieTitle: r.MovieTitle,
                ShowDate: r.ShowDate.ToString("dd.MM.yyyy"),
                ShowTime: r.ShowTime.ToString(@"hh\:mm"),
                Format: r.Format,
                SeatNumber: r.SeatNumber,
                IsVip: r.IsVip,
                Price: r.Price,
                PaidAt: DateTime.Now.ToString("yyyy-MM--dd HH:mm:ss")
                ));
            WriteAll(all);
        }

        public List<TicketRecord> GetForUser(int userId) => ReadAll().FindAll(t => t.UserId == userId);
    }
}
