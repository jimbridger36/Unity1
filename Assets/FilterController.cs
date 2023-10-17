/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterController : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider,sliderMax;
    public Vector2 minMaxCost=new Vector2(0,99999999);
    public Vector2 currentMax=new Vector3(0,9999999);
    public List<AppartementController> allCurrentAppartements = new List<AppartementController>();
    public Transform selectedBuilding;
    IEnumerator Start()
    {
        foreach (AppartementController a in selectedBuilding.GetComponentsInChildren<AppartementController>(true))
        {
            allCurrentAppartements.Add(a);
        }
        yield return null;
            RefreshMaxPrice();
    }
    public void OnSlide()
    {
        slider.value = Mathf.Clamp(slider.value, slider.minValue,1- sliderMax.value);
       // sliderMax.value = Mathf.Clamp(slider.value, slider.minValue, sliderMax.value);
        currentMax.x = Mathf.Lerp(minMaxCost.x, minMaxCost.y, slider.value);
        currentMax.y = Mathf.Lerp(minMaxCost.x, minMaxCost.y,1- sliderMax.value);
        Filter();
    }
    void Filter()
    {
        foreach(AppartementController a in allCurrentAppartements)
        {
            a.gameObject.SetActive(ShouldShow(a));
        }
    }
    bool ShouldShow(AppartementController a)
    {
        if (a.info.cost < currentMax.x) return false;
        if (a.info.cost > currentMax.y) return false;
        return true;
    }
  void   RefreshMaxPrice()
    {
       
        foreach(AppartementController a in allCurrentAppartements)
        {
            if (a.info.cost > minMaxCost.y) minMaxCost.y = a.info.cost;
            if (a.info.cost < minMaxCost.x) minMaxCost.x = a.info.cost;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/