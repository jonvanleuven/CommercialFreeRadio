using System.Collections.Generic;

namespace CommercialFreeRadio
{
    interface IDeviceScanner
    {
        IEnumerable<IPlayer> Scan();
    }
}
