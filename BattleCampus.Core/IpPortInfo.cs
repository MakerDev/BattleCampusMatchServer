namespace BattleCampus.Core
{
    public class IpPortInfo
    {
        public string IpAddress { get; set; } = "127.0.0.1";
        public int DesktopPort { get; set; } = 7777;
        public int WebsocketPort { get; set; } = 7778;

        public override string ToString()
        {
            return $"{IpAddress}:{DesktopPort}-{WebsocketPort}";
        }

        public override bool Equals(object obj)
        {
            if (obj is not IpPortInfo ipPortInfo)
            {
                return false;
            }

            return ipPortInfo.IpAddress == IpAddress
                && ipPortInfo.DesktopPort == DesktopPort
                && ipPortInfo.WebsocketPort == WebsocketPort;
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
