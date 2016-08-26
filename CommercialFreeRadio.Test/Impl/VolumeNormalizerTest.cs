using CommercialFreeRadio.Impl;
using NUnit.Framework;

namespace CommercialFreeRadio.Test.Impl
{
    [TestFixture]
    public class VolumeNormalizerTest
    {
        [Test]
        public void Normalize()
        {
            var volume = 100;
            var normalizer = new VolumeNormalizer(i => volume = i, () => volume);

            normalizer.Normalize(0);
            Assert.AreEqual(100, volume);
            normalizer.Normalize(4);
            Assert.AreEqual(104, volume);
            normalizer.Normalize(0);
            Assert.AreEqual(100, volume);
            normalizer.Normalize(-10);
            Assert.AreEqual(90, volume);
            normalizer.Normalize(0);
            Assert.AreEqual(100, volume);
        }
    }
}
