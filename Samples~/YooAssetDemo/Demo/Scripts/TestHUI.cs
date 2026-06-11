using System;
using HUI;
using HUI.Extension;
using UnityEngine;


public class TestHUI : MonoBehaviour
{
    private bool useQueue;
    private int queueId;
    private bool insert = false;

    private TestParameter parameter = new();

    private void Start()
    {
        var initialize = new YooAssetInitialize();
        initialize.Initialize();

        var loader = new YooAssetsUILoader();
        var root = GameObject.Find("UIRoot");

        UIKit.Initialized += () => {
            UIKit.Manager.Events.OnChanged += (ui) => {
                Debug.Log("GlobalUIEvent " + ui.Name + " " + ui.State);
            };
        };

        UIKit.Initialize(loader, root);
    }


    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            useQueue = GUILayout.Toggle(useQueue, "Queue", GUILayout.Width(100), GUILayout.Height(50));
            insert = GUILayout.Toggle(insert, "Insert", GUILayout.Width(100), GUILayout.Height(50));
            queueId = int.Parse(GUILayout.TextField(queueId.ToString(), GUILayout.Width(100), GUILayout.Width(100), GUILayout.Height(50)));

            if (insert)
            {
                useQueue = true;
            }

            if (GUILayout.Button("Pause", GUILayout.Width(100), GUILayout.Height(50)))
            {
                UIKit.Manager.Queue.Pause(queueId);
            }
            if (GUILayout.Button("Resume", GUILayout.Width(100), GUILayout.Height(50)))
            {
                UIKit.Manager.Queue.Resume(queueId);
            }
            if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.Height(50)))
            {
                UIKit.Manager.Queue.Clear(queueId);
            }
            if (GUILayout.Button("ClearAll", GUILayout.Width(100), GUILayout.Height(50)))
            {
                UIKit.Manager.Queue.ClearAll();
            }
        }


        if (GUILayout.Button("closeAll", GUILayout.Width(100), GUILayout.Height(50)))
            UIKit.Manager.CloseAllUI(p => true, true);

        for (int i = 1; i <= 2; i++)
        {
            var name = "TestUI" + i;
            Type type = Type.GetType(name);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"open{i}", GUILayout.Width(100), GUILayout.Height(50)))
                {

                    parameter = new TestParameter();
                    parameter.value = UnityEngine.Random.Range(0, 100);

                    if (useQueue)
                    {
                        if (insert)
                        {
                            UIKit.InsertQueueUI(0, name, type, parameter, queueId);
                        }
                        else
                        {
                            UIKit.OpenQueueUI(name, type, parameter, queueId);
                        }
                    }
                    else
                    {
                        UIKit.OpenUI(name, type, parameter).OnChanged(h => Debug.Log("SingleUIEvent " + h.Name + " " + h.State));
                    }
                }
                if (GUILayout.Button($"hide{i}", GUILayout.Width(100), GUILayout.Height(50)))
                {
                    UIKit.CloseUI(name, false);
                }
                if (GUILayout.Button($"close{i}", GUILayout.Width(100), GUILayout.Height(50)))
                {
                    UIKit.CloseUI(name);
                }

            }
        }

        if (GUILayout.Button("TestView", GUILayout.Height(50)))
        {
            var ui = UIKit.OpenUI(typeof(NewUI));
        }
        if (GUILayout.Button("TestUI", GUILayout.Height(50)))
        {
            UIKit.OpenUI<TestUI>().OnShow(() => Debug.Log("some log"));
        }
    }
}