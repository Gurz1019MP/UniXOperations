namespace XOPS
{
    public class Block
    {
        #region .ctor

        public Block(short id, float[] floatInfo, int[] intInfo)
        {
            Id = id;
            FloatInfo = floatInfo;
            IntInfo = intInfo;
        }

        #endregion

        #region Property

        public short Id { get; set; }

        public float[] FloatInfo { get; private set; }

        public int[] IntInfo { get; private set; }

        #endregion
    }
}
