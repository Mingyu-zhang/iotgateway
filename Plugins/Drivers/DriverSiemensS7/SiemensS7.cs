﻿using PluginInterface;
using S7.Net;
using System;

namespace DriverSiemensS7
{
    [DriverSupported("1500")]
    [DriverSupported("1200")]
    [DriverSupported("400")]
    [DriverSupported("300")]
    [DriverSupported("200")]
    [DriverSupported("200Smart")]
    [DriverInfoAttribute("SiemensS7", "V1.0.0", "Copyright WHD© 2021-12-19")]
    public class SiemensS7 : IDriver
    {
        private Plc plc = null;
        #region 配置参数

        [ConfigParameter("设备Id")]
        public Guid DeviceId { get; set; }

        [ConfigParameter("PLC类型")]
        public CpuType CpuType { get; set; } = CpuType.S71200;

        [ConfigParameter("IP地址")]
        public string IpAddress { get; set; } = "127.0.0.1";

        [ConfigParameter("端口号")]
        public int Port { get; set; } = 102;

        [ConfigParameter("Rack")]
        public short Rack { get; set; } = 0;

        [ConfigParameter("Slot")]
        public short Slot { get; set; } = 0;

        [ConfigParameter("超时时间ms")]
        public uint Timeout { get; set; } = 3000;

        [ConfigParameter("最小通讯周期ms")]
        public uint MinPeriod { get; set; } = 3000;

        #endregion

        public SiemensS7(Guid deviceId)
        {
            DeviceId = deviceId;
            plc = new Plc(CpuType, IpAddress, Port, Rack, Slot);
        }


        public bool IsConnected
        {
            get
            {
                return plc != null && plc.IsConnected;
            }
        }

        public bool Connect()
        {
            try
            {
                plc.Open();
            }
            catch (Exception)
            {
                return false;
            }
            return IsConnected;
        }

        public bool Close()
        {
            try
            {
                plc?.Close();
                return !IsConnected;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                plc = null;
            }
            catch (Exception)
            {

            }
        }

        [Method("读西门子PLC", description: "读西门子PLC")]
        public DriverReturnValueModel Read(DriverAddressIoArgModel ioarg)
        {
            var ret = new DriverReturnValueModel { StatusType = VaribaleStatusTypeEnum.Good };

            if (plc != null && plc.IsConnected)
            {
                try
                {
                    ret.Value = plc.Read(ioarg.Address);
                }
                catch (Exception ex)
                {

                    ret.StatusType = VaribaleStatusTypeEnum.Bad;
                    ret.Message = $"读取失败,{ex.Message}";
                }
            }
            else
            {
                ret.StatusType = VaribaleStatusTypeEnum.Bad;
                ret.Message = "连接失败";
            }
            return ret;
        }

        //预留了大小端转换的 
        private ushort[] ChangeBuffersOrder(ushort[] buffers, DataTypeEnum dataType)
        {
            var newBuffers = new ushort[buffers.Length];
            if (dataType.ToString().Contains("32") || dataType.ToString().Contains("Float"))
            {
                var A = buffers[0] & 0xff00;//A
                var B = buffers[0] & 0x00ff;//B
                var C = buffers[1] & 0xff00;//C
                var D = buffers[1] & 0x00ff;//D
                if (dataType.ToString().Contains("_1"))
                {
                    newBuffers[0] = (ushort)(A + B);//AB
                    newBuffers[1] = (ushort)(C + D);//CD
                }
                else if (dataType.ToString().Contains("_2"))
                {
                    newBuffers[0] = (ushort)((A >> 8) + (B << 8));//BA
                    newBuffers[1] = (ushort)((C >> 8) + (D << 8));//DC
                }
                else if (dataType.ToString().Contains("_3"))
                {
                    newBuffers[0] = (ushort)((C >> 8) + (D << 8));//DC
                    newBuffers[1] = (ushort)((A >> 8) + (B << 8));//BA
                }
                else
                {
                    newBuffers[0] = (ushort)(C + D);//CD
                    newBuffers[1] = (ushort)(A + B);//AB
                }
            }
            else if (dataType.ToString().Contains("64") || dataType.ToString().Contains("Double"))
            {
                if (dataType.ToString().Contains("_1"))
                {

                }
                else
                {
                    newBuffers[0] = buffers[3];
                    newBuffers[1] = buffers[2];
                    newBuffers[2] = buffers[1];
                    newBuffers[3] = buffers[0];
                }
            }
            else
            {
                if (dataType.ToString().Contains("_1"))
                {
                    var h8 = buffers[0] & 0xf0;
                    var l8 = buffers[0] & 0x0f;
                    newBuffers[0] = (ushort)(h8 >> 8 + l8 << 8);
                }
                else
                    newBuffers[0] = buffers[0];
            }
            return newBuffers;
        }
    }
}