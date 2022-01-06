using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// ����������� ����� � ��������������� ������������
// �������� ������ � IMAGE_WIDTH x IMAGE_HEIGHT ����������, � ������� �������� �������� ���� 0 ���� 1

[System.Serializable]
public class GenstureArray
{
	[SerializeField] public List<GenstureImage> genstures;
	[SerializeField] private GenstureImage[] arr;
	public GenstureArray()
    {
		genstures = new List<GenstureImage>();
	}
	
	public void add(GenstureImage a)
    {
		genstures.Add(a);
	}
	public void saveToJSON()
	{
		if (genstures.Count<=0)
			return;
		//arr = genstures.ToArray();
		// string data = JsonUtility.ToJson(arr);
		string data = "{ \"images\": [ " + genstures[0].toJSON();
		for (int i = 1; i < genstures.Count; i++) 
			data += "," + genstures[i].toJSON();
		data += "]}";

		System.IO.File.WriteAllText(Application.persistentDataPath + "/genstures.json", data);		
	}
}

[System.Serializable]
public class GenstureImage
{
	public int[] imageCoordinates;

	public GenstureImage()
    {
		imageCoordinates = new int[Constants.IMAGE_WIDTH * Constants.IMAGE_HEIGHT];

	}
	public string toJSON() {
		return JsonUtility.ToJson(this);
	}
}
public class Wizzardy : MonoBehaviour
{
	//public PlayerController controller;

	// public float runSpeed = 40f;
	Animator animator;
	public Canvas SpellCanvas;
	bool isSpellMode = false;
	// float horizontalMove = 0f;
	// bool jump = false;
	// bool crouch = false;

	// Mouse drawing 
	public GameObject SpellPanel;
	private LineRenderer line;
	private List<Vector3> pointsList; // array of mouse coordinates for gensture
	private List<Vector3> pointsListWorld; // array of world coordinates for gensture (required for drawing)
	
	private Vector3 mousePos;
	private Vector3 mousePosWorld;

	private GenstureArray imageArray;
	private bool saveGenstures = false;
	public GenstureRecognizer SpellRecognizer;

	public Transform FirePoint;
	public GameObject Fireball;
	public SimplePlayerController PlayerController;

	private int[] drawedSigns;
	private int drawedSignsCount = 0;

	void Awake()
	{
		// animator = GetComponent<Animator>();
		// Create line renderer component and set its property
		// line = gameObject.AddComponent<LineRenderer>();
		//PrepareLineRender();
		isSpellMode = false;
		pointsList = new List<Vector3>();
		pointsListWorld = new List<Vector3>();
		PrepareLineRender();

		imageArray = new GenstureArray();
    }

    // Update is called once per frame
    void Update()
	{

		/*
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		// ������� ��������
		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
		}
		if (Input.GetButtonDown("Crouch"))
		{
			crouch = true;
		}
		else if (Input.GetButtonUp("Crouch"))
		{
			crouch = false;
		}
		animator.SetBool("Crouch", crouch);
		*/
		// If booth mouse buttons pressed - gensture finished
		// Left button + already in spell mode
		if (Input.GetMouseButtonDown(0) && isSpellMode) {
			GenstureComplete();
		}
		
		// Begining of spell drawing
		// only right button
		else if (Input.GetMouseButtonDown(1))
		{
			if (!SpellCanvas.enabled)
            {
				SpellCanvas.enabled = true;
				// SpellPanel.enabled = true;
				SpellPanel.SetActive(true);
				isSpellMode = true;
				drawedSigns = new int[Constants.MAX_SIGN_COUNT];
				drawedSignsCount = 0;
				line.SetVertexCount(0);
				pointsList.RemoveRange(0, pointsList.Count);
				pointsListWorld.RemoveRange(0, pointsListWorld.Count);
				
				line.SetColors(Color.green, Color.green);
			}			
		}
		
		// Right button Up - Spell casting complete
		else if (Input.GetMouseButtonUp(1) && isSpellMode)
		{
			EndSpellSession();
		}

		// Still in casting mode
		if (isSpellMode)
		{
			mousePos = Input.mousePosition;	
			mousePosWorld = Camera.main.ScreenToWorldPoint(mousePos);
			mousePosWorld.z = 0;
			// mousePos = Input.mousePosition;
			// �������������� � ScreenToWorldPoint �� ����� �.�. mousePosition ��� � pixel Coordinates ������ 
			// ������ ����� ����� ���������� ������ � ��������������� �.�. ���������� �������� ������������ SpellPanel 

			if (!pointsList.Contains(mousePos))
			{
				pointsList.Add(mousePos); // Input.
				pointsListWorld.Add(mousePosWorld); 
				line.SetVertexCount(pointsListWorld.Count);
				line.SetPosition(pointsListWorld.Count - 1, (Vector3)pointsListWorld[pointsListWorld.Count - 1]);

			}
		}
	}


	// ��������� ��������� ����� 
	void GenstureComplete()
    {
		// convert current gensture
		// save gensture to array
		int genstureClass = processGensture();
		print("GenstureComplete. Recognized class " + genstureClass);
		if (genstureClass >= 0)
		{
			// drawedSigns - array of drawed genstures 
			drawedSigns[drawedSignsCount] = genstureClass;
			drawedSignsCount++;
			// Maximum number of genstures reached 
			if (drawedSignsCount >= Constants.MAX_SIGN_COUNT)
			{				
				EndSpellSession();
			}
		}
		else
		{
			// Recongizer returned unknown gensture 
			print("Unrecognized gensture");
			//print("Unrecognized gensture. Finishing spell session");
			// EndSpellSession();
		}
		// Clear current gensture
		line.SetVertexCount(0);
		pointsList.RemoveRange(0, pointsList.Count);
		pointsListWorld.RemoveRange(0, pointsListWorld.Count);

		// TODO: save gensture in display area 
	}

	// Casting complete. Check 
	void EndSpellSession()
    {
		// Последний символ учитываем без ЛКМ
		if (drawedSignsCount < Constants.MAX_SIGN_COUNT){
			GenstureComplete();
		}
		// Debug mode - save genstures to file
		if (saveGenstures)
		{
			imageArray.saveToJSON();
			print("Data has been saved at " + Application.persistentDataPath);
		}
		// TODO: process last gensture if there were lot of points 
		SpellCanvas.enabled = false;
		//SpellPanel.enabled = false;
		SpellPanel.SetActive(false);
		
		isSpellMode = false;

		// Prepare debug message with drawed calssed 
		string szSpellMsg = "Casting complete. Drawed classes: ";
		for (int i = 0; i < drawedSignsCount; i++)
		{
			szSpellMsg += drawedSigns[i] + " ";
		}
		print(szSpellMsg);
		if (drawedSignsCount > 0)
			CastSpell();
	}

	// Check drawed signs and cast spell
	void CastSpell()
    {
		/*
		https://colab.research.google.com/drive/134RoHp8g5ePOa3eWUjFStJCiVNmgz5s8#scrollTo=7OzrK61p4ACc
		# Разметка данных по классам (ДЕЛАЕМ РУКАМИ)
		# 1 - подкова (пересечение)
		# 2 - подкова (объединение)
		# 3 - восьмёрка
		# 4 - круг
		# 5 - угол влево
		# 6 - угол вправо
		# 7 - вертикальная линия
		# 8 - горизонтальная линия
		# 9 - Z
		# 0 - волна
		*/

		// 1 - запустить файрболл
		if (drawedSignsCount == 1 && drawedSigns[0] == 4 /*&& drawedSigns[1] == 3*/) {
			//Instantiate(Fireball, FirePoint.position, FirePoint.rotation);
			PlayerController.CastFireball();
		}

		// 3 - невидимость
		else if (drawedSignsCount == 1 && drawedSigns[0] == 3 /*&& drawedSigns[1] == 3*/) {
			//Instantiate(Fireball, FirePoint.position, FirePoint.rotation);
			PlayerController.CastInisibility();
		}
		
		/*
		if (drawedSignsCount == 2 && drawedSigns[0] == 1 && drawedSigns[1] == 3) {
			Instantiate(Fireball, FirePoint.position, FirePoint.rotation);
		}
		*/
	}

	/*void FixedUpdate()
	{
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;
	}*/

	void PrepareLineRender() {
		//line = SpellPanel.AddComponent<LineRenderer>();
		line = SpellPanel.GetComponent<LineRenderer>();
		if (line != null)
		{
			line.material = new Material(Shader.Find("Standard"));
			line.SetVertexCount(0);
			line.SetWidth(0.1f, 0.1f);
			line.SetColors(Color.green, Color.green);
			line.useWorldSpace = true;
		}
		else {
			line = SpellPanel.AddComponent<LineRenderer>();

			line.SetVertexCount(0);
			line.SetWidth(0.1f, 0.1f);
			line.SetColors(Color.green, Color.green);

			print("WARNING! No line on spell canvas");
		} 

	}

	// Process single gensture 
	int processGensture() 
	{
		// Current mouse gensture is stored in 
		//		pointsList = Screen Coordinates: List of Vector3
		//		pointsListWorld = World Coordinates: List of Vector3

		// Resize image 
		// Convert gensture image to Neural Network Input
		// We are working with mouse coordinates
		float minX = pointsList[0].x;
		float maxX = pointsList[0].x;
		float minY = pointsList[0].y;
		float maxY = pointsList[0].y;

		// Find min/max X,Y for image conversion
		for (int i = 0; i < pointsList.Count; i++)
		{
			if (pointsList[i].x < minX)
				minX = pointsList[i].x;
			else if (pointsList[i].x > maxX)
				maxX = pointsList[i].x;
			if (pointsList[i].y < minY)
				minY = pointsList[i].y;
			else if (pointsList[i].y > maxY)
				maxY = pointsList[i].y;
		}

		// Calculate stretch coefficient
		float kX = (maxX - minX) > 1 ? Constants.IMAGE_WIDTH / (maxX - minX) : 1;
		float kY = (maxY - minY) > 1 ? Constants.IMAGE_HEIGHT / (maxY - minY) : 1;

		// Find image center
		float xCenter = (maxX - minX) / 2 + minX;
		float yCenter = (maxY - minY) / 2 + minY;

		// ��������� ������ � ����������������� ������������ + Convert to array of 1/0
		// ���������� � Unity ������������� �� ������ ������� ����, � ������� ��������� ���������� ���� � ������ ��������� ����
		// � ��� 
		// Преобрзаование массива точек в массив для распознавания 
		// Массив точек - это массив координат. А нам нужен массив 1 и 0, где индекс соответствует координате. Более того, направлениие Y в Unity идет снизу вверх, а для распознвания
		// сверху вниз
		//		x  - same as unity
		//		y = IMAGE_HEIGHT - y - 1
		var scaledCoords = new Vector3[pointsList.Count];
		int[] imageCoordinates = new int[Constants.IMAGE_WIDTH * Constants.IMAGE_HEIGHT];
		var a = new GenstureImage();
		
		// Init array with zeroes
		for (int i = 0; i < Constants.IMAGE_WIDTH * Constants.IMAGE_HEIGHT; i++) {
			a.imageCoordinates[i] = 0;
		}
		var arrIndex = 0;
		// ��������������� ����������� � ����������� � ������������� ����������
		for (int i = 0; i < pointsList.Count; i++)
		{
			int x, y;
			x = (int)((pointsList[i].x - xCenter) * kX) + Constants.IMAGE_WIDTH / 2;
			y = (int)((pointsList[i].y - yCenter) * kY) + Constants.IMAGE_HEIGHT / 2;
			scaledCoords[i] = new Vector3(x, y, 0); // unity ����������						

			// посчитали координаты. Теперь в нужном месте массива нужно поставить единицу
			arrIndex = ((Constants.IMAGE_HEIGHT - y - 1) * Constants.IMAGE_WIDTH + x);
			// отладка - если неправильно посчитали индекс в массиве
			if (arrIndex >= a.imageCoordinates.Length || arrIndex < 0)
			{
				print("DEBUGGER"); 
			}

			a.imageCoordinates[arrIndex] = 1;			
		}

		// Debug - write result to file 
		byte[] byteMessage = new byte[a.imageCoordinates.Length];
		char[] charMessage = new char[a.imageCoordinates.Length]; 
 
        // Is there a way to convert the whole array at once instead of looping through it?
        for (int i = 0; i < a.imageCoordinates.Length; i++)
        {
            byteMessage[i] = ((byte)a.imageCoordinates[i]);
			charMessage[i] = a.imageCoordinates[i] == 0 ? '0' : '1'; 
        }
		string stringMessage = new string(charMessage);
		File.AppendAllText("mylog.txt", stringMessage + "\r\n");
		//File.AppendAllText("mylog.txt", DateTime.Now.ToString() + byteMessage + Environment.NewLine);
		//File.WriteAllBytes("mylog.txt", byteMessage);
		// ENDOF Debug

		// Add Image to Array 
		
		imageArray.add(a);
		

		// RECOGNIZE IMAGE
		int genstureClass = -1;
		genstureClass = SpellRecognizer.RecognizeImage(a.imageCoordinates);
		return genstureClass;

	}

	// ��������������� ������������ ������ � ������������ ���
	void redrawSign()
	{
		// Convert gensture image to Neural Network Input
		// We are working with mouse coordinates
		float minX = pointsList[0].x;
		float maxX = pointsList[0].x;
		float minY = pointsList[0].y;
		float maxY = pointsList[0].y;
	
		// ����� ������� ���������� ������������� �����
		for (int i = 0; i < pointsList.Count; i++)
		{
			if (pointsList[i].x < minX)
				minX = pointsList[i].x;
			else if (pointsList[i].x > maxX)
				maxX = pointsList[i].x;
			if (pointsList[i].y < minY)
				minY = pointsList[i].y;
			else if (pointsList[i].y > maxY)
				maxY = pointsList[i].x;
		}

		// ����� ����������� ���������������
		float kX = (maxX - minX) > Constants.IMAGE_WIDTH ? Constants.IMAGE_WIDTH / (maxX - minX) : 1;
		float kY = (maxY - minY) > Constants.IMAGE_HEIGHT ? Constants.IMAGE_HEIGHT / (maxY - minY) : 1;

		// ���������� �����
		float xCenter = (maxX - minX) / 2;
		float yCenter = (maxY - minY) / 2;

		// ��������� ������ � ����������������� ������������		
		// scaledCoords.RemoveRange(0, scaledCoords.Count);
		//scaledCoords = new List<Vector2>();
		var scaledCoords = new Vector3[pointsList.Count];

		for (int i = 0; i < pointsList.Count; i++)
		{
			scaledCoords[i] = new Vector3((pointsList[i].x - xCenter) * kX + Constants.IMAGE_WIDTH / 2, (pointsList[i].y - yCenter) * kY + Constants.IMAGE_HEIGHT / 2, 0);
		}

		// ���������� ������������������ �����
		if (scaledCoords.Length > 0)
		{
			/*
			print("Original coord x = " + pointsList[0].x + ", y = " + pointsList[0].y);
			print("World coord x = " + pointsListWorld[0].x + ", y = " + pointsListWorld[0].y);
			print("Scaled coord x = " + scaledCoords[0].x + ", y = " + scaledCoords[0].y);
			*/

			// Convert scaled cords into world coords 
			for (int i = 0; i < scaledCoords.Length; i++) {
				scaledCoords[i] = Camera.main.ScreenToWorldPoint(scaledCoords[i]);
			}
			//print("Scaled WORLD coord x = " + scaledCoords[0].x + ", y = " + scaledCoords[0].y);

			// PrepareLineRender();
			


			//line.material = new Material(Shader.Find("Test"));
			Material myMaterial = (Material)Resources.Load("Test", typeof(Material));
			line.material = myMaterial;
			//yourObject.renderer.sharedMaterial = yourMaterial;

			/*
			line.SetColors(Color.red, Color.red);			;
			line.SetVertexCount(0);
			line.SetVertexCount(scaledCoords.Length);		
			line.SetPositions(scaledCoords);
			*/

			// ���������� ����� ������
			// �� ������ ������� ���� � ������� ������
			var point1 = Camera.main.ScreenToWorldPoint(new Vector3(10,10,0));
			var point2 = Camera.main.ScreenToWorldPoint(new Vector3(1200, 700, 0));

			point1.z = 10;
			point2.z = 10;

			print("Point 1 WORLD coordinates " + point1);
			print("Point 2 WORLD coordinates " + point2);

			/*
			line.SetVertexCount(2);
			line.SetPosition(0, point1);
			line.SetPosition(1, point2);
			*/

			// Instantiate prefab line 
			GameObject prefab = (GameObject)Resources.Load("Prefabs/LinePrefab"); // OBJECT SHOULD BE IN RESOURCES FOLDER!!!
			print("prefab = " + prefab);
			GameObject myLine = Instantiate(prefab) as GameObject;
			myLine.transform.parent = SpellPanel.transform;
			LineRenderer myLineRenderer = myLine.GetComponent<LineRenderer>();

			myLineRenderer.SetVertexCount(2);
			myLineRenderer.SetPosition(0, point1);
			myLineRenderer.SetPosition(1, point2);


			/*for (int i = 0; i < scaledCoords.Count; i++)
			{
				line.SetPosition(i, (Vector3)scaledCoords[i]);
			}
			*/

		}


	}
}
