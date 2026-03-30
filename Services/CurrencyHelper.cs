// Folder: Utils
// File path: Utils/CurrencyHelper.cs

using System;

namespace Tour_2040.Utils
{
    public static class CurrencyHelper
    {
        private static readonly string[] ChuSo = { " không", " một", " hai", " ba", " bốn", " năm", " sáu", " bảy", " tám", " chín" };
        private static readonly string[] Tien = { "", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ" };

        // Hàm đọc số thành chữ
        public static string NumberToText(decimal total)
        {
            try
            {
                long number = (long)total;
                int lan = 0;
                int i = 0;
                double so = 0;
                string KetQua = "";
                string tmp = "";
                int[] position = new int[6];

                if (number < 0) return "Số tiền âm";
                if (number == 0) return "Không đồng";

                if (number > 0)
                {
                    so = number;
                }
                else
                {
                    so = -number;
                }
                if (number > 8999999999999999)
                {
                    // Số quá lớn
                    return "";
                }
                position[5] = (int)(so / 1000000000000000);
                so = so - position[5] * 1000000000000000;
                position[4] = (int)(so / 1000000000000);
                so = so - position[4] * 1000000000000;
                position[3] = (int)(so / 1000000000);
                so = so - position[3] * 1000000000;
                position[2] = (int)(so / 1000000);
                position[1] = (int)((so % 1000000) / 1000);
                position[0] = (int)(so % 1000);

                if (position[5] > 0)
                {
                    lan = 5;
                }
                else if (position[4] > 0)
                {
                    lan = 4;
                }
                else if (position[3] > 0)
                {
                    lan = 3;
                }
                else if (position[2] > 0)
                {
                    lan = 2;
                }
                else if (position[1] > 0)
                {
                    lan = 1;
                }
                else
                {
                    lan = 0;
                }

                for (i = lan; i >= 0; i--)
                {
                    tmp = ReadGroup3(position[i]);
                    KetQua += tmp;
                    if (position[i] > 0) KetQua += Tien[i];
                    if ((i > 0) && (tmp.Length > 0)) KetQua += ",";
                }
                if (KetQua.Substring(KetQua.Length - 1) == ",")
                {
                    KetQua = KetQua.Substring(0, KetQua.Length - 1);
                }
                KetQua = KetQua.Substring(1, 2).ToUpper() + KetQua.Substring(3);
                return KetQua;
            }
            catch
            {
                return "";
            }
        }

        private static string ReadGroup3(int basoso)
        {
            string KetQua = "";
            int tram;
            int chuc;
            int donvi;
            tram = basoso / 100;
            chuc = (basoso % 100) / 10;
            donvi = basoso % 10;
            if (tram == 0 && chuc == 0 && donvi == 0) return "";
            if (tram != 0)
            {
                KetQua += ChuSo[tram] + " trăm";
                if ((chuc == 0) && (donvi != 0)) KetQua += " linh";
            }
            if ((chuc != 0) && (chuc != 1))
            {
                KetQua += ChuSo[chuc] + " mươi";
                if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " linh";
            }
            if (chuc == 1) KetQua += " mười";
            switch (donvi)
            {
                case 1:
                    if ((chuc != 0) && (chuc != 1))
                    {
                        KetQua += " mốt";
                    }
                    else
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
                case 5:
                    if (chuc == 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    else
                    {
                        KetQua += " lăm";
                    }
                    break;
                default:
                    if (donvi != 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
            }
            return KetQua;
        }
    }
}