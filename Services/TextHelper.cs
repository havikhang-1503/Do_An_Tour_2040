/* Folder: Utils
   File name: TextHelper.cs
   Mô tả: Hỗ trợ loại bỏ dấu tiếng Việt để so sánh chuỗi chính xác hơn.
*/
using System.Text;
using System.Text.RegularExpressions;

namespace Tour_2040.Utils
{
    public static class TextHelper
    {
        public static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // 1. Chuyển về chữ thường
            string temp = text.ToLowerInvariant();

            // 2. Chuẩn hóa Unicode (FormD) để tách dấu ra khỏi ký tự gốc
            string normalizedString = temp.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // 3. Chuyển đ -> d
            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            return result.Replace("đ", "d");
        }
    }
}   