using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReflectionExample : MonoBehaviour
{

    public GameObject firePoint;
    public GameObject bullet;
    public float rotateSpeed = 1.0f;
    public float bulletSpeed = 1.0f;
    public float radius = 1.0f;
    public int numPredictIterations = 100;
    public GameObject predictBullet;

    Scene previewScene;

    public Dictionary<GameObject, GameObject> sceneMap = new Dictionary<GameObject, GameObject>();

    // Start is called before the first frame update
    void Start()
    {

        Physics2D.autoSimulation = false;
        InitPreview();
        predictBullet = GameObject.Instantiate(this.bullet);
        SceneManager.MoveGameObjectToScene(predictBullet, previewScene);


    }

    public GameObject CopyToPreview(GameObject go)
    {
        var instance = GameObject.Instantiate(go);
        SceneManager.MoveGameObjectToScene(go, previewScene);
        return instance;
    }


    public void InitPreview()
    {

        previewScene = SceneManager.CreateScene("preview", new CreateSceneParameters() { localPhysicsMode = LocalPhysicsMode.Physics2D});

        var scene = SceneManager.GetActiveScene();
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            if(gameObject == this.gameObject)
            {
                continue; //don't add another copy of this GO because this is the one that makes the preview scene
            }
            var instance = CopyToPreview(gameObject);
            sceneMap[gameObject] = instance;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Rotate(Vector3.forward * rotateSpeed);
        }


        if (Input.GetKey(KeyCode.D))
        {
            this.transform.Rotate(Vector3.forward * -rotateSpeed);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var newBullet = GameObject.Instantiate(this.bullet);
            newBullet.transform.position = this.transform.position;
            newBullet.GetComponent<Rigidbody2D>().velocity = this.transform.up * this.bulletSpeed;
            
        }

        DrawPredictionDisplay();

        PredictPhysics();


    }


    private void PredictPhysics()
    {

        predictBullet.transform.position = this.transform.position;
        predictBullet.GetComponent<Rigidbody2D>().velocity = this.transform.up * this.bulletSpeed;



        for (int i = 0; i < numPredictIterations; i++)
        {
            previewScene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
            DrawCircle(predictBullet.transform.position, radius, UnityEngine.Color.red);
        }

    }

    private void FixedUpdate()
    {
        
        Physics2D.Simulate(Time.fixedDeltaTime);
    }


    private void UpdatePhysics()
    {

    }

    private void DrawPredictionDisplay()
    {

        Vector2 origin = firePoint.transform.position; //unity has a built in type converter that converts vector3 to vector2 by dropping the z component

        Vector2 direction = firePoint.transform.up;

        RaycastHit2D distanceCheck = Physics2D.Raycast(origin, direction);

        RaycastHit2D hit = Physics2D.CircleCast(origin, radius, direction);


        Debug.DrawLine(origin, direction * 10000, UnityEngine.Color.black);
        DrawCircle(origin, radius, UnityEngine.Color.black);

        if (hit)
        {
            
            origin = hit.point + (hit.normal * radius);
            direction = Vector2.Reflect(direction, hit.normal);

            hit = Physics2D.CircleCast(origin, radius, direction);

            Debug.DrawLine(origin, direction * 10000, UnityEngine.Color.blue);
            DrawCircle(origin, radius, UnityEngine.Color.blue);
        }
    }


    private void DrawCircle(Vector2 center, float radius, UnityEngine.Color color)
    {
        Vector2 prevPoint = new Vector2(Mathf.Sin(0f) * radius, Mathf.Cos(0f) * radius);

        for (float t = 0.1f; t < 2 * Mathf.PI; t = t + 0.1f)
        {
            var nextPoint = new Vector2(Mathf.Sin(t) * radius, Mathf.Cos(t) * radius);

            Debug.DrawLine(center + prevPoint, center + nextPoint, color);

            prevPoint = nextPoint;
        }


    }
}
