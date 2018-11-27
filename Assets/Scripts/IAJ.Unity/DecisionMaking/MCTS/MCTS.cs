using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public enum BestStrategy
    {
        Max,
        Robust,
        MaxRobust,
        Secure
    };

    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int PlayoutIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }

        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }
        private readonly BestStrategy strategy = BestStrategy.Max;

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        private System.Random RandomGenerator { get; set; }
        public AutonomousCharacter autonomousCharacter;
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 1000;
            this.PlayoutIterations = 1;
            this.MaxIterationsProcessedPerFrame = 100;
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
            MCTSNode selectedNode = new MCTSNode(this.CurrentStateWorldModel);
            Reward reward;

            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;

            int MaxIterations = this.MaxIterationsProcessedPerFrame / PlayoutIterations;
            while (this.CurrentIterationsInFrame < MaxIterations)
            {
                MCTSNode newNode = Selection(selectedNode);
                reward = Playout(newNode.State);
                //for (int i = 0; i < this.PlayoutIterations - 1; i++)
                //{
                //    Reward newReward = Playout(newNode.State);
                //    if (newReward.Value > reward.Value) reward = newReward;
                //}
                Backpropagate(newNode, reward);
                this.CurrentIterationsInFrame++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;

            MCTSNode best = BestChild(selectedNode);
            BestActionSequence.Clear();
            GOB.Action bestAction = best != null ? best.Action : null;
            if (bestAction != null) BestActionSequence.Add(bestAction);
            return (bestAction);
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            MCTSNode currentNode = initialNode;
            GOB.Action nextAction = currentNode.State.GetNextAction();

            while (!currentNode.State.IsTerminal())
            {
                if (nextAction != null) return Expand(currentNode, nextAction);
                else
                {
                    MCTSNode newNode = BestUCTChild(currentNode);
                    if (newNode != null) currentNode = newNode;
                    else return currentNode;
                }
            }
            return currentNode;
        }

        virtual protected Reward Playout(WorldModel initialPlayoutState)
        {
            Reward reward = new Reward();
            while (!initialPlayoutState.IsTerminal())
            {
                GOB.Action[] possibleActions = initialPlayoutState.GetExecutableActions();

                int actionIndex = this.RandomGenerator.Next(0, possibleActions.Length);
                GOB.Action chosenAction = possibleActions[actionIndex];
                chosenAction.ApplyActionEffects(initialPlayoutState);
                initialPlayoutState.CalculateNextPlayer();
                reward.Value = initialPlayoutState.GetScore();
                reward.PlayerID = initialPlayoutState.GetNextPlayer();
            }
            return reward;
        }

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
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

        virtual protected MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            FutureStateWorldModel newState = new FutureStateWorldModel((FutureStateWorldModel)parent.State);
            action.ApplyActionEffects(newState);
            newState.CalculateNextPlayer();
            MCTSNode newNode = new MCTSNode(newState);
            newNode.Parent = parent;
            newNode.Q = 0;
            newNode.N = 0;
            newNode.Action = action;
            parent.ChildNodes.Add(newNode);
            return newNode;
        }

        //gets the best child of a node, using the UCT formula
        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0) return null;

            MCTSNode bestChild = node.ChildNodes[0];
            float bestReward = bestChild.Q / bestChild.N;
            for (int i = 1; i < node.ChildNodes.Count; i++)
            {
                float newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N + C * ((float)Math.Log10(node.N) / node.N);
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
            if (node.ChildNodes.Count == 0) return null;

            MCTSNode bestChild = null;
            float bestReward = 0;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                float newReward = 0;
                switch (strategy)
                {
                    case BestStrategy.Max:
                        newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N;
                        break;
                    case BestStrategy.Robust:
                        newReward = node.ChildNodes[i].N;
                        break;
                    case BestStrategy.MaxRobust:
                        newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N + node.ChildNodes[i].N;
                        break;
                    case BestStrategy.Secure:
                        //TODO: !!
                        //newReward = node.ChildNodes[i].Q / node.ChildNodes[i].N - A / Mathf.Sqrt(node.ChildNodes[i].N);
                        break;
                }
                if (newReward > bestReward || bestChild == null)
                {
                    bestChild = node.ChildNodes[i];
                    bestReward = newReward;
                }
            }
            if (bestChild == null || bestChild.Action == null)
            {
                BestActionSequence.Clear();
            }
            return bestChild;
        }
    }
}
