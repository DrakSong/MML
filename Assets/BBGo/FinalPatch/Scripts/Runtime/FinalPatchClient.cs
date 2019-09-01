using UnityEngine;

namespace BBGo.FinalPatch
{
    public class FinalPatchClient : MonoBehaviour
    {
        public string deployDataUrl = $"http://localhost:8000/{FinalPatchConst.DEPLOY_DATA_FILE_NAME}";
        public string channel;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            FinalPatchData data = Resources.Load<FinalPatchData>("FinalPatchData");
            FinalPool.Init(data);
            FinalLoader.Init(data);
            FinalPatcher.Init(data, deployDataUrl, channel);
        }

        private void Update()
        {
            FinalPool.Update();
        }
    }
}