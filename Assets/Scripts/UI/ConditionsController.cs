using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace NBodySimulator
{
    public class ConditionsController : MonoBehaviour
    {
        public static ConditionsController active;

        public GameObject conditionPrefab;
        public Transform conditionInstanceParent;

        [HideInInspector] public List<ConditionView> conditionViews = new List<ConditionView>();
        public Dictionary<string, InitialCondition> initialConditionsPresets = new Dictionary<string, InitialCondition>();

        public GameObject menu;

        void Awake()
        {
            active = this;
            initialConditionsPresets = GetInitialConditionsPresets();
        }

        public void Add()
        {
            Add(initialConditionsPresets["Default"]);
        }

        public void Add(InitialCondition initialCondition)
        {
            GameObject instance = Instantiate(conditionPrefab, conditionInstanceParent);
            instance.SetActive(true);

            ConditionView conditionView = instance.GetComponent<ConditionView>();
            conditionViews.Add(conditionView);
            conditionView.Initialize(initialCondition);
        }

        public void StartAll()
        {
            for (int i = 0; i < conditionViews.Count; i++)
            {
                if (!conditionViews[i].isRunning)
                {
                    conditionViews[i].AddOrRemove();
                }
            }
        }

        public void MenuToggle()
        {
            menu.SetActive(!menu.activeSelf);
        }

        Dictionary<string, InitialCondition> GetInitialConditionsPresets()
        {
            return new Dictionary<string, InitialCondition>
            {
                {
                    "Default",
                    new InitialCondition
                    {
                        numberOfParticles = 1000,
                        center = float3.zero,
                        radius = 30f,
                        shapeElipsoid = new float3(1f, 1f, 1f),
                        turbulenceStrength = 0f,
                        turbulenceElipsoid = new float3(1f, 1f, 1f),
                        angularVelocity = 0.004f,
                        seed = 0
                    }
                },
                {
                    "Rotating Disc",
                    new InitialCondition
                    {
                        numberOfParticles = 3000,
                        center = new float3(0f, -15f, 0f),
                        radius = 30f,
                        shapeElipsoid = new float3(1f, 0.02f, 1f),
                        turbulenceStrength = 0.01f,
                        turbulenceElipsoid = new float3(1f, 1f, 1f),
                        angularVelocity = 0.004f,
                        seed = 1
                    }
                },
                {
                    "Static Disc",
                    new InitialCondition
                    {
                        numberOfParticles = 3000,
                        center = new float3(0f, 15f, 0f),
                        radius = 30f,
                        shapeElipsoid = new float3(1f, 0.005f, 1f),
                        turbulenceStrength = 0f,
                        turbulenceElipsoid = new float3(1f, 1f, 1f),
                        angularVelocity = 0f,
                        seed = 2
                    }
                }
            };
        }
    }
}
