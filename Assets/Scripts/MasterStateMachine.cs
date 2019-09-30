using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterStateMachine : MonoBehaviour {

    public enum State {
        PLACING_TILES = 0,
        RUN_GAME = 1,
        numberOfStates = 2
    }

    //create an array of lists, one list per state to store functions to call on state changes
    private List<System.Action>[] stateEnterFunctions = new List<System.Action>[(int)State.numberOfStates];
    private List<System.Action>[] stateExitFunctions = new List<System.Action>[(int)State.numberOfStates];

    private State currentState = State.numberOfStates; //irrational state to catch the switch to START

    void Start() {
        Application.targetFrameRate = 60;

        // populate state list array with new lists
        for (int i = 0; i < (int)State.numberOfStates; i++) {
            stateEnterFunctions[i] = new List<System.Action>();
        }
        for (int i = 0; i < (int)State.numberOfStates; i++) {
            stateExitFunctions[i] = new List<System.Action>();
        }
        InstigateStateChange(0); //Begin!
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void InstigateStateChange(State s) {
        for (int i = 0; i < stateExitFunctions[(int)s].Count; i++) {
            stateExitFunctions[(int)currentState][i]();
        }
        currentState = s;
        for (int i = 0; i < stateEnterFunctions[(int)s].Count; i++) {
            stateEnterFunctions[(int)currentState][i]();
        }
    }

    public void AddStateEnterListener(State s, System.Action a) {
        if (s == currentState) {
            a();
        }
        stateEnterFunctions[(int)s].Add(() => a());
    }

    public void RemoveStateEnterListener(State s, System.Action a) {
        stateEnterFunctions[(int)s].Remove(() => a());
    }

    public void AddStateExitListener(State s, System.Action a) {
        stateExitFunctions[(int)s].Add(() => a());
    }

    public void RemoveStateExitListener(State s, System.Action a) {
        stateExitFunctions[(int)s].Remove(() => a());
    }

    public void NextState() {
        InstigateStateChange((State)(currentState + 1));
    }
}