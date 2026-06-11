using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class YooAssetInitialize
{
    public event Action Initialized;
    private string packageName;
    private ResourcePackage package;

    public void Initialize(string packageName = "DefaultPackage")
    {
        this.packageName = packageName;

        YooAssets.Initialize();
        package = YooAssets.CreatePackage(this.packageName);
        YooAssets.SetDefaultPackage(package);

        InitPackage();
    }

    private void InitPackage()
    {
#if UNITY_EDITOR
        var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
        var packageRoot = buildResult.PackageRootDirectory;
        var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

        var createParameters = new EditorSimulateModeParameters();
        createParameters.EditorFileSystemParameters = fileSystemParams;
#else
        var fileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        var createParameters = new OfflinePlayModeParameters();
        createParameters.BuildinFileSystemParameters = fileSystemParams;
#endif

        var initOperation = package.InitializeAsync(createParameters);
        initOperation.Completed += handle => {
            if (handle.Status == EOperationStatus.Succeed)
            {
                Debug.Log("YooAssets initialize succeed.");
                RequestPackageVersion();
            }
            else
            {
                Debug.LogError($"YooAssets initialize failed. {handle.Error}");
            }
        };
    }

    private void RequestPackageVersion()
    {
        var operation = package.RequestPackageVersionAsync();
        operation.Completed += handle => {
            if (handle.Status == EOperationStatus.Succeed)
            {
                string packageVersion = operation.PackageVersion;
                Debug.Log($"Request package Version : {packageVersion}");
                UpdatePackageManifest(packageVersion);
            }
            else
            {
                Debug.LogError(handle.Error);
            }
        };
    }
    private void UpdatePackageManifest(string packageVersion)
    {
        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        operation.Completed += handle => {
            if (handle.Status == EOperationStatus.Succeed)
            {
                Initialized?.Invoke();
            }
            else
            {
                Debug.LogError(handle.Error);
            }
        };
    }
}
