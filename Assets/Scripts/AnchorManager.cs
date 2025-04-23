using UnityEngine;
using Oculus.Interaction;
using Oculus.Platform;
using Meta.XR.BuildingBlocks;
using Oculus.Interaction.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AnchorManager : MonoBehaviour
{
    public GameObject anchorPreview; // �ǽð� �̸����� ������ (��: ���� ��ü)
    public GameObject anchorVisual;  // ������ ��Ŀ ������
    private GameObject currentPreview;
    private OVRSpatialAnchor _currentAnchor;

    //private OVRRoomLayoutManager roomLayoutManager;

    [SerializeField]
    private float maxRayDistance = 10f; // Ray �ִ� �Ÿ�
    //[SerializeField] GameObject anchorPrefab;
    [SerializeField] SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

    [SerializeField] Controller OVRRightController;
    List<Guid> listAnchorGUID = new List<Guid>();

    void Start()
    {
        // Passthrough Ȱ��ȭ
        OVRManager.instance.isInsightPassthroughEnabled = true;

        // �̸����� ��Ŀ �ʱ�ȭ
        currentPreview = Instantiate(anchorPreview);
        currentPreview.SetActive(false);

        int cnt = PlayerPrefs.GetInt("AnchorCount");
        for(int q = 0; q < cnt; ++q)
        {
            string guid = PlayerPrefs.GetString("AnchorGUID");
            Guid id;
            if(Guid.TryParse(guid, out id))
                listAnchorGUID.Add(id);
        }
        _spatialAnchorCore.LoadAndInstantiateAnchors(anchorVisual, listAnchorGUID);
    }

    void Update()
    {
        // ��Ʈ�ѷ� �Ǵ� �ü����� Ray �߻�
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        //Ray ray = new Ray(OVRRightController.transform.position, OVRRightController.transform.forward);

        // Passthrough ȯ�濡�� Raycast (Collider ���̵� ȯ�� ����)
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            // Ray�� ��ü(�� ��)�� ������ �̸����� ǥ��
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // ���鿡 ����

            // ��ư �Է����� ��Ŀ ����
            if (OVRInput.GetDown(OVRInput.Button.One)) // A ��ư
            {
                CreateAnchorAtHitPoint(hit.point, hit.normal);
            }
        }
        else
        {
            // Ray�� �ƹ��͵� �� ������ �ִ� �Ÿ� ������ �̸����� ǥ�� (�ɼ�)
            currentPreview.SetActive(true);
            currentPreview.transform.position = ray.GetPoint(maxRayDistance);
            currentPreview.transform.rotation = Quaternion.identity; // �⺻ ����
            
            // ��ư �Է� �� �ִ� �Ÿ� ������ ��Ŀ ����
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                CreateAnchorAtHitPoint(ray.GetPoint(maxRayDistance), Vector3.up);
            }
        }
    }

    public void OnAnchorCreateComplete(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if(result == OVRSpatialAnchor.OperationResult.Success)
        {
            listAnchorGUID.Add(anchor.Uuid);
            PlayerPrefs.SetInt("AnchorCount", listAnchorGUID.Count);
            for(int q = 0; q < listAnchorGUID.Count; ++q)
            {
                PlayerPrefs.SetString("AnchorGUID", listAnchorGUID[q].ToString());
            }
        }
    }

    void CreateAnchorAtHitPoint(Vector3 position, Vector3 normal)
    {
        _spatialAnchorCore.InstantiateSpatialAnchor(anchorVisual, position, Quaternion.identity);
        
        // Spatial Anchor ����
        /*GameObject anchorObject = new GameObject("WallAnchor");
        anchorObject.transform.position = position;
        anchorObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        _currentAnchor = anchorObject.AddComponent<OVRSpatialAnchor>();
        */
        /*_currentAnchor.CreateSpatialAnchor((anchor, success) =>
        {
            if (success)
            {
                Debug.Log("��Ŀ ���� ����: " + anchor.transform.position);
                Instantiate(anchorVisual, anchor.transform.position, anchor.transform.rotation);
                SaveAnchor(anchor);
            }
            else
            {
                Debug.LogError("��Ŀ ���� ����");
            }
        });*/
    }
    /*
    void SaveAnchor(OVRSpatialAnchor anchor)
    {
        anchor.Save((a, success) =>
        {
            if (success)
            {
                PlayerPrefs.SetString("WallAnchorUUID", anchor.Uuid.ToString());
                Debug.Log("��Ŀ ���� �Ϸ�!");
            }
        });
    }

    // ����� ��Ŀ �ε� (�ɼ�)
    void LoadSavedAnchor()
    {
        OVRSpatialAnchor.LoadOptions options = new OVRSpatialAnchor.LoadOptions();
        OVRSpatialAnchor.LoadUnboundAnchors(options, (OVRSpatialAnchor.UnboundAnchor[] anchors) =>
        {
            foreach (var unbound in anchors)
            {
                unbound.Localize((anchor, success) =>
                {
                    if (success && anchor.Uuid.ToString() == PlayerPrefs.GetString("WallAnchorUUID"))
                    {
                        Instantiate(anchorVisual, anchor.transform.position, anchor.transform.rotation);
                        Debug.Log("��Ŀ �ε� ����!");
                    }
                });
            }
        });
    }*/
}