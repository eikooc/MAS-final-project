using Common.Interfaces;
using MAClient.Classes.Goals;
using MAClient.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace MAClient.Classes.Entities
{
    // refactor agent, such that all subgoals are replaced with objectives.
    // objectives should now contain multiple subgoals.
    public class Agent : IEntity
    {
        public int col { get; set; }
        public int row { get; set; }
        public int uid { get; set; }

        public string color;
        public Node CurrentBeliefs;
        public Strategy strategy;
        private Objective CurrentObjective;
        public Stack<Objective> objectives;
        public SubGoal CurrentSubgoal { get { return this.CurrentObjective.Current; } }


        public Agent(int x, int y, int id, string color)
        {
            this.col = x;
            this.row = y;
            this.uid = id;
            this.color = color;
            objectives = new Stack<Objective>();
        }

        // use objective's getnextMove
        public Node getNextMove(Node n)
        {
            Node nextMove = null;
            bool running = true;
            while (running)
            {
                if (CurrentObjective == null || CurrentObjective.IsComplete)
                {
                    if (this.objectives.Count > 0)
                    {
                        this.CurrentObjective = this.objectives.Pop();
                    }
                    else
                    {
                        running = false;
                        return Objective.PerformNoOp(this, n);
                    }
                }
                else if (CurrentObjective != null && !CurrentObjective.PlanCompleted)
                {
                    nextMove = CurrentObjective.GetNextMove(this, n);
                    if (nextMove != null && !CurrentObjective.PlanCompleted)
                    {
                        running = false;
                    }
                }
            }

            CurrentBeliefs = nextMove;
            return CurrentBeliefs;
        }

        public void acceptNextMove()
        {
            if (CurrentObjective.acceptNextMove())
            {
                this.ResetBeliefs();
            }
        }

        private void ResetBeliefs()
        {
            CurrentBeliefs.boxList.Entities.Where(x => x.color != this.color).ToList().ForEach(b => CurrentBeliefs.boxList.Remove(b.uid));
            CurrentBeliefs.agentList.Entities.Where(x => x.uid != this.uid).ToList().ForEach(a => CurrentBeliefs.agentList.Remove(a.uid));
        }

        public void backTrack()
        {
            this.CurrentObjective.UndoAction(this.CurrentBeliefs);
            this.CurrentBeliefs = this.CurrentBeliefs.parent;
        }

        public void AddEncounteredObject(IEntity obstacle)
        {
            this.CurrentObjective.AddEncounteredObject(obstacle);
        }

        public bool AcceptObjective(Objective objective) // must be able to reject objectives under special circumstances. 
        {
            this.objectives.Push(objective);
            return true;
        }

        public Node ResolveConflict(Node n)
        {
            CurrentObjective.CreatePlan(this.strategy, this.CurrentBeliefs);
            if (CurrentObjective.HasPlan)
            {
                return this.CurrentObjective.ResolveConflict(this, n);
            }
            else
            {
                return Objective.PerformNoOp(this, n);
            }
        }

        public void UpdateSubgoalStates(Node n)
        {
            this.CurrentObjective?.UpdateSubgoalStates(n);
        }


        public override int GetHashCode()
        {
            return (this.row * Node.MAX_ROW) + this.col;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (!(obj is Agent))
                return false;

            Agent other = (Agent)obj;
            return (this.col == other.col && this.row == other.row && this.uid == other.uid);
        }

        public IEntity Clone()
        {
            Agent clone = new Agent(this.col, this.row, this.uid, this.color);
            clone.CurrentObjective = this.CurrentObjective;
            clone.strategy = this.strategy;
            clone.CurrentBeliefs = this.CurrentBeliefs;

            return clone;
        }
    }
}
