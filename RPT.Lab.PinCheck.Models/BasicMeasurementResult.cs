using System;
using System.Collections.Generic;
using System.Text;

namespace RPT.Lab.PinCheck.Models
{
    public class BasicMeasurementResult
    {
        public int VfChGndPin { get; set; }
        public int VfChTestPin { get; set; }
        public double Resistance { get; set; }
        public bool CommonConnected { get; set; }
        public bool OutOfRange => Resistance.Equals(double.NaN) == true;
        public bool Result => ((CommonConnected && Resistance <= 10) || (!CommonConnected && OutOfRange));
    }
}
