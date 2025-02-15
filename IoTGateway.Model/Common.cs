﻿using System.ComponentModel.DataAnnotations;

namespace IoTGateway.Model
{
    public enum DeviceTypeEnum
    {
        [Display(Name = "采集组")]
        Group = 0,

        [Display(Name = "采集点")]
        Device = 1
    }
    public enum AccessEnum
    {
        [Display(Name = "只读")]
        ReadOnly = 0,
        [Display(Name = "读写")]
        ReadAndWrite = 1
    }
}