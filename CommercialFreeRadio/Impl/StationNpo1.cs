using System;

namespace CommercialFreeRadio.Impl
{
    public class StationNpo1 : IRadioStation
    {
        public string Name { get { return "NPO 1"; } }
        public string Uri { get { return "http://level3-eu.npo.offload.streamzilla.xlcdn.com/sz/npomcdn/nginx/live/ZLxnLPNg/live/npo/tvlive/ned1/ned1.isml/ned1-audio=128000-video=1800000.m3u8"; } }
        public bool? IsPlayingCommercialBreak()
        {
            throw new NotImplementedException();
        }
    }
}
