using NationalInstruments.ModularInstruments.NIDmm;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPT.Lab.PinCheck.Models
{
    public class NiDmm4065 : IDisposable
    {
        private NIDmm? _sampleDmmSession;
        private bool _configurationDone = false;

        public List<string>? DeviceNames { get; }
        public string? SelectedDevice { get; set; }

        public NiDmm4065()
        {
            ModularInstrumentsSystem modularInstrumentsSystem = new ModularInstrumentsSystem("NI-DMM");
            DeviceNames = modularInstrumentsSystem.DeviceCollection.Cast<DeviceInfo>().Select(d => d.Name).ToList();
            if (DeviceNames.Count > 0)
                SelectedDevice = DeviceNames[0];
        }

        private static void ConfigureDmm4Wire(NIDmm sampleDmmSession, bool autoRange)
        {
            double range = 0.02;
            sampleDmmSession.ConfigureMeasurementDigits(DmmMeasurementFunction.FourWireResistance, (autoRange ? -1 : range), 6.5);
            sampleDmmSession.Advanced.PowerlineFrequency = 50;
            sampleDmmSession.Advanced.AutoZero = DmmAuto.On;
        }

        private string OpenSession()
        {
            if (!string.IsNullOrEmpty(SelectedDevice))
            {
                try
                {
                    // Create a Dmm Session
                    _sampleDmmSession = new NIDmm(SelectedDevice, true, true);
                    return "Success.";
                }
                catch (Exception ex)
                {
                    return $"Failure at NiDmm OpenSession(): {ex.Message}";
                }
            }
            else
                return "No NiDmm 4065 device selected.";
        }

        public string PerformSelfTest()
        {
            if (_sampleDmmSession == null)
                OpenSession();
            if (_sampleDmmSession != null)
            {
                try
                {
                    var result = _sampleDmmSession.DriverUtility.SelfTest();
                    return $"Self test result: {result.Message}\nCode: {result.Code}";
                }
                catch (Exception ex)
                {
                    return $"Failure at NiDmm PerformSelfTest(): {ex.Message}";
                }
            }
            else
                return "No device selected.";
        }

        public DmmSelfTestResult PerformSelfTestDetail()
        {
            if (_sampleDmmSession == null)
                OpenSession();
            if (_sampleDmmSession != null)
            {
                try
                {
                    return _sampleDmmSession.DriverUtility.SelfTest();
                }
                catch (Exception ex)
                {
                    return new DmmSelfTestResult(999, $"Failure at NiDmm PerformSelfTest(): {ex.Message}");
                }
            }
            else
                return new DmmSelfTestResult(999, "No device selected.");
        }

        public double? PerformResistanceMeasurement(bool autoRange, bool reset, bool configure)
        {
            double? reading = null;
            if (_sampleDmmSession == null)
                OpenSession();
            if (_sampleDmmSession != null)
            {
                try
                {
                    if (!_configurationDone)
                    {
                        // Create a Dmm Session
                        if (reset)
                            _sampleDmmSession.DriverUtility.Reset();
                        if (configure)
                            ConfigureDmm4Wire(_sampleDmmSession, autoRange);
                        _configurationDone = true;
                    }

                    // Obtain the reading
                    reading = _sampleDmmSession.Measurement.Read();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failure at NiDmm PerformResistanceMeasurement(): {ex.Message}", ex);
                }
            }

            return reading;
        }

        public void Reset()
        {
            if (_sampleDmmSession == null)
                OpenSession();
            _sampleDmmSession?.DriverUtility.Reset();
        }

        public void Configure(bool autoRange)
        {
            if (_sampleDmmSession != null)
                ConfigureDmm4Wire(_sampleDmmSession, autoRange);
        }

        public double? PerformResistanceMeasurement()
        {
            //try
            //{
                return _sampleDmmSession?.Measurement.Read();
            //}
            //catch (Exception ex)
            //{
            //    State.Message = $"Exception Message: {ex.Message}\nStack Trace: {ex.StackTrace}";
            //    throw new Exception($"Failure at NiDmm PerformResistanceMeasurement(): {ex.Message}", ex);
            //}
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sampleDmmSession != null)
                {
                    _sampleDmmSession.Close();
                    _configurationDone = false;
                    _sampleDmmSession = null;
                }
            }
        }
    }
}
