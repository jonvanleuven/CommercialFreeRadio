using System;
using System.Linq;
using NUnit.Framework;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio.Test.Impl
{
    [TestFixture]
    public class Playlist3FmInterfaceTest
    {
        [Test]
        public void CallWebService()
        {
            var sut = new Station3fm.Playlist3FmInterface();

            var t = sut.ReadCurrentTrack();

            Console.WriteLine(t);
        }

        [Test]
        public void CallWebServiceMultipleTimes()
        {
            var sut = new Station3fm.Playlist3FmInterface();
            foreach (var everySecond in Poller.Create(new TimeSpan(0, 0, 1)).Take(20))
            {
                var t = sut.ReadCurrentTrack();

                Console.WriteLine("{0:dd-MM-yyyy HH:mm:ss} {1} ", DateTime.Now, t);
            }
        }
    }
}
