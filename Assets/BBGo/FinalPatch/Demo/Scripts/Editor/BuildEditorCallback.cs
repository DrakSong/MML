using System.IO;
using BBGo.FinalPatch;

namespace BBGo.FinalPatch
{
    public class BuildEditorCallback : IBuildEditorCallback
    {
        public void OnBuildFullFinished(BuildVersion buildVersion)
        {
        }

        public void OnBuildPackageFinished(BuildVersion buildVersion)
        {
            string packageDir = $"{FinalPatchConst.ROOT_PATH}/{buildVersion.name}/{FinalPatchConst.PACKAGE_DIRECTORY_NAME}/{buildVersion.version}";
            string toDir = $"CDN/{FinalPatchConst.ROOT_PATH}/{buildVersion.name}/{buildVersion.version}";
            foreach (var file in Directory.GetFiles(packageDir, "*", SearchOption.AllDirectories))
            {
                string fileName = file.Remove(0, packageDir.Length + 1);
                string toPath = $"{toDir}/{fileName}";
                string directory = Path.GetDirectoryName(toPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(file, toPath, true);
            }
        }
    }
}