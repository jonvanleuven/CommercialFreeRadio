using System.Collections.Generic;

namespace CommercialFreeRadio.Impl
{
    public class DeviceScanner : IDeviceScanner
    {
        public IEnumerable<IPlayer> Scan()
        {
            return new[] //TODO implement network scan
            {
                new SonosPlayer(new UpnpInterface("192.168.1.16"), "Woonkamer")
            };
        }
    }
}
