using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

    public delegate void HttpRequestReturn(string mensage, long errorCode, string value = null);
    public delegate void HttpRequestReturn<T>(T requestObject, long errorCode, string messageError) where T : new();

public static class HttpRequest
{
    private static string m_apiUrl = "https://api-devunity2.virtual.town/api/";

    public static string API_URL
    {
        get
        {
#if UNITY_EDITOR
            return m_apiUrl;
#else
                return m_apiUrl;
#endif
        }
        set
        {
            m_apiUrl = value;
        }
    }

    private static Dictionary<long, string> m_httpResponse = new Dictionary<long, string>()
        {
            { 0, "Servidor offline." },
            { 400, "Problema com a requisição." },
            { 401, "Ação não autorizada." },
            { 404, "Não foi possível processar a requisição." },
            { 500, "Não foi possível se comunicar com o servidor." },
            { 502, "Conexão com o servidor interrompida." },
            { 503, "Serviço indisponível." }
        };

    [System.Serializable]
    public class DataLogin
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class ReturnLogin
    {
        public string token;
        public bool userValidated;
    }

    public static IEnumerator Get<T>(HttpRequestReturn<T> callback, string query = "") where T : RequestData, new()
    {
        T deserializedData = new T();

        if (!deserializedData.HasInPHPServer)
        {
            callback(deserializedData, 200, string.Empty);
            yield break;
        }

        //Request to login
        UnityWebRequest request = new UnityWebRequest(string.Concat(API_URL, deserializedData.Route + query), "GET");
        Debug.LogWarning("" + request.url);

       /* if (!string.IsNullOrEmpty(Managers.GameManager.Instance.GameData.baseData.language))
            request.SetRequestHeader("language", Managers.GameManager.Instance.GameData.baseData.language);*/

        foreach (var kvp in deserializedData.Headers)
        {
            request.SetRequestHeader(kvp.Key, kvp.Value);
        }

        /*if (!string.IsNullOrEmpty(Managers.GameManager.Instance.GameData.baseData.token))
            request.SetRequestHeader("Authorization", "Bearer " + Managers.GameManager.Instance.GameData.baseData.token);*/

        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            callback(
                null,
                request.responseCode,
                m_httpResponse.ContainsKey(request.responseCode) ? m_httpResponse[request.responseCode] : request.responseCode.ToString()
            );
            yield break;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log(
            "GetValue from link " + API_URL + deserializedData.Route + query + "\n" +
            "Data:\n" +
            request.downloadHandler.text
        );
#endif

        JsonUtility.FromJsonOverwrite(request.downloadHandler.text, deserializedData);

        callback(deserializedData, 200, string.Empty);
    }

    public static IEnumerator Post<T>(RequestData postData, HttpRequestReturn<T> callback, string query = "") where T : new()
    {
        T deserializedData = new T();

        if (!postData.HasInPHPServer)
        {
            callback(deserializedData, 200, string.Empty);
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(string.Concat(API_URL, postData.Route + query), "POST");

        foreach (var kvp in postData.Headers)
        {
            request.SetRequestHeader(kvp.Key, kvp.Value);
        }

        /*if (!string.IsNullOrEmpty(Managers.GameManager.Instance.GameData.baseData.token))
            request.SetRequestHeader("Authorization", "Bearer " + Managers.GameManager.Instance.GameData.baseData.token);*/


       /* if (!string.IsNullOrEmpty(Managers.GameManager.Instance.GameData.baseData.language))
            request.SetRequestHeader("language", Managers.GameManager.Instance.GameData.baseData.language);*/

        if (postData.Forms != null && postData.Forms.Count > 0)
        {
            WWWForm formData = new WWWForm();
            foreach (var f in postData.Forms)
            {
                if (f.content != null && f.content.Length > 0)
                {
                    formData.AddBinaryData(f.fieldName, f.content, f.fileName, f.mimeType);
                }
                else
                {
                    formData.AddField(f.fieldName, f.value);
                }
            }

            request = UnityWebRequest.Post(string.Concat(API_URL, postData.Route), formData);

            foreach (var kvp in postData.Headers)
            {
                request.SetRequestHeader(kvp.Key, kvp.Value);
            }

            /*if (!string.IsNullOrEmpty(Managers.GameManager.Instance.GameData.baseData.token))
                request.SetRequestHeader("Authorization", "Bearer " + Managers.GameManager.Instance.GameData.baseData.token);*/
        }
        else
        {
            string jsonData = UnityEngine.JsonUtility.ToJson(postData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        }

        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        Debug.Log("Initializing WebRequest from " + postData.ToString());
        yield return request.SendWebRequest();
        Debug.Log("Finished WebRequest from " + postData.ToString());

        Debug.Log("Parsing Text result to " + postData.ToString() + " in " + deserializedData.ToString());
        JsonUtility.FromJsonOverwrite(request.downloadHandler.text, deserializedData);
        Debug.Log("Finished Parse to " + postData.ToString() + " in " + deserializedData.ToString());

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error + " Link: " + string.Concat(API_URL, postData.Route + query));
            callback(
                deserializedData,
                request.responseCode,
                m_httpResponse.ContainsKey(request.responseCode) ? m_httpResponse[request.responseCode] : string.Empty
            );
            yield break;
        }

        //Debug.Log("Parsing Text result to " + postData.ToString() + " in " + deserializedData.ToString());
        //JsonUtility.FromJsonOverwrite(request.downloadHandler.text, deserializedData);
        //Debug.Log("Finished Parse to " + postData.ToString() + " in " + deserializedData.ToString());

        callback(deserializedData, 200, string.Empty);
    }
}