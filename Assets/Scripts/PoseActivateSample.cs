using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseActivateSample : MonoBehaviour
{
    [SerializeField] GameObject _targetObject;
    
    [SerializeField, Interface(typeof(IActiveState))]
    UnityEngine.Object _activeState;

    [SerializeField] Transform _objectAnchor;
    [SerializeField] GameObject _objectCache;


    bool _oldState = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool curState = (_activeState as IActiveState).Active;
        if(_oldState != curState)
        {
            if(curState) 
                _targetObject.SetActive( !_targetObject.activeSelf );

            _oldState = curState;
        }
    }

    public void OnBtnObjectLoadClicked()
    {
        if (_objectAnchor == null || _objectCache == null)
            return;
        
        
        Vector3 vPos = _objectAnchor.position + Vector3.one * Random.Range(-0.1f, 0.1f);

        GameObject.Instantiate(_objectCache, vPos, _objectAnchor.rotation);

        Debug.Log("OnBtnObject Load Clicked...");
    }
}
