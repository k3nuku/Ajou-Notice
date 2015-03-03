using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ajou_Notice.Code.Modules
{
    class TrivialModule
    {
        public static int voidnum(int year, int month) // 제공된 년월 기준으로 1일이 일월화수~토부터 시작 칸 수를 알아냄
        {
            int temp = 0;
            DateTime tempdate = new DateTime(year, month, 1);

            if (tempdate.DayOfWeek == DayOfWeek.Sunday)
                temp = 0;
            else if (tempdate.DayOfWeek == DayOfWeek.Monday)
                temp = 1;
            else if (tempdate.DayOfWeek == DayOfWeek.Tuesday)
                temp = 2;
            else if (tempdate.DayOfWeek == DayOfWeek.Wednesday)
                temp = 3;
            else if (tempdate.DayOfWeek == DayOfWeek.Thursday)
                temp = 4;
            else if (tempdate.DayOfWeek == DayOfWeek.Friday)
                temp = 5;
            else if (tempdate.DayOfWeek == DayOfWeek.Saturday)
                temp = 6;

            return temp;
        }
    }
}
