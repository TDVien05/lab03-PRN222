# Huong Dan Chay Project Lab03

## 1. Yeu cau moi truong

Can cai dat:

- .NET SDK phu hop voi project `net10.0`
- PostgreSQL
- Trinh duyet web

Kiem tra .NET:

```cmd
dotnet --version
```

## 2. Tao database

Mo terminal tai thu muc project:

```cmd
cd C:\Users\kietta\Documents\working\lab03
```

Chay script tao database PostgreSQL:

```cmd
psql -U postgres -f create_database.sql
```

Neu PostgreSQL cua ban dung user/password khac, hay dieu chinh lenh tren cho phu hop.

## 3. Cau hinh connection string

Mo file:

```text
lab03\appsettings.json
```

Kiem tra `ConnectionStrings:DefaultConnection` tro dung database PostgreSQL da tao.

Vi du:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=group_chat_app;Username=postgres;Password=your_password"
  }
}
```

## 4. Chay project

Tai thu muc root:

```cmd
cd C:\Users\kietta\Documents\working\lab03
```

Chay app:

```cmd
dotnet run --project lab03\lab03.csproj --launch-profile http
```

Neu thanh cong, terminal se hien:

```text
Now listening on: http://0.0.0.0:5026
Application started. Press Ctrl+C to shut down.
```

De app tiep tuc hoat dong, giu cua so terminal nay mo.

## 5. Truy cap tren may cua ban

Mo trinh duyet:

```text
http://localhost:5026
```

Hoac dung IPv4 cua may ban:

```text
http://YOUR_IPV4:5026
```

## 6. Cho ban be cung Wi-Fi truy cap

May ban be phai cung mang Wi-Fi/LAN voi may chay project.

Tim IPv4 cua may ban:

```cmd
ipconfig
```

Lay IPv4 trong phan Wi-Fi, vi du:

```text
192.168.1.25
```

Ban be truy cap:

```text
http://192.168.1.25:5026
```

Khong dung:

```text
http://localhost:5026
http://0.0.0.0:5026
https://192.168.1.25:5026
```

## 7. Neu ban be khong vao duoc

Kiem tra:

- Hai may co cung lop mang khong, vi du deu la `192.168.1.x`
- Khong dung guest Wi-Fi
- Khong bat VPN
- Router khong bat AP Isolation / Client Isolation
- Windows Firewall cho phep TCP port `5026`

Mo port tren Windows Firewall:

1. Mo Windows Defender Firewall with Advanced Security
2. Chon Inbound Rules
3. Chon New Rule
4. Chon Port
5. Chon TCP
6. Nhap port `5026`
7. Chon Allow the connection
8. Chon Private
9. Dat ten `ASP.NET Chat 5026`
10. Finish

Thu nhanh bang hotspot dien thoai: cho ca hai may ket noi cung hotspot roi truy cap lai.

## 8. Upload file

Gioi han upload hien tai:

```text
1.5 GB
```

Neu upload file lon, can dam bao:

- O dia may chay server con du dung luong
- Mang Wi-Fi on dinh
- Khong tat terminal dang chay app trong luc upload

File upload duoc luu trong:

```text
lab03\wwwroot\uploads\chat-files
```

## 9. Dung project

Trong terminal dang chay app, nhan:

```cmd
Ctrl + C
```

## 10. Build va test

Build project:

```cmd
dotnet build lab03.slnx
```

Chay test:

```cmd
dotnet test lab03.slnx
```

Neu build bi loi file dang bi khoa, hay tat app dang chay bang `Ctrl + C` roi build lai.
