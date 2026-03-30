using OpenAI.Chat;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using System.Text.Json;

namespace Tour_2040.Services
{
    public interface ISupportAiService
    {
        Task<string> GetSupportAnswerAsync(string userMessage);
        Task<List<string>> SuggestLocationsAsync(string provinceOrArea);
        Task<string> SuggestTourInfoAsync(string tourName);
        Task<string> SuggestLocationDetailAsync(string locationName);
    }

    public class SupportAiService : ISupportAiService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public SupportAiService(IConfiguration config, ApplicationDbContext db)
        {
            _config = config;
            _db = db;
        }

        // Danh sách 63 tỉnh thành Việt Nam (Chuẩn hóa 2026)
        // Danh sách 34 Tỉnh/Thành phố mới (Sắp xếp lại 2026)
        private static readonly string _vietnamProvinces = string.Join(", ", new[]
        {
            // 6 Thành phố trực thuộc trung ương
            "Hà Nội", "Hồ Chí Minh", "Hải Phòng", "Đà Nẵng", "Cần Thơ", "Huế",
            
            // 28 Tỉnh
            "Quảng Ninh", "Cao Bằng", "Lạng Sơn", "Lai Châu", "Điện Biên", "Sơn La", 
            "Thanh Hóa", "Nghệ An", "Hà Tĩnh", 
            "Tuyên Quang", "Lào Cai", "Thái Nguyên", "Phú Thọ", "Bắc Ninh", "Hưng Yên", "Ninh Bình",
            "Quảng Trị", "Quảng Ngãi", "Gia Lai", "Khánh Hòa", "Lâm Đồng", "Đắk Lắk",
            "Đồng Nai", "Đồng Tháp", "Vĩnh Long", "Long An", "An Giang", "Cà Mau"
        });

        public async Task<string> GetSupportAnswerAsync(string userMessage)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "{\"text\": \"Lỗi: Chưa cấu hình API Key.\", \"buttons\": []}";

            try
            {
                // 1. LẤY DỮ LIỆU TOUR TỪ DATABASE
                var tours = await _db.Tours

.Select(t => new { t.MaTour, t.TenTour, t.GiaTour }).Take(5)
                    .ToListAsync();

                string tourInfo = string.Join("\n", tours.Select(t => $"- {t.TenTour} (ID: {t.MaTour}): Giá {t.GiaTour:N0} VNĐ"));

                // 2. CẤU HÌNH AI GIAO TIẾP QUA JSON
                ChatClient client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage($@"
                        Bạn là trợ lý ảo của Royal Ascend Travel. 
                        DỮ LIỆU TOUR THỰC TẾ:
                        {tourInfo}

                        CÁC ĐƯỜNG DẪN HỆ THỐNG (Dùng cho buttons):
                        - Trang chủ: /
                        - Khám phá tất cả Tour: /Tours
                        - Săn Voucher: /Vouchers/Available
                        - Đăng ký: /Identity/Account/Register
                        - Đăng nhập: /Identity/Account/Login

                        QUY TẮC TRẢ LỜI:
                        Bạn PHẢI trả lời duy nhất dưới định dạng JSON như sau:
                        {{
                            ""text"": ""Nội dung tư vấn thân thiện bằng tiếng Việt"",
                            ""buttons"": [
                                {{ ""label"": ""Tên nút"", ""action"": ""URL_hoac_ID"" }}
                            ]
                        }}

                        VÍ DỤ:
                        Nếu khách hỏi về tour Phú Quốc (ID: 1), hãy trả về button có action là ""/Tours/Details/1"".
                        Nếu khách chưa rõ, hãy gợi ý nút ""Khám phá tất cả Tour"".
                    "),
                    new UserChatMessage(userMessage)
                };

                ChatCompletion completion = await client.CompleteChatAsync(messages);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> [AI_ERROR]: {ex.Message}");
                return "{\"text\": \"Hệ thống đang bận, bạn thử lại sau nhé!\", \"buttons\": []}";
            }
        }

        public async Task<List<string>> SuggestLocationsAsync(string provinceOrArea)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return new List<string> { "Chưa cấu hình API Key" };

            try
            {
                ChatClient client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("Bạn là chuyên gia du lịch am hiểu tường tận về Việt Nam và các xu hướng du lịch mới nhất. Hãy liệt kê 5-10 địa điểm tham quan ĐA DẠNG, bao gồm cả những điểm nổi tiếng và NHỮNG VIÊN NGỌC ẨN (Hidden Gems) ít người biết. Cố gắng thay đổi danh sách mỗi lần để không bị trùng lặp. CHỈ TRẢ VỀ MẢNG JSON STRING, không có text nào khác. Ví dụ: [\"Hồ Gươm\", \"Hẻm Bia Lost in HongKong\", \"Cà phê đường tàu\"]"),
                    new UserChatMessage($"Gợi ý địa điểm du lịch ĐỘC ĐÁO ở: {provinceOrArea}")
                };

                // Tăng độ sáng tạo (Temperature) để kết quả đa dạng như đang tìm trên web
                ChatCompletion completion = await client.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = 0.9f });
                var text = completion.Content[0].Text;
                
                text = text.Replace("```json", "").Replace("```", "").Trim();
                
                try 
                {
                    return JsonSerializer.Deserialize<List<string>>(text) ?? new List<string>();
                }
                catch
                {
                   return new List<string>(); 
                }
            }
            catch
            {
                return new List<string>();
                return new List<string>();
            }
        }

        public async Task<string> SuggestTourInfoAsync(string tourName)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "{}";

            try
            {
                ChatClient client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage($@"Bạn là trợ lý tạo tour du lịch chuyên nghiệp và sáng tạo. 
                    Hãy tưởng tượng bạn đang tra cứu dữ liệu du lịch mới nhất trên Web để thiết kế tour.
                    Khi nhận được Tên Tour, hãy sinh ra dữ liệu mẫu thật hấp dẫn, tránh văn mẫu nhàm chán.
                    Cấu trúc JSON (KHÔNG MARKDOWN):
                    {{
                        ""moTa"": ""Bài viết marketing thu hút, trending, dài khoảng 200 chữ..."",
                        ""giaTour"": 1500000,
                        ""soNgay"": 3,
                        ""schedule"": [
                            {{ 
                                ""day"": 1, 
                                ""province"": ""Tên Tỉnh (VD: Kiên Giang, Lâm Đồng, Hà Nội...)"", 
                                ""location"": ""Tên điểm đến chính"",
                                ""activities"": [
                                    {{ ""time"": ""08:00"", ""endTime"": ""11:00"", ""title"": ""Tham quan A"", ""note"": ""Ghi chú"" }}
                                ]
                            }}
                        ]
                    }}
                    QUAN TRỌNG: 
                    1. Trường 'province' BẮT BUỘC phải thuộc danh sách sau: [{_vietnamProvinces}]. 
                    2. Hãy chọn các địa điểm 'Hot trend' nếu phù hợp.
                    3. Trả về đúng JSON, không Markdown."),
                    new UserChatMessage($"Hãy tạo dữ liệu SÁNG TẠO cho tour: {tourName}")
                };

                ChatCompletion completion = await client.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = 0.8f });
                var text = completion.Content[0].Text;
                text = text.Replace("```json", "").Replace("```", "").Trim();
                
                return text;
            }
            catch
            {
                return "{}";
            }
        }

        public async Task<string> SuggestLocationDetailAsync(string locationName)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "{}";

            try
            {
                ChatClient client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage($@"Bạn là chuyên gia địa lý du lịch Việt Nam. 
                    Hãy mô phỏng việc tìm kiếm thông tin trên Google Maps/Wikipedia để trả về thông tin chính xác nhất.
                    Khi nhận được Tên Địa Điểm, hãy sinh ra dữ liệu JSON (KHÔNG MARKDOWN) với cấu trúc:
                    {{
                        ""moTa"": ""Mô tả chi tiết, sinh động, chuẩn văn phong du lịch..."",
                        ""diaChi"": ""Địa chỉ ước lượng hoặc chính xác"",
                        ""tinhThanh"": ""Tên Tỉnh/Thành phố (VD: Đà Nẵng, Hà Nội...)"",
                        ""xaPhuong"": ""Tên Quận/Huyện/Xã/Phường (nếu biết)""
                    }}
                    QUAN TRỌNG: 
                    1. Trường 'tinhThanh' BẮT BUỘC phải thuộc danh sách sau: [{_vietnamProvinces}].
                    2. Dựa vào ngữ cảnh để chọn Tỉnh chính xác nhất.
                    3. Trả về đúng JSON, không Markdown."),
                    new UserChatMessage($"Thông tin chi tiết về: {locationName}")
                };

                ChatCompletion completion = await client.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = 0.5f }); // Keep loc detail simpler/accurate
                var text = completion.Content[0].Text;
                text = text.Replace("```json", "").Replace("```", "").Trim();
                
                return text;
            }
            catch
            {
                return "{}";
            }
        }
    }
}