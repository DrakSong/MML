namespace BBGo.FinalPatch
{
    public class DefaultPathConverter : IPathConverter
    {
        public string GetAssetBundleName(string assetPath)
        {
            int lastIndexOfDot = assetPath.LastIndexOf('.');
            if (lastIndexOfDot != -1)
            {
                assetPath = assetPath.Remove(lastIndexOfDot);
            }
            return assetPath.ToLower();
        }
    }
}