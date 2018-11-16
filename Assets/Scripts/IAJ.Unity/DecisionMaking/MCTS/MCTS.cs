using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }

        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        private System.Random RandomGenerator { get; set; }
        
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 100;
            this.MaxIterationsProcessedPerFrame = 10;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();
        }

        public GOB.Action Run()
        {
            MCTSNode selectedNode = new MCTSNode(this.CurrentStateWorldModel.GenerateChildWorldModel());
            Reward reward;

            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;

            while (this.CurrentIterationsInFrame < this.MaxIterationsProcessedPerFrame)
            {
                MCTSNode newNode = Selection(selectedNode);
                reward = Playout(newNode.State);
                Backpropagate(newNode, reward);
                this.CurrentIterationsInFrame++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;

            return BestUCTChild(selectedNode).Action;
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            MCTSNode currentNode = initialNode;
            GOB.Action nextAction = currentNode.State.GetNextAction();

            while (!currentNode.State.IsTerminal())
            {
                if (nextAction != null) return Expand(currentNode, nextAction);
                else currentNode = BestChild(currentNode);
            }
            return currentNode;
        }

        private Reward Playout(WorldModel initialPlayoutState)
        {
            Reward reward = new Reward();
            while (initialPlayoutState.IsTerminal())
            {
                GOB.Action[] possibleActions = initialPlayoutState.GetExecutableActions();

                //TODO: Verify
                if (possibleActions.Length == 0) return reward;

                int actionIndex = this.RandomGenerator.Next(0, possibleActions.Length);
                GOB.Action chosenAction = possibleActions[actionIndex];
                chosenAction.ApplyActionEffects(initialPlayoutState);
                reward.Value = initialPlayoutState.GetScore();
                reward.PlayerID = 0;
            }
            return reward;
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
            while (node != null)
            {
                node.N += 1;
                if (node.Parent != null)
                {
                    if (node.Parent.PlayerID == reward.PlayerID) node.Q += reward.Value;
                    else node.Q -= reward.Value;
                }
                node = node.Parent;
            }
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            action.ApplyActionEffects(parent.State);
            MCTSNode newNode = new MCTSNode(parent.State);
            newNode.Parent = parent;
            newNode.Q = 0;
            newNode.N = 0;
            newNode.Action = action;
            parent.ChildNodes.Add(newNode);
            return newNode;
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)
        {
            //TODO: Pick best based on heuristics (visitado mais vezes)
            MCTSNode bestChild = node.ChildNodes[0];
            float bestReward = bestChild.Q / bestChild.N;
            for (int i = 1; i < node.ChildNodes.Count; i++)
            {
                float newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N;
                if (newReward > bestReward)
                {
                    bestChild = node.ChildNodes[i];
                    bestReward = newReward;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode bestChild = node.ChildNodes[0];
            float bestReward = bestChild.Q / bestChild.N;
            for (int i = 1; i < node.ChildNodes.Count; i++)
            {
                //TODO: Improve
                float newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N;
                if (newReward > bestReward)
                {
                    bestChild = node.ChildNodes[i];
                    bestReward = newReward;
                }
            }
            return bestChild;
        }
    }
}
