<div align="center">
<img src="docs/dxkuma.png" width="20%">

# 迪拉熊 Bot - Derakkuma Bot

## 酸橙熊熊 - Limekuma

</div>

[![CodeFactor](https://www.codefactor.io/repository/github/derakkumadev/limekuma-printer/badge)](https://www.codefactor.io/repository/github/derakkumadev/limekuma-printer)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/679c7eab2c8d48af8069da446f5ff8ae)](https://app.codacy.com/gh/DerakkumaDev/limekuma-printer/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![QQ](https://img.shields.io/badge/2689340931-gray?logo=qq&style=social)](https://qm.qq.com/cgi-bin/qm/qr?k=LyQOTRI7ViXYSTg0zbS2sGgcmkbYrxbP)

## 📖 项目简介

酸橙熊熊（青柠熊熊）是一个舞萌 DX 数据渲染服务，提供高性能的图像生成和数据处理能力。项目基于 .NET 构建，采用 gRPC 协议提供高效的远程过程调用服务。

## ✨ 主要特性

- 🎯 **多数据源支持**: 同时支持落雪和水鱼数据源
- 🎨 **高质量渲染**: 基于 SixLabors.ImageSharp 的高性能图像渲染引擎
- 📊 **数据统计**: 提供 Best 50、歌曲列表等多种数据展示
- 🔄 **实时处理**: 支持流式图像数据传输
- 🎭 **动画支持**: 可生成动画效果的 Best 50 图片
- 🎮 **游戏集成**: 专为舞萌 DX 游戏设计的数据处理

## 🏗️ 技术架构

### 核心技术栈

- **运行时**: .NET
- **通信协议**: gRPC
- **图像处理**: SixLabors.ImageSharp.Drawing
- **字体渲染**: SixLabors.Fonts
- **表达式引擎**: NCalc
- **模板引擎**: SmartFormat

### 项目结构

```
Limekuma/
├── src/                      # 源代码目录
│   ├── Prober/               # 数据探针模块
│   │   ├── Common/           # 通用数据模型
│   │   ├── DivingFish/       # 水鱼数据源
│   │   └── Lxns/             # 落雪数据源
│   ├── Render/               # 渲染引擎
│   │   ├── ExpressionEngine/ # 表达式引擎
│   │   ├── Nodes/            # 渲染节点
│   │   └── XmlSceneLoader.cs # XML场景加载器
│   ├── Services/             # gRPC服务实现
│   │   ├── BestsService.cs   # Best 50服务
│   │   ├── ListService.cs    # 列表服务
│   │   └── ...
│   ├── Utils/                # 工具类
│   ├── Protos/               # Protocol Buffers定义
│   └── Resources/            # 资源文件
├── docs/                     # 文档资源
└── .github/                  # GitHub工作流
```

## 🚀 快速开始

### 环境要求

- .NET 10.0 SDK
- 支持 HTTP/2 的客户端

### 安装与运行

1. **克隆项目**

   ```bash
   git clone https://github.com/DerakkumaDev/limekuma-printer.git
   cd limekuma-printer
   ```

2. **恢复依赖**

   ```bash
   dotnet restore
   ```

3. **运行服务**
   ```bash
   dotnet run --project src/Limekuma.csproj
   ```

### 服务端点

服务默认运行在 HTTP/2 端口，支持以下 gRPC 服务：

#### BestsApi 服务

- `GetFromLxns`: 从落雪获取 Best 50
- `GetAnimeFromLxns`: 从落雪获取动画版 Best 50
- `GetFromLxnsWithLevelSeg`: 从落雪获取带等级建议的 Best 50
- `GetFromDivingFish`: 从水鱼获取 Best 50
- `GetAnimeFromDivingFish`: 从水鱼获取动画版 Best 50
- `GetFromDivingFishWithLevelSeg`: 从水鱼获取带等级建议的 Best 50

#### ListApi 服务

- `GetFromLxns`: 从落雪获取达成表
- `GetFromDivingFish`: 从水鱼获取达成表

## 📊 API 使用示例

### gRPC 客户端调用

```csharp
// 创建gRPC客户端
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new BestsApi.BestsApiClient(channel);

// 请求落雪Best 50数据
var request = new LxnsBestsRequest
{
    DevToken = "your_dev_token",
    Qq = 123456789,
    PersonalToken = "your_personal_token"
};

using var call = client.GetFromLxns(request);
await foreach (var imageResponse in call.ResponseStream.ReadAllAsync())
{
    // 处理图像数据
    var imageData = imageResponse.Data;
}
```

## 🎨 渲染系统

### XML 场景描述

项目使用自定义的 XML 格式描述渲染场景：

```xml
<Canvas width="1200" height="1600" background="#FFFFFFFF">
    <Layer opacity="1">
        <Positioned x="100" y="100">
            <Text value="玩家Best 50" fontFamily="NotoSans" fontSize="24" color="#000000"/>
        </Positioned>
        <For items="records" var="record" indexVar="i">
            <Positioned x="{100 + (i % 5) * 120}" y="{200 + (i / 5) * 120}">
                <Image namespace="Jacket" id="{record.JacketId}"/>
            </Positioned>
        </For>
    </Layer>
</Canvas>
```

### 支持的节点类型

- `Canvas`: 画布容器
- `Layer`: 图层（支持透明度）
- `Positioned`: 定位容器
- `Image`: 图像节点
- `Text`: 文本节点
- `Stack`: 堆叠布局
- `If`: 条件渲染
- `For`: 循环渲染

## 🔧 配置说明

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http2"
    }
  }
}
```

## 📈 性能优化

- **内存管理**: 使用 ImageSharp 的高效内存处理
- **并行处理**: 支持异步并行图像处理
- **资源缓存**: 智能资源缓存机制
- **流式传输**: 支持分块流式图像传输

## 🤝 贡献指南

1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull request

## 📄 许可证

本项目采用 GNU Affero General Public License v3.0 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🐛 问题反馈

如果您遇到任何问题或有建议，请通过以下方式联系我们：

- 📧 QQ: 2689340931
- 🐛 [源域漏洞追踪系统](https://l.srcz.one/kumabugs)

## 🙏 致谢

感谢以下开源项目的支持：

- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Grpc.AspNetCore](https://github.com/grpc/grpc-dotnet)
- [NCalc](https://github.com/ncalc/ncalc)
- [SmartFormat](https://github.com/axuno/SmartFormat)

---

<div align="center">

**让音游数据渲染更简单** · **Powered by .NET**

</div>
