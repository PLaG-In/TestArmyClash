using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ArmyClash.Core.Formations
{
    /// <summary>
    /// UI component for selecting army formations
    /// Displays available formations and allows user to choose
    /// </summary>
    public class FormationSelector : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Dropdown _formationDropdown;
        [SerializeField] private TMP_Dropdown _formationDropdownTMP;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _applyButton;
        
        [Header("Preview")]
        [SerializeField] private GameObject _previewPanel;
        [SerializeField] private Transform _previewContainer;
        [SerializeField] private GameObject _previewDotPrefab;

        private readonly List<IFormation> _availableFormations = new List<IFormation>();
        private IFormation _selectedFormation;
        private Team _targetTeam;

        private void Awake()
        {
            InitializeFormations();
            SetupUI();
        }

        private void InitializeFormations()
        {
            _availableFormations.Clear();
            _availableFormations.Add(new OffensiveLineFormation());
            _availableFormations.Add(new DefensiveSquareFormation());
            _availableFormations.Add(new WedgeFormation());

            _selectedFormation = _availableFormations[0];
        }

        private void SetupUI()
        {
            // Setup dropdown (regular or TextMeshPro version)
            if (_formationDropdownTMP != null)
            {
                _formationDropdownTMP.ClearOptions();
                List<string> options = new List<string>();
                foreach (var formation in _availableFormations)
                {
                    options.Add(formation.Name);
                }
                _formationDropdownTMP.AddOptions(options);
                _formationDropdownTMP.onValueChanged.AddListener(OnFormationChanged);
            }
            else if (_formationDropdown != null)
            {
                _formationDropdown.ClearOptions();
                List<string> options = new List<string>();
                foreach (var formation in _availableFormations)
                {
                    options.Add(formation.Name);
                }
                _formationDropdown.AddOptions(options);
                _formationDropdown.onValueChanged.AddListener(OnFormationChanged);
            }

            // Setup apply button
            if (_applyButton != null)
            {
                _applyButton.onClick.AddListener(OnApplyFormation);
            }

            UpdateDescription();
        }

        private void OnFormationChanged(int index)
        {
            if (index >= 0 && index < _availableFormations.Count)
            {
                _selectedFormation = _availableFormations[index];
                UpdateDescription();
                UpdatePreview();
            }
        }

        private void UpdateDescription()
        {
            if (_descriptionText != null && _selectedFormation != null)
            {
                UnitStats bonus = _selectedFormation.GetFormationBonus();
                _descriptionText.text = $"<b>{_selectedFormation.Name}</b>\n\n" +
                                       $"{_selectedFormation.Description}\n\n" +
                                       $"<b>Bonuses:</b>\n" +
                                       FormatStatChange("HP", bonus.HP) +
                                       FormatStatChange("ATK", bonus.ATK) +
                                       FormatStatChange("Speed", bonus.Speed) +
                                       FormatStatChange("AtkSpd", bonus.AtkSpeed);
            }
        }

        private string FormatStatChange(string statName, float value)
        {
            if (value == 0) return "";
            
            string color = value > 0 ? "#00FF00" : "#FF0000";
            string sign = value > 0 ? "+" : "";
            return $"<color={color}>{sign}{value:F0} {statName}</color>\n";
        }

        private void UpdatePreview()
        {
            if (_previewPanel == null || _previewContainer == null || _previewDotPrefab == null)
                return;

            // Clear existing preview
            foreach (Transform child in _previewContainer)
            {
                Destroy(child.gameObject);
            }

            // Create preview dots
            int previewUnitCount = 20; // Preview with 20 units
            for (int i = 0; i < previewUnitCount; i++)
            {
                Vector3 position = _selectedFormation.CalculatePosition(i, previewUnitCount, Vector3.zero);
                
                GameObject dot = Instantiate(_previewDotPrefab, _previewContainer);
                RectTransform rectTransform = dot.GetComponent<RectTransform>();
                
                if (rectTransform != null)
                {
                    // Scale and position for 2D preview
                    float scale = 10f;
                    rectTransform.anchoredPosition = new Vector2(position.x * scale, position.y * scale);
                }
            }
        }

        private void OnApplyFormation()
        {
            // This will be called by the parent UI controller
            Debug.Log($"Applied formation: {_selectedFormation.Name}");
        }

        public IFormation GetSelectedFormation()
        {
            return _selectedFormation;
        }

        public void SetTargetTeam(Team team)
        {
            _targetTeam = team;
        }

        public string[] GetFormationNames()
        {
            string[] names = new string[_availableFormations.Count];
            for (int i = 0; i < _availableFormations.Count; i++)
            {
                names[i] = _availableFormations[i].Name;
            }
            return names;
        }

        private void OnDestroy()
        {
            if (_formationDropdownTMP != null)
                _formationDropdownTMP.onValueChanged.RemoveListener(OnFormationChanged);
            
            if (_formationDropdown != null)
                _formationDropdown.onValueChanged.RemoveListener(OnFormationChanged);
            
            if (_applyButton != null)
                _applyButton.onClick.RemoveListener(OnApplyFormation);
        }
    }
}
