using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject
{
    public interface ICancelable
    {
        bool IsCanceled { get; set; }
        void Cancel();
        string GetReservationDetails();
    }
}
