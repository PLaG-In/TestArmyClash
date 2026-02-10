using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Main Zenject installer - binds all dependencies for the game
    /// Install this on a SceneContext in your battle scene
    /// </summary>
    public class BattleSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameConfig _gameConfig;

        public override void InstallBindings()
        {
            // Bind configuration
            Container.BindInstance(_gameConfig).AsSingle();

            // Bind factories
            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            // Bind targeting strategy (can be swapped)
            Container.Bind<ITargetingStrategy>().To<NearestTargetingStrategy>().AsSingle();

            // Bind controllers
            Container.BindInterfacesAndSelfTo<CombatController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MovementController>().AsSingle();

            // Bind main manager
            Container.BindInterfacesAndSelfTo<BattleManager>().AsSingle();
        }
    }
}
