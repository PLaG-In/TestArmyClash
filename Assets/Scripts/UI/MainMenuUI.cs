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
        [Header("Buttons")]
        [SerializeField] private Button startBattleButton;
        [SerializeField] private Button randomizeButton;
        [SerializeField] private Button quitButton;
        
        [Header("Army Preview")]
        [SerializeField] private TextMeshProUGUI team1PreviewText;
        [SerializeField] private TextMeshProUGUI team2PreviewText;
        [SerializeField] private GameObject previewPanel;
        
        [Header("Settings")]
        [SerializeField] private Slider unitsPerTeamSlider;
        [SerializeField] private TextMeshProUGUI unitsCountText;
        [SerializeField] private Toggle showFormationsToggle;
        
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image titleBackground;

        [Header("BattleUI")]
        [SerializeField] private GameObject battleUI;

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
            if (startBattleButton != null)
            {
                startBattleButton.onClick.AddListener(OnStartBattle);
                startBattleButton.interactable = false; // Disabled until armies generated
            }

            if (randomizeButton != null)
            {
                randomizeButton.onClick.AddListener(OnRandomizeArmies);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }
        }

        private void SetupSettings()
        {
            if (unitsPerTeamSlider != null)
            {
                unitsPerTeamSlider.minValue = 5;
                unitsPerTeamSlider.maxValue = 50;
                unitsPerTeamSlider.value = 20;
                unitsPerTeamSlider.onValueChanged.AddListener(OnUnitsCountChanged);
                UpdateUnitsCountText();
            }
        }

        private void UpdateTitle()
        {
            if (titleText != null)
            {
                titleText.text = "ARMY CLASH";
            }
        }

        private void OnStartBattle()
        {
            if (!_armiesGenerated)
            {
                Debug.LogWarning("Generate armies first!");
                return;
            }

            gameObject.SetActive(false);
            battleUI.SetActive(true);
            _battleManager.StartBattle();
        }

        private void OnRandomizeArmies()
        {
            _battleManager.RandomizeArmies();
            _armiesGenerated = true;
            
            if (startBattleButton != null)
            {
                startBattleButton.interactable = true;
            }

            UpdateArmyPreviews();
        }

        private void UpdateArmyPreviews()
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(true);
            }

            // Get army compositions
            var team1Units = _battleManager.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _battleManager.GetUnitsOfTeam(Core.Team.Team2);

            if (team1PreviewText != null)
            {
                team1PreviewText.text = GetArmyCompositionText(team1Units, "Team 1");
            }

            if (team2PreviewText != null)
            {
                team2PreviewText.text = GetArmyCompositionText(team2Units, "Team 2");
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
            
            if (startBattleButton != null)
            {
                startBattleButton.interactable = false;
            }
        }

        private void UpdateUnitsCountText()
        {
            if (unitsCountText != null && unitsPerTeamSlider != null)
            {
                int count = (int)unitsPerTeamSlider.value;
                unitsCountText.text = $"Units per team: {count}";
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
            if (startBattleButton != null)
                startBattleButton.onClick.RemoveListener(OnStartBattle);
            
            if (randomizeButton != null)
                randomizeButton.onClick.RemoveListener(OnRandomizeArmies);
            
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuit);
            
            if (unitsPerTeamSlider != null)
                unitsPerTeamSlider.onValueChanged.RemoveListener(OnUnitsCountChanged);
        }
    }
}
