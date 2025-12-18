using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Net;
using System.Net.Sockets;

public class DisplayJoinCode : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private Button copyCodeButton;
    [SerializeField] private SessionHolder sessionHolder;

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        DisplayCode();
    }

    private void DisplayCode()
    {
        if (sessionHolder.ActiveSession != null)
        {
            codeText.text = sessionHolder.ActiveSession?.Code ?? "";
            copyCodeButton.interactable = true;
        }

        else
        {
            codeText.text = GetLocalIPAddress().ToString();
            copyCodeButton.interactable = true;
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }

        return "127.0.0.1"; 
    }

    public void CopySessionCodeToClipboard()
    {
        // deselect the button when clicked
        EventSystem.current.SetSelectedGameObject(null);

        string code = codeText.text;

        // if there is no code (for some reason)
        if (string.IsNullOrEmpty(code))
            return;

        // copy the text to the clipboard
        GUIUtility.systemCopyBuffer = code;
    }
}
