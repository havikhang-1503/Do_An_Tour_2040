// Folder: Utils
// File path: Utils/MaSoHelper.cs

using System;

namespace Tour_2040.Utils
{
    /// <summary>
    /// Helper hỗ trợ sinh mã định danh tự động cho toàn hệ thống
    /// </summary>
    public static class MaSoHelper
    {
        // Khởi tạo Random tĩnh để tránh trùng lặp khi gọi hàm quá nhanh
        private static readonly Random _random = new Random();

        /// <summary>
        /// Sinh mã dạng PREFIX + 6 chữ số ngẫu nhiên
        /// Ví dụ: TaoMa("MT") -> MT009988
        /// </summary>
        /// <param name="prefix">Tiền tố định danh (MT, MHD, MGD...)</param>
        /// <returns>Chuỗi mã định danh duy nhất</returns>
        public static string TaoMa(string prefix)
        {
            // Sinh một số ngẫu nhiên từ 0 đến 999,999
            int number = _random.Next(0, 1000000);

            // "D6" đảm bảo chuỗi luôn có 6 chữ số (ví dụ: số 123 thành "000123")
            return $"{prefix}{number:D6}";
        }
    }
}