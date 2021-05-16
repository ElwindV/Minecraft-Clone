using System;

namespace Voxel.JsonObjects
{
    [Serializable]
    public class Block
    {
        public int id;
        public string name;
        public bool transparent;

        public TextureMap textures;
    }
}
