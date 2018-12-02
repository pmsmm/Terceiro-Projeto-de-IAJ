using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts;
using UnityEngine;
using Assets.Scripts.DecisionMakingActions;

namespace Assets.Scripts.GameManager
{
    public class PropertyArrayWorldModel : FutureStateWorldModel
    {
        private Dictionary<string, object> AllProperties { get; set; }

        private Dictionary<string, float> GoalValues { get; set; } 

        public PropertyArrayWorldModel(GameManager gameManager, List<Action> actions) : base(gameManager, actions)
        {
            this.AllProperties = new Dictionary<string, object> {
                                                                { Properties.HP, 10 },
                                                                { Properties.MAXHP, 10 },
                                                                { Properties.SHIELDHP, 0 },
                                                                { Properties.MANA, 0 },
                                                                { Properties.MONEY, 0 },
                                                                { Properties.TIME, 0f },
                                                                { Properties.XP, 0 },
                                                                { Properties.LEVEL, 1 },
                                                                { Properties.POSITION, gameManager.character.transform.position },
                                                                { "Skeleton1", true },
                                                                { "Skeleton2", true },
                                                                { "Skeleton3", true },
                                                                { "Skeleton4", true },
                                                                { "Skeleton5", true },
                                                                { "Skeleton6", true },
                                                                { "Skeleton7", true },
                                                                { "Orc1", true },
                                                                { "Orc2", true },
                                                                { "Dragon", true },
                                                                { "ManaPotion1", true },
                                                                { "ManaPotion2", true },
                                                                { "HealthPotion1", true },
                                                                { "HealthPotion2", true },
                                                                { "Chest1", true },
                                                                { "Chest2", true },
                                                                { "Chest3", true },
                                                                { "Chest4", true },
                                                                { "Chest5", true },
                                                                };
            this.GoalValues = new Dictionary<string, float>     {
                                                                {AutonomousCharacter.BE_QUICK_GOAL, 0f },
                                                                {AutonomousCharacter.GET_RICH_GOAL, 0f },
                                                                {AutonomousCharacter.GAIN_XP_GOAL, 0f },
                                                                {AutonomousCharacter.SURVIVE_GOAL, 0f },
                                                                };
        }

        public PropertyArrayWorldModel(PropertyArrayWorldModel parent) : base(parent)
        {
            this.AllProperties = parent.AllProperties;
            this.GoalValues = parent.GoalValues;
        }

        public void Initialize()
        {
            this.ActionEnumerator.Reset();
        }

        public override object GetProperty(string propertyIndex)
        {
            if (!this.AllProperties.ContainsKey(propertyIndex)) return null;

            return this.AllProperties[propertyIndex];
        }

        public override void SetProperty(string propertyIndex, object value)
        {
            if (!this.AllProperties.ContainsKey(propertyIndex)) return;
            this.AllProperties[propertyIndex] = value;
        }

        public override float GetGoalValue(string goalName)
        {
            if (!this.GoalValues.ContainsKey(goalName)) return 0f;

            return this.GoalValues[goalName];
        }

        public override void SetGoalValue(string goalName, float value)
        {
            if (!this.GoalValues.ContainsKey(goalName)) return;

            var limitedValue = value;
            if (value > 10.0f) limitedValue = 10.0f;
            else if (value < 0.0f) limitedValue = 0.0f;
            this.GoalValues[goalName] = limitedValue;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new PropertyArrayWorldModel(this);
        }

        public override bool IsTerminal()
        {
            return (int)this.AllProperties[Properties.HP] <= 0 || 
                    (float)this.AllProperties[Properties.TIME] >= 200 || 
                    (this.NextPlayer == 0 && (int)this.AllProperties[Properties.MONEY] == 25);
        }

        public override float GetScore()
        {
            float moneyScore = (float)this.AllProperties[Properties.MONEY] / 25f;
            float hpScore = (float)this.AllProperties[Properties.HP] / (float)this.AllProperties[Properties.MAXHP];
            float timeScore = (float)this.AllProperties[Properties.TIME] / 200f;

            Vector3 result = new Vector3(moneyScore * 0.05f, hpScore * 0.1f, timeScore * 1f);
            return result.sqrMagnitude;
        }

        public override void CalculateNextPlayer()
        {
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool)this.AllProperties[enemy.name];
                if (enemyEnabled && (enemy.transform.position - (Vector3)this.AllProperties[Properties.POSITION]).sqrMagnitude <= 100)
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
