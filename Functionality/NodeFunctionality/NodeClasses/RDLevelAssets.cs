using System.Collections.Generic;
using System.IO;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelAssets(RDLevelAssets.Asset[] assets)
    {
        public RDLevelAssets() : this(null) { }

        public List<Asset> Collect() => this;

        public readonly Asset[] Assets = assets;

        public static implicit operator List<Asset>(RDLevelAssets instance)
            => instance.Assets != null ? [.. instance.Assets] : [];

        public static implicit operator RDLevelAssets(List<Asset> assets) => new(assets.ToArray());

        public static RDLevelAssets FromFiles(string prefix, string[] files)
        {
            Asset[] assets = new Asset[files.Length];

            int index = 0;
            foreach (var file in files)
            {
                assets[index++] = new(prefix, file);
            }

            return new(assets);
        }

        public readonly record struct Asset(string Directory, string Filename)
        {
            public void CopyTo(string otherDirectory)
            {
                string fullInitialPath = Path.Combine(Directory, Filename);
                string fullResultPath = Path.Combine(otherDirectory, Filename);

                if (!File.Exists(fullInitialPath))
                {
                    Plugin.LogWarn($"Tried to copy '{fullInitialPath}' to '{fullResultPath}', but the former does not exist");
                    return;
                }

                File.Copy(fullInitialPath, fullResultPath, overwrite: true);
            }

            public bool SameFinalAssetAs(in Asset other) => Filename == other.Filename;
        }
    }
}
