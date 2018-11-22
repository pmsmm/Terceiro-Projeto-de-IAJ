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
            Reward reward = new Reward();
            while (!initialPlayoutState.IsTerminal())
            {
                GOB.Action[] possibleActions = initialPlayoutState.GetExecutableActions();
                GOB.Action bestAction = possibleActions[0];
                WorldModel model = initialPlayoutState.GenerateChildWorldModel();
                bestAction.ApplyActionEffects(model);
                float bestScore = model.GetScore();

                for (int i = 1; i < possibleActions.Length; i++)
                {
                    WorldModel newModel = initialPlayoutState.GenerateChildWorldModel();
                    possibleActions[i].ApplyActionEffects(newModel);
                    float newScore = newModel.GetScore();
                    if (newScore > bestScore)
                    {
                        bestAction = possibleActions[i];
                        bestScore = newScore;
                    }
                }

                bestAction.ApplyActionEffects(initialPlayoutState);
                reward.Value = initialPlayoutState.GetScore();
                reward.PlayerID = 0;
            }
            return reward;
        }

        protected MCTSNode Expand(WorldModel parentState, GOB.Action action)
        {
            //TODO: implement
            throw new NotImplementedException();
        }
    }
}
