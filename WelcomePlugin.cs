using System.Text;
using DuckovTogether.Plugins;

namespace ExamplePlugin;

[PluginInfo(
    Name = "Welcome Plugin",
    Version = "1.0.0",
    Author = "Duckov Team",
    Description = "A warm welcome plugin that greets players and provides helpful server utilities."
)]
public class WelcomePlugin : IPlugin
{
    public string Name => "Welcome Plugin";
    public string Version => "1.0.0";
    public string Author => "Duckov Team";
    public string Description => "A warm welcome plugin that greets players and provides helpful server utilities.";
    
    private IPluginHost _host = null!;
    private readonly Dictionary<int, DateTime> _playerJoinTimes = new();
    private readonly Dictionary<int, int> _playerKillCounts = new();
    private int _totalPlayersServed;
    private int _announcementTaskId;
    
    public void OnLoad(IPluginHost host)
    {
        _host = host;
        
        PluginEventDispatcher.Register<PlayerJoinEvent>(OnPlayerJoin);
        PluginEventDispatcher.Register<PlayerLeaveEvent>(OnPlayerLeave);
        PluginEventDispatcher.Register<PlayerDeathEvent>(OnPlayerDeath);
        PluginEventDispatcher.Register<PlayerChatEvent>(OnPlayerChat);
        
        _host.ScheduleRepeating(300f, AnnouncementTick, out _announcementTaskId);
        
        _host.Log("Welcome Plugin loaded successfully!");
        _host.Log("Thank you for choosing our server. We hope you have a wonderful experience!");
    }
    
    public void OnUnload()
    {
        PluginEventDispatcher.Unregister<PlayerJoinEvent>(OnPlayerJoin);
        PluginEventDispatcher.Unregister<PlayerLeaveEvent>(OnPlayerLeave);
        PluginEventDispatcher.Unregister<PlayerDeathEvent>(OnPlayerDeath);
        PluginEventDispatcher.Unregister<PlayerChatEvent>(OnPlayerChat);
        
        _host.CancelTask(_announcementTaskId);
        
        _host.Log("Welcome Plugin unloaded. See you next time!");
    }
    
    public void OnUpdate(float deltaTime)
    {
    }
    
    private void OnPlayerJoin(PlayerJoinEvent evt)
    {
        _playerJoinTimes[evt.PeerId] = DateTime.Now;
        _playerKillCounts[evt.PeerId] = 0;
        _totalPlayersServed++;
        
        var onlineCount = _host.GetOnlinePlayers().Count();
        
        _host.Log($"Welcome, {evt.PlayerName}! We are delighted to have you with us.");
        _host.Log($"Current players online: {onlineCount}");
        
        BroadcastToAll($"[Server] Welcome {evt.PlayerName} to the server! Let's make this raid memorable.");
    }
    
    private void OnPlayerLeave(PlayerLeaveEvent evt)
    {
        if (_playerJoinTimes.TryGetValue(evt.PeerId, out var joinTime))
        {
            var playTime = DateTime.Now - joinTime;
            _host.Log($"Farewell, {evt.PlayerName}. You played for {playTime.TotalMinutes:F1} minutes.");
            _host.Log($"We hope to see you again soon. Take care out there!");
            
            _playerJoinTimes.Remove(evt.PeerId);
        }
        
        if (_playerKillCounts.TryGetValue(evt.PeerId, out var kills))
        {
            if (kills > 0)
            {
                _host.Log($"{evt.PlayerName} leaves with {kills} confirmed eliminations. Impressive work!");
            }
            _playerKillCounts.Remove(evt.PeerId);
        }
        
        BroadcastToAll($"[Server] {evt.PlayerName} has left the server. Safe travels, friend.");
    }
    
    private void OnPlayerDeath(PlayerDeathEvent evt)
    {
        var victim = _host.GetPlayer(evt.VictimPeerId);
        var killer = evt.KillerPeerId > 0 ? _host.GetPlayer(evt.KillerPeerId) : null;
        
        if (killer != null && evt.KillerPeerId != evt.VictimPeerId)
        {
            if (_playerKillCounts.ContainsKey(evt.KillerPeerId))
                _playerKillCounts[evt.KillerPeerId]++;
            
            var killCount = _playerKillCounts.GetValueOrDefault(evt.KillerPeerId, 1);
            
            if (killCount == 5)
            {
                BroadcastToAll($"[Server] {killer.Name} is on fire! 5 eliminations and counting!");
            }
            else if (killCount == 10)
            {
                BroadcastToAll($"[Server] {killer.Name} is unstoppable! A true warrior with 10 eliminations!");
            }
        }
    }
    
    private void OnPlayerChat(PlayerChatEvent evt)
    {
        var msg = evt.Message.ToLower();
        
        if (msg == "!help")
        {
            SendToPlayer(evt.PeerId, "Available commands:");
            SendToPlayer(evt.PeerId, "  !help - Show this message");
            SendToPlayer(evt.PeerId, "  !stats - View your statistics");
            SendToPlayer(evt.PeerId, "  !online - See who is online");
            SendToPlayer(evt.PeerId, "  !time - Check your play time");
            evt.Cancelled = true;
        }
        else if (msg == "!stats")
        {
            var kills = _playerKillCounts.GetValueOrDefault(evt.PeerId, 0);
            SendToPlayer(evt.PeerId, $"Your Statistics:");
            SendToPlayer(evt.PeerId, $"  Eliminations: {kills}");
            evt.Cancelled = true;
        }
        else if (msg == "!online")
        {
            var players = _host.GetOnlinePlayers().ToList();
            SendToPlayer(evt.PeerId, $"Players Online ({players.Count}):");
            foreach (var p in players)
            {
                var latency = p.Latency > 0 ? $" ({p.Latency}ms)" : "";
                SendToPlayer(evt.PeerId, $"  - {p.Name}{latency}");
            }
            evt.Cancelled = true;
        }
        else if (msg == "!time")
        {
            if (_playerJoinTimes.TryGetValue(evt.PeerId, out var joinTime))
            {
                var playTime = DateTime.Now - joinTime;
                SendToPlayer(evt.PeerId, $"You have been playing for {playTime.TotalMinutes:F1} minutes.");
                SendToPlayer(evt.PeerId, "Thank you for spending your time with us!");
            }
            evt.Cancelled = true;
        }
    }
    
    private void AnnouncementTick()
    {
        var messages = new[]
        {
            "Remember to stay hydrated and take breaks. Your health matters to us!",
            "Working together is the key to survival. Watch each other's backs out there.",
            "Found good loot? Consider sharing with teammates who need it more.",
            "Having a great time? Tell your friends about our server!",
            "Respect your fellow players. We are all here to have fun together."
        };
        
        var msg = messages[new Random().Next(messages.Length)];
        BroadcastToAll($"[Tip] {msg}");
    }
    
    private void BroadcastToAll(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        _host.BroadcastMessage(4, data);
    }
    
    private void SendToPlayer(int peerId, string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        _host.SendToPlayer(peerId, 4, data);
    }
    
    [Command("welcome", "Show welcome message to all players")]
    public void WelcomeCommand(string[] args)
    {
        BroadcastToAll("[Server] Welcome to Duckov Together! We are thrilled to have each and every one of you here.");
        _host.Log("Sent welcome message to all players.");
    }
    
    [Command("serverstats", "Display server statistics")]
    public void ServerStatsCommand(string[] args)
    {
        var onlineCount = _host.GetOnlinePlayers().Count();
        _host.Log($"Server Statistics:");
        _host.Log($"  Total players served: {_totalPlayersServed}");
        _host.Log($"  Currently online: {onlineCount}");
        _host.Log($"  Active sessions: {_playerJoinTimes.Count}");
    }
    
    [Command("broadcast", "Send a message to all players")]
    public void BroadcastCommand(string[] args)
    {
        if (args.Length == 0)
        {
            _host.Log("Usage: broadcast <message>");
            return;
        }
        
        var message = string.Join(" ", args);
        BroadcastToAll($"[Announcement] {message}");
        _host.Log($"Broadcast sent: {message}");
    }
}
