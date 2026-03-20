This is a directory filled with .dll files, used as reference in compilation.  

For obvious reasons, these will not be pushed. Instead, here is the list of dll files required for compiling, *most* of which can be found in `[Game Root]/Rhythm Doctor_Data/Managed`:  
- Assembly-CSharp_publicized.dll __(*)__
- Assembly-CSharp-firstpass_publicized.dll __(*)__
- DOTween.dll
- MultiWindow.dll
- RDTools.dll
- System.Collections.Immutable.dll
- Unity.TextMeshPro.dll
- UnityEngine.UI.dll
- UnityFileDialog.dll

(*) These files **cannot** be found natively. They have to be generated from the game's assemblies through the [BepInEx-Publicizer](https://github.com/elliotttate/Bepinex-Tools/releases/tag/1.0.1-Publicizer) tool. This is done such that accessing private fields and methods is as simple as accessing them directly, instead of going through reflection or Harmony's AccessTools.