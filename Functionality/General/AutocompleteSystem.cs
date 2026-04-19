using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.General
{
    public class AutocompleteSystem : MonoBehaviour
    {
        public static AutocompleteSystem Setup(string name, List<string> options, Func<string, bool> validator, UnityAction<string> onClick)
        {
            GameObject clone = GameObject.Instantiate(Template, Template.transform.parent);
            clone.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(AutocompleteSystem)}_{name}";
            return clone.GetComponent<AutocompleteSystem>().With(options, validator, onClick);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Open(Vector3 position)
        {
            gameObject.SetActive(true);

            foreach (Transform option in resultsContainer)
            {
                GameObject.Destroy(option.gameObject);
            }

            int shown = 0;
            foreach (var option in options)
            {
                if (!validator(option))
                {
                    continue;
                }
                shown++;

                GameObject instance = GameObject.Instantiate(resultPrefab, resultsContainer);
                instance.SetActive(true);

                Text text = instance.GetComponentInChildren<Text>();
                Button button = instance.GetComponentInChildren<Button>();

                text.text = option;
                button.onClick.AddListener(() =>
                {
                    Close();
                    onClick(option);
                });
            }

            if (shown == 0)
            {
                Close();
                return;
            }

            RectTransform component = GetComponent<RectTransform>();
            component.position = component.position.WithY(position.y);
            component.anchoredPosition = component.anchoredPosition.WithY(component.anchoredPosition.y - 17f);
            float height = shown * 16f + 4f;
            this.resultsContainer.sizeDelta = new Vector2(resultsContainer.sizeDelta.x, height);
            float clampedHeight = Mathf.Min(height, 100f);
            component.sizeDelta = new Vector2(component.sizeDelta.x, clampedHeight);
        }

        private AutocompleteSystem With(List<string> options, Func<string, bool> validator, UnityAction<string> onClick)
        {
            this.onClick = onClick;
            this.options = options;
            this.validator = validator;
            return this;
        }

        private void CollectFrom(MethodAutocompleteUI instance)
        {
            resultsContainer = instance.resultsContainer;
            resultPrefab = instance.resultPrefab;
        }

        [SerializeField] private RectTransform resultsContainer;
        [SerializeField] private GameObject resultPrefab;
        [SerializeField] private UnityAction<string> onClick;
        [SerializeField] private List<string> options;
        [SerializeField] private Func<string, bool> validator;

        private static GameObject Template
        {
            get
            {
                if (template == null)
                {
                    var vanillaUI = GameObject.FindObjectOfType<MethodAutocompleteUI>(includeInactive: true);

                    template = GameObject.Instantiate(vanillaUI.gameObject, vanillaUI.transform.parent);

                    var autocomplete = template.GetComponent<MethodAutocompleteUI>();
                    var system = template.AddComponent<AutocompleteSystem>();
                    system.CollectFrom(autocomplete);
                    system.Close();
                    system.resultPrefab.SetActive(false);

                    GameObject.DestroyImmediate(autocomplete);
                }

                return template;
            }
        }

        private static GameObject template;
    }
}
