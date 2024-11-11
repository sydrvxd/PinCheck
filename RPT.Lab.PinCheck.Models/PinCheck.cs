using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPT.Lab.PinCheck.Models
{
    public static class PinCheck
    {
        private static readonly int _numberOfPins = 512;
        public static bool Cancel { get; set; }
        public static bool IsBusy { get; set; }

        public static List<BasicMeasurementResult> PerformPinCheck(BasicExecutionPlan? executionPlan)
        {
            if (IsBusy) return new List<BasicMeasurementResult>();

            IsBusy = true;

            var result = executionPlan?.IsBatchMeasurement == true
                ? PerformBatchPinCheck(executionPlan)
                : PerformSinglePinCheck(executionPlan);

            IsBusy = false;
            return result;
        }

        private static List<BasicMeasurementResult> PerformSinglePinCheck(BasicExecutionPlan? executionPlan)
        {
            Cancel = false;

            List<BasicMeasurementResult> measurementResults = new List<BasicMeasurementResult>();

            if (executionPlan != null)
            {
                measurementResults = MeasurePins(executionPlan, out _);
            }

            IsBusy = false;
            return measurementResults;
        }

        private static List<BasicMeasurementResult> PerformBatchPinCheck(BasicExecutionPlan? executionPlan, int batchSize = 50)
        {
            Cancel = false;
            List<BasicMeasurementResult> measurementResults = new List<BasicMeasurementResult>();

            if (executionPlan != null)
            {
                measurementResults = BatchMeasurePins(executionPlan, batchSize, out _);
            }

            IsBusy = false;
            return measurementResults;
        }

        private static List<BasicMeasurementResult> MeasurePins(BasicExecutionPlan executionPlan, out int errorCode)
        {
            errorCode = 0;
            var results = new List<BasicMeasurementResult>();

            using (var switchMatrix = new SwitchMatrix())
            {
                using (var dmm = new NiDmm4065())
                {
                    dmm.Reset();
                    dmm.Configure(executionPlan.UseDmmAutoRange);

                    foreach (var pinCheckCombination in executionPlan.BasicPinCheckCombinations ?? new List<BasicPinCheckCombination>())
                    {
                        if (Cancel) break;

                        foreach (var testPin in pinCheckCombination.TestVfPins)
                        {
                            if (executionPlan.DischargeAllPinsAfterOperation)
                            {
                                errorCode = switchMatrix.DischargeAllPins(_numberOfPins, pinCheckCombination.GndVfPins);
                                if (errorCode < 0) break;
                            }
                            if (errorCode >= 0)
                            {
                                errorCode = switchMatrix.ClearSwitchMatrix();
                                if (errorCode < 0) break;
                            }
                            if (errorCode >= 0)
                            {
                                errorCode = switchMatrix.SetSwitchMatrix(new List<int> { testPin }, pinCheckCombination.GndVfPins);
                                if (errorCode < 0) break;
                            }

                            var resistance = dmm.PerformResistanceMeasurement() ?? 999;
                            results.Add(new BasicMeasurementResult
                            {
                                Resistance = resistance,
                                CommonConnected = pinCheckCombination.CommonConnectedVfPins.Contains(testPin),
                                VfChTestPin = testPin,
                                VfChGndPin = pinCheckCombination.GndVfPins.FirstOrDefault()
                            });
                        }

                        if (errorCode < 0) break;
                    }

                    if (errorCode >= 0)
                        errorCode = switchMatrix.ClearSwitchMatrix();
                }
            }

            return results;
        }

        private static List<BasicMeasurementResult> BatchMeasurePins(BasicExecutionPlan executionPlan, int batchSize, out int errorCode)
        {
            errorCode = 0;
            var results = new List<BasicMeasurementResult>();

            using (var switchMatrix = new SwitchMatrix())
            {
                using (var dmm = new NiDmm4065())
                {
                    dmm.Reset();
                    dmm.Configure(executionPlan.UseDmmAutoRange);

                    foreach (var pinCheckCombination in executionPlan.BasicPinCheckCombinations ?? new List<BasicPinCheckCombination>())
                    {
                        if (Cancel) break;

                        List<int> testPins = pinCheckCombination.TestVfPins.Select(x => x).ToList();

                        testPins.RemoveAll(p => pinCheckCombination.CommonConnectedVfPins.Contains(p));
                        var splitTestPins = testPins.SplitList(batchSize);

                        foreach (var testPinList in splitTestPins)
                        {
                            if (executionPlan.DischargeAllPinsAfterOperation)
                            {
                                errorCode = switchMatrix.DischargeAllPins(_numberOfPins, pinCheckCombination.GndVfPins);
                                if (errorCode < 0) break;
                            }
                            if (errorCode >= 0)
                            {
                                errorCode = switchMatrix.ClearSwitchMatrix();
                                if (errorCode < 0) break;
                            }
                            if (errorCode >= 0)
                            {
                                errorCode = switchMatrix.SetSwitchMatrix(testPinList, pinCheckCombination.GndVfPins);
                                if (errorCode < 0) break;
                            }

                            var resistance = dmm.PerformResistanceMeasurement() ?? 999;
                            bool fail = false;

                            foreach (var testPin in testPinList)
                            {
                                var measurementResult = new BasicMeasurementResult
                                {
                                    Resistance = resistance,
                                    CommonConnected = pinCheckCombination.CommonConnectedVfPins.Contains(testPin),
                                    VfChTestPin = testPin,
                                    VfChGndPin = pinCheckCombination.GndVfPins.FirstOrDefault()
                                };

                                if (measurementResult.Result)
                                    results.Add(measurementResult);
                                else
                                {
                                    fail = true;
                                    break;
                                }
                            }

                            if (fail)
                            {
                                results.RemoveAll(m => testPinList.Contains(m.VfChTestPin) &&
                                                       pinCheckCombination.GndVfPins.Contains(m.VfChGndPin));

                                foreach (var testPin in testPinList)
                                {
                                    if (executionPlan.DischargeAllPinsAfterOperation)
                                    {
                                        errorCode = switchMatrix.DischargeAllPins(_numberOfPins, pinCheckCombination.GndVfPins);
                                        if (errorCode < 0) break;
                                    }
                                    if (errorCode >= 0)
                                    {
                                        errorCode = switchMatrix.ClearSwitchMatrix();
                                        if (errorCode < 0) break;
                                    }
                                    if (errorCode >= 0)
                                    {
                                        errorCode = switchMatrix.SetSwitchMatrix(new List<int> { testPin }, pinCheckCombination.GndVfPins);
                                        if (errorCode < 0) break;
                                    }

                                    resistance = dmm.PerformResistanceMeasurement() ?? 999;

                                    var measurementResult = new BasicMeasurementResult
                                    {
                                        Resistance = resistance,
                                        CommonConnected = pinCheckCombination.CommonConnectedVfPins.Contains(testPin),
                                        VfChTestPin = testPin,
                                        VfChGndPin = pinCheckCombination.GndVfPins.FirstOrDefault()
                                    };

                                    results.Add(measurementResult);
                                }
                            }

                            if (errorCode < 0) break;
                        }
                    }

                    if (errorCode >= 0)
                    {
                        errorCode = switchMatrix.ClearSwitchMatrix();
                    }
                }
            }

            return results;
        }

        public static PinCheckSelfTestResult? PerformSelfTest()
        {
            Cancel = false;

            PinCheckSelfTestResult finalResult = new PinCheckSelfTestResult();

            using var dmm = new NiDmm4065();
            var dmmSelfTestResult = dmm.PerformSelfTestDetail();


            string dmmMessage = $"DMM Self Test:\n" +
                 $"\tSelf test result: {dmmSelfTestResult.Message}\n" +
                 $"\tCode: {dmmSelfTestResult.Code}";

            if (dmmSelfTestResult.Code == 0)
            {
                int errorCode = 0;
                using (var switchMatrix = new SwitchMatrix())
                {
                    errorCode = switchMatrix.DischargeAllPins(_numberOfPins, new List<int> { 1 });
                    dmm.Reset();
                    dmm.Configure(false);
                    List<bool> resultList = new List<bool>();
                    for (int i = 0; i < _numberOfPins; i++)
                    {
                        if (!Cancel)
                        {
                            List<int> pin = new List<int> { i + 1 };
                            if (errorCode >= 0)
                                errorCode = switchMatrix.ClearSwitchMatrix();
                            if (errorCode >= 0)
                                errorCode = switchMatrix.SetSwitchMatrix(pin, pin);

                            var resistance = dmm.PerformResistanceMeasurement() ?? 999;


                            if (resistance < 1)
                                resultList.Add(true);
                            else
                            {
                                resultList.Add(false);
                                finalResult.FailPins.Add(pin[0], resistance);
                            }
                        }
                    }
                    finalResult.Pass = resultList.All(x => x);
                }
            }
            else
                finalResult.Pass = false;

            finalResult.Message = dmmMessage;
            if (!Cancel)
                return finalResult;
            else
                return null;
        }
    }
}
