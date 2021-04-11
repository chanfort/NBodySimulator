using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace NBodySimulator
{
    public class ConditionView : MonoBehaviour
    {
        public Text title;

        public InputField numberOfParticles;

        public InputField centerX;
        public InputField centerY;
        public InputField centerZ;

        public InputField radius;

        public InputField shapeElipsoidX;
        public InputField shapeElipsoidY;
        public InputField shapeElipsoidZ;

        public InputField turbulenceStrength;

        public InputField turbulenceElipsoidX;
        public InputField turbulenceElipsoidY;
        public InputField turbulenceElipsoidZ;

        public InputField angularVelocity;
        public InputField seed;
        public Dropdown presets;

        public Text addOrRemoveButton;

        [HideInInspector] public bool isRunning;
        RuntimeCondition condition;
        bool switchAddOrRemove;

        public void Initialize(InitialCondition initialCondition)
        {
            title.text = $"Set {ConditionsController.active.conditionViews.Count}";
            SetInitialConditionUIValues(initialCondition);

            presets.ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData("-"));

            foreach (string key in ConditionsController.active.initialConditionsPresets.Keys)
            {
                options.Add(new Dropdown.OptionData(key));
            }

            presets.AddOptions(options);

            int j = 0;
            int presetValue = 0;

            foreach (string key in ConditionsController.active.initialConditionsPresets.Keys)
            {
                if (AreInitialConditionsEquals(ConditionsController.active.initialConditionsPresets[key], initialCondition))
                {
                    presetValue = j + 1;
                }
                j++;
            }

            // Debug.Log(initialCondition.angularVelocity == ConditionsController.active.initialConditionsPresets["Rotating Disc"].angularVelocity);
            presets.value = presetValue;
        }

        void SetInitialConditionUIValues(InitialCondition initialCondition)
        {
            numberOfParticles.text = initialCondition.numberOfParticles.ToString();

            centerX.text = initialCondition.center.x.ToString();
            centerY.text = initialCondition.center.y.ToString();
            centerZ.text = initialCondition.center.z.ToString();

            radius.text = initialCondition.radius.ToString();

            shapeElipsoidX.text = initialCondition.shapeElipsoid.x.ToString();
            shapeElipsoidY.text = initialCondition.shapeElipsoid.y.ToString();
            shapeElipsoidZ.text = initialCondition.shapeElipsoid.z.ToString();

            turbulenceStrength.text = initialCondition.turbulenceStrength.ToString();

            turbulenceElipsoidX.text = initialCondition.turbulenceElipsoid.x.ToString();
            turbulenceElipsoidY.text = initialCondition.turbulenceElipsoid.y.ToString();
            turbulenceElipsoidZ.text = initialCondition.turbulenceElipsoid.z.ToString();

            angularVelocity.text = initialCondition.angularVelocity.ToString();
            seed.text = initialCondition.seed.ToString();
        }

        void Update()
        {
            if (switchAddOrRemove)
            {
                AddOrRemove();
                switchAddOrRemove = false;
            }
        }

        public void ButtonAddOrRemove()
        {
            switchAddOrRemove = true;
        }

        public void AddOrRemove()
        {
            if (isRunning)
            {
                Remove();
            }
            else
            {
                Add();
            }
        }

        void Add()
        {
            List<InitialCondition> initialConditions = new List<InitialCondition>
        {
            new InitialCondition
            {
                numberOfParticles = InputFieldToInt(numberOfParticles),
                center = new float3(InputFieldToFloat(centerX), InputFieldToFloat(centerY), InputFieldToFloat(centerZ)),
                radius = InputFieldToFloat(radius),
                shapeElipsoid = new float3(InputFieldToFloat(shapeElipsoidX), InputFieldToFloat(shapeElipsoidY), InputFieldToFloat(shapeElipsoidZ)),
                turbulenceStrength = InputFieldToFloat(turbulenceStrength),
                turbulenceElipsoid = new float3(InputFieldToFloat(turbulenceElipsoidX), InputFieldToFloat(turbulenceElipsoidY), InputFieldToFloat(turbulenceElipsoidZ)),
                angularVelocity = InputFieldToFloat(angularVelocity),
                seed = InputFieldToInt(seed)
            }
        };

            Simulation.active.AddInitialConditions(initialConditions);
            condition = Simulation.active.runtimeConditionsSet[Simulation.active.runtimeConditionsSet.Count - 1];

            isRunning = true;
            addOrRemoveButton.text = "Remove";
        }

        void Remove()
        {
            Simulation.active.RemoveRuntimeCondition(condition);
            condition = null;
            isRunning = false;
            addOrRemoveButton.text = "Add";
        }

        public void Close()
        {
            if (isRunning)
            {
                Remove();
            }

            ConditionsController.active.conditionViews.Remove(this);
            Destroy(gameObject);
        }

        public void DropdownUpdate()
        {
            string key = presets.options[presets.value].text;
            if (!ConditionsController.active.initialConditionsPresets.ContainsKey(key))
            {
                return;
            }

            InitialCondition initialCondition = ConditionsController.active.initialConditionsPresets[key];
            SetInitialConditionUIValues(initialCondition);
        }

        int InputFieldToInt(InputField field)
        {
            if (int.TryParse(field.text, out int result))
            {
                return result;
            }

            return 0;
        }

        float InputFieldToFloat(InputField field)
        {
            if (float.TryParse(field.text, out float result))
            {
                return result;
            }

            return 0f;
        }

        bool AreInitialConditionsEquals(InitialCondition conditionA, InitialCondition conditionB)
        {
            return
                conditionA.numberOfParticles == conditionB.numberOfParticles &&

                conditionA.center.x == conditionB.center.x &&
                conditionA.center.y == conditionB.center.y &&
                conditionA.center.z == conditionB.center.z &&

                conditionA.radius == conditionB.radius &&

                conditionA.shapeElipsoid.x == conditionB.shapeElipsoid.x &&
                conditionA.shapeElipsoid.y == conditionB.shapeElipsoid.y &&
                conditionA.shapeElipsoid.z == conditionB.shapeElipsoid.z &&

                conditionA.turbulenceStrength == conditionB.turbulenceStrength &&

                conditionA.turbulenceElipsoid.x == conditionB.turbulenceElipsoid.x &&
                conditionA.turbulenceElipsoid.y == conditionB.turbulenceElipsoid.y &&
                conditionA.turbulenceElipsoid.z == conditionB.turbulenceElipsoid.z &&

                conditionA.angularVelocity == conditionB.angularVelocity &&
                conditionA.seed == conditionB.seed;
        }
    }
}
