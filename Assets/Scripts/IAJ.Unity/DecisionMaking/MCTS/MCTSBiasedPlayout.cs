using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            FutureStateWorldModel newState = new FutureStateWorldModel((FutureStateWorldModel)initialPlayoutState);
            Reward reward = new Reward();
            while (!newState.IsTerminal())
            {
                GOB.Action[] possibleActions = newState.GetExecutableActions();
                GOB.Action bestAction = null;
                float chosenScore = 0f;

                //TODO: Add randomness
                //float heuristics[] = new float[];

                for (int i = 0; i < possibleActions.Length; i++)
                {
                    float heuristicValue = Heuristic(newState, possibleActions[i]);
                    if (bestAction == null || heuristicValue > chosenScore)
                    {
                        chosenScore = heuristicValue;
                        bestAction = possibleActions[i];
                    }
                }

                bestAction.ApplyActionEffects(newState);
                reward.Value = chosenScore;
                reward.PlayerID = 0;
            }
            return reward;
        }

        float Heuristic(WorldModel state, GOB.Action action)
        {
            if (action.Name == "DivineWrath") return 1f;
            if (action.Name == "LevelUp") return 1f;

            int money = (int)state.GetProperty(Properties.MONEY);
            int HP = (int)state.GetProperty(Properties.HP);
            int MaxHP = (int)state.GetProperty(Properties.MAXHP);
            float time = (float)state.GetProperty(Properties.TIME);

            float moneyScore = (float)money / 25f;
            float hpScore = (float)HP / (float)MaxHP;
            float timeScore = time / 200f;

            if (hpScore < 0.5f)
            {
                if (action.Name == "LayOnHands") return 1f;
                if (action.Name == "GetHealthPotion") return 0.7f + 0.3f / action.GetDuration();
                if (action.Name == "SwordAttack") return 0.1f;
            }

            if (moneyScore >= 0.95f) return 1.1f;

            return timeScore;
        }
    }
}
