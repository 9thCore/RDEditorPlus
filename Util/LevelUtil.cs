using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RDEditorPlus.Util
{
    public static class LevelUtil
    {
        public static bool TryLevelLoad(string path, out List<LevelEvent_Base> events)
        {
            events = [];

            if (!File.Exists(path))
            {
                Plugin.LogError($"Error loading level from {path}, as it does not exist");
                return false;
            }

            try
            {
                var text = RDFile.ReadAllText(path, null);
                var root = Json.Deserialize(text) as Dictionary<string, object>;

                if (root.TryGetValue("events", out var eventObject)
                    && eventObject is List<object> eventList)
                {
                    foreach (var levelEvent in eventList)
                    {
                        if (levelEvent is not Dictionary<string, object> dict)
                        {
                            continue;
                        }

                        string type = dict["type"] as string;
                        string fullType = $"RDLevelEditor.LevelEvent_{type}";

                        Type eventType = GameAssembly.GetType(fullType);
                        if (eventType == null)
                        {
                            continue;
                        }

                        var instance = Activator.CreateInstance(eventType) as LevelEvent_Base;
                        instance.Decode(dict);
                        events.Add(instance);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Error loading level from {path}:\n{ex}");

                return false;
            }
        }

        private static readonly Assembly GameAssembly = typeof(LevelEvent_Base).Assembly;
    }
}
