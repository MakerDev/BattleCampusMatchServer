namespace BattleCampusMatchServer.Models
{
    public class IpPortInfo
    {
        public string IpAddress { get; set; } = "127.0.0.1";
        public int DesktopPort { get; set; } = 7777;
        public int WebsocketPort { get; set; } = 7778;
    }
}
