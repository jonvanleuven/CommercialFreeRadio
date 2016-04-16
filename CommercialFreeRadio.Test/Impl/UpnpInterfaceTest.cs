using System;
using CommercialFreeRadio.Impl;
using NUnit.Framework;

namespace CommercialFreeRadio.Test.Impl
{
    [TestFixture]
    public class UpnpInterfaceTest
    {
        [Test]
        public void GetMediaInfo()
        {
            var i = CreateInterface();

            var r = i.GetMediaInfo();

            Console.WriteLine(r.CurrentURI);
            Console.WriteLine(r.CurrentURIMetaData);
        }

        [Test]
        public void SetAVTransportURI()
        {
            var i = CreateInterface();
            //i.SetAVTransportURI("x-rincon-mp3radio://stream01.sublimefm.nl/SublimeFM_mp3");
            i.SetAVTransportURI("x-sonosapi-stream:s25777?sid=254&flags=8224&sn=0", @"<DIDL-Lite xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:upnp=""urn:schemas-upnp-org:metadata-1-0/upnp/"" xmlns:r=""urn:schemas-rinconnetworks-com:metadata-1-0/"" xmlns=""urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/""><item id=""-1"" parentID=""-1"" restricted=""true""><dc:title>Sublime FM</dc:title><upnp:class>object.item.audioItem.audioBroadcast</upnp:class><desc id=""cdudn"" nameSpace=""urn:schemas-rinconnetworks-com:metadata-1-0/"">SA_RINCON65031_</desc></item></DIDL-Lite>");
        }

        private static UpnpInterface CreateInterface()
        {
            return new UpnpInterface("192.168.1.8");
        }
    }
}
