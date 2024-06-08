namespace XOPS
{
    public class Triangle
    {
        #region .ctor

        public Triangle(int index1, int index2, int index3, int textureId)
        {
            Indexes = new int[] { index1, index2, index3 };
            TextureId = textureId;
        }

        #endregion

        #region Property

        public int[] Indexes { get; private set; }

        public int TextureId { get; private set; }

        #endregion
    }
}
