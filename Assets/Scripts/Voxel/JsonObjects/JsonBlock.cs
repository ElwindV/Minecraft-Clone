using System;

namespace Voxel.JsonObjects
{
    [Serializable]
    public class JsonBlock
    {
        public int id;
        public string name;
        public bool transparent;

        public JsonTextureMap textures;
    }
}
