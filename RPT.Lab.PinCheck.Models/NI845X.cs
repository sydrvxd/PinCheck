using NationalInstruments.DataInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RPT.Lab.PinCheck.Models
{
    public class NI845X
    {
        //==============================================================================
        //
        // Local Errors - Reference ni845x.h
        //
        //==============================================================================
        private const string DllName = "Ni845x.dll";

        public enum Ni845xError
        {
            // Successful Operation
            SUCCESS = 0,

            // General Errors
            InsufficientMemory = -301700,
            InvalidResourceName = -301701,
            InvalidClockRate = -301702,
            TooManyScriptReads = -301703,
            InvalidScriptReadIndex = -301704,
            InvalidScriptReference = -301705,
            InvalidDeviceId = -301706,
            ConnectionLost = -301707,
            Timeout = -301708,
            InternalError = -301709,
            InvalidConfigurationReference = -301710,
            TooManyConfigurations = -301711,
            InvalidActiveProperty = -301712,
            InvalidParameter = -301713,
            ResourceBusy = -301714,
            InvalidMasterCode = -301715,
            MasterCodeAck = -301716,
            OverCurrentError = -301718,
            SpiStreamingModeNotSupported = -301780,
            I2cSlaveModeNotSupported = -301781,
            InvalidI2cSlaveEventResponse = -301782,
            I2cSlaveEventPending = -301783,

            UnknownError = -301719,

            // General Errors from the device
            BadOpcode = -301720,
            UnknownStatus = -301721,
            ProtocolViolation = -301722,
            InvalidScript = -301723,
            InvalidFirmware = -301724,
            IncompatibleFirmware = -301725,

            // SPI Errors from the device
            MasterWriteCollision = -301730,
            InvalidSpiPortNumber = -301732,
            InvalidCsPortNumber = -301733,
            InvalidChipSelect = -301734,
            InvalidBitsPerSample = -301735,

            // I2C Errors from the device
            MasterBusFreeTimeout = -301740,
            MasterCodeArbLost = -301741,
            MasterAddressNotAcknowledged = -301742,
            MasterDataNotAcknowledged = -301743,
            MasterAddressArbitrationLost = -301744,
            MasterDataArbitrationLost = -301745,
            InvalidI2CPortNumber = -301746,

            // DIO Errors from the device
            InvalidDioPortNumber = -301750,
            InvalidDioLineNumber = -301751,

            // SPI Streaming Errors from the device
            InStreamingMode = -301717,
            NotInStreamingMode = -301760,

            // I2C Slave Errors from the device
            InSlaveMode = -301770,
            NotInSlaveMode = -301771,
            InvalidDataBufferSize = -301772,
            InvalidSlaveAddress = -301773,
            I2cSpecViolation = -301774
        }

        // Enums for SPI Function Arguments
        public enum SpiClockPolarity
        {
            IdleLow = 0, // Idle Low
            IdleHigh = 1 // Idle High
        }

        public enum SpiClockPhase
        {
            FirstEdge = 0, // First Edge
            SecondEdge = 1 // Second Edge
        }

        // Enums for I2C Function Arguments
        public enum I2cAddressingMode
        {
            Address7Bit = 0, // 7-Bit Addressing
            Address10Bit = 1 // 10-Bit Addressing
        }

        public enum I2cNakMode
        {
            AckLastByte = 0, // ACK Last Byte
            NakLastByte = 1 // NAK Last Byte
        }

        public enum I2cHsMode
        {
            Disable = 0, // Disable HS Mode
            Enable = 1 // Enable HS Mode
        }

        public enum I2cPullupMode
        {
            Disable = 0, // Disable Onboard Pullups
            Enable = 1 // Enable Onboard Pullups
        }

        // Enums for DIO Function Arguments
        public enum DioDirection
        {
            Input = 0, // DIO Direction Input
            Output = 1 // DIO Direction Output
        }

        public enum DioLogicLevel
        {
            Low = 0, // DIO Level Low
            High = 1 // DIO Level High
        }

        // Enums for Generic Function Arguments
        public enum DriveMode : byte
        {
            OpenDrain = 0, // Open Drain
            PushPull = 1 // Push Pull
        }

        public enum VoltageLevel : byte
        {
            ThreePointThreeVolts = 33, // 3.3V
            TwoPointFiveVolts = 25, // 2.5V
            OnePointEightVolts = 18, // 1.8V
            OnePointFiveVolts = 15, // 1.5V
            OnePointTwoVolts = 12 // 1.2V
        }

        // Define type aliases
        [StructLayout(LayoutKind.Sequential)]
        public struct Array1DU8_t
        {
            public uint dimSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] elt;
        }

        // Define function prototypes
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xFindDevice(
            byte[] firstDevice,
            out NiHandleSafeHandle findDeviceHandle,
            out uint numberFound);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xFindDeviceNext(
            NiHandleSafeHandle findDeviceHandle,
            byte[] nextDevice);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xCloseFindDeviceHandle(
            NiHandleSafeHandle findDeviceHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xOpen(
            byte[] resourceName,
            out NiHandleSafeHandle deviceHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xClose(
            NiHandleSafeHandle deviceHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xDeviceLock(
            NiHandleSafeHandle deviceHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xDeviceUnlock(
            NiHandleSafeHandle deviceHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ni845xStatusToString(
            int statusCode,
            uint maxSize,
            byte[] statusString);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSetIoVoltageLevel(
            NiHandleSafeHandle deviceHandle,
            byte voltageLevel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSetTimeout(
            NiHandleSafeHandle deviceHandle,
            uint timeout);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiWriteRead(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle,
            uint writeSize,
            byte[] writeData,
            out uint readSize,
            byte[] readData);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationOpen(
            out NiHandleSafeHandle configurationHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationClose(
            NiHandleSafeHandle configurationHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetChipSelect(
            NiHandleSafeHandle configurationHandle,
            uint chipSelect);

        /// <summary>
        /// Value to be provided in kHz (means 50000 = 50MHz).
        /// Supported clock rates: 25 kHz, 32 kHz, 40 kHz, 50 kHz, 80 kHz, 100 kHz, 125 kHz, 160 kHz, 200 kHz, 250 kHz, 400 kHz, 500 kHz, 625 kHz, 800 kHz, 1 MHz, 1.25 MHz, 2.5 MHz, 3.125 MHz, 4 MHz, 5 MHz, 6.25 MHz, 10 MHz, 12.5 MHz, 20 MHz, 25 MHz, 33.33 MHz, 50 MHz
        /// </summary>
        /// <param name="configurationHandle"></param>
        /// <param name="clockRate"></param>
        /// <returns></returns>

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetClockRate(
            NiHandleSafeHandle configurationHandle,
            ushort clockRate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetClockPolarity(
            NiHandleSafeHandle configurationHandle,
            int clockPolarity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetClockPhase(
            NiHandleSafeHandle configurationHandle,
            int clockPhase);

        /// <summary>
        /// 4 to 64 bits, software selectable
        /// </summary>
        /// <param name="ConfigurationHandle"></param>
        /// <param name="NumBits"></param>
        /// <returns></returns>

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetNumBitsPerSample(
            NiHandleSafeHandle configurationHandle,
            ushort numBitsPerSample);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiConfigurationSetPort(
            NiHandleSafeHandle configurationHandle,
            byte portNumber);

        //kNI845XExport int32 NI845X_FUNC ni845xSpiStreamConfigurationSetNumBits(
        //  NiHandleSafeHandle ConfigurationHandle,
        //  uInt8 NumBits
        //  );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiStreamConfigurationSetNumBits(NiHandleSafeHandle ConfigurationHandle,
                                                           byte NumBits);

        //kNI845XExport int32 NI845X_FUNC ni845xDioSetDriverType(
        //   NiHandleSafeHandle DeviceHandle,
        //   uInt8    DioPort,
        //   uInt8    Type
        //   );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xDioSetDriverType(NiHandleSafeHandle DeviceHandle,
                                                            byte DioPort,
                                                            byte Type);

        //kNI845XExport int32 NI845X_FUNC ni845xDioSetPortLineDirectionMap (
        //   NiHandleSafeHandle DeviceHandle,
        //   uInt8    PortNumber,
        //   uInt8    Map
        //   );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xDioSetPortLineDirectionMap(NiHandleSafeHandle DeviceHandle,
                                                                        byte PortNumber,
                                                                        byte Map);

        //kNI845XExport int32 NI845X_FUNC ni845xDioWritePort (
        //   NiHandleSafeHandle DeviceHandle,
        //   uInt8    PortNumber,
        //   uInt8    WriteData
        //   );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xDioWritePort(NiHandleSafeHandle DeviceHandle,
                                                            byte PortNumber,
                                                            byte WriteData);

        // Define other function prototypes as needed...

        // SPI Streaming functions
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiStreamStart(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiStreamRead(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle,
            uint numBytesToRead,
            byte[] readData,
            out uint readSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xSpiStreamStop(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle);

        // Define other SPI streaming functions...

        // I2C functions
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xI2cWrite(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle,
            uint writeSize,
            byte[] writeData);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xI2cRead(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle,
            uint numBytesToRead,
            out uint readSize,
            byte[] readData);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xI2cWriteRead(
            NiHandleSafeHandle deviceHandle,
            NiHandleSafeHandle configurationHandle,
            uint writeSize,
            byte[] writeData,
            uint numBytesToRead,
            out uint readSize,
            byte[] readData);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ni845xI2cSetPullupEnable(
            NiHandleSafeHandle deviceHandle,
            byte enable);

        // Define other I2C functions...
    }
}
