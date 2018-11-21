using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;

namespace Assets.Scripts.DecisionMakingActions
{
    public class LayOnHands : IAJ.Unity.DecisionMaking.GOB.Action
    {
        public AutonomousCharacter Character { get; set; }

		public LayOnHands(AutonomousCharacter character) : base("LayOnHands")
		{
            this.Character = character;
		}

        public override bool CanExecute()
		{
            if (!base.CanExecute()) return false;
            if (this.Character.GameManager.characterData.Level < 2) return false;
            if (this.Character.GameManager.characterData.HP >= this.Character.GameManager.characterData.MaxHP) return false;
            return this.Character.GameManager.characterData.Mana >= 7;
        }

		public override bool CanExecute(WorldModel worldModel)
		{
            if (!base.CanExecute(worldModel)) return false;

            int level = (int)worldModel.GetProperty(Properties.LEVEL);
            if (level < 2) return false;
            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);
            if (hp >= maxHp) return false;
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana >= 7;
        }

		public override void Execute()
		{
            base.Execute();
            this.Character.GameManager.LayOnHands();
        }


		public override void ApplyActionEffects(WorldModel worldModel)
		{
            base.ApplyActionEffects(worldModel);
            worldModel.SetProperty(Properties.MANA, (int)worldModel.GetProperty(Properties.MANA) - 7);
            worldModel.SetProperty(Properties.HP, (int)worldModel.GetProperty(Properties.MAXHP));
        }

    }
}
