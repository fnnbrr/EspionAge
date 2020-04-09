namespace NPCs
{
    public class MazeNurse : BasicNurse
    {
        protected override void SetPatrolling()
        {
            base.SetPatrolling();
            
            questionMark.SetActive(true);
        }
    }
}
