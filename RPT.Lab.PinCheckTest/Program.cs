using RPT.Lab.PinCheck.Models;


SelfTestExample();
PinCheckExample();



partial class Program
{
    private static void PinCheckExample()
    {
        BasicExecutionPlan plan = new BasicExecutionPlan()
        {
            BasicPinCheckCombinations = new List<BasicPinCheckCombination>
                {
                    new BasicPinCheckCombination
                    {
                        GndVfPins = new List<int> { 1 },
                        TestVfPins = new List<int> { 2, 3, 4 }
                    }
                },
            IsBatchMeasurement = false,
            UseDmmAutoRange = false
        };

        var result = PinCheck.PerformPinCheck(plan);
        foreach (var item in result)
        {
            Console.WriteLine($"{item.Result}: {item.VfChTestPin} vs. {item.VfChGndPin} --> {item.Resistance}Ω.");
        }

    }

    private static void SelfTestExample()
    {
        var selfTestResult = PinCheck.PerformSelfTest();
        Console.WriteLine(selfTestResult?.Message ?? "NULL");
    }
}