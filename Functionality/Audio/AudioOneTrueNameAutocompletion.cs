using RDEditorPlus.Functionality.Mixins;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RDEditorPlus.Functionality.Audio
{
    public class AudioOneTrueNameAutocompletion : MonoBehaviour, IAutocompletionStorage
    {
        public static AudioOneTrueNameAutocompletion Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(AudioOneTrueNameAutocompletion)}");
                    instance = holder.AddComponent<AudioOneTrueNameAutocompletion>();
                }

                return instance;
            }
        }

        public List<AudioData> Audio { get; init; } = [];

        PluginConfig.AutocompleteBehaviour IAutocompletionStorage.Behaviour => PluginConfig.AudioOTNAutocompleteBehaviour;
        string IAutocompletionStorage.Identifier => "Audio OTN";
        string IAutocompletionStorage.URL => URL;
        string IAutocompletionStorage.PlayerSuppliedFile => PlayerSuppliedFile;
        string IAutocompletionStorage.TemporaryAutodownloadFile => TemporaryAutodownloadFile;
        int IAutocompletionStorage.RefreshTime => PluginConfig.AudioOTNAutocompleteRefreshTime;

        IEnumerator IAutocompletionStorage.Parse(string text)
        {
            // Only parse so much per frame
            const int MaxReadTimeout = 150;
            int timeout = MaxReadTimeout;

            using StringReader stringReader = new(text);

            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                int firstSeparator = line.IndexOf(',');
                if (firstSeparator != -1)
                {
                    int secondSeparator = line.IndexOf(',', firstSeparator + 1);
                    if (secondSeparator != -1)
                    {
                        if (float.TryParse(line.Substring(firstSeparator + 1, secondSeparator - firstSeparator - 1), out var offset)
                            && float.TryParse(line.Substring(secondSeparator + 1), out var volume))
                        {
                            string name = line.Substring(0, firstSeparator);
                            Audio.Add(new AudioData(name, offset, volume));
                        }
                    }
                }

                timeout--;
                if (timeout <= 0)
                {
                    timeout = MaxReadTimeout;
                    yield return null;
                }
            }
        }

        public record AudioData(string Name, float Offset, float Volume);

        public const string PlayerSuppliedFile = "audioOneTrueNames.csv";
        public const string TemporaryAutodownloadFile = "auto_audioOneTrueNames.csv";
        public const string URL = "https://docs.google.com/uc?export=download&id=15gkqSAWMgqL0REyjooleWHjngZlEt5tI";

        private static AudioOneTrueNameAutocompletion instance;
    }
}
