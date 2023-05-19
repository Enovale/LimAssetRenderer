namespace AssetRenderer
{
    public class RecordInfo
    {
        public int PersonalityId;
        public float MaxSeconds;
        public bool Gacksung;

        public RecordInfo(int personalityId, float maxSeconds, bool gacksung = true)
        {
            (PersonalityId, MaxSeconds, Gacksung) = (personalityId, maxSeconds, gacksung);
        }
    }
}