using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    //very simple (and unefficient) implementation of the open/closed sets
    public class DictionaryList : IClosedSet
    {
        private Dictionary<NodeRecord, NodeRecord> NodeRecords { get; set; }

        public DictionaryList()
        {
            this.NodeRecords = new Dictionary<NodeRecord, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }

        public int CountOpen()
        {
            return this.NodeRecords.Count;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord, nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord);
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            //here I cannot use the == comparer because the nodeRecord will likely be a different computational object
            //and therefore pointer comparison will not work, we need to use Equals
            //LINQ with a lambda expression
            if (this.NodeRecords.ContainsKey(nodeRecord)) return this.NodeRecords[nodeRecord];
            return null;
        }

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values;
        }
    }
}
