using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;

namespace Assets.Scripts.DecisionMakingActions
{
    public class ShieldOfFaith : IAJ.Unity.DecisionMaking.GOB.Action
    {
        public AutonomousCharacter Character { get; set; }

		public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
		{
            this.Character = character;
		}

		public override float GetGoalChange(Goal goal)
		{
            var change = base.GetGoalChange(goal);
            if (goal.Name.Equals(AutonomousCharacter.SURVIVE_GOAL))
            {
                change -= 5;
            }
            return change;
        }

		public override bool CanExecute()
		{
            if (!base.CanExecute()) return false;
            return this.Character.GameManager.characterData.Mana >= 5;
        }

		public override bool CanExecute(WorldModel worldModel)
		{
            if (!base.CanExecute(worldModel)) return false;

            var mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana >= 5;
        }

		public override void Execute()
		{
            base.Execute();
            this.Character.GameManager.ShieldOfFaith();
        }


		public override void ApplyActionEffects(WorldModel worldModel)
		{
            base.ApplyActionEffects(worldModel);
            worldModel.SetProperty(Properties.MANA, (int)worldModel.GetProperty(Properties.MANA) - 5);
            worldModel.SetProperty(Properties.SHIELDHP, 5);

            //disables the target object so that it can't be reused again
            //worldModel.SetProperty(this.Target.name, false);
        }

    }
}
