using UnityEngine;
using Voxel.Enums;

namespace Voxel
{
    public class CameraGun : MonoBehaviour
    {
        public new Camera camera;

        public VoxelHandler voxelHandler;

        public float explosionSize = 5f;
        
        public void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                Mine();
                // PlantTree();
            }
            if (Input.GetMouseButtonDown(1))
            {
                Build();
            }
        }

        private void Mine()
        {
            RaycastHit hit;
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, 4f))
            {
                return;
            }

            var somewhereInBlock = hit.point + ray.direction.normalized * 0.01f;

            var x = Mathf.FloorToInt(somewhereInBlock.x);
            var y = Mathf.FloorToInt(somewhereInBlock.y);
            var z = Mathf.FloorToInt(somewhereInBlock.z);

            VoxelHandler.instance.SetBlock(x, y, z, Blocks.Air);
        }

        protected void PlantTree()
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 4f))
            {
                return;
            }

            var somewhereBeforeBlock = hit.point - ray.direction.normalized * 0.01f;

            var x = Mathf.FloorToInt(somewhereBeforeBlock.x);
            var z = Mathf.FloorToInt(somewhereBeforeBlock.z);

            VoxelHandler.instance.PlaceTree(x, z, true);
        }

        private void Build()
        {
            RaycastHit hit;
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, 4f))
            {
                return;
            }

            var somewhereBeforeBlock = hit.point - ray.direction.normalized * 0.01f;

            var x = Mathf.FloorToInt(somewhereBeforeBlock.x);
            var y = Mathf.FloorToInt(somewhereBeforeBlock.y);
            var z = Mathf.FloorToInt(somewhereBeforeBlock.z);

            VoxelHandler.instance.SetBlock(x, y, z, Blocks.Wood);
        }
    }
}
