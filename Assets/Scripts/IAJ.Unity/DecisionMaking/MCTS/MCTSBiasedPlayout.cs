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
            FutureStateWorldModel futureModel = (FutureStateWorldModel)initialPlayoutState;
            Reward reward = new Reward();
            while (!initialPlayoutState.IsTerminal())
            {
                GOB.Action[] possibleActions = initialPlayoutState.GetExecutableActions();
                GOB.Action bestAction = null;
                float bestScore = 0f;

                for (int i = 0; i < possibleActions.Length; i++)
                {
                    FutureStateWorldModel newModel = new FutureStateWorldModel(futureModel);
                    possibleActions[i].ApplyActionEffects(newModel);
                    newModel.CalculateNextPlayer();
                    float newScore = newModel.GetScore();
                    if (bestAction == null || newScore > bestScore)
                    {
                        bestAction = possibleActions[i];
                        bestScore = newScore;
                    }
                }

                bestAction.ApplyActionEffects(initialPlayoutState);
                reward.Value = initialPlayoutState.GetScore();
                reward.PlayerID = initialPlayoutState.GetNextPlayer();
            }
            return reward;
        }

        protected MCTSNode Expand(WorldModel parentState, GOB.Action action)
        {
            FutureStateWorldModel futureModel = (FutureStateWorldModel)parentState;
            action.ApplyActionEffects(futureModel);

            MCTSNode newNode = new MCTSNode(futureModel);
            //TODO: Where to find parent?
            //newNode.Parent = ;
            newNode.Q = 0;
            newNode.N = 0;
            newNode.Action = action;
            //parent.ChildNodes.Add(newNode);
            return newNode;
        }
    }
}
