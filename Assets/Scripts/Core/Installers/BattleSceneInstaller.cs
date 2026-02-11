using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Main Zenject installer for battle scene
    /// Movement is now autonomous in UnitView (no MovementController)
    /// </summary>
    public class BattleSceneInstaller : MonoInstaller
    {
        [Header("Configurations")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private UnitPrefabConfig prefabConfig;

        public override void InstallBindings()
        {
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

            // Bind BattleState FIRST (no dependencies)
            Container.BindInterfacesAndSelfTo<BattleState>().AsSingle();

            // Bind factories
            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            // Bind targeting strategy
            Container.Bind<ITargetingStrategy>().To<NearestTargetingStrategy>().AsSingle();

            // Bind combat controller (only targeting, no attacks/movement)
            Container.BindInterfacesAndSelfTo<CombatController>().AsSingle();

            // NO MovementController - movement is autonomous in UnitView!

            // Bind battle manager
            Container.BindInterfacesAndSelfTo<BattleManager>().AsSingle();

            Debug.Log("[BattleSceneInstaller] All dependencies bound successfully");
        }
    }
}
