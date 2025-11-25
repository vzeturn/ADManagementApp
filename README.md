# ACTIVE DIRECTORY MANAGEMENT APPLICATION
## Ứng dụng quản lý Domain Controller - WPF .NET

### 📁 CẤU TRÚC DỰ ÁN

```
ADManagementApp/
│
├── ADManagementApp.sln                 # Solution file
│
├── ADManagementApp/                    # Main Project
│   ├── ADManagementApp.csproj          # Project file
│   ├── App.xaml                        # Application XAML
│   ├── App.xaml.cs                     # Application code-behind
│   ├── MainWindow.xaml                 # Main Window XAML
│   ├── MainWindow.xaml.cs              # Main Window code-behind
│   │
│   ├── Models/                         # Data Models
│   │   ├── ADUser.cs                   # User model
│   │   ├── ADGroup.cs                  # Group model
│   │   └── DomainStats.cs              # Statistics model
│   │
│   ├── Services/                       # Business Logic
│   │   ├── ADService.cs                # Active Directory service
│   │   └── IADService.cs               # Interface
│   │
│   ├── ViewModels/                     # MVVM ViewModels
│   │   ├── MainViewModel.cs            # Main ViewModel
│   │   ├── UserManagementViewModel.cs  # User management ViewModel
│   │   ├── GroupManagementViewModel.cs # Group management ViewModel
│   │   └── BaseViewModel.cs            # Base ViewModel
│   │
│   ├── Views/                          # User Interface Views
│   │   ├── DashboardView.xaml          # Dashboard
│   │   ├── UserManagementView.xaml     # User management
│   │   ├── GroupManagementView.xaml    # Group management
│   │   └── SettingsView.xaml           # Settings
│   │
│   ├── Helpers/                        # Helper classes
│   │   ├── RelayCommand.cs             # Command implementation
│   │   └── Converters.cs               # Value converters
│   │
│   ├── Resources/                      # Resources
│   │   ├── Styles.xaml                 # Application styles
│   │   └── Icons/                      # Icon files
│   │
│   └── appsettings.json                # Configuration file
│
└── README.md                           # This file
```

### 🎯 TÍNH NĂNG CHÍNH

#### 1. Dashboard
- Thống kê tổng quan domain
- Số lượng users (enabled/disabled)
- Số lượng groups
- Recent activities

#### 2. User Management
- ✅ Xem danh sách users
- ✅ Tìm kiếm và lọc users
- ✅ Tạo user mới
- ✅ Chỉnh sửa thông tin user
- ✅ Xóa user
- ✅ Enable/Disable user
- ✅ Reset password
- ✅ Xem chi tiết user (groups, last logon, etc.)
- ✅ Unlock account

#### 3. Group Management
- ✅ Xem danh sách groups
- ✅ Tìm kiếm groups
- ✅ Tạo group mới
- ✅ Xóa group
- ✅ Thêm/xóa members
- ✅ Xem danh sách members

#### 4. Settings
- Cấu hình kết nối Domain Controller
- Credentials management
- Application preferences

### 🛠️ CÔNG NGHỆ SỬ DỤNG

- **Framework**: .NET 8.0 WPF
- **UI**: WPF với Material Design
- **Architecture**: MVVM (Model-View-ViewModel)
- **AD Integration**: System.DirectoryServices & System.DirectoryServices.AccountManagement
- **UI Components**: MaterialDesignThemes
- **Icons**: Material Design Icons

### 📋 YÊU CẦU HỆ THỐNG

#### Phần mềm cần thiết:
1. **Visual Studio 2022** (Community/Professional/Enterprise)
   - Workload: .NET Desktop Development
   - Workload: Windows Presentation Foundation

2. **.NET 8.0 SDK** hoặc cao hơn
   - Download: https://dotnet.microsoft.com/download

3. **Windows 10/11** hoặc **Windows Server 2016+**

#### Quyền yêu cầu:
- Tài khoản có quyền quản trị Domain Controller
- Hoặc tài khoản được ủy quyền quản lý AD

### 🚀 HƯỚNG DẪN TRIỂN KHAI CHI TIẾT

#### BƯỚC 1: Cài đặt môi trường

1. **Cài đặt Visual Studio 2022**
   ```
   - Tải từ: https://visualstudio.microsoft.com/downloads/
   - Chọn workload: ".NET desktop development"
   - Đảm bảo chọn component: "Windows Presentation Foundation"
   ```

2. **Kiểm tra .NET SDK**
   ```bash
   dotnet --version
   # Phải >= 8.0
   ```

#### BƯỚC 2: Tạo dự án mới

**Option A: Sử dụng Visual Studio**
```
1. Mở Visual Studio 2022
2. Create new project
3. Chọn "WPF Application" (C#)
4. Project name: ADManagementApp
5. Framework: .NET 8.0
6. Click Create
```

**Option B: Sử dụng Command Line**
```bash
# Tạo solution và project
dotnet new sln -n ADManagementApp
dotnet new wpf -n ADManagementApp -f net8.0-windows
dotnet sln add ADManagementApp/ADManagementApp.csproj

# Di chuyển vào thư mục project
cd ADManagementApp
```

#### BƯỚC 3: Cài đặt NuGet Packages

**Visual Studio:**
```
1. Right-click vào project > Manage NuGet Packages
2. Browse tab > Search và Install các packages sau:
   - MaterialDesignThemes (4.9.0+)
   - MaterialDesignColors (3.1.0+)
   - System.DirectoryServices (8.0.0+)
   - System.DirectoryServices.AccountManagement (8.0.0+)
   - Microsoft.Extensions.Configuration (8.0.0+)
   - Microsoft.Extensions.Configuration.Json (8.0.0+)
   - Newtonsoft.Json (13.0.3+)
```

**Command Line:**
```bash
dotnet add package MaterialDesignThemes --version 4.9.0
dotnet add package MaterialDesignColors --version 3.1.0
dotnet add package System.DirectoryServices --version 8.0.0
dotnet add package System.DirectoryServices.AccountManagement --version 8.0.0
dotnet add package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 8.0.0
dotnet add package Newtonsoft.Json --version 13.0.3
```

#### BƯỚC 4: Tạo cấu trúc thư mục

```bash
# Trong thư mục ADManagementApp/
mkdir Models Services ViewModels Views Helpers Resources
mkdir Resources/Icons
```

#### BƯỚC 5: Copy các file source code

```
Sao chép tất cả các file .cs và .xaml được tạo trong dự án này vào đúng thư mục tương ứng
```

#### BƯỚC 6: Cấu hình appsettings.json

Tạo file `appsettings.json` trong project:
```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.local",
    "DefaultOU": "OU=Users,DC=yourdomain,DC=local",
    "AdminUsername": "",
    "AdminPassword": ""
  },
  "Application": {
    "Theme": "Light",
    "AutoRefreshInterval": 30
  }
}
```

**Lưu ý:** Đặt Properties của file này:
- Build Action: Content
- Copy to Output Directory: Copy if newer

#### BƯỚC 7: Cập nhật .csproj file

Đảm bảo file `ADManagementApp.csproj` có cấu hình:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>icon.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

#### BƯỚC 8: Build và Test

**Visual Studio:**
```
1. Build > Build Solution (Ctrl + Shift + B)
2. Kiểm tra Output window không có error
3. Debug > Start Debugging (F5)
```

**Command Line:**
```bash
# Build project
dotnet build

# Nếu build thành công, chạy ứng dụng
dotnet run
```

#### BƯỚC 9: Cấu hình kết nối Domain

1. Khi chạy ứng dụng lần đầu
2. Vào Settings
3. Nhập thông tin:
   - Domain Name: yourdomain.local
   - Username: Domain\Administrator (hoặc user có quyền)
   - Password: ***
4. Click "Test Connection"
5. Click "Save"

#### BƯỚC 10: Publish ứng dụng

**Option A: Self-Contained (Không cần cài .NET Runtime)**
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Output: bin/Release/net8.0-windows/win-x64/publish/
```

**Option B: Framework-Dependent (Cần cài .NET Runtime)**
```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true

# File nhỏ hơn nhưng máy cần có .NET Runtime
```

**Visual Studio Publish:**
```
1. Right-click project > Publish
2. Target: Folder
3. Configuration: Release
4. Target Framework: net8.0-windows
5. Deployment Mode: Self-contained hoặc Framework-dependent
6. Target Runtime: win-x64
7. File publish options:
   - ☑ Produce single file
   - ☑ Enable ReadyToRun compilation
8. Click Publish
```

### 📦 TRIỂN KHAI TỚI CLIENT

#### Cách 1: Copy file EXE
```
1. Build/Publish ứng dụng
2. Copy thư mục publish/ đến máy client
3. Copy file appsettings.json (đã cấu hình)
4. Chạy ADManagementApp.exe
```

#### Cách 2: Tạo Installer (ClickOnce)
```
1. Visual Studio > Project > Publish
2. Chọn ClickOnce
3. Publish Location: Network share hoặc Web
4. Install Mode: Available online and offline
5. Finish > Publish
6. User có thể install từ setup.exe
```

#### Cách 3: Tạo MSI Installer (WiX Toolset)
```
1. Cài WiX Toolset: https://wixtoolset.org/
2. Thêm WiX Setup Project vào solution
3. Configure product information
4. Build MSI file
5. Distribute MSI
```

### 🔒 BẢO MẬT

#### Lưu trữ Credentials an toàn:
```csharp
// Sử dụng Windows Credential Manager
using System.Security.Cryptography;

// Hoặc mã hóa trong appsettings.json
// Không lưu plain text password!
```

#### Best Practices:
1. ✅ Sử dụng Windows Authentication khi có thể
2. ✅ Mã hóa credentials trong config
3. ✅ Sử dụng HTTPS cho remote connection
4. ✅ Implement audit logging
5. ✅ Giới hạn quyền theo role

### 🐛 TROUBLESHOOTING

#### Lỗi: "Unable to connect to domain"
```
- Kiểm tra domain name đúng format: domain.local
- Kiểm tra network connection đến DC
- Verify credentials có quyền
- Check firewall settings
```

#### Lỗi: "Access Denied"
```
- User cần quyền tối thiểu:
  - Read all user information
  - Create, delete, and manage user accounts
  - Reset user passwords
  - Modify group membership
```

#### Lỗi: Material Design không load
```
- Rebuild solution
- Clean bin/obj folders
- Reinstall MaterialDesignThemes NuGet
```

### 📊 PERFORMANCE OPTIMIZATION

1. **Lazy Loading**: Load users/groups khi cần
2. **Paging**: Phân trang cho danh sách lớn
3. **Caching**: Cache thông tin domain
4. **Async Operations**: Sử dụng async/await cho AD queries
5. **Background Tasks**: Search và filter trong background thread

### 🔄 CẬP NHẬT VÀ BẢO TRÌ

#### Update Dependencies:
```bash
# Check outdated packages
dotnet list package --outdated

# Update all packages
dotnet add package MaterialDesignThemes
```

#### Version Control:
```bash
git init
git add .
git commit -m "Initial commit"
```

### 📝 GHI CHÚ QUAN TRỌNG

1. **Testing Environment**: 
   - Test trên test domain trước
   - Không test trên production domain
   - Backup AD trước khi test

2. **User Training**:
   - Hướng dẫn user cách sử dụng
   - Cảnh báo về tác động của các thao tác
   - Document các best practices

3. **Monitoring**:
   - Enable logging
   - Monitor application performance
   - Track user activities

### 🆘 HỖ TRỢ

- Documentation: README.md trong project
- Issues: Tạo issue trong repository
- Email support: support@yourcompany.com

### 📄 LICENSE

MIT License - Free to use and modify

---

**Được phát triển bởi**: IT Infrastructure Team
**Phiên bản**: 1.0.0
**Cập nhật**: 2024