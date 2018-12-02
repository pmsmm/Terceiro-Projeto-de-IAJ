using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {

        public int DEPTH_LIMIT = 3;

        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            FutureStateWorldModel newState = new FutureStateWorldModel((FutureStateWorldModel)initialPlayoutState);
            Reward reward = new Reward();
            int numberOfIterations = 0;
            while (!newState.IsTerminal() && !(numberOfIterations >= DEPTH_LIMIT) || !newState.IsTerminal() && DEPTH_LIMIT == -1)
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
                numberOfIterations++;
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

            if(manaScore < 0.5f)
            {
                if (action.Name == "GetManaPotion") return 0.7f + 0.3f / action.GetDuration();
                if(action.Name == "SwordAttack") return 0.65f;
            }

            if(hpScore > 0.6f && moneyScore>=0.35f && moneyScore <= 0.65f)
            {
                if (action.Name == "PickUpChest") return 0.7f;
                if (action.Name == "SwordAttack") return 0.6f;
            }

            if (moneyScore >= 0.95f) return 1.1f;

            return timeScore;
        }
    }
}
