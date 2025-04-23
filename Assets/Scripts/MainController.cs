using Oculus.Interaction;
using Oculus.Interaction.DebugTree;
using Oculus.Interaction.Samples;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine.UIElements;
using Meta.XR.BuildingBlocks;
using System.Threading.Tasks;


[Serializable]
public class LoadObject
{
    //Meta.XR.MRUtilityKit.
    //QRCodeAnchorInfo
    //QRCodeAnchorInfo info;
    //OVRSceneAnchor

    

    //SpatialAnchorManager anchorManager;
  //  OVRSceneManager 

    [SerializeField] string id;
    [SerializeField] GameObject objTarget;

    public string Id => id;
    public GameObject ObjectTarget => objTarget;
}

public class MainController : MonoBehaviour
{
    [SerializeField] GameObject dialogObjectSelection;
    [SerializeField] MRPassthrough paththroghComp;

    [SerializeField] GameObject _poseReactionTarget;
    [SerializeField, Interface(typeof(IActiveState))]
    UnityEngine.Object _activeState;

    [SerializeField] Transform _objectSpawnAnchor;
    [SerializeField] List<LoadObject> _loadObjects;

    [SerializeField] bool isPaththroughOn = true;

    [SerializeField] UnityTransportServer _transportServer;
    [SerializeField] TMP_Text _txtInfo;

    [SerializeField] SpatialAnchorCoreBuildingBlock spatialAnchorCore;
    [SerializeField] GameObject objectAnchorWithManupulator;
    [SerializeField] GameObject anchorPrefab;
    //OVRSpatialAnchor.OperationResult Result;

    bool _oldState = false;

    string _dataCache;

    // Start is called before the first frame update
    void Start()
    {
        dialogObjectSelection.SetActive(false);

        if(isPaththroughOn)
        {
            StartCoroutine(coTriggerActionWithDelay(0.2f, () =>
            {
                paththroghComp.TogglePassThrough();
            }));
        }

        _transportServer.OnEventDataReceived += OnEventServerDateReceived;
        _transportServer.OnEventMainLog += OnEventServerMainLog;

        spatialAnchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreateComplete);

        string anchorUuid = PlayerPrefs.GetString("AnchorGUID", string.Empty);
        if(!string.IsNullOrEmpty(anchorUuid))
        {
            Guid uuid;
            if (Guid.TryParse(anchorUuid, out uuid))
            {
                Debug.Log("Try loading Anchor..." + uuid.ToString());
                spatialAnchorCore.LoadAndInstantiateAnchors(anchorPrefab, new List<Guid>(){uuid});
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool curState = (_activeState as IActiveState).Active;
        if(_oldState != curState)
        {
            if(curState) 
                _poseReactionTarget.SetActive( !_poseReactionTarget.activeSelf );

            _oldState = curState;
        }
    }



    public void OnBtnLoadQuad()
    {
        SpawnObject("cube");
    }
    public void OnBtnLoadCapsule()
    {
        SpawnObject("capsule");
    }
    public void OnBtnLoadCylinder()
    {
        SpawnObject("cylinder");
    }
    public void OnBtnLoadSphere()
    {
        SpawnObject("sphere");
    }

    GameObject curAnchorCache;
    public void OnBtnLoadAnchor()
    {
        Vector3 vPos = _objectSpawnAnchor.position + Vector3.one * UnityEngine.Random.Range(-0.1f, 0.1f);
        //spatialAnchorCore.InstantiateSpatialAnchor(objectAnchorWithManupulator, vPos, Quaternion.identity);
        //InstantiateSpatialAnchor(objectAnchor, vPos, Quaternion.identity);
        var obj = GameObject.Instantiate(objectAnchorWithManupulator, vPos, _objectSpawnAnchor.rotation);
        obj.SetActive(true);
        curAnchorCache = obj;
    }

    public void OnBtnFixAnchor()
    {
        var trAnchor = curAnchorCache.transform.Find("DemoAnchorPlacementBuildingBlock");
        /*trAnchor.SetParent(curAnchorCache.transform.parent);
        GameObject.Destroy( curAnchorCache );
        curAnchorCache = null;

        var spatialAnchor = trAnchor.gameObject.AddComponent<OVRSpatialAnchor>();
        InitSpatialAnchorAsync(spatialAnchor);
        */
        
        var vPos = trAnchor.transform.position;
        GameObject.Destroy( curAnchorCache );
        curAnchorCache = null;

        spatialAnchorCore.InstantiateSpatialAnchor(anchorPrefab, vPos, Quaternion.identity);
    }

    void SpawnObject(string key)
    {
        Vector3 vPos = _objectSpawnAnchor.position + Vector3.one * UnityEngine.Random.Range(-0.1f, 0.1f);
        var obj = GameObject.Instantiate(GetObjectById(key), vPos, _objectSpawnAnchor.rotation);
        obj.SetActive(true);
    }

    GameObject GetObjectById(string id)
    {
        for(int q = 0; q < _loadObjects.Count; q++)
        {
            if(_loadObjects[q].Id.ToLower().Contains(id.ToLower()))
                return _loadObjects[q].ObjectTarget;
        }
        Assert.IsTrue(false, "Couldn't find the object..." + id);
        return null;
    }

    public void OnAnchorCreateComplete(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if(result == OVRSpatialAnchor.OperationResult.Success)
        {
            //curAnchorCache = anchor.gameObject;
            //anchor.gameObject.SetActive(true);
            Debug.Log("Anchor creation has been successful! " + anchor.Uuid.ToString());

            //listAnchorGUID.Add(anchor.Uuid);
            //PlayerPrefs.SetInt("AnchorCount", listAnchorGUID.Count);
            //for(int q = 0; q < listAnchorGUID.Count; ++q)
            //{
            PlayerPrefs.SetString("AnchorGUID", anchor.Uuid.ToString());
            //}
        }
        else 
            Debug.LogError("Anchor creation has been failed.!");
    }

    IEnumerator coTriggerActionWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action?.Invoke();
    }


    void OnEventServerMainLog(string msg)
    {
        _txtInfo.text = $"[Logger]\n{msg}\n{_dataCache}";
    }

    void OnEventServerDateReceived(string data)
    {
        _dataCache = data;
    }

    private void OnDestroy()
    {
        _transportServer.OnEventDataReceived -= OnEventServerDateReceived;
        _transportServer.OnEventMainLog -= OnEventServerMainLog;

        spatialAnchorCore.OnAnchorCreateCompleted.RemoveListener(OnAnchorCreateComplete);
    }



    /*public void InstantiateSpatialAnchor(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            prefab = new GameObject("Spatial Anchor");
        }

        var anchorGameObject = Instantiate(prefab, position, rotation);
        anchorGameObject.SetActive(true);
        var trAnchor = anchorGameObject.transform.Find("DemoAnchorPlacementBuildingBlock");
        var spatialAnchor = trAnchor.gameObject.AddComponent<OVRSpatialAnchor>();
        InitSpatialAnchorAsync(spatialAnchor);
    }

    private async void InitSpatialAnchorAsync(OVRSpatialAnchor anchor)
    {
        await WaitForInit(anchor);
        if (Result == OVRSpatialAnchor.OperationResult.Failure)
        {
            //OnAnchorCreateCompleted?.Invoke(anchor, Result);
            Debug.LogError("Anchor creation has been failed.!");
            return;
        }

        Debug.Log("Anchor creation has been successful!");
        PlayerPrefs.SetString("AnchorUuid", anchor.Uuid.ToString());

        //await SaveAsync(anchor);
        //OnAnchorCreateCompleted?.Invoke(anchor, Result);
    }

    protected async Task WaitForInit(OVRSpatialAnchor anchor)
    {
        float timeoutThreshold = 5f;
        float startTime = Time.time;

        while (anchor && !anchor.Created)
        {
            if (Time.time - startTime >= timeoutThreshold)
            {
                Debug.LogWarning($"[{nameof(SpatialAnchorCoreBuildingBlock)}] Failed to create the spatial anchor due to timeout.");
                Result = OVRSpatialAnchor.OperationResult.Failure;
                return;
            }
            await Task.Yield();
        }

        if (anchor == null)
        {
            Debug.LogWarning($"[{nameof(SpatialAnchorCoreBuildingBlock)}] Failed to create the spatial anchor.");
            Result = OVRSpatialAnchor.OperationResult.Failure;
        }
    }*/
}
