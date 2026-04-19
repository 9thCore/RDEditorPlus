using DG.Tweening;
using RDEditorPlus.Functionality.General;
using RDEditorPlus.Functionality.Mixins;
using RDLevelEditor;
using RhythmWeightlifter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

        public void Setup(PropertyControl_Sound[] sounds)
        {
            foreach (var sound in sounds)
            {
                var inputField = sound.soundInput.filenameInputField;
                inputField.onValueChanged.AddListener(text => OnFilenameChanged(sound, inputField, text));
                inputField.onEndEdit.AddListener(OnEndEdit);
            }
        }

        public void Setup(PropertyControl_SetGameSound[] setGameSounds)
        {
            foreach (var setGameSound in setGameSounds)
            {
                for (int i = 0; i < setGameSound.gameSounds.Length; i++)
                {
                    var index = i;
                    var gameSound = setGameSound.gameSounds[i];
                    var inputField = gameSound.soundInput.filenameInputField;
                    inputField.onValueChanged.AddListener(text => OnFilenameChanged(setGameSound, gameSound, index, inputField, text));
                    inputField.onEndEdit.AddListener(OnEndEdit);
                }
            }
        }

        public void Setup(PropertyControl_SetCountingSound[] countingSounds)
        {
            foreach (var countingSound in countingSounds)
            {
                var inputField = countingSound.soundInput.filenameInputField;
                inputField.onValueChanged.AddListener(text => OnFilenameChanged(countingSound, inputField, text));
                inputField.onEndEdit.AddListener(OnEndEdit);
            }
        }

        PluginConfig.AutocompleteBehaviour IAutocompletionStorage.Behaviour => PluginConfig.AudioOTNAutocompleteBehaviour;
        string IAutocompletionStorage.Identifier => "Audio OTN";
        string IAutocompletionStorage.URL => URL;
        string IAutocompletionStorage.PlayerSuppliedFile => PlayerSuppliedFile;
        string IAutocompletionStorage.TemporaryAutodownloadFile => TemporaryAutodownloadFile;
        int IAutocompletionStorage.RefreshTime => PluginConfig.AudioOTNAutocompleteRefreshTime;

        public const string PlayerSuppliedFile = "audioOneTrueNames.csv";
        public const string TemporaryAutodownloadFile = "auto_audioOneTrueNames.csv";
        public const string URL = "https://docs.google.com/uc?export=download&id=15gkqSAWMgqL0REyjooleWHjngZlEt5tI";

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
                            audio.Add(new AudioData(name, (int)(1000 * offset), (int)(100 * volume)));
                            options.Add(name);
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

        private void OnFilenameChanged(PropertyControl_Sound soundControl, InputField inputField, string text)
            => OnFilenameChanged(new SoundPropertyHandler(soundControl), inputField, text);

        private void OnFilenameChanged(PropertyControl_SetGameSound setGameSoundControl, PropertyControl_GameSound gameSoundControl,
            int index, InputField inputField, string text)
            => OnFilenameChanged(new GameSoundPropertyHandler(setGameSoundControl, index, gameSoundControl), inputField, text);

        private void OnFilenameChanged(PropertyControl_SetCountingSound soundControl, InputField inputField, string text)
            => OnFilenameChanged(new CountingSoundPropertyHandler(soundControl), inputField, text);

        private void OnFilenameChanged(IAudioHandler container, InputField inputField, string text)
        {
            if (text.IsNullOrEmpty() || !inputField.isFocused)
            {
                currentInputField = null;
                currentAudioContainer = null;
                Hide();
                return;
            }

            currentInputField = inputField;
            currentAudioContainer = container;
            Show(inputField.transform.position);
        }

        private void OnEndEdit(string _) => DOVirtual.DelayedCall(0.1f, Hide, ignoreTimeScale: true);
        private void Hide() => autocompleteSystem.Close();
        private void Show(Vector3 position) => autocompleteSystem.Open(position);
        private bool Validator(string text) => text.IndexOf(currentInputField.text, StringComparison.InvariantCultureIgnoreCase) != -1;
        private AudioData GetAudioData(string name) => audio.First(data => data.Name == name);

        private void Start()
        {
            autocompleteSystem = AutocompleteSystem.Setup(nameof(AudioOneTrueNameAutocompletion), options, Validator, name =>
            {
                if (scnEditor.instance.selectedControls.Count == 0)
                {
                    Plugin.LogError("no selected events?? can't continue with audio autocomplete system");
                    return;
                }

                if (currentInputField == null || currentAudioContainer == null)
                {
                    Plugin.LogError("audio autocomplete system used without a proper input field or sound property, discarding");
                    return;
                }

                var data = GetAudioData(name);

                currentAudioContainer.Save(scnEditor.instance.selectedControls[0].levelEvent, data);
                currentInputField.text = data.Name;
            });
        }

        private InputField currentInputField;
        private IAudioHandler currentAudioContainer;
        private AutocompleteSystem autocompleteSystem;

        private readonly List<string> options = [];
        private readonly List<AudioData> audio = [];

        private record AudioData(string Name, int Offset, int Volume);

        private record CountingSoundPropertyHandler(PropertyControl_SetCountingSound PropertyControl)
            : AutoPropertyHandler<PropertyControl_SetCountingSound>(PropertyControl)
        {
            protected override PropertyControl_SoundInput SoundInput => PropertyControl.soundInput;
        }

        private record GameSoundPropertyHandler(PropertyControl_SetGameSound ParentControl, int Index, PropertyControl_GameSound PropertyControl)
             : SimpleHandler<PropertyControl_GameSound>(PropertyControl)
        {
            protected override PropertyControl_SoundInput SoundInput => PropertyControl.soundInput;
            protected override void Save(LevelEvent_Base levelEvent) => ParentControl.Save(levelEvent);
        }

        private record SoundPropertyHandler(PropertyControl_Sound PropertyControl) : AutoPropertyHandler<PropertyControl_Sound>(PropertyControl)
        {
            protected override PropertyControl_SoundInput SoundInput => PropertyControl.soundInput;
        }

        private abstract record AutoPropertyHandler<PropertyControlType>(PropertyControlType PropertyControl)
            : SimpleHandler<PropertyControlType>(PropertyControl)
            where PropertyControlType : PropertyControl
        {
            protected override void Save(LevelEvent_Base levelEvent) => PropertyControl.Save(levelEvent);
        }

        private abstract record SimpleHandler<PropertyControlType>(PropertyControlType PropertyControl) : IAudioHandler
        {
            void IAudioHandler.Save(LevelEvent_Base levelEvent, AudioData data)
            {
                var lastSoundInput = PropertyControl_SoundInput.currentSoundInput;

                PropertyControl_SoundInput.currentSoundInput = SoundInput;
                Update(SoundInput.soundSettingsPopup, SoundInput.soundData, data);
                Save(levelEvent);

                PropertyControl_SoundInput.currentSoundInput = lastSoundInput;
            }

            protected abstract PropertyControl_SoundInput SoundInput { get; }
            protected abstract void Save(LevelEvent_Base levelEvent);
        }

        private interface IAudioHandler
        {
            void Save(LevelEvent_Base levelEvent, AudioData data);
        }

        private static void Update(SoundSettingsPopup soundSettingsPopup, SoundDataStruct soundData, AudioData data)
        {
            soundSettingsPopup.UpdateData(soundData);
            soundSettingsPopup.volumeInputField.text = data.Volume.ToString();
            // soundSettingsPopup.audioOffsetInputField.text = data.Offset.ToString(); // seems to be ignored for internal names anyway?
        }

        private static AudioOneTrueNameAutocompletion instance;
    }
}
