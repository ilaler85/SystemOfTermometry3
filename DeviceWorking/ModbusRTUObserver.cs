using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
//using Modbus.Device;

namespace SystemOfTermometry2.DeviceWorking
{
    /// <summary>
    /// 
    /// </summary>
    class ModbusRTUObserver : DeviceObserver
    {
        ModbusSerialMaster modbusMasterRTU;
        private int waitTime = 200;

        public override bool IsPortOpen
        {
            get
            {
                if (modbusMasterRTU != null)
                    if (port.IsOpen)
                        return true;
                return false;
            }
        }

        public ModbusRTUObserver()
        {
        }

        public override PortExceptionEnum ConnectComPort(string r_ComPort, int r_BaudRate, Parity r_parity, int r_DataBits, StopBits r_StopBits, int r_Timeout)
        {
            var result = base.ConnectComPort(r_ComPort, r_BaudRate, r_parity, r_DataBits, r_StopBits, r_Timeout);
            modbusMasterRTU = ModbusSerialMaster.CreateRtu(port);
            return result;
        }

        public override float[] GetData(byte devieceAddress, ushort leg, ushort countSens)
        {
            var ResponseBuf = new float[countSens];

            try
            {
                for (var i = 0; i < countSens; i++) //что бы были не нули
                    ResponseBuf[i] = -99.0f;

                var ResponseBufferHoldingRegs = new ushort[Math.Max(countSens, 2u)];

                //log.Info("ModbusRTU: Send nuber leg to device (nubmer leg = " + leg.ToString() + " )");
                modbusMasterRTU.WriteSingleRegister(devieceAddress, 0, leg);
                modbusMasterRTU.WriteSingleRegister(devieceAddress, 1, 1);

                System.Threading.Thread.Sleep(waitTime);

                while (ResponseBufferHoldingRegs[1] != 2)
                {
                    //log.Info("ModbusRTU: Waiting answer from device");
                    ResponseBufferHoldingRegs = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 0, 2);

                    if (ResponseBufferHoldingRegs[1] == 2) //ждать уже не надо
                        break;

                    System.Threading.Thread.Sleep(200);
                }

                //log.Info("ModbusRTU: Answer getted");


                if (countSens > 50)
                {
                    var ResponseBufferHoldingRegs1 = new ushort[25];
                    var ResponseBufferHoldingRegs2 = new ushort[25];
                    var ResponseBufferHoldingRegs3 = new ushort[countSens - 50];
                    ResponseBufferHoldingRegs1 = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 2, Convert.ToUInt16(25));
                    ResponseBufferHoldingRegs2 = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 27, Convert.ToUInt16(25));
                    ResponseBufferHoldingRegs3 = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 52, Convert.ToUInt16(countSens - 50));
                    ResponseBufferHoldingRegs = new ushort[countSens];
                    for (var h = 0; h < 25; h++)
                    {
                        ResponseBufferHoldingRegs[h] = ResponseBufferHoldingRegs1[h];
                    }
                    for (var h = 25; h < 50; h++)
                    {
                        ResponseBufferHoldingRegs[h] = ResponseBufferHoldingRegs2[h - 25];
                    }
                    for (var h = 50; h < countSens; h++)
                    {
                        ResponseBufferHoldingRegs[h] = ResponseBufferHoldingRegs2[h - 50];
                    }
                }
                else if (countSens > 25 && countSens < 51)
                {
                    var ResponseBufferHoldingRegs1 = new ushort[25];
                    var ResponseBufferHoldingRegs2 = new ushort[countSens - 25];
                    ResponseBufferHoldingRegs1 = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 2, Convert.ToUInt16(25));
                    ResponseBufferHoldingRegs2 = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 27, Convert.ToUInt16(countSens - 25));
                    ResponseBufferHoldingRegs = new ushort[countSens];
                    for (var h = 0; h < 25; h++)
                    {
                        ResponseBufferHoldingRegs[h] = ResponseBufferHoldingRegs1[h];
                    }
                    for (var h = 25; h < countSens; h++)
                    {
                        ResponseBufferHoldingRegs[h] = ResponseBufferHoldingRegs2[h - 25];
                    }
                }
                else
                {
                    ResponseBufferHoldingRegs = modbusMasterRTU.ReadHoldingRegisters(devieceAddress, 2, Convert.ToUInt16(countSens));
                }

                for (var sensorIdx = 0; sensorIdx < countSens; sensorIdx++)
                {
                    int tempINTMult100 = ResponseBufferHoldingRegs[sensorIdx];// > 59000 ? ResponseBufferHoldingRegs[sensorIdx] : ResponseBufferHoldingRegs[sensorIdx] / 10;

                    if (tempINTMult100 / 10 == 3276 || tempINTMult100 == 8500 || tempINTMult100 / 10 == 999) // Некорректные значения          
                        ResponseBuf[sensorIdx] = -99.9f;
                    else if (tempINTMult100 > 59000) // отрицательные значения.
                        ResponseBuf[sensorIdx] = (65535 - tempINTMult100) / -100.0f;
                    else
                        ResponseBuf[sensorIdx] = tempINTMult100 / 100.0f;
                }
                return ResponseBuf;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                modbusMasterRTU.WriteSingleRegister(devieceAddress, 1, 0);
            }

        }

    }
}
