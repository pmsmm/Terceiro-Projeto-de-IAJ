using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.DecisionMakingActions
{
    public class DivineWrath : IAJ.Unity.DecisionMaking.GOB.Action
    {
        public AutonomousCharacter Character { get; set; }

        protected List<GameObject> Enemies { get; set; }

        public DivineWrath(AutonomousCharacter character, List<GameObject> enemies) : base("DivineWrath")
		{
            this.Character = character;
            this.Enemies = enemies;

            this.Utility = 1f;
        }

		public override bool CanExecute()
		{
            if (!base.CanExecute()) return false;
            if (this.Character.GameManager.characterData.Level < 3) return false;
            if (this.Character.GameManager.enemies.Count == 0) return false;
            if (this.Character.GameManager.characterData.HP >= this.Character.GameManager.characterData.MaxHP) return false;
            return this.Character.GameManager.characterData.Mana >= 7;
        }

		public override bool CanExecute(WorldModel worldModel)
		{
            if (!base.CanExecute(worldModel)) return false;

            int level = (int)worldModel.GetProperty(Properties.LEVEL);
            if (level < 3) return false;
            if (Enemies.Count == 0) return false;
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana >= 10;
        }

		public override void Execute()
		{
            base.Execute();
            this.Character.GameManager.DivineWrath();
        }


		public override void ApplyActionEffects(WorldModel worldModel)
		{
            base.ApplyActionEffects(worldModel);
            worldModel.SetProperty(Properties.MANA, (int)worldModel.GetProperty(Properties.MANA) - 10);

            foreach (GameObject enemy in Enemies)
            {
                worldModel.SetProperty(enemy.name, false);
            }
        }

    }
}
