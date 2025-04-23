using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;
using System;

public class UnityTransportServer : MonoBehaviour
{
    private NetworkDriver driver;
    private List<NetworkConnection> connections;


    public Action<string> OnEventMainLog;
    public Action<string> OnEventDataReceived;

    // Start is called before the first frame update
    void Start()
    {
        driver = NetworkDriver.Create();
        // Debug.Log("Address " + NetworkEndpoint.AnyIpv4.Address);
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(9000);
        if (driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 9000");
            return;
        }
        driver.Listen();
        connections = new List<NetworkConnection>();// 16, Allocator.Persistent);
        Debug.Log("Server is listening....");

        StartCoroutine(coUpdate());
    }


    // Update is called once per frame
    void Update() { }

    IEnumerator coUpdate()
    {
        while(true)
        {
            driver.ScheduleUpdate().Complete();

            NetworkConnection c;
            while ((c = driver.Accept()) != default)
            {
                connections.Add(c);
                Debug.Log("Accepted a connection");
            }

            Debug.Log("Connected client...." + connections.Count);
            OnEventMainLog?.Invoke("Connected client...." + connections.Count);


            for (int i = 0; i < connections.Count; i++)
            {
                if (!connections[i].IsCreated)
                    continue;

                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection(connections[i], out var reader)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        var message = reader.ReadFixedString128();
                        Debug.Log($"Received: {message}");

                        OnEventDataReceived?.Invoke("Received.." + message);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected");
                        connections[i] = default;
                    }
                }
            }

            connections.RemoveAll(conn => conn==default);

            yield return new WaitForSeconds(1.0f);
        }
    }


    void OnDestroy()
    {
        driver.Dispose();
        // connections.Dispose();
    }
}
