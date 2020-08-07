using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams_Zoom_Sample.Models
{
    public class Appointment
    {
        public string CarType { get; set; }
        public string PackageType { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Name { get; set; }
        public string Phone{ get; set; }

    }

    public class Data
    {
        public string dateTimeValue { get; set; }
    }
}
