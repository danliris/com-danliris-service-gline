using Com.DanLiris.Service.Gline.Lib.Enums;
using Com.DanLiris.Service.Gline.Lib.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Helpers
{
    public static class General
    {
        public const string JsonMediaType = "application/json";
        public static List<ShiftViewModel> Shift = new List<ShiftViewModel>
        {
            new ShiftViewModel 
            {
                name = "--.00 - 08.00",
                from = new TimeSpan(6, 31 , 0),
                to = new TimeSpan(7, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "08.00 - 09.00",
                from = new TimeSpan(8, 0, 0),
                to = new TimeSpan(8, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "09.00 - 10.00",
                from = new TimeSpan(9, 0, 0),
                to = new TimeSpan(9, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "10.00 - 10.30",
                from = new TimeSpan(10, 0, 0),
                to = new TimeSpan(10, 29, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "10.30 - 11.00",
                from = new TimeSpan(10, 30, 0),
                to = new TimeSpan(10, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "11.00 - 12.00",
                from = new TimeSpan(11, 0, 0),
                to = new TimeSpan(11, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "12.00 - 13.00",
                from = new TimeSpan(12, 0, 0),
                to = new TimeSpan(12, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "13.00 - 14.00",
                from = new TimeSpan(13, 0, 0),
                to = new TimeSpan(13, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "14.00 - 15.00",
                from = new TimeSpan(14, 0, 0),
                to = new TimeSpan(14, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "15.00 - 16.00",
                from = new TimeSpan(15, 0, 0),
                to = new TimeSpan(15, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "16.00 - 16.30",
                from = new TimeSpan(16, 0, 0),
                to = new TimeSpan(16, 29, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "16.30 - 17.00",
                from = new TimeSpan(16, 30, 0),
                to = new TimeSpan(17, 29, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "17.00 - 18.0",
                from = new TimeSpan(17, 0, 0),
                to = new TimeSpan(17, 59, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "18.00 - 18.30",
                from = new TimeSpan(18, 0, 0),
                to = new TimeSpan(18, 29, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "18.30 - 19.--",
                from = new TimeSpan(18, 30, 0),
                to = new TimeSpan(19, 30, 59),
                group = ShiftGroup.SIANG
            },
            new ShiftViewModel
            {
                name = "21.00 - 22.00",
                from = new TimeSpan(21, 0, 0),
                to = new TimeSpan(21, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "22.00 - 23.00",
                from = new TimeSpan(22, 0, 0),
                to = new TimeSpan(22, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "23.00 - 00.00",
                from = new TimeSpan(23, 0, 0),
                to = new TimeSpan(23, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "00.00 - 01.00",
                from = new TimeSpan(0, 0, 0),
                to = new TimeSpan(0, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "01.00 - 02.00",
                from = new TimeSpan(1, 0, 0),
                to = new TimeSpan(1, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "02.00 - 03.00",
                from = new TimeSpan(2, 0, 0),
                to = new TimeSpan(2, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "03.00 - 04.00",
                from = new TimeSpan(3, 0, 0),
                to = new TimeSpan(3, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "04.00 - 05.00",
                from = new TimeSpan(4, 0, 0),
                to = new TimeSpan(4, 59, 59),
                group = ShiftGroup.MALAM
            },
            new ShiftViewModel
            {
                name = "05.00 - 06.--",
                from = new TimeSpan(5, 0, 0),
                to = new TimeSpan(6, 30, 0),
                group = ShiftGroup.MALAM
            }
        };
    }
}
