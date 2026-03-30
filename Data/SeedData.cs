// Folder: Data
// File path: Data/SeedData.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Tour_2040.Models;

namespace Tour_2040.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            // 1. Lấy DbContext để đảm bảo Database đã được cập nhật Migration
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Lệnh này cực kỳ quan trọng: Nó sẽ tự động chạy "Update-Database" bằng code
            // Giúp bạn không bị lỗi "Table not found" khi chạy lần đầu
            await context.Database.MigrateAsync();

            // 2. Lấy các Service quản lý User và Role
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 3. Tạo Roles (Quyền truy cập)
            string[] roles = { "Admin", "Staff", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 4. Tạo tài khoản ADMIN mặc định (nếu chưa có)
            await CreateUserIfNotExists(userManager,
                email: "admin@tour2040.com",
                password: "Admin@123", // Mật khẩu cần có: Hoa, thường, số, ký tự đặc biệt
                role: "Admin",
                hoTen: "Quản Trị Viên",
                maNguoiDung: "ADMIN01");

            // 5. Seed dữ liệu địa lý (Tỉnh/Xã/Địa điểm)
            await SeedLocationsAsync(context);
        }

        private static async Task SeedLocationsAsync(ApplicationDbContext context)
        {
            // Chỉ seed nếu chưa có dữ liệu TinhThanh
            if (await context.TinhThanhs.AnyAsync()) return;

            var locations = new List<(string Tinh, int MaTinh, (string Xa, string[] DiaDiems)[] Xas)>
            {
                // --- 6 THÀNH PHỐ TRỰC THUỘC TRUNG ƯƠNG ---
                ("Hà Nội", 1, new[] {
                    ("Hoàn Kiếm", new[] { "Hồ Gươm", "Nhà hát lớn Hà Nội", "Tràng Tiền Plaza", "Phố Cổ" }),
                    ("Ba Đình", new[] { "Lăng Bác", "Quảng trường Ba Đình", "Chùa Một Cột" }),
                    ("Tây Hồ", new[] { "Hồ Tây", "Chùa Trấn Quốc" })
                }),
                ("Hồ Chí Minh", 2, new[] { // Sáp nhập Bà Rịa - Vũng Tàu, Bình Dương
                    ("Quận 1", new[] { "Chợ Bến Thành", "Phố đi bộ Nguyễn Huệ", "Dinh Độc Lập" }),
                    ("Bình Thạnh", new[] { "Landmark 81", "Khu du lịch Văn Thánh" }),
                    ("Vũng Tàu (Cũ)", new[] { "Tượng Chúa Kitô Vua", "Bãi Sau", "Bãi Trước" }) 
                }),
                ("Hải Phòng", 3, new[] { // Sáp nhập Hải Dương
                    ("Hồng Bàng", new[] { "Nhà hát lớn Hải Phòng" }),
                    ("Cát Hải", new[] { "Đảo Cát Bà", "Vịnh Lan Hạ" }),
                    ("Hải Dương (Cũ)", new[] { "Đảo Cò Chi Lăng", "Côn Sơn - Kiếp Bạc" })
                }),
                ("Đà Nẵng", 4, new[] { // Sáp nhập Quảng Nam
                    ("Hải Châu", new[] { "Cầu Rồng", "Chợ Hàn" }),
                    ("Sơn Trà", new[] { "Biển Mỹ Khê", "Chùa Linh Ứng" }),
                    ("Hội An (Cũ)", new[] { "Phố cổ Hội An", "Chùa Cầu", "Rừng dừa Bảy Mẫu" })
                }),
                ("Cần Thơ", 5, new[] { // Sáp nhập Sóc Trăng, Hậu Giang
                    ("Ninh Kiều", new[] { "Bến Ninh Kiều", "Chợ nổi Cái Răng" }),
                    ("Sóc Trăng (Cũ)", new[] { "Chùa Dơi", "Chùa Chén Kiểu" })
                }),
                ("Huế", 6, new[] { // Từ Thừa Thiên Huế
                    ("Huế", new[] { "Đại Nội Huế", "Chùa Thiên Mụ", "Lăng Khải Định", "Sông Hương" })
                }),

                // --- 28 TỈNH ---
                ("Quảng Ninh", 7, new[] { ("Hạ Long", new[] { "Vịnh Hạ Long", "Sun World Hạ Long" }) }),
                ("Cao Bằng", 8, new[] { ("Trùng Khánh", new[] { "Thác Bản Giốc" }) }),
                ("Lạng Sơn", 9, new[] { ("TP Lạng Sơn", new[] { "Ải Chi Lăng" }) }),
                ("Lai Châu", 10, new[] { ("Tam Đường", new[] { "Đèo Ô Quy Hồ" }) }),
                ("Điện Biên", 11, new[] { ("Điện Biên Phủ", new[] { "Đồi A1", "Hầm Đờ Cát" }) }),
                ("Sơn La", 12, new[] { ("Mộc Châu", new[] { "Rừng thông Bản Áng", "Đồi chè Trái Tim" }) }),
                ("Thanh Hóa", 13, new[] { ("Sầm Sơn", new[] { "Bãi biển Sầm Sơn" }) }),
                ("Nghệ An", 14, new[] { ("Cửa Lò", new[] { "Biển Cửa Lò" }), ("Nam Đàn", new[] { "Làng Sen Quê Bác" }) }),
                ("Hà Tĩnh", 15, new[] { ("Can Lộc", new[] { "Ngã ba Đồng Lộc" }) }),
                ("Tuyên Quang", 16, new[] { ("TP Tuyên Quang", new[] { "Khu di tích Tân Trào" }) }),
                ("Lào Cai", 17, new[] { ("Sa Pa", new[] { "Đỉnh Fansipan", "Bản Cát Cát", "Nhà thờ Đá Sa Pa" }) }), 
                ("Thái Nguyên", 18, new[] { ("TP Thái Nguyên", new[] { "Hồ Núi Cốc" }) }),
                ("Phú Thọ", 19, new[] { ("Việt Trì", new[] { "Đền Hùng" }) }), 
                ("Bắc Ninh", 20, new[] { ("TP Bắc Ninh", new[] { "Chùa Phật Tích" }) }),
                ("Hưng Yên", 21, new[] { ("Phố Hiến", new[] { "Văn miếu Xích Đằng" }) }),
                ("Ninh Bình", 22, new[] { ("Hoa Lư", new[] { "Tràng An", "Hang Múa", "Chùa Bái Đính" }) }),
                ("Quảng Trị", 23, new[] { ("Đông Hà", new[] { "Thành cổ Quảng Trị" }) }),
                ("Quảng Ngãi", 24, new[] { ("Lý Sơn", new[] { "Đảo Lý Sơn" }) }),
                ("Gia Lai", 25, new[] { ("Pleiku", new[] { "Biển Hồ Pleiku" }) }),
                ("Khánh Hòa", 26, new[] { ("Nha Trang", new[] { "Vinpearl Land", "Tháp Bà Ponagar" }), ("Cam Ranh", new[] { "Đảo Bình Ba" }) }),
                ("Lâm Đồng", 27, new[] { ("Đà Lạt", new[] { "Hồ Xuân Hương", "Langbiang", "Thung lũng Tình Yêu" }) }),
                ("Đắk Lắk", 28, new[] { ("Buôn Ma Thuột", new[] { "Bảo tàng Thế giới Cà phê" }) }),
                ("Đồng Nai", 29, new[] { ("Biên Hòa", new[] { "Khu du lịch Bửu Long" }) }),
                ("Đồng Tháp", 30, new[] { ("Sa Đéc", new[] { "Làng hoa Sa Đéc" }) }),
                ("Vĩnh Long", 31, new[] { ("Bình Minh", new[] { "Chùa Ông" }) }),
                ("Long An", 32, new[] { ("Tân An", new[] { "Làng nổi Tân Lập" }) }),
                ("An Giang", 33, new[] { ("Châu Đốc", new[] { "Miếu Bà Chúa Xứ" }) }),
                ("Cà Mau", 34, new[] { ("Đất Mũi", new[] { "Mũi Cà Mau" }) })
            };

            int xaIdCounter = 1;
            
            foreach (var t in locations)
            {
                var tinh = new TinhThanh { MaTinhThanh = t.MaTinh, TenTinh = t.Tinh };
                context.TinhThanhs.Add(tinh);

                foreach (var x in t.Xas)
                {
                    var xa = new XaPhuong 
                    { 
                        MaXaPhuong = (t.MaTinh * 100) + (xaIdCounter++), // Generate unique ID based on Province code
                        TenXaPhuong = x.Xa,
                        TinhThanh = tinh 
                    };
                    context.XaPhuongs.Add(xa);

                    foreach (var ddName in x.DiaDiems)
                    {
                        var diaDiem = new DiaDiem
                        {
                            // MaDiaDiem được tạo tự động trong Constructor (Sử dụng MaSoHelper)
                            TenDiaDiem = ddName,
                            XaPhuong = xa,
                            MoTa = $"Địa điểm nổi tiếng tại {x.Xa}, {t.Tinh}",
                            ConSuDung = true
                        };
                        context.DiaDiems.Add(diaDiem);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role,
            string hoTen,
            string maNguoiDung)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true, // Xác thực email luôn để đăng nhập được ngay
                    HoTen = hoTen,
                    MaNguoiDung = maNguoiDung,
                    NgaySinh = DateTime.Now,
                    ProfilePictureUrl = null
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}