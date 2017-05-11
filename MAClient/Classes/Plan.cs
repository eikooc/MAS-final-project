using Common.Interfaces;
using MAClient.Classes.Entities;
using System.Collections.Generic;

namespace MAClient.Classes
{
    public class Plan
    {
        public Stack<Node> path;

        public Plan(Node n)
        {
            List<Node> estimatedPlan = new List<Node>();
            while (!n.isInitialState())
            {
                estimatedPlan.Insert(0, n);
                n = n.parent;
            }
            estimatedPlan.Reverse();
            this.path = new Stack<Node>(estimatedPlan);
        }

        public bool Completed { get { return this.path.Count == 0; } }

        public Node GetNextAction()
        {
            if (!this.Completed)
            {
                return this.path.Pop();
            }
            return null;
        }

        public void UndoAction(Node action)
        {
            this.path.Push(action);
        }

        public List<IEntity> ExtractUsedFields()
        {
            List<IEntity> usedFields = new List<IEntity>();
            // extract agents used fields according to its plan
            foreach (Node node in this.path)
            {
                usedFields.Add(new Position(node.agentCol, node.agentRow));
                foreach (Box box in node.boxList.Entities)
                {
                    usedFields.Add(new Position(box.col, box.row));
                }
            }

            return usedFields;
        }

    }
}
