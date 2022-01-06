using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;

public class GenstureRecognizer : MonoBehaviour
{
    public const float RECOGNITION_LIMIT = 0.85f;
    public string fileName = "Assets/Resources/genstureRecognizer.tflite";
    Interpreter interpreter;
    float[,] inputs = new float[Constants.IMAGE_HEIGHT, Constants.IMAGE_WIDTH];
    float[] outputs = new float[10];
    ComputeBuffer inputBuffer;

    System.Text.StringBuilder sb = new System.Text.StringBuilder();

    private bool TraceMode = false;

    // Start is called before the first frame update
    void Start()
    {
        var options = new Interpreter.Options()
        {
            threads = 2
        };
        //interpreter = new Interpreter(FileUtil.LoadFile(fileName), options);
        var Model = System.IO.File.ReadAllBytes(fileName);
        interpreter = new Interpreter(Model, options);
        interpreter.ResizeInputTensor(0, new int[] { 1, Constants.IMAGE_HEIGHT, Constants.IMAGE_WIDTH, 1 });
        interpreter.AllocateTensors();

        inputBuffer = new ComputeBuffer(Constants.IMAGE_HEIGHT * Constants.IMAGE_WIDTH, sizeof(int));
    }

    void OnDestroy()
    {
        interpreter?.Dispose();
        inputBuffer?.Dispose();
    }

    void SetInputs(int[] inp)
    {
        if(inp.Length != Constants.IMAGE_HEIGHT * Constants.IMAGE_WIDTH)
        {
            Debug.LogError("Invalid Input provided to Gensture Recognizer. Expected array[" + Constants.IMAGE_HEIGHT * Constants.IMAGE_WIDTH + "], got " + inp.Length);
            return;
        }
        int row = 0;
        for(int i = 0; i<inp.Length;i++)
        {
            row = (int)(i / Constants.IMAGE_WIDTH);
            inputs[row, i - row * Constants.IMAGE_WIDTH] = inp[i];
        }
    }

    public int RecognizeImage(int[] inp)
    {
        float[] retCodes = new float[2];
        if (inp.Length != Constants.IMAGE_HEIGHT * Constants.IMAGE_WIDTH)
        {
            Debug.LogError("Invalid Input provided to Gensture Recognizer. Expected array[" + Constants.IMAGE_HEIGHT * Constants.IMAGE_WIDTH + "], got " + inp.Length);
            return -2;
        }
        int row = 0;
        for (int i = 0; i < inp.Length; i++)
        {
            row = (int)(i / Constants.IMAGE_WIDTH);
            inputs[row, i - row * Constants.IMAGE_WIDTH] = inp[i];
        }
        int OnesCount = 0;
        int firstIndex = -1;
        for (int i = 0; i < inp.Length; i++)
        {
            if (inp[i] == 1)
            {
                OnesCount++;
                if (firstIndex == -1)
                    firstIndex = i;
            }
            
        }
        // Debug.Log($"Input got {OnesCount} ones. First one located at {firstIndex}");
        //inputBuffer.GetData(inputs);

        float startTime = Time.realtimeSinceStartup;
        interpreter.SetInputTensorData(0, inputs);
        interpreter.Invoke();
        interpreter.GetOutputTensorData(0, outputs);
        float duration = Time.realtimeSinceStartup - startTime;
        // ������ �������
        int predictClass = -1;
        float predictPossibility = -1;
        for (int i = 0; i < outputs.Length; i++)
        {
            if (outputs[i] > predictPossibility)
            {
                predictPossibility = outputs[i];
                predictClass = i;
            }
        }

        // ���� ���� ������� ���������� ����������
        if (TraceMode)
        {
            var szMsg = "NN Result length " + outputs.Length + ", values : [";            
            for (int i = 0; i < outputs.Length; i++) {
                szMsg += outputs[i] + ", ";

                if(outputs[i] > predictPossibility)
                {
                    predictPossibility = outputs[i];
                    predictClass = i;
                }

            }
            szMsg+= $"]\r\n PREDICTED CLASS: {predictClass} with probability {predictPossibility}";
            szMsg+=($"   \r\nProcess time: {duration: 0.00000} sec");
            Debug.Log(szMsg);
        }

        if (predictPossibility >= RECOGNITION_LIMIT)
            return (int)predictClass;
        else 
            return -1;
    }
}
