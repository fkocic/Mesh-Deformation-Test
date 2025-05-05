using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float speed = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (Input.GetMouseButton(1))
            {
                transform.position = Vector3.MoveTowards(transform.position, hit.point, speed * Time.deltaTime);
            }
        }
    }
}
