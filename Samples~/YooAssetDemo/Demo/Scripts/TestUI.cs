using HUI;
using UnityEngine;
using UnityEngine.UI;



public class TestUI : BaseUI
{
    public Button button { get; private set; }

    protected override void OnOpen()
    {
        button = View.GetComponentInChildren<Button>();
        button.onClick.AddListener(Button_ButtonOnClick);
    }
    
    private void Button_ButtonOnClick()
    {
        this.Close();
    }
}

public class TestParameter
{
    public string test;
    public int value;
}

public class TestUI1 : BaseUI<TestParameter>
{
    protected override void OnOpen()
    {
        Debug.Log("TestUI1 " + Parameter.value);
    }
}
public class TestUI2 : TestUI1
{
    protected override void OnOpen()
    {

        Debug.Log("TestUI2 " + Parameter.value);
    }
}