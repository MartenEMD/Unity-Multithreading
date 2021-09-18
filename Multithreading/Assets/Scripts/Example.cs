using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    private static Multithreading multithreading = new Multithreading();  //Multithreading instance

    // Start is called before the first frame update
    void Start()
    {
        Task.Run(() =>
        {
            //Get GameObject where the text is attached to
            Multithreading.Result<GameObject> textGameObjectResult = new Multithreading.Result<GameObject>();       //Create result instance with the parameter T as return type
            multithreading.FindGameObject("ExampleText", textGameObjectResult);                                     //Call method you want to use
            GameObject textGameObject = textGameObjectResult.GetData;                                               //Get the returned data

            //Get the text component
            Multithreading.Result<Text> textComponentResult = new Multithreading.Result<Text>();
            multithreading.GetComponent<Text>(textGameObject, textComponentResult);
            Text textComponent = textComponentResult.GetData;

            //Set new text
            Multithreading.Result setTextResult = new Multithreading.Result();      //Instance without return type
            multithreading.SetText(textComponent, "Text changed from sub-thread", setTextResult);
            setTextResult.Wait();        //Sleep this task until the text is changed

            Debug.Log("Text changed");
        });
    }

    // Update is called once per frame
    void Update()
    {
        multithreading.Execute();
    }
}
