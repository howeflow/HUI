using System;
using TMPro;
using HUI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestView : BaseView
{
    public EventTrigger EventTrigger_Image;
    public BaseView BaseView_TestUIComponent;
    public BaseView BaseView_TestUIComponent1;
    public BaseView BaseView_TestUIComponent2;
    public Button Button_Button;
    public Button Button_Button1;
}

[UIPath(nameof(TestView))]
public class NewUI : BaseViewUI<TestView, int>
{

    private BaseComponent c0;
    private BaseComponent c1;
    private BaseComponent c2;

    protected override void OnOpen()
    {
        c0 = Container.Add<TestUIComponent, string>(View.BaseView_TestUIComponent, Random.value.ToString());
        c1 = Container.Add(nameof(View.BaseView_TestUIComponent1), typeof(TestUIComponent2), View.BaseView_TestUIComponent1);
        c2 = Container.Add(nameof(View.BaseView_TestUIComponent2), typeof(TestUIComponent2), View.BaseView_TestUIComponent2);

        Container.HideAll();

        Container.Show(c1.Name);

        var enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            Container.Show(c0.Name);
        });

        var exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            Container.Hide(c0.Name);
        });


        View.EventTrigger_Image.triggers.Add(enterEntry);
        View.EventTrigger_Image.triggers.Add(exitEntry);

        View.Button_Button.onClick.AddListener(Button_ButtonOnClick);
        View.Button_Button1.onClick.AddListener(Button_Button1OnClick);
    }

    private void Button_ButtonOnClick()
    {
        Container.HideAll();
        Container.Show(c1.Name);
    }

    private void Button_Button1OnClick()
    {
        Container.HideAll();
        Container.Show(c2.Name);
    }
}
