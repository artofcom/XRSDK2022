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
    public GameObject anchorPreview; // 실시간 미리보기 프리팹 (예: 작은 구체)
    public GameObject anchorVisual;  // 고정된 앵커 프리팹
    private GameObject currentPreview;
    private OVRSpatialAnchor _currentAnchor;

    //private OVRRoomLayoutManager roomLayoutManager;

    [SerializeField]
    private float maxRayDistance = 10f; // Ray 최대 거리
    //[SerializeField] GameObject anchorPrefab;
    [SerializeField] SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

    [SerializeField] Controller OVRRightController;
    List<Guid> listAnchorGUID = new List<Guid>();

    void Start()
    {
        // Passthrough 활성화
        OVRManager.instance.isInsightPassthroughEnabled = true;

        // 미리보기 앵커 초기화
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
        // 컨트롤러 또는 시선에서 Ray 발사
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        //Ray ray = new Ray(OVRRightController.transform.position, OVRRightController.transform.forward);

        // Passthrough 환경에서 Raycast (Collider 없이도 환경 추정)
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            // Ray가 물체(벽 등)에 닿으면 미리보기 표시
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // 벽면에 수직

            // 버튼 입력으로 앵커 생성
            if (OVRInput.GetDown(OVRInput.Button.One)) // A 버튼
            {
                CreateAnchorAtHitPoint(hit.point, hit.normal);
            }
        }
        else
        {
            // Ray가 아무것도 안 맞으면 최대 거리 지점에 미리보기 표시 (옵션)
            currentPreview.SetActive(true);
            currentPreview.transform.position = ray.GetPoint(maxRayDistance);
            currentPreview.transform.rotation = Quaternion.identity; // 기본 방향
            
            // 버튼 입력 시 최대 거리 지점에 앵커 생성
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
        
        // Spatial Anchor 생성
        /*GameObject anchorObject = new GameObject("WallAnchor");
        anchorObject.transform.position = position;
        anchorObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        _currentAnchor = anchorObject.AddComponent<OVRSpatialAnchor>();
        */
        /*_currentAnchor.CreateSpatialAnchor((anchor, success) =>
        {
            if (success)
            {
                Debug.Log("앵커 생성 성공: " + anchor.transform.position);
                Instantiate(anchorVisual, anchor.transform.position, anchor.transform.rotation);
                SaveAnchor(anchor);
            }
            else
            {
                Debug.LogError("앵커 생성 실패");
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
                Debug.Log("앵커 저장 완료!");
            }
        });
    }

    // 저장된 앵커 로드 (옵션)
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
                        Debug.Log("앵커 로드 성공!");
                    }
                });
            }
        });
    }*/
}