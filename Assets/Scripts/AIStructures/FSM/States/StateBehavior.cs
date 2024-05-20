using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FSMMono
{
    [System.Serializable]
    public class Transition
    {
        public Condition condition;
        public StateBehavior nextState;
    }

    public abstract class StateBehavior : MonoBehaviour
    {
        [SerializeField] private UnityEvent onEnter;
        [SerializeField] private UnityEvent onExit;
        [SerializeField] private UnityEvent onUpdate;
        [SerializeField] private List<Transition> transitions;

        AIController controller;

        public List<Transition> Transitions { get { return transitions; } }
        public AIController Controller { get { return controller; } set { controller = value; } }


        virtual public void Awake()
        {
            //Duplicate Condition Scriptable Object to make multiple enemies with the same scriptable object
            foreach(Transition transition in transitions)
                transition.condition = Instantiate<Condition>(transition.condition);
        }

        private void OnEnable()
        {
            onEnter?.Invoke();
        }

        private void OnDisable()
        {
            onExit?.Invoke();
        }

        private void Update()
        {
            onUpdate?.Invoke();
        }
    }
}
