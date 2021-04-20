# SmartMusicNetCoreAudio
一个智能音箱，跨三端（手机端控制，嵌入式端播放，服务器端转发手机端网络协议）

# 使用方法
## 嵌入式端
SmartMusicNetCoreAudio 是嵌入式端程序，需要系统安装 .NET 5.0 及以上框架，然后进入到bin所在层级目录， dotnet run，程序运行
 .NET 框架安装请查询微软官方
## 服务器端
同理，服务器安装 .NET 框架，运行程序 dotnet run（记得开启端口 8888）
## 客户端
使用unity 可以打包适合自己的客户端（windows Android Mac）启动后连接服务器地址即可

# 注意事项
三端皆为C#制作，使用LitJson组装网络协议，注释详细，可以作为网络入门。
