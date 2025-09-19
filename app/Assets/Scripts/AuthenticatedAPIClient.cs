using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using Reown.AppKit.Unity;

/// <summary>
/// Handles authenticated API requests to the backend server
/// Manages SIWE authentication tokens and session state
/// </summary>
public class AuthenticatedAPIClient : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Base URL of your backend server")]
    public string serverBaseUrl = "https://rivals.nyc";
    
    [Tooltip("Reference to AppKit initializer for authentication")]
    public AppKitInitializer appKitInitializer;
    
    [Tooltip("Reference to SIWE authentication component")]
    public SIWEAuthentication siweAuthentication;
    
    // Session management
    private string authToken = null;
    private bool isAuthenticated = false;
    private string connectedAddress = null;
    
    // Events
    public event System.Action OnAuthenticationSuccess;
    public event System.Action OnAuthenticationFailure;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (appKitInitializer == null)
        {
            appKitInitializer = FindFirstObjectByType<AppKitInitializer>();
        }
        
        if (siweAuthentication == null)
        {
            siweAuthentication = FindFirstObjectByType<SIWEAuthentication>();
        }
        
        // Subscribe to wallet events
        if (appKitInitializer != null)
        {
            // Subscribe to authentication events
            try
            {
                AppKit.AccountConnected += OnWalletConnected;
                AppKit.AccountDisconnected += OnWalletDisconnected;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AuthenticatedAPIClient: Could not subscribe to wallet events: {e.Message}");
            }
        }
        
        // Subscribe to SIWE authentication events
        if (siweAuthentication != null)
        {
            siweAuthentication.OnAuthenticationResult += OnSIWEAuthenticationResult;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        try
        {
            AppKit.AccountConnected -= OnWalletConnected;
            AppKit.AccountDisconnected -= OnWalletDisconnected;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"AuthenticatedAPIClient: Could not unsubscribe from wallet events: {e.Message}");
        }
        
        if (siweAuthentication != null)
        {
            siweAuthentication.OnAuthenticationResult -= OnSIWEAuthenticationResult;
        }
    }
    
    void OnWalletConnected(object sender, EventArgs e)
    {
        Debug.Log("AuthenticatedAPIClient: Wallet connected, will authenticate on next API call");
        UpdateAuthenticationState();
    }
    
    void OnWalletDisconnected(object sender, EventArgs e)
    {
        Debug.Log("AuthenticatedAPIClient: Wallet disconnected, clearing authentication");
        ClearAuthentication();
    }
    
    void OnSIWEAuthenticationResult(bool success)
    {
        if (success)
        {
            Debug.Log("AuthenticatedAPIClient: SIWE authentication successful");
            UpdateAuthenticationState();
            OnAuthenticationSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("AuthenticatedAPIClient: SIWE authentication failed");
            ClearAuthentication();
            OnAuthenticationFailure?.Invoke();
        }
    }
    
    void UpdateAuthenticationState()
    {
        if (appKitInitializer != null)
        {
            bool walletConnected = appKitInitializer.IsWalletConnected;
            connectedAddress = appKitInitializer.ConnectedWalletAddress;
            
            // Check SIWE authentication if enabled
            bool siweAuthRequired = appKitInitializer.IsSIWEEnabled;
            bool siweAuthenticated = siweAuthentication != null && siweAuthentication.IsAuthenticated;
            
            if (siweAuthRequired)
            {
                // SIWE is required - check both wallet connection and SIWE authentication
                isAuthenticated = walletConnected && siweAuthenticated;
                Debug.Log($"AuthenticatedAPIClient: SIWE mode - Wallet: {walletConnected}, SIWE: {siweAuthenticated}, Final: {isAuthenticated}");
            }
            else
            {
                // SIWE not required - just check wallet connection
                isAuthenticated = walletConnected;
                Debug.Log($"AuthenticatedAPIClient: Non-SIWE mode - Authenticated: {isAuthenticated}");
            }
            
            if (isAuthenticated && !string.IsNullOrEmpty(connectedAddress))
            {
                Debug.Log($"AuthenticatedAPIClient: Authentication state updated - connected to {connectedAddress}");
            }
        }
    }
    
    void ClearAuthentication()
    {
        authToken = null;
        isAuthenticated = false;
        connectedAddress = null;
    }
    
    /// <summary>
    /// Make an authenticated POST request to the API
    /// </summary>
    public IEnumerator MakeAuthenticatedRequest(string endpoint, string jsonBody, System.Action<string> onSuccess, System.Action<string> onError)
    {
        UpdateAuthenticationState();
        
        if (!isAuthenticated)
        {
            onError?.Invoke("Wallet not connected. Please connect your wallet first.");
            yield break;
        }
        
        string url = $"{serverBaseUrl}/api/{endpoint}";
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonBody, "application/json"))
        {
            // Add authentication headers if we have a session
            // Note: In this implementation, we rely on the Reown AppKit SIWE integration
            // The session will be managed by NextAuth cookies on the server side
            
            Debug.Log($"AuthenticatedAPIClient: Making request to {url} from wallet {connectedAddress}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"AuthenticatedAPIClient: Request successful: {request.downloadHandler.text}");
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                string errorMessage = $"Request failed: {request.error}";
                if (request.responseCode == 401)
                {
                    errorMessage = "Authentication required. Please sign in with your wallet.";
                    OnAuthenticationFailure?.Invoke();
                }
                
                Debug.LogError($"AuthenticatedAPIClient: {errorMessage}");
                onError?.Invoke(errorMessage);
            }
        }
    }
    
    /// <summary>
    /// Check if the client is ready to make authenticated requests
    /// </summary>
    public bool IsReadyForRequests()
    {
        UpdateAuthenticationState();
        return isAuthenticated && !string.IsNullOrEmpty(connectedAddress);
    }
    
    /// <summary>
    /// Trigger SIWE authentication if required and not already authenticated
    /// </summary>
    public void TriggerSIWEAuthenticationIfNeeded()
    {
        if (appKitInitializer == null || !appKitInitializer.IsWalletConnected)
        {
            Debug.LogWarning("AuthenticatedAPIClient: Wallet not connected, cannot trigger SIWE authentication");
            return;
        }
        
        if (!appKitInitializer.IsSIWEEnabled)
        {
            Debug.Log("AuthenticatedAPIClient: SIWE not enabled, skipping authentication");
            return;
        }
        
        if (siweAuthentication != null && !siweAuthentication.IsAuthenticated)
        {
            Debug.Log("AuthenticatedAPIClient: Triggering SIWE authentication...");
            siweAuthentication.TriggerAuthentication();
        }
        else if (siweAuthentication == null)
        {
            Debug.LogWarning("AuthenticatedAPIClient: SIWEAuthentication component not found!");
        }
    }
    
    /// <summary>
    /// Get the currently connected wallet address
    /// </summary>
    public string GetConnectedAddress()
    {
        UpdateAuthenticationState();
        return connectedAddress;
    }
    
    /// <summary>
    /// Force re-authentication (useful if session expires)
    /// </summary>
    public void ForceReauthentication()
    {
        ClearAuthentication();
        if (appKitInitializer != null)
        {
            appKitInitializer.OpenWalletModal();
        }
    }
}
