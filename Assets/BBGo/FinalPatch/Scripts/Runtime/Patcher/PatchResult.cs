namespace BBGo.FinalPatch
{
    public enum PatchResult
    {
        None,
        Collected,
        Success,
        Patching,
        Failure_OutOfDate,
        Failure_NotFoundDeployData,
        Failure_NotFoundChannel,
        Failure_NotFoundServerPatchData,
        Failure_BundleDownloadFailure,
    }

}