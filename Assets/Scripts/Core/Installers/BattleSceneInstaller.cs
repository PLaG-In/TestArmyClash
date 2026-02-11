using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Main Zenject installer - binds all dependencies for the battle scene
    /// Includes BattleState to avoid circular dependencies
    /// </summary>
    public class BattleSceneInstaller : MonoInstaller
    {
        [Header("Configurations")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private UnitPrefabConfig prefabConfig;

        public override void InstallBindings()
        {
            // Validate configs
            if (gameConfig == null)
            {
                Debug.LogError("[BattleSceneInstaller] GameConfig is not assigned!");
                return;
            }

            if (prefabConfig == null)
            {
                Debug.LogError("[BattleSceneInstaller] UnitPrefabConfig is not assigned!");
                return;
            }

            // Bind configurations
            Container.BindInstance(gameConfig).AsSingle();
            Container.BindInstance(prefabConfig).AsSingle();

            // IMPORTANT: Bind BattleState FIRST (no dependencies)
            Container.BindInterfacesAndSelfTo<BattleState>().AsSingle();

            // Bind factories
            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            // Bind targeting strategy
            Container.Bind<ITargetingStrategy>().To<NearestTargetingStrategy>().AsSingle();

            // Bind controllers (they depend on BattleState, not BattleManager)
            Container.BindInterfacesAndSelfTo<CombatController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MovementController>().AsSingle();

            // Bind main battle manager (depends on BattleState)
            Container.BindInterfacesAndSelfTo<BattleManager>().AsSingle();

            Debug.Log("[BattleSceneInstaller] All dependencies bound (no circular refs)");
        }
    }
}
