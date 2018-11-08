using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;

namespace Assets.Scripts.DecisionMakingActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private int xpChange;

		public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite",character,target)
		{
            if (target.tag.Equals("Skeleton")) this.xpChange = 5;
            else if (target.tag.Equals("Orc")) this.xpChange = 10;
            else if (target.tag.Equals("Dragon")) this.xpChange = 15;
        }

		public override float GetGoalChange(Goal goal)
		{
            var change = base.GetGoalChange(goal);
            if (goal.Name.Equals(AutonomousCharacter.GAIN_XP_GOAL))
            {
                change -= this.xpChange;
            }
            return change;
		}

		public override bool CanExecute()
		{
            if (!base.CanExecute()) return false;
            return (this.Target.tag.Equals("Skeleton") && this.Character.GameManager.characterData.Mana >= 2);
		}

		public override bool CanExecute(WorldModel worldModel)
		{
            if (!base.CanExecute(worldModel)) return false;
            if (!this.Target.tag.Equals("Skeleton")) return false;

            var mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana >= 2;
        }

		public override void Execute()
		{
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
		}


		public override void ApplyActionEffects(WorldModel worldModel)
		{
            base.ApplyActionEffects(worldModel);
            worldModel.SetProperty(Properties.MANA, (int)worldModel.GetProperty(Properties.MANA) - 2);
            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

    }
}
