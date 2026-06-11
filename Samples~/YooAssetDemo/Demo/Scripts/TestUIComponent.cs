using System.Collections;
using System.Collections.Generic;
using TMPro;
using HUI;
using UnityEngine;

public class TestUIComponent : BaseComponent<string>
{
    private string data;
    private TextMeshProUGUI tmp;
    protected override void OnAdded()
    {
        Debug.Log($"[TestSubUI] OnAdded.  Parent:{Parent.GetType().Name} Name:{Name}");

        tmp= View.GetComponentInChildren<TextMeshProUGUI>();
        data= tmp.text;
    }
    protected override void OnShow()
    {
        tmp.text = data + Parameter;
    }
}

public class TestUIComponent2 : BaseComponent
{
    protected override void OnAdded()
    {
        Debug.Log($"[TestSubUI2] OnAdded.  Parent:{Parent.GetType().Name} Name:{Name}");
    }
    protected override void OnRemoved()
    {
        Debug.Log($"[TestSubUI2] OnRemoved.  Parent:{Parent.GetType().Name} Name:{Name}");
    }
    protected override void OnShow()
    {
        Debug.Log($"[TestSubUI2] OnShow.  Parent:{Parent.GetType().Name} Name:{Name}");
    }
    protected override void OnHide()
    {
        Debug.Log($"[TestSubUI2] OnHide.  Parent:{Parent.GetType().Name} Name:{Name}");
    }
}
