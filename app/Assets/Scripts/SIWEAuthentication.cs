using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using Reown.AppKit.Unity;

/// <summary>
/// Handles SIWE (Sign In With Ethereum) authentication flow
/// Manages message creation, signing, and verification with the backend
/// </summary>
public class SIWEAuthentication : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Base URL of your backend server")]
    public string serverBaseUrl = "https://rivals.nyc";
    
    [Tooltip("Reference to AppKit initializer")]
    public AppKitInitializer appKitInitializer;
    
    // Authentication state
    private bool isAuthenticated = false;
    private string currentAddress = null;
    private string currentChainId = null;
    
    // Events
    public event System.Action<bool> OnAuthenticationResult;
    
    void Start()
    {
        // Auto-find AppKit initializer if not assigned
        if (appKitInitializer == null)
        {
            appKitInitializer = FindFirstObjectByType<AppKitInitializer>();
        }
        
        // Subscribe to wallet events
        try
        {
            AppKit.AccountConnected += OnWalletConnected;
            AppKit.AccountDisconnected += OnWalletDisconnected;
            AppKit.AccountChanged += OnWalletChanged;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SIWEAuthentication: Could not subscribe to wallet events: {e.Message}");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        try
        {
            AppKit.AccountConnected -= OnWalletConnected;
            AppKit.AccountDisconnected -= OnWalletDisconnected;
            AppKit.AccountChanged -= OnWalletChanged;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SIWEAuthentication: Could not unsubscribe from wallet events: {e.Message}");
        }
    }
    
    void OnWalletConnected(object sender, EventArgs e)
    {
        Debug.Log("SIWEAuthentication: Wallet connected, starting SIWE authentication...");
        StartCoroutine(AuthenticateWithSIWE());
    }
    
    void OnWalletDisconnected(object sender, EventArgs e)
    {
        Debug.Log("SIWEAuthentication: Wallet disconnected, clearing authentication");
        ClearAuthentication();
    }
    
    void OnWalletChanged(object sender, EventArgs e)
    {
        Debug.Log("SIWEAuthentication: Wallet changed, re-authenticating...");
        StartCoroutine(AuthenticateWithSIWE());
    }
    
    void ClearAuthentication()
    {
        isAuthenticated = false;
        currentAddress = null;
        currentChainId = null;
        OnAuthenticationResult?.Invoke(false);
    }
    
    /// <summary>
    /// Perform SIWE authentication flow
    /// </summary>
    public IEnumerator AuthenticateWithSIWE()
    {
        if (appKitInitializer == null || !appKitInitializer.IsWalletConnected)
        {
            Debug.LogWarning("SIWEAuthentication: Wallet not connected");
            OnAuthenticationResult?.Invoke(false);
            yield break;
        }
        
        currentAddress = appKitInitializer.ConnectedWalletAddress;
        if (string.IsNullOrEmpty(currentAddress))
        {
            Debug.LogError("SIWEAuthentication: Could not get wallet address");
            OnAuthenticationResult?.Invoke(false);
            yield break;
        }
        
        Debug.Log($"SIWEAuthentication: Starting authentication for address {currentAddress}");
        
        // Step 1: Get nonce from server
        string nonce = null;
        yield return StartCoroutine(GetNonce((result) => nonce = result));
        
        if (string.IsNullOrEmpty(nonce))
        {
            Debug.LogError("SIWEAuthentication: Failed to get nonce");
            OnAuthenticationResult?.Invoke(false);
            yield break;
        }
        
        // Step 2: Create SIWE message
        string message = CreateSIWEMessage(currentAddress, nonce);
        Debug.Log($"SIWEAuthentication: Created SIWE message: {message}");
        
        // Step 3: Request signature from wallet
        string signature = null;
        yield return StartCoroutine(RequestSignature(message, (result) => signature = result));
        
        if (string.IsNullOrEmpty(signature))
        {
            Debug.LogError("SIWEAuthentication: Failed to get signature");
            OnAuthenticationResult?.Invoke(false);
            yield break;
        }
        
        // Step 4: Verify signature with server
        bool verified = false;
        yield return StartCoroutine(VerifySignature(message, signature, (result) => verified = result));
        
        if (verified)
        {
            isAuthenticated = true;
            Debug.Log("SIWEAuthentication: Authentication successful!");
            OnAuthenticationResult?.Invoke(true);
        }
        else
        {
            Debug.LogError("SIWEAuthentication: Authentication failed");
            OnAuthenticationResult?.Invoke(false);
        }
    }
    
    /// <summary>
    /// Get nonce from server
    /// </summary>
    IEnumerator GetNonce(System.Action<string> callback)
    {
        string url = $"{serverBaseUrl}/api/auth/csrf";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<NonceResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.csrfToken);
                }
                catch (Exception e)
                {
                    Debug.LogError($"SIWEAuthentication: Failed to parse nonce response: {e.Message}");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"SIWEAuthentication: Failed to get nonce: {request.error}");
                callback?.Invoke(null);
            }
        }
    }
    
    /// <summary>
    /// Create SIWE message according to EIP-4361
    /// </summary>
    string CreateSIWEMessage(string address, string nonce)
    {
        string domain = "rivals.nyc";
        string uri = serverBaseUrl;
        string version = "1";
        string chainId = "1"; // Mainnet - adjust based on your network
        string statement = "Please sign with your account to authenticate with Rivals";
        string issuedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        // Build the SIWE message according to EIP-4361 specification
        string message = domain + " wants you to sign in with your Ethereum account:\n" +
                        address + "\n\n" +
                        statement + "\n\n" +
                        "URI: " + uri + "\n" +
                        "Version: " + version + "\n" +
                        "Chain ID: " + chainId + "\n" +
                        "Nonce: " + nonce + "\n" +
                        "Issued At: " + issuedAt;
        
        return message;
    }
    
    /// <summary>
    /// Request signature from wallet using AppKit
    /// </summary>
    IEnumerator RequestSignature(string message, System.Action<string> callback)
    {
        Debug.Log("SIWEAuthentication: Requesting signature from wallet...");
        Debug.Log($"Message to sign: {message}");
        
        // Try to use AppKit's signature methods
        // Common Unity AppKit signature APIs might be:
        bool signatureComplete = false;
        string resultSignature = null;
        bool hasError = false;
        
        // Method 1: Try personal_sign
        Debug.Log("SIWEAuthentication: Attempting personal_sign...");
        
        try
        {
            // This is the typical way to request a signature in Web3 wallets
            // We'll use the generic AppKit request mechanism if available
            RequestWalletSignature(message, (signature) => {
                resultSignature = signature;
                signatureComplete = true;
            });
        }
        catch (Exception apiException)
        {
            Debug.LogError($"SIWEAuthentication: AppKit signature API error: {apiException.Message}");
            Debug.LogWarning("SIWEAuthentication: Falling back to alternative signature method...");
            
            // Alternative: Show manual instructions to user
            ShowManualSignatureInstructions(message);
            hasError = true;
        }
        
        // If there was an error, exit early
        if (hasError)
        {
            callback?.Invoke(null);
            yield break;
        }
        
        // Wait for signature result
        float timeout = 60f; // 60 second timeout
        float elapsed = 0f;
        
        while (!signatureComplete && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (signatureComplete && !string.IsNullOrEmpty(resultSignature))
        {
            Debug.Log($"SIWEAuthentication: Signature received: {resultSignature.Substring(0, 10)}...");
            callback?.Invoke(resultSignature);
        }
        else
        {
            Debug.LogError("SIWEAuthentication: Signature request timed out or failed");
            callback?.Invoke(null);
        }
    }
    
    /// <summary>
    /// Request wallet signature using available AppKit methods
    /// </summary>
    private void RequestWalletSignature(string message, System.Action<string> callback)
    {
        try
        {
            // Try to find the correct AppKit signature method
            // This may vary based on the Unity AppKit version
            
            // Option 1: Check if AppKit has a direct sign method
            if (HasSigningCapabilities())
            {
                Debug.Log("SIWEAuthentication: Using AppKit signing capabilities");
                // Use the actual signing method here
                InvokeAppKitSigning(message, callback);
            }
            else
            {
                Debug.LogWarning("SIWEAuthentication: No direct signing capabilities found");
                Debug.LogWarning("User will need to sign manually in their wallet");
                callback?.Invoke(null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SIWEAuthentication: Error in wallet signature request: {e.Message}");
            callback?.Invoke(null);
        }
    }
    
    /// <summary>
    /// Check if AppKit has signing capabilities
    /// </summary>
    private bool HasSigningCapabilities()
    {
        // This would check if the AppKit has the necessary APIs
        // For now, return false to indicate we need manual implementation
        return false;
    }
    
    /// <summary>
    /// Invoke AppKit signing (placeholder for actual implementation)
    /// </summary>
    private void InvokeAppKitSigning(string message, System.Action<string> callback)
    {
        // This would contain the actual AppKit signing call
        // For example: AppKit.SignPersonalMessage(message, callback);
        Debug.LogWarning("SIWEAuthentication: AppKit signing not yet implemented");
        callback?.Invoke(null);
    }
    
    /// <summary>
    /// Show manual signature instructions to user
    /// </summary>
    private void ShowManualSignatureInstructions(string message)
    {
        Debug.Log("=== MANUAL SIGNATURE REQUIRED ===");
        Debug.Log("Please sign this message in your wallet:");
        Debug.Log(message);
        Debug.Log("================================");
        
        // In a real implementation, you might show a UI dialog with instructions
    }
    
    /// <summary>
    /// Verify signature with server
    /// </summary>
    IEnumerator VerifySignature(string message, string signature, System.Action<bool> callback)
    {
        string url = $"{serverBaseUrl}/api/auth/callback/credentials";
        
        var payload = new SIWEVerificationPayload
        {
            message = message,
            signature = signature
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonPayload, "application/json"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("SIWEAuthentication: Signature verified successfully");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"SIWEAuthentication: Signature verification failed: {request.error}");
                callback?.Invoke(false);
            }
        }
    }
    
    /// <summary>
    /// Check if currently authenticated
    /// </summary>
    public bool IsAuthenticated => isAuthenticated;
    
    /// <summary>
    /// Get current authenticated address
    /// </summary>
    public string CurrentAddress => currentAddress;
    
    /// <summary>
    /// Manually trigger authentication
    /// </summary>
    public void TriggerAuthentication()
    {
        if (appKitInitializer != null && appKitInitializer.IsWalletConnected)
        {
            StartCoroutine(AuthenticateWithSIWE());
        }
        else
        {
            Debug.LogWarning("SIWEAuthentication: Wallet not connected, cannot authenticate");
        }
    }
}

[Serializable]
public class NonceResponse
{
    public string csrfToken;
}

[Serializable]
public class SIWEVerificationPayload
{
    public string message;
    public string signature;
}
