using System;

namespace CommercialFreeRadio.Impl
{
    public class VolumeNormalizer
    {
        private readonly Action<int> setVolume;
        private readonly Func<int> getVolume;
        private int currentLevel = 0;

        public VolumeNormalizer(Action<int> setVolume, Func<int> getVolume)
        {
            this.setVolume = setVolume;
            this.getVolume = getVolume;
        }

        public void Normalize(int level)
        {
            if ((level - currentLevel) != 0)
                setVolume(getVolume() + (level - currentLevel));
            currentLevel = level;
        }
    }
}
