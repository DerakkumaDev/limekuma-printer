<div align="center">

<img src="docs/dxkuma.png" width="22%">

# 迪拉熊Bot - Derakkuma Bot

## 酸橙熊熊 - Limekuma

为舞萌DX设计的成绩图片渲染微服务

</div>

![Static Badge](https://img.shields.io/badge/Ver-LI26.14--A-blue)
![Static Badge](https://img.shields.io/badge/License-AGPLv3-orange)
[![CodeFactor](https://www.codefactor.io/repository/github/derakkumadev/limekuma-printer/badge)](https://www.codefactor.io/repository/github/derakkumadev/limekuma-printer)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/34a3bb83aa2a4182ac748954c18aac89)](https://app.codacy.com/gh/DerakkumaDev/limekuma-printer/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![QQ](https://img.shields.io/badge/Bot-迪拉熊-grey?style=social&logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxMTIuODIgMTMwLjg5Ij48ZyBkYXRhLW5hbWU9IuWbvuWxgiAyIj48ZyBkYXRhLW5hbWU9IuWbvuWxgiAxIj48cGF0aCBkPSJNNTUuNjMgMTMwLjhjLTcgMC0xMy45LjA4LTIwLjg2IDAtMTkuMTUtLjI1LTMxLjcxLTExLjQtMzQuMjItMzAuMy00LjA3LTMwLjY2IDE0LjkzLTU5LjIgNDQuODMtNjYuNjQgMi0uNTEgNS4yMS0uMzEgNS4yMS0xLjYzIDAtMi4xMy4xNC0yLjEzLjE0LTUuNTcgMC0uODktMS4zLTEuNDYtMi4yMi0yLjMxLTYuNzMtNi4yMy03LjY3LTEzLjQxLTEtMjAuMTggNS40LTUuNTIgMTEuODctNS40IDE3LjgtLjU5IDYuNDkgNS4yNiA2LjMxIDEzLjA4LS44NiAyMS0uNjguNzQtMS43OCAxLjYtMS43OCAyLjY3djQuMjFjMCAxLjM1IDIuMiAxLjYyIDQuNzkgMi4zNSAzMS4wOSA4LjY1IDQ4LjE3IDM0LjEzIDQ1IDY2LjM3LTEuNzYgMTguMTUtMTQuNTYgMzAuMjMtMzIuNyAzMC42My04LjAyLjE5LTE2LjA3LS4wMS0yNC4xMy0uMDF6IiBmaWxsPSIjMDI5OWZlIi8+PHBhdGggZD0iTTMxLjQ2IDExOC4zOGMtMTAuNS0uNjktMTYuOC02Ljg2LTE4LjM4LTE3LjI3LTMtMTkuNDIgMi43OC0zNS44NiAxOC40Ni00Ny44MyAxNC4xNi0xMC44IDI5Ljg3LTEyIDQ1LjM4LTMuMTkgMTcuMjUgOS44NCAyNC41OSAyNS44MSAyNCA0NS4yOS0uNDkgMTUuOS04LjQyIDIzLjE0LTI0LjM4IDIzLjUtNi41OS4xNC0xMy4xOSAwLTE5Ljc5IDAiIGZpbGw9IiNmZWZlZmUiLz48cGF0aCBkPSJNNDYuMDUgNzkuNThjLjA5IDUgLjIzIDkuODItNyA5Ljc3LTcuODItLjA2LTYuMS01LjY5LTYuMjQtMTAuMTktLjE1LTQuODItLjczLTEwIDYuNzMtOS44NHM2LjM3IDUuNTUgNi41MSAxMC4yNnoiIGZpbGw9IiMxMDlmZmUiLz48cGF0aCBkPSJNODAuMjcgNzkuMjdjLS41MyAzLjkxIDEuNzUgOS42NC01Ljg4IDEwLTcuNDcuMzctNi44MS00LjgyLTYuNjEtOS41LjItNC4zMi0xLjgzLTEwIDUuNzgtMTAuNDJzNi41OSA0Ljg5IDYuNzEgOS45MnoiIGZpbGw9IiMwODljZmUiLz48L2c+PC9nPjwvc3ZnPg==)](https://qun.qq.com/qunpro/robot/qunshare?robot_appid=102613765&robot_uin=3889525249)
[![QQ](https://img.shields.io/badge/Bot-迪拉熊-gray?logo=qq&style=social)](https://qm.qq.com/cgi-bin/qm/qr?k=LyQOTRI7ViXYSTg0zbS2sGgcmkbYrxbP)
[![QQ](https://img.shields.io/badge/Group-迪拉熊开发者交流群-gray?logo=qq&style=social)](https://qm.qq.com/cgi-bin/qm/qr?k=g-3hU7eFmvcFUuCKdCE-3COu8Ej9LfnD&jump_from=webapi&authKey=IpqRZXflOY9UKkYPn0Ho2RrVBw+UF2pfTZk6WnhU39idA4AyJt65nAwfuPfn1yZ+)

## 📖 项目简介

酸橙熊熊（青柠熊熊）是一个高性能图片生成后端服务。主要用于接入舞萌DX的查分器数据，为玩家的游玩成绩（如B50、成绩列表等）生成精美的图片，常作为机器人（如迪拉熊Bot）的后端渲染引擎。

本项目通过纯托管图形库ImageSharp实现了脱离原生环境依赖的图形渲染，并内置了基于XML的灵活布局引擎，能够动态渲染丰富多样的玩家成绩卡片。

## 📚 项目文档

关于酸橙熊熊的详细介绍、架构设计以及协作者接入指南，请参阅我们的[官方文档](https://dxkuma.srcz.one/collaborator-guide/limekuma-intro.html)。

## ✨ 核心特性

- 🚀 **高性能渲染**：基于ImageSharp，无需依赖本地系统GDI或libgdiplus，跨平台支持好。
- 📊 **多数据源支持**:
  - 原生支持[落雪咖啡屋 (Lxns)](https://maimai.lxns.net)查分器API
  - 原生支持[水鱼 (Diving-Fish)](https://www.diving-fish.com/maimaidx/prober/)查分器API
- 🖼️ **丰富的图像生成接口**:
  - **BestsApi**：生成玩家B50成绩图，支持段位、等级、头像细分及动图组合。
  - **ListApi**：生成玩家特定等级谱面的成绩列表图。
- 🔧 **自定义布局引擎**：渲染逻辑与布局分离，基于XML节点定义的灵活布局系统。
- 📡 **流式gRPC通信**：基于Protobuf定义，提供流式的图片字节序列返回，降低大图片传输的内存占用。

## 🛠️ 技术栈

- **核心框架**：.NET
- **通信协议**：gRPC
- **图像处理**：ImageSharp
- **表达式引擎**：NCalc
- **模板引擎**：SmartFormat
- **配置管理**：Hocon

## 📦 快速开始

### 1. 环境要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 2. 获取源码

```bash
git clone <your-repo-url>/limekuma-printer.git
cd limekuma-printer
```

### 3. 安装依赖

```bash
dotnet restore
```

### 4. 运行项目

```bash
dotnet run --project src/Limekuma.csproj
```

项目启动后，将通过配置中指定的端口提供gRPC服务。

## 📜 接口定义

API接口均定义在[`src/Protos/kumabot.proto`](src/Protos/kumabot.proto)文件中。

### 支持的RPC方法

- **BestsApi**
  - `GetFromLxns` / `GetFromDivingFish`：获取常规B50图片。
  - `GetAnimeFromLxns` / `GetAnimeFromDivingFish`：获取包含动画版B50图片。
  - `GetFromLxnsWithLevelSeg` / `GetFromDivingFishWithLevelSeg`：获取带等级建议的B50图片。
- **ListApi**
  - `GetFromLxns` / `GetFromDivingFish`：获取指定等级或页码的玩家成绩列表图片。

## 📄 开源协议

本项目基于**GNU Affero General Public License v3.0（AGPL-3.0）**协议开源。详细信息请参阅[LICENSE](LICENSE)文件。

## 🐛 问题反馈

如果您遇到任何问题或有建议，请通过以下方式联系我们：

- 📧 [迪拉熊开发者交流群](https://qm.qq.com/cgi-bin/qm/qr?k=g-3hU7eFmvcFUuCKdCE-3COu8Ej9LfnD&jump_from=webapi&authKey=IpqRZXflOY9UKkYPn0Ho2RrVBw+UF2pfTZk6WnhU39idA4AyJt65nAwfuPfn1yZ+)
- 🐛 [Codeberg](https://codeberg.org/Derakkuma/dxkuma-bot)
