using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RPT.Lab.PinCheck.Models.NI845X;

namespace RPT.Lab.PinCheck.Models
{
    public class SwitchMatrix : IDisposable
    {
        private byte[] _bytes;
        private NiHandleSafeHandle _deviceHandle = new NiHandleSafeHandle();
        private bool _connected = false;
        private uint _connectedDevicesCount = 0;
        private readonly byte _maxTransferSize = 64;
        private static readonly object _lock = new object();

        public uint ConnectedDevicesCount => _connectedDevicesCount;

        public SwitchMatrix(byte[]? bytes = null)
        {
            _bytes = bytes ?? new byte[] { 0 };
        }

        public static int ExecuteThreadSafe(Func<int> function)
        {
            lock (_lock)
            {
                return function();
            }
        }

        public static int ThreadSafeNi845xFindDevice(byte[] firstDevice, out NiHandleSafeHandle findDeviceHandle, out uint numberFound)
        {
            lock (_lock)
            {
                return ni845xFindDevice(firstDevice, out findDeviceHandle, out numberFound);
            }
        }

        public static int ThreadSafeNI845xOpen(byte[] bytes, out NiHandleSafeHandle deviceHandle)
        {
            lock (_lock)
            {
                return ni845xOpen(bytes, out deviceHandle);
            }
        }

        public static int ThreadSafeNI845xSpiConfigurationOpen(out NiHandleSafeHandle configurationHandle)
        {
            lock (_lock)
            {
                return ni845xSpiConfigurationOpen(out configurationHandle);
            }
        }

        public static int ThreadSafeNI845xDioSetPortLineDirectionMap(NiHandleSafeHandle deviceHandle, byte portNumber, byte map)
        {
            return ExecuteThreadSafe(() => ni845xDioSetPortLineDirectionMap(deviceHandle, portNumber, map));
        }

        public static int ThreadSafeNI845xDioWritePort(NiHandleSafeHandle deviceHandle, byte portNumber, byte value)
        {
            return ExecuteThreadSafe(() => ni845xDioWritePort(deviceHandle, portNumber, value));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetChipSelect(NiHandleSafeHandle deviceHandle, uint chipSelect)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetChipSelect(deviceHandle, chipSelect));
        }

        public static int ThreadSafeNI845xSpiWriteRead(NiHandleSafeHandle deviceHandle, NiHandleSafeHandle configurationHandle, uint writeSize, byte[] writeData, out uint readSize, byte[] readData)
        {
            lock (_lock)
            {
                return ni845xSpiWriteRead(deviceHandle, configurationHandle, writeSize, writeData, out readSize, readData);
            }
        }

        public static int ThreadSafeNI845xSpiConfigurationClose(NiHandleSafeHandle configurationHandle)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationClose(configurationHandle));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetNumBitsPerSample(NiHandleSafeHandle configurationHandle, ushort numBitsPerSample)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetNumBitsPerSample(configurationHandle, numBitsPerSample));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetPort(NiHandleSafeHandle configurationHandle, byte portNumber)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetPort(configurationHandle, portNumber));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetClockRate(NiHandleSafeHandle configurationHandle, ushort clockRate)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetClockRate(configurationHandle, clockRate));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetClockPolarity(NiHandleSafeHandle configurationHandle, int clockPolarity)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetClockPolarity(configurationHandle, clockPolarity));
        }

        public static int ThreadSafeNI845xSpiConfigurationSetClockPhase(NiHandleSafeHandle configurationHandle, int clockPhase)
        {
            return ExecuteThreadSafe(() => ni845xSpiConfigurationSetClockPhase(configurationHandle, clockPhase));
        }

        private int OpenConnection()
        {
            int errorCode = ThreadSafeNi845xFindDevice(_bytes, out _deviceHandle, out _connectedDevicesCount);
            if (errorCode == (int)Ni845xError.SUCCESS && _deviceHandle?.IsInvalid == false)
            {
                errorCode = ThreadSafeNI845xOpen(_bytes, out _deviceHandle);
                if (errorCode == (int)Ni845xError.SUCCESS)
                {
                    errorCode = SetupDevice();
                    _connected = (errorCode == (int)Ni845xError.SUCCESS);
                }
            }
            return errorCode;
        }

        private int SetupDevice()
        {
            int errorCode = ni845xSetIoVoltageLevel(_deviceHandle, (byte)VoltageLevel.ThreePointThreeVolts); // 3.3V
            if (errorCode == (int)Ni845xError.SUCCESS)
            {
                errorCode = ni845xDioSetDriverType(_deviceHandle, 0, (byte)DriveMode.PushPull); // Push-Pull
            }
            return errorCode;
        }

        public int SetSwitchMatrix(List<int> verifierBoardPinChannelsZap, List<int> verifierBoardPinChannelsGround)
        {
            var shiftSelections = GetShiftRegisterCommand(verifierBoardPinChannelsZap, verifierBoardPinChannelsGround);
            return WriteSpi(shiftSelections);
        }

        public int WriteSpi(List<ShiftSelection> shiftSelections)
        {
            int errorCode = 0;
            NiHandleSafeHandle configurationHandle = new NiHandleSafeHandle();
            if (!_connected)
                errorCode = OpenConnection();
            if (errorCode == (int)Ni845xError.SUCCESS)
                errorCode = ThreadSafeNI845xSpiConfigurationOpen(out configurationHandle);
            if (errorCode == (int)Ni845xError.SUCCESS && !configurationHandle.IsInvalid)
            {
                errorCode = ConfigureSpi(configurationHandle);

                if (errorCode == (int)Ni845xError.SUCCESS)
                {
                    foreach (var shiftSelection in shiftSelections)
                    {
                        errorCode = WriteSpiShiftSelection(configurationHandle, shiftSelection);
                        if (errorCode != (int)Ni845xError.SUCCESS) break;
                    }
                }
                errorCode = ThreadSafeNI845xSpiConfigurationClose(configurationHandle);
            }
            return errorCode;
        }

        private int WriteSpiShiftSelection(NiHandleSafeHandle configurationHandle, ShiftSelection shiftSelection)
        {
            int errorCode = ThreadSafeNI845xSpiConfigurationSetChipSelect(configurationHandle, Convert.ToUInt32(shiftSelection.IndexChipSelect));
            if (errorCode != (int)Ni845xError.SUCCESS) return errorCode;

            int maxLength = 8;
            byte[] values1 = new byte[maxLength];
            byte[] values2 = new byte[maxLength];
            FillShiftRegisterValues(values1, values2, shiftSelection);

            for (int i = 0; i < 2; i++)
            {
                errorCode = ThreadSafeNI845xSpiWriteRead(_deviceHandle, configurationHandle, Convert.ToUInt32(values1.Length), i == 0 ? values1 : values2, out _, new byte[maxLength]);
                if (errorCode != (int)Ni845xError.SUCCESS) break;
            }
            return errorCode;
        }

        private static void FillShiftRegisterValues(byte[] values1, byte[] values2, ShiftSelection shiftSelection)
        {
            for (int i = 0; i < values1.Length; i++)
            {
                values1[i] = shiftSelection.IndexShiftRegisterArray == i ? shiftSelection.ShiftRegisterValue : (byte)0;
            }
            for (int i = values1.Length; i < values1.Length + values2.Length; i++)
            {
                values2[i - values1.Length] = shiftSelection.IndexShiftRegisterArray == i ? shiftSelection.ShiftRegisterValue : (byte)0;
            }
        }

        private int ConfigureSpi(NiHandleSafeHandle configurationHandle)
        {
            int errorCode = ThreadSafeNI845xSpiConfigurationSetNumBitsPerSample(configurationHandle, _maxTransferSize);
            if (errorCode == (int)Ni845xError.SUCCESS)
            {
                errorCode = ThreadSafeNI845xSpiConfigurationSetPort(configurationHandle, 0);
                if (errorCode == (int)Ni845xError.SUCCESS)
                {
                    errorCode = ThreadSafeNI845xSpiConfigurationSetClockRate(configurationHandle, 2500); // 2.5MHz
                    if (errorCode == (int)Ni845xError.SUCCESS)
                    {
                        errorCode = ThreadSafeNI845xSpiConfigurationSetClockPolarity(configurationHandle, (int)SpiClockPolarity.IdleLow);
                        if (errorCode == (int)Ni845xError.SUCCESS)
                        {
                            errorCode = ThreadSafeNI845xSpiConfigurationSetClockPhase(configurationHandle, (int)SpiClockPhase.FirstEdge);
                        }
                    }
                }
            }
            return errorCode;
        }

        public int ClearSwitchMatrix()
        {
            byte portNumber = 0;
            int errorCode = 0;
            if (!_connected)
                errorCode = OpenConnection();
            if (errorCode == (int)Ni845xError.SUCCESS)
                errorCode = ThreadSafeNI845xDioSetPortLineDirectionMap(_deviceHandle, portNumber, 1);
            if (errorCode == (int)Ni845xError.SUCCESS)
            {
                for (int i = 0; i < 4; i++)
                {
                    byte value = Convert.ToByte(i % 2);
                    errorCode = ThreadSafeNI845xDioWritePort(_deviceHandle, portNumber, value);
                    if (errorCode != (int)Ni845xError.SUCCESS) break;
                }
            }
            if (errorCode == (int)Ni845xError.SUCCESS)
            {
                List<ShiftSelection> shiftSelections = Enumerable.Range(0, 8).Select(i => new ShiftSelection
                {
                    IndexChipSelect = (byte)i,
                    IndexShiftRegisterArray = 0,
                    ShiftRegisterValue = 0
                }).ToList();
                errorCode = WriteSpi(shiftSelections);
            }
            return errorCode;
        }

        public int DischargeAllPins(int numberOfPins, List<int> gndPins)
        {
            int errorCode = 0;
            const int maxDischargePins = 50;
            List<int> pins = Enumerable.Range(1, numberOfPins).ToList();

            var dischargeClusters = pins.SplitList(maxDischargePins).ToList();
            foreach (var item in dischargeClusters)
            {
                errorCode = SetSwitchMatrix(item, gndPins);
                if (errorCode != (int)Ni845xError.SUCCESS) break;
            }

            return errorCode;
        }

        private static List<ShiftSelection> MergeShiftRegisterValues(List<int> verifierBoardPinChannels, bool zapPin)
        {
            return verifierBoardPinChannels.Select(channel => new ShiftSelection
            {
                IndexChipSelect = (byte)((channel - 1) / 64),
                IndexShiftRegisterArray = (byte)(15 - ((channel - 1) % 64) / 4),
                ShiftRegisterValue = zapPin
                    ? ((byte)((channel - 1) % 4)).GetChannelNumberTesterZapPin()
                    : ((byte)((channel - 1) % 4)).GetChannelNumberTesterGround()
            }).ToList();
        }

        private static List<ShiftSelection> GetShiftRegisterCommand(List<int> verifierBoardPinChannelsZap, List<int> verifierBoardPinChannelsGround)
        {
            var zapPinSelections = MergeShiftRegisterValues(verifierBoardPinChannelsZap, true);
            var groundPinSelections = MergeShiftRegisterValues(verifierBoardPinChannelsGround, false);

            return zapPinSelections
                .Concat(groundPinSelections)
                .GroupBy(x => new { x.IndexChipSelect, x.IndexShiftRegisterArray })
                .Select(g => new ShiftSelection
                {
                    IndexChipSelect = g.Key.IndexChipSelect,
                    IndexShiftRegisterArray = g.Key.IndexShiftRegisterArray,
                    ShiftRegisterValue = (byte)g.Sum(x => x.ShiftRegisterValue)
                })
                .OrderBy(x => x.IndexChipSelect)
                .ThenBy(x => x.IndexShiftRegisterArray)
                .ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _connected)
            {
                _ = ni845xClose(_deviceHandle);
                _deviceHandle.Dispose();
                _connected = false;
            }
        }
    }

    public class ShiftSelection
    {
        public byte IndexChipSelect { get; set; }
        public byte IndexShiftRegisterArray { get; set; }
        public byte ShiftRegisterValue { get; set; }
    }

    internal static class SwitchMatrixExtension
    {
        internal static byte GetChannelNumberTesterZapPin(this byte registerToSelect) => registerToSelect switch
        {
            0 => 1,
            1 => 4,
            2 => 16,
            3 => 64,
            _ => 0,
        };

        internal static byte GetChannelNumberTesterGround(this byte registerToSelect) => registerToSelect switch
        {
            0 => 2,
            1 => 8,
            2 => 32,
            3 => 128,
            _ => 0,
        };
    }

    public static class ListExtensions
    {
        public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int size)
        {
            for (int i = 0; i < list.Count; i += size)
            {
                yield return list.GetRange(i, Math.Min(size, list.Count - i));
            }
        }
    }
}




