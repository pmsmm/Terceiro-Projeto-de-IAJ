using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.GameManager
{
    public class DefinedPropertyWorldModel : FutureStateWorldModel
    {
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int ShieldHP { get; set; }
        public int Mana { get; set; }
        public int XP { get; set; }
        public float Time { get; set; }
        public int Money { get; set; }
        public int Level { get; set; }
        public Vector3 Position { get; set; }

        public float BeQuickGoal { get; set; }
        public float GetRichGoal { get; set; }
        public float GainXPGoal { get; set; }
        public float SurviveGoal { get; set; }


        private Dictionary<string, float> GoalValues { get; set; } 

        public DefinedPropertyWorldModel(GameManager gameManager, List<Action> actions) : base(gameManager, actions)
        {
            this.HP = 10;
            this.MaxHP = 10;
            this.ShieldHP = 0;
            this.Mana = 0;
            this.Money = 0;
            this.Time = 0f;
            this.XP = 0;
            this.Level = 1;
            this.Position = gameManager.character.transform.position;

            this.SurviveGoal = 0f;
            this.GainXPGoal = 0f;
            this.GetRichGoal = 0f;
            this.BeQuickGoal = 0f;
        }

        public DefinedPropertyWorldModel(DefinedPropertyWorldModel parent) : base(parent)
        {
            this.HP = parent.HP;
            this.MaxHP = parent.MaxHP;
            this.ShieldHP = parent.ShieldHP;
            this.Mana = parent.Mana;
            this.Money = parent.Money;
            this.Time = parent.Time;
            this.XP = parent.XP;
            this.Level = parent.Level;
            this.Position = parent.Position;

            this.BeQuickGoal = parent.BeQuickGoal;
            this.GetRichGoal = parent.GetRichGoal;
            this.GainXPGoal = parent.GainXPGoal;
            this.SurviveGoal = parent.SurviveGoal;
        }

        public override object GetProperty(string propertyName)
        {
            if (propertyName.Equals(Properties.HP)) return this.HP;
            if (propertyName.Equals(Properties.MAXHP)) return this.MaxHP;
            if (propertyName.Equals(Properties.SHIELDHP)) return this.ShieldHP;
            if (propertyName.Equals(Properties.MANA)) return this.Mana;
            if (propertyName.Equals(Properties.MONEY)) return this.Money;
            if (propertyName.Equals(Properties.TIME)) return this.Time;
            if (propertyName.Equals(Properties.XP)) return this.XP;
            if (propertyName.Equals(Properties.LEVEL)) return this.Level;
            if (propertyName.Equals(Properties.POSITION)) return this.Position;
            return null;
        }

        public override void SetProperty(string propertyName, object value)
        {
            if (propertyName.Equals(Properties.HP)) this.HP = (int)value;
            if (propertyName.Equals(Properties.MAXHP)) this.MaxHP = (int)value;
            if (propertyName.Equals(Properties.SHIELDHP)) this.ShieldHP = (int)value;
            if (propertyName.Equals(Properties.MANA)) this.Mana = (int)value;
            if (propertyName.Equals(Properties.MONEY)) this.Money = (int)value;
            if (propertyName.Equals(Properties.TIME)) this.Time = (float)value;
            if (propertyName.Equals(Properties.XP)) this.XP = (int)value;
            if (propertyName.Equals(Properties.LEVEL)) this.Level = (int)value;
            if (propertyName.Equals(Properties.POSITION)) this.Position = (Vector3)value;
        }

        public override float GetGoalValue(string goalName)
        {
            if (goalName.Equals(AutonomousCharacter.BE_QUICK_GOAL)) return this.BeQuickGoal;
            if (goalName.Equals(AutonomousCharacter.GET_RICH_GOAL)) return this.GetRichGoal;
            if (goalName.Equals(AutonomousCharacter.GAIN_XP_GOAL)) return this.GainXPGoal;
            if (goalName.Equals(AutonomousCharacter.SURVIVE_GOAL)) return this.SurviveGoal;

            return 0f;
        }

        public override void SetGoalValue(string goalName, float value)
        {
            var limitedValue = value;
            if (value > 10.0f)
            {
                limitedValue = 10.0f;
            }

            else if (value < 0.0f)
            {
                limitedValue = 0.0f;
            }

            if (goalName.Equals(AutonomousCharacter.BE_QUICK_GOAL)) this.BeQuickGoal = limitedValue;
            if (goalName.Equals(AutonomousCharacter.GET_RICH_GOAL)) this.GetRichGoal = limitedValue;
            if (goalName.Equals(AutonomousCharacter.GAIN_XP_GOAL)) this.GainXPGoal = limitedValue;
            if (goalName.Equals(AutonomousCharacter.SURVIVE_GOAL)) this.SurviveGoal = limitedValue;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new DefinedPropertyWorldModel(this);
        }

        public override bool IsTerminal()
        {
            return this.HP <= 0 || this.Time >= 200 || (this.NextPlayer == 0 && this.Money == 25);
        }

        public override float GetScore()
        {
            float moneyScore = (float)this.Money / 25f;
            float hpScore = (float)this.HP / (float)this.MaxHP;
            float timeScore = this.Time / 200f;

            Vector3 result = new Vector3(moneyScore * 0.05f, hpScore * 0.1f, timeScore * 1f);
            return result.sqrMagnitude;
        }

        public override void CalculateNextPlayer()
        {
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool)this.GetProperty(enemy.name);
                if (enemyEnabled && (enemy.transform.position - this.Position).sqrMagnitude <= 100)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.GameManager.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return;
                }
            }
            this.NextPlayer = 0;
        }
    }
}
