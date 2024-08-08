using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class RequestData
{
    public abstract Dictionary<string, string> Headers { get; }

    public virtual List<Form> Forms { get; }

    public abstract string Route { get; }

    public abstract bool HasInPHPServer { get; }

    public class Form
    {
        public string fieldName = string.Empty;
        public string fileName = string.Empty;
        public byte[] content = new byte[0];
        public string value = string.Empty;
        public string mimeType = string.Empty;
    }
}
#region AGORA IO POST
[System.Serializable]
public class AgoraPostToken : RequestData
{
    public string channel;
    public string uid;
    public string expireTs;

    [System.NonSerialized]
    private Dictionary<string, string> header = new Dictionary<string, string>()
        {
            {"Content-Type", "application/json" },
            {"Accept", "application/json" }
        };

    public override Dictionary<string, string> Headers { get { return header; } }

    public override string Route => "agoraio/token";

    public override bool HasInPHPServer { get { return true; } }
}

[System.Serializable]
public class ReturnAgoraToken
{
    public string rtcToken;
}
#endregion
