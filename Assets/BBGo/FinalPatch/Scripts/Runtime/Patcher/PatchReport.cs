using System.Collections.Generic;

namespace BBGo.FinalPatch
{
    public class PatchReport
    {
        public PatchResult Result { get; set; }
        public List<BundleData> PatchBundles { get; set; }
        public List<BundleData> DeleteBundles { get; set; }
        public long TotalSize { get; set; }
        public long PatchedSize { get; set; }
        public PatchData ServerPatchData { get; set; }
        public ClientData ClientData { get; set; }
        public ChannelData ChannelData { get; set; }
    }
}