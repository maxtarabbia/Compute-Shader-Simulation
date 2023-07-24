using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FramerateCounter : MonoBehaviour
{

    public int smoothingCount;

    public float[] frametimes;

    TextMeshProUGUI Textbox;

    // Start is called before the first frame update
    void Start()
    {
        frametimes = new float[smoothingCount];
        Textbox = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        frametimes[0] = Time.deltaTime;

        float[] newTimes = new float[smoothingCount];


        float avgtime = 0;

        for (int i = 0; i < frametimes.Length; i++)
        {
            newTimes[(i + 1) % newTimes.Length] = frametimes[i];
            avgtime += frametimes[i];
        }
        newTimes.CopyTo(frametimes,0);
        avgtime = avgtime / frametimes.Length;



        Textbox.text = "FPS: " + (1f/ avgtime).ToString("0.0");
    }
}
