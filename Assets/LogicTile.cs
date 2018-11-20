public class LogicTile {


    public enum ActionType {None, Undo, Grow, New };
    public int NumValue = 0;
    
    public int PossibleNumValue = 0;
    public ActionType PossibleAction = ActionType.None;


    public void Finlize() {
        if (PossibleAction == ActionType.Undo) {
            PossibleNumValue = NumValue;
        } else {
            NumValue = PossibleNumValue;
        }

        
        PossibleAction = ActionType.None;
    }

 

}
