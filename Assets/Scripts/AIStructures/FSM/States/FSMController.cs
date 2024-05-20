using UnityEngine;


namespace FSMMono
{
    public class FSMController : MonoBehaviour
    {
        [SerializeField] private StateBehavior currentState;
        [SerializeField] private float updateFrequency = 0.1f;
        AIController controller;

        float currentUpdateTime;

        #region MonoBehavior
        private void Awake()
        {
            controller = transform.parent.GetComponent<AIController>();
        }

        // Start is called before the first frame update
        void Start()
        {

            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<StateBehavior>().Controller = controller;
            }
            currentUpdateTime = 0;
            SwitchBehavior(transform.GetChild(0).GetComponent<StateBehavior>());
            InitConditions();

        }

        // Update is called once per frame
        void Update()
        {
            currentUpdateTime -= Time.deltaTime;

            if (currentUpdateTime < 0)
            {
                currentUpdateTime = updateFrequency;

                //Check if condition is valid for each transition and switch the state if valid
                foreach (Transition transition in currentState.Transitions)
                {
                    if (transition.condition.UpdateCondition(updateFrequency, WorldState.Instance)) 
                    {
                        SwitchBehavior(transition.nextState);
                    }
                }
            }

        }
        #endregion

        private void SwitchBehavior(StateBehavior nextState)
        {
            //Disable previous state and enable the new one

            if (currentState)
                currentState.enabled = false;

            currentState = nextState;
            currentState.enabled = true;
            InitConditions();
            Debug.Log("New Strategic State: " + currentState);
        }

        private void InitConditions()
        {
            foreach (Transition transition in currentState.Transitions)
                transition.condition.Init(WorldState.Instance);
        }
    }
}

