using System;
using System.Collections.Generic;
using System.Text;

namespace RPT.Lab.PinCheck.Models
{
    public class PinCheckSelfTestResult
    {
        public bool Pass { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public Dictionary<int, double> FailPins { get; set; } = new Dictionary<int, double>();
    }
}
