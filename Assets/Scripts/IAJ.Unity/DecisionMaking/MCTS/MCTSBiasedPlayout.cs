using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        //public int DEPTH_LIMIT = -1;
        public int DEPTH_LIMIT = 20;

        public MCTSBiasedPlayout(PropertyArrayWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            FutureStateWorldModel newState = new FutureStateWorldModel((FutureStateWorldModel)initialPlayoutState);
            Reward reward = new Reward();
            int numberOfIterations = 0;
            while (!newState.IsTerminal() && (!(numberOfIterations >= DEPTH_LIMIT) || DEPTH_LIMIT <= 0))
            {
                GOB.Action[] possibleActions = newState.GetExecutableActions();
                List<double> results = new List<double>();
                float chosenScore = 0f;
                int i;
                for (i = 0; i < possibleActions.Length; i++)
                {
                    results.Add(Heuristic(newState, possibleActions[i]));
                }

                GOB.Action bestAction = null;
                List<double> exponentialResults = results.Select(Math.Exp).ToList();
                double sumExponentials = exponentialResults.Sum();
                List<double> softmax = exponentialResults.Select(j => j / sumExponentials).ToList();

                double prob = this.RandomGenerator.NextDouble();
                double probabilitySum = 0;
                for (i = 0; i < possibleActions.Length; i++)
                {
                    probabilitySum += softmax[i];
                    if (probabilitySum >= prob)
                    {
                        bestAction = possibleActions[i];
                        break;
                    }
                }

                bestAction.ApplyActionEffects(newState);
                reward.Value = chosenScore;
                reward.PlayerID = 0;
                if (DEPTH_LIMIT > 0) numberOfIterations++;
            }
            return reward;
        }

        float Heuristic(WorldModel state, GOB.Action action)
        {
            if (action.Name == "LevelUp") return 1f;
            if (action.Name == "DivineWrath") return 1f;    //Precisamos de saber a Target de modo a usar isto de forma inteligente

            int money = (int)state.GetProperty(Properties.MONEY);
            int mana = (int)state.GetProperty(Properties.MANA);
            int HP = (int)state.GetProperty(Properties.HP);
            int MaxHP = (int)state.GetProperty(Properties.MAXHP);
            float time = (float)state.GetProperty(Properties.TIME);

            float moneyScore = (float)money / 25f;
            float manaScore = (float)mana / 10f;
            float hpScore = (float)HP / (float)MaxHP;
            float timeScore = time / 200f;

            if (hpScore < 0.5f)
            {
                if (action.Name == "LayOnHands") return 1f;
                if (action.Name == "GetHealthPotion") return 0.7f + 0.3f / action.GetDuration();
                if (action.Name == "SwordAttack") return 0.1f;
            }

            if (manaScore < 0.5f)
            {
                if (action.Name == "GetManaPotion") return 0.7f + 0.3f / action.GetDuration();
                if (action.Name == "SwordAttack") return 0.65f;
            }

            if (hpScore > 0.6f && moneyScore >= 0.35f && moneyScore <= 0.65f)
            {
                if (action.Name == "PickUpChest") return 0.7f;
                if (action.Name == "SwordAttack") return 0.6f;
            }

            if (moneyScore >= 0.95f) return 1.1f;

            return timeScore;
        }
    }
}
