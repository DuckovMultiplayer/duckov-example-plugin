# Duckov Together 服务端 - 插件开发指南

感谢您对扩展 Duckov Together 服务端功能的关注。本指南将引导您创建自己的插件，为社区带来更丰富的多人游戏体验。

## 开始之前

### 环境要求

- .NET 8.0 SDK
- Visual Studio 2022 或 JetBrains Rider
- 引用 `DuckovTogetherServer.dll`

### 项目配置

创建一个面向 .NET 8.0 的类库项目：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Library</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="DuckovTogetherServer">
      <HintPath>path/to/DuckovTogetherServer.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

## 创建您的第一个插件

### 基本插件结构

每个插件必须实现 `IPlugin` 接口：

```csharp
using DuckovTogether.Plugins;

[PluginInfo(
    Name = "我的插件",
    Version = "1.0.0",
    Author = "您的名字",
    Description = "简要描述您的插件功能。"
)]
public class MyPlugin : IPlugin
{
    public string Name => "我的插件";
    public string Version => "1.0.0";
    public string Author => "您的名字";
    public string Description => "简要描述您的插件功能。";
    
    private IPluginHost _host;
    
    public void OnLoad(IPluginHost host)
    {
        _host = host;
        _host.Log("插件已加载，准备就绪！");
    }
    
    public void OnUnload()
    {
        _host.Log("插件正在卸载，下次再见！");
    }
    
    public void OnUpdate(float deltaTime)
    {
        // 每个服务器Tick调用
    }
}
```

## 插件主机 API

`IPluginHost` 接口提供了您所需的一切功能：

### 日志系统

```csharp
_host.Log("信息消息");
_host.LogWarning("需要注意的内容");
_host.LogError("发生错误");
```

### 控制台命令

注册服务器管理员可使用的自定义命令：

```csharp
_host.RegisterCommand("mycommand", "命令描述", args => {
    _host.Log($"命令执行，参数数量：{args.Length}");
});

// 或使用特性方式
[Command("hello", "向服务器问好")]
public void HelloCommand(string[] args)
{
    _host.Log("插件向您问好！");
}
```

### 玩家管理

```csharp
// 获取所有在线玩家
foreach (var player in _host.GetOnlinePlayers())
{
    _host.Log($"玩家：{player.Name}（延迟：{player.Latency}ms）");
}

// 获取特定玩家
var player = _host.GetPlayer(peerId);
```

### 消息收发

```csharp
// 广播给所有人
_host.BroadcastMessage(messageType, data);

// 发送给特定玩家
_host.SendToPlayer(peerId, messageType, data);

// 处理收到的消息
_host.RegisterMessageHandler(100, (peerId, data) => {
    _host.Log($"收到来自玩家 {peerId} 的自定义消息");
});
```

### 任务调度

```csharp
// 延迟执行一次
_host.ScheduleTask(5.0f, () => {
    _host.Log("5秒后执行");
});

// 重复执行
_host.ScheduleRepeating(60.0f, () => {
    _host.Log("每分钟执行一次");
}, out int taskId);

// 取消重复任务
_host.CancelTask(taskId);
```

## 事件系统

订阅游戏事件以响应玩家行为：

```csharp
public void OnLoad(IPluginHost host)
{
    _host = host;
    
    PluginEventDispatcher.Register<PlayerJoinEvent>(OnPlayerJoin);
    PluginEventDispatcher.Register<PlayerLeaveEvent>(OnPlayerLeave);
    PluginEventDispatcher.Register<PlayerDeathEvent>(OnPlayerDeath);
    PluginEventDispatcher.Register<PlayerChatEvent>(OnPlayerChat);
}

public void OnUnload()
{
    PluginEventDispatcher.Unregister<PlayerJoinEvent>(OnPlayerJoin);
    PluginEventDispatcher.Unregister<PlayerLeaveEvent>(OnPlayerLeave);
    PluginEventDispatcher.Unregister<PlayerDeathEvent>(OnPlayerDeath);
    PluginEventDispatcher.Unregister<PlayerChatEvent>(OnPlayerChat);
}

private void OnPlayerJoin(PlayerJoinEvent evt)
{
    _host.Log($"欢迎 {evt.PlayerName} 加入服务器！");
}

private void OnPlayerChat(PlayerChatEvent evt)
{
    if (evt.Message.StartsWith("!"))
    {
        // 作为命令处理
        evt.Cancelled = true; // 阻止消息广播
    }
}
```

### 可用事件列表

| 事件 | 描述 |
|------|------|
| `PlayerJoinEvent` | 玩家连接到服务器 |
| `PlayerLeaveEvent` | 玩家断开连接 |
| `PlayerChatEvent` | 玩家发送聊天消息 |
| `PlayerMoveEvent` | 玩家位置变化 |
| `PlayerDamageEvent` | 玩家受到伤害 |
| `PlayerDeathEvent` | 玩家死亡 |
| `AISpawnEvent` | AI实体生成 |
| `AIDeathEvent` | AI实体被击杀 |
| `SceneChangeEvent` | 服务器切换场景 |
| `ExtractStartEvent` | 玩家开始撤离 |
| `ExtractCompleteEvent` | 玩家完成撤离 |
| `ItemPickupEvent` | 玩家拾取物品 |
| `ItemDropEvent` | 玩家丢弃物品 |

## 数据存储

将插件数据存储在指定目录：

```csharp
var dataFile = Path.Combine(_host.DataPath, "myplugin_data.json");
var json = File.ReadAllText(dataFile);
```

## 安装插件

1. 编译您的插件项目
2. 将生成的 `.dll` 文件复制到服务器的 `Plugins` 目录
3. 重启服务器
4. 插件将自动加载

## 最佳实践

1. **清理资源** - 务必在 `OnUnload` 中注销事件和取消任务
2. **处理异常** - 将风险操作包装在 try-catch 块中
3. **注意性能** - 避免在 `OnUpdate` 中执行耗时操作
4. **有意义的日志** - 帮助服务器管理员了解插件状态
5. **以玩家为中心** - 创建能提升玩家体验的功能

## 示例插件

包含的 `WelcomePlugin` 演示了：

- 玩家加入时的欢迎消息
- 游戏时间和统计追踪
- 聊天命令（!help, !stats, !online, !time）
- 定时服务器公告
- 自定义控制台命令

希望本指南能帮助您为社区创建出色的插件。祝您编码愉快，感谢您成为 Duckov Together 大家庭的一员！
