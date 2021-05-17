using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WormPiece
{
    public GameObject gameObject;
    public DampedTransform damp;

    public WormPiece(GameObject gameObject, DampedTransform dampedTransform)
    {
        this.gameObject = gameObject;
        damp = dampedTransform;
    }

}
public class WormBuilder : MonoBehaviour
{
    [SerializeField] int bodyCount = 10;
    [SerializeField] GameObject templatePiece = default;
    [SerializeField] Transform bodyParent = default;
    [SerializeField] Transform rigParent = default;

    DampedTransform templateDamp;
    List<WormPiece> pieces = new List<WormPiece>();

    void OnEnable()
    {

        if (Application.isPlaying)
        {
            templateDamp = (new GameObject("Damp")).AddComponent<DampedTransform>();
            templateDamp.data.dampRotation = 0.7f;
            templateDamp.data.maintainAim = true;
            SetupWorm();
        }
    }

    void SetupWorm()
    {
        Vector3 startPosition = bodyParent.position;
        for (int i = 0; i < bodyCount; i++)
        {
            startPosition += -transform.forward * 2;
            GameObject obj = Instantiate(templatePiece);
            obj.transform.parent = i == 0 ? bodyParent : pieces[i - 1].gameObject.transform;
            obj.transform.position = startPosition;
            obj.transform.forward = transform.forward;

            DampedTransform damp = Instantiate(templateDamp);
            damp.transform.parent = rigParent;
            damp.data.constrainedObject = obj.transform;
            damp.data.sourceObject = i == 0 ? bodyParent : pieces[i - 1].gameObject.transform;

            WormPiece newPiece = new WormPiece(obj, damp);
            pieces.Add(newPiece);
        }
    }

}
