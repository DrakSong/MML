namespace BBGo.FinalPatch
{
    public interface IBuildEditorCallback
    {
        void OnBuildFullFinished(BuildVersion buildVersion);
        void OnBuildPackageFinished(BuildVersion buildVersion);
    }
}