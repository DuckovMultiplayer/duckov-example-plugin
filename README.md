# Duckov Together Server - Plugin Development Guide

Thank you for your interest in extending the Duckov Together server. This guide will walk you through creating your own plugins to enhance the multiplayer experience for your community.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or JetBrains Rider
- Reference to `DuckovTogetherServer.dll`

### Project Setup

Create a new Class Library project targeting .NET 8.0:

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

## Creating Your First Plugin

### Basic Plugin Structure

Every plugin must implement the `IPlugin` interface:

```csharp
using DuckovTogether.Plugins;

[PluginInfo(
    Name = "My Plugin",
    Version = "1.0.0",
    Author = "Your Name",
    Description = "A brief description of what your plugin does."
)]
public class MyPlugin : IPlugin
{
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    public string Author => "Your Name";
    public string Description => "A brief description of what your plugin does.";
    
    private IPluginHost _host;
    
    public void OnLoad(IPluginHost host)
    {
        _host = host;
        _host.Log("My plugin has been loaded. Ready to serve!");
    }
    
    public void OnUnload()
    {
        _host.Log("My plugin is unloading. Until next time!");
    }
    
    public void OnUpdate(float deltaTime)
    {
        // Called every server tick
    }
}
```

## Plugin Host API

The `IPluginHost` interface provides everything you need:

### Logging

```csharp
_host.Log("Information message");
_host.LogWarning("Something needs attention");
_host.LogError("Something went wrong");
```

### Console Commands

Register custom commands that server administrators can use:

```csharp
_host.RegisterCommand("mycommand", "Description", args => {
    _host.Log($"Command executed with {args.Length} arguments");
});

// Or use the attribute approach
[Command("hello", "Greets the server")]
public void HelloCommand(string[] args)
{
    _host.Log("Hello from the plugin!");
}
```

### Player Management

```csharp
// Get all online players
foreach (var player in _host.GetOnlinePlayers())
{
    _host.Log($"Player: {player.Name} (Ping: {player.Latency}ms)");
}

// Get specific player
var player = _host.GetPlayer(peerId);
```

### Messaging

```csharp
// Send to everyone
_host.BroadcastMessage(messageType, data);

// Send to specific player
_host.SendToPlayer(peerId, messageType, data);

// Handle incoming messages
_host.RegisterMessageHandler(100, (peerId, data) => {
    _host.Log($"Received custom message from player {peerId}");
});
```

### Task Scheduling

```csharp
// Run once after delay
_host.ScheduleTask(5.0f, () => {
    _host.Log("This runs after 5 seconds");
});

// Run repeatedly
_host.ScheduleRepeating(60.0f, () => {
    _host.Log("This runs every minute");
}, out int taskId);

// Cancel a repeating task
_host.CancelTask(taskId);
```

## Event System

Subscribe to game events to react to player actions:

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
    _host.Log($"Welcome, {evt.PlayerName}! We are so glad you could join us.");
}

private void OnPlayerChat(PlayerChatEvent evt)
{
    if (evt.Message.StartsWith("!"))
    {
        // Handle as command
        evt.Cancelled = true; // Prevent message from being broadcast
    }
}
```

### Available Events

| Event | Description |
|-------|-------------|
| `PlayerJoinEvent` | Player connected to server |
| `PlayerLeaveEvent` | Player disconnected |
| `PlayerChatEvent` | Player sent chat message |
| `PlayerMoveEvent` | Player position changed |
| `PlayerDamageEvent` | Player took damage |
| `PlayerDeathEvent` | Player died |
| `AISpawnEvent` | AI entity spawned |
| `AIDeathEvent` | AI entity killed |
| `SceneChangeEvent` | Server changed scene |
| `ExtractStartEvent` | Player started extraction |
| `ExtractCompleteEvent` | Player completed extraction |
| `ItemPickupEvent` | Player picked up item |
| `ItemDropEvent` | Player dropped item |

## Data Storage

Store plugin data in the designated directory:

```csharp
var dataFile = Path.Combine(_host.DataPath, "myplugin_data.json");
var json = File.ReadAllText(dataFile);
```

## Installation

1. Build your plugin project
2. Copy the resulting `.dll` to the server's `Plugins` directory
3. Restart the server
4. Your plugin will be automatically loaded

## Best Practices

1. **Clean up resources** - Always unregister events and cancel tasks in `OnUnload`
2. **Handle exceptions** - Wrap risky operations in try-catch blocks
3. **Be respectful of performance** - Avoid heavy operations in `OnUpdate`
4. **Log meaningfully** - Help server admins understand what your plugin is doing
5. **Think about the players** - Create features that enhance their experience

## Example Plugin

The included `WelcomePlugin` demonstrates:

- Greeting players when they join
- Tracking play time and statistics
- Chat commands (!help, !stats, !online, !time)
- Periodic server announcements
- Custom console commands

We hope this guide helps you create wonderful plugins for your community. Happy coding, and thank you for being part of the Duckov Together family!
