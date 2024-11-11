using System;
using System.Collections.Generic;
using System.Text;

namespace RPT.Lab.PinCheck.Models
{
    public class BasicPinCheckCombination
    {
        public List<int> TestVfPins { get; set; } = new List<int>();
        public List<int> GndVfPins { get; set; } = new List<int>();
        public List<int> CommonConnectedVfPins { get; set; } = new List<int>();
    }
}
