using RDEditorPlus.Functionality.ArbitraryPanel;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.LevelOptions.Mods
{
    public class ModPanelHolder : ArbitraryPanelHolder
    {
        private static ModPanelHolder instance;
        public static ModPanelHolder Instance
        {
            get
            {
                if (instance == null || !instance.Valid())
                {
                    instance = new();
                }

                return instance;
            }
        }

        public override void OnHide()
        {

        }

        public override void OnShow()
        {
            scrollbar.value = 1f;
            UpdateUI();
        }

        private ModPanelHolder()
        {
            const float FirstColumnX = 0f;
            const float SecondColumnX = 160f;
            const float StartYPosition = 8f;
            const float YDistance = 14f;
            const float SizeToStartScrollFrom = 152f;

            title.text = "Level Mod Selector";
            title.color = Color.red;

            string path = FileUtil.GetFilePathFromAssembly(ModListFile);
            if (!File.Exists(path))
            {
                mods = DefaultMods;
            }
            else
            {
                try
                {
                    mods = [.. File.ReadAllLines(path).Where(line => !line.IsNullOrEmpty())];
                }
                catch (Exception exception)
                {
                    Plugin.LogError($"Tried to read from {path}, but was met with {exception} instead. Using default mod list.");
                    mods = DefaultMods;
                }
            }

            toggles = new Toggle[mods.Length];

            UnityUtil.CreateScrollView(rectTransform, out var scrollRect, out var contentRT);
            scrollbar = scrollRect.verticalScrollbar;

            bool firstColumn = true;
            float yPosition = StartYPosition - YDistance;
            int index = 0;
            foreach (var mod in mods)
            {
                if (firstColumn)
                {
                    yPosition += YDistance;
                }

                CreateModToggle(contentRT, index, mod, firstColumn ? FirstColumnX : SecondColumnX, yPosition);

                firstColumn = !firstColumn;
                index++;
            }

            float size = Math.Max(0f, yPosition - SizeToStartScrollFrom);
            contentRT.SizeDeltaY(size);
        }

        private void UpdateUI()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                var mod = mods[i].ToString();
                var active = LevelMods.Contains(mod);
                toggles[i].SetIsOnWithoutNotify(active);
            }
        }

        private void CreateModToggle(Transform parent, int index, string mod, float xPosition, float yPosition)
        {
            GameObject textGO = new($"text_{mod}");
            textGO.AddComponent<EightSidedOutline>().effectColor = Color.black;

            var text = textGO.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleLeft;
            text.text = mod.ToString();
            text.ApplyRDFont();

            var textRT = textGO.transform as RectTransform;
            textRT.SetParent(parent, worldPositionStays: false);
            textRT.anchorMin = new Vector2(0f, 1f);
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(xPosition + 24f, -5f);
            textRT.offsetMax = new Vector2(xPosition + 24f, 5f);
            textRT.AnchorPosY(-yPosition);

            GameObject toggleGO = new($"toggle_{mod}");

            var toggleImage = toggleGO.AddComponent<Image>();
            toggleImage.sprite = AssetUtil.ButtonSprite;
            toggleImage.type = Image.Type.Tiled;

            var toggleRT = toggleGO.transform as RectTransform;
            toggleRT.SetParent(parent, worldPositionStays: false);
            toggleRT.anchorMin = new Vector2(0f, 1f);
            toggleRT.anchorMax = new Vector2(0f, 1f);
            toggleRT.offsetMin = new Vector2(xPosition + 10f, -5f);
            toggleRT.offsetMax = new Vector2(xPosition + 20f, 5f);
            toggleRT.AnchorPosY(-yPosition);

            GameObject checkmarkGO = new("checkmark");

            var checkmarkImage = checkmarkGO.AddComponent<Image>();
            checkmarkImage.sprite = AssetUtil.CheckmarkSprite;
            checkmarkImage.color = Color.black;

            var checkmarkRT = checkmarkGO.transform as RectTransform;
            checkmarkRT.SetParent(toggleRT, worldPositionStays: false);
            checkmarkRT.anchorMin = Vector2.zero;
            checkmarkRT.anchorMax = Vector2.one;
            checkmarkRT.offsetMin = Vector2.zero;
            checkmarkRT.offsetMax = Vector2.zero;

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.graphic = checkmarkImage;

            toggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    LevelMods = [.. LevelMods, mod];
                }
                else
                {
                    LevelMods = [.. LevelMods.Where(val => val != mod)];
                }
            });

            toggles[index] = toggle;
        }

        private readonly Scrollbar scrollbar;
        private readonly Toggle[] toggles;
        private readonly string[] mods;

        private static ref string[] LevelMods => ref scnEditor.instance.levelSettings.mods;

        private static string[] DefaultMods => ["bombBeats", "cpuIsP2On2P", "startImmediately", "booleansDefaultToTrue",
            "classicHitParticles", "legacyTaggedEvents", "playerDrives7thBeat", "runTaggedEventsWhileScrubbing",
            "noShadowRowWeighting", "oldVFXEasing", "adaptRowsToRoomHeight", "bossLevel", "showHitStripOnlyOnActiveBeats"];

        public const string ModListFile = "mods.txt";
    }
}
