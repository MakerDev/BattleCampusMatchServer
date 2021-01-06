namespace BattleCampusMatchServer.Models
{
    public class IpPortInfo
    {
        public string IpAddress { get; set; } = null;
        public int DesktopPort { get; set; } = 7777;
        public int WebsocketPort { get; set; } = 7778;
    }
}
