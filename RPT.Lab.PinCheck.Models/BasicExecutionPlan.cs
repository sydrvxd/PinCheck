using System;
using System.Collections.Generic;
using System.Text;

namespace RPT.Lab.PinCheck.Models
{
    public class BasicExecutionPlan
    {
        public int EPadVfPin { get; set; }
        public bool UseDmmAutoRange { get; set; } = false;
        public bool DischargeAllPinsAfterOperation { get; set; } = true;
        public bool IsBatchMeasurement { get; set; } = true;
        public List<BasicPinCheckCombination>? BasicPinCheckCombinations { get; set; }
    }
}
