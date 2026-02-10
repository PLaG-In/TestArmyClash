using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ArmyClash.UI
{
    /// <summary>
    /// Main menu UI controller
    /// Handles army randomization preview and battle start
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _randomizeButton;
        [SerializeField] private Button _quitButton;
        
        [Header("Army Preview")]
        [SerializeField] private TextMeshProUGUI _team1PreviewText;
        [SerializeField] private TextMeshProUGUI _team2PreviewText;
        [SerializeField] private GameObject _previewPanel;
        
        [Header("Settings")]
        [SerializeField] private Slider _unitsPerTeamSlider;
        [SerializeField] private TextMeshProUGUI _unitsCountText;
        [SerializeField] private Toggle _showFormationsToggle;
        
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _titleBackground;

        private Core.BattleManager _battleManager;
        private bool _armiesGenerated = false;

        [Inject]
        public void Construct(Core.BattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        private void Start()
        {
            SetupButtons();
            SetupSettings();
            UpdateTitle();
        }

        private void SetupButtons()
        {
            if (_startBattleButton != null)
            {
                _startBattleButton.onClick.AddListener(OnStartBattle);
                _startBattleButton.interactable = false; // Disabled until armies generated
            }

            if (_randomizeButton != null)
            {
                _randomizeButton.onClick.AddListener(OnRandomizeArmies);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuit);
            }
        }

        private void SetupSettings()
        {
            if (_unitsPerTeamSlider != null)
            {
                _unitsPerTeamSlider.minValue = 5;
                _unitsPerTeamSlider.maxValue = 50;
                _unitsPerTeamSlider.value = 20;
                _unitsPerTeamSlider.onValueChanged.AddListener(OnUnitsCountChanged);
                UpdateUnitsCountText();
            }
        }

        private void UpdateTitle()
        {
            if (_titleText != null)
            {
                _titleText.text = "ARMY CLASH";
            }
        }

        private void OnStartBattle()
        {
            if (!_armiesGenerated)
            {
                Debug.LogWarning("Generate armies first!");
                return;
            }

            // Hide main menu
            gameObject.SetActive(false);
            
            // BattleManager will handle the actual battle start
            // This is triggered by UIController
        }

        private void OnRandomizeArmies()
        {
            _battleManager.RandomizeArmies();
            _armiesGenerated = true;
            
            if (_startBattleButton != null)
            {
                _startBattleButton.interactable = true;
            }

            UpdateArmyPreviews();
        }

        private void UpdateArmyPreviews()
        {
            if (_previewPanel != null)
            {
                _previewPanel.SetActive(true);
            }

            // Get army compositions
            var team1Units = _battleManager.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _battleManager.GetUnitsOfTeam(Core.Team.Team2);

            if (_team1PreviewText != null)
            {
                _team1PreviewText.text = GetArmyCompositionText(team1Units, "Team 1");
            }

            if (_team2PreviewText != null)
            {
                _team2PreviewText.text = GetArmyCompositionText(team2Units, "Team 2");
            }
        }

        private string GetArmyCompositionText(System.Collections.Generic.List<Core.Unit> units, string teamName)
        {
            if (units == null || units.Count == 0)
                return $"{teamName}: Not Generated";

            int cubes = 0, spheres = 0;
            int blues = 0, greens = 0, reds = 0;
            int small = 0, big = 0;
            float totalHP = 0, totalATK = 0;

            foreach (var unit in units)
            {
                if (unit.Shape == Core.UnitShape.Cube) cubes++;
                else spheres++;

                if (unit.Color == Core.UnitColor.Blue) blues++;
                else if (unit.Color == Core.UnitColor.Green) greens++;
                else reds++;

                if (unit.Size == Core.UnitSize.Small) small++;
                else big++;

                totalHP += unit.Stats.HP;
                totalATK += unit.Stats.ATK;
            }

            return $"<b>{teamName}</b>\n" +
                   $"Units: {units.Count}\n" +
                   $"Shapes: {cubes} Cubes, {spheres} Spheres\n" +
                   $"Colors: {blues} Blue, {greens} Green, {reds} Red\n" +
                   $"Sizes: {small} Small, {big} Big\n" +
                   $"Total HP: {totalHP:F0}\n" +
                   $"Total ATK: {totalATK:F0}";
        }

        private void OnUnitsCountChanged(float value)
        {
            UpdateUnitsCountText();
            _armiesGenerated = false;
            
            if (_startBattleButton != null)
            {
                _startBattleButton.interactable = false;
            }
        }

        private void UpdateUnitsCountText()
        {
            if (_unitsCountText != null && _unitsPerTeamSlider != null)
            {
                int count = (int)_unitsPerTeamSlider.value;
                _unitsCountText.text = $"Units per team: {count}";
            }
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_startBattleButton != null)
                _startBattleButton.onClick.RemoveListener(OnStartBattle);
            
            if (_randomizeButton != null)
                _randomizeButton.onClick.RemoveListener(OnRandomizeArmies);
            
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
            
            if (_unitsPerTeamSlider != null)
                _unitsPerTeamSlider.onValueChanged.RemoveListener(OnUnitsCountChanged);
        }
    }
}
