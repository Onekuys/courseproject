using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject.Data
{
     public record MovieRevenue(string Title, int Tickets, decimal Revenue);
     public record ShowtimePercentBought(string Title, TimeSpan Time, string Format, int Reserved, int Total, double Percent);

     public record AdminStats(
         decimal TotalRevenue,
         decimal VipRevenue,
         decimal PaidRevenue,
         int TotalTickets,
         int VipTickets,
         int PaidTickets,
         int CanceledCount,
         double AvgCapacity,
         List<MovieRevenue> MovieRevenues,
         List<ShowtimePercentBought> ShowtimeLoads
     );
}
