using NUnit.Framework;
using CommercialFreeRadio;

namespace CommercialFreeRadio.Test
{
    [TestFixture]
    public class CommandLineArgumentTest
    {
        [Test]
        public void Empty()
        {
            var args = new CommercialFreeRadio.Program.CommandLineArgument(null);

            Assert.IsFalse(args.UseSonosPlayer);
            Assert.IsFalse(args.UseVlcPlayer);
            Assert.IsFalse(args.UseVerbose);
            Assert.IsTrue(args.PrintUsage);
        }

        [Test]
        public void Usage()
        {
            Assert.IsTrue(new CommercialFreeRadio.Program.CommandLineArgument("/?".Split(' ')).PrintUsage);
            Assert.IsTrue(new CommercialFreeRadio.Program.CommandLineArgument("?".Split(' ')).PrintUsage);
            Assert.IsTrue(new CommercialFreeRadio.Program.CommandLineArgument("-?".Split(' ')).PrintUsage);
            Assert.IsFalse(new CommercialFreeRadio.Program.CommandLineArgument("other".Split(' ')).PrintUsage);
        }

        [Test]
        public void Player()
        {
            var args = new CommercialFreeRadio.Program.CommandLineArgument("/sonos".Split( ' ' ) );

            Assert.IsTrue(args.UseSonosPlayer);
            Assert.IsFalse(args.UseVlcPlayer);
        }

        [Test]
        public void Verbose()
        {
            var args = new CommercialFreeRadio.Program.CommandLineArgument("/verbose".Split(' '));

            Assert.IsTrue(args.UseVerbose);
        }

        [Test]
        public void SonisIp()
        {
            Assert.AreEqual("192.168.99.99", new CommercialFreeRadio.Program.CommandLineArgument("/sonos=192.168.99.99".Split(' ')).SonosIp);
        }
    }
}
