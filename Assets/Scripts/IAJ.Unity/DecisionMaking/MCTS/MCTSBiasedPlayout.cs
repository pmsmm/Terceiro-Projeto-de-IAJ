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
        public int DEPTH_LIMIT = 6;

        public MCTSBiasedPlayout(PropertyArrayWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            FutureStateWorldModel newState = new FutureStateWorldModel((FutureStateWorldModel)initialPlayoutState);
            Reward reward = new Reward();
            int numberOfIterations = 0;
            while (!newState.IsTerminal() && (DEPTH_LIMIT <= 0 || !(numberOfIterations >= DEPTH_LIMIT)))
            {
                GOB.Action[] possibleActions = newState.GetExecutableActions();
                List<double> results = new List<double>();
                float chosenScore = 0f;
                int i;
                for (i = 0; i < possibleActions.Length; i++)
                {
                    //results.Add(Heuristic(newState, possibleActions[i]));
                    results.Add(possibleActions[i].GetUtility());
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
                        chosenScore = (float)softmax[i];
                        break;
                    }
                }

                bestAction.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();
                reward.Value = chosenScore;
                reward.PlayerID = 0;
                if (DEPTH_LIMIT > 0) numberOfIterations++;
            }
            return reward;
        }

        float Heuristic(WorldModel state, GOB.Action action)
        {
            if (action.Name.Contains("LevelUp")) return 1f;
            if (action.Name.Contains("DivineWrath")) return 1f;
            if (action.Name.Contains("DivineSmite")) return 0.95f;
            if (action.Name.Contains("ShieldOfFaith")) return 0.9f;

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
                if (action.Name.Contains("LayOnHands")) return 1f;
                if (action.Name.Contains("GetHealthPotion")) return 0.7f + 0.3f / (action.GetDuration() + 1f);
                if (action.Name.Contains("SwordAttack")) return 0.01f;
            }

            if (manaScore < 0.5f)
            {
                if (action.Name.Contains("GetManaPotion")) return 0.7f + 0.3f / (action.GetDuration() + 1f);
            }

            float duration = action.GetDuration();
            if (duration < 2f)
            {
                if (action.Name.Contains("SwordAttack"))
                {
                    return 0.3f;
                }
                else
                {
                    return 0.8f;
                }
            }

            return timeScore;
        }
    }
}
