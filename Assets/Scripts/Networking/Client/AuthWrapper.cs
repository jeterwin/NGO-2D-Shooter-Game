using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public static class AuthWrapper
{
    public static AuthState AuthState { get; private set; } = global::AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        // Check if we are authenticated
        if(AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        // If we try to auth but we are trying currently we do not
        // want to create a new request
        if(AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating!");
            await Authenticating();

            return AuthState;
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(300);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxTries)
    {
        AuthState = AuthState.Authenticating;

        int tries = 0;
        while(AuthState == AuthState.Authenticating && tries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if(AuthenticationService.Instance.IsSignedIn && 
                AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch(AuthenticationException authError)
            {
                // We fail to auth
                AuthState = AuthState.Error;
                Debug.LogError(authError);
            } 
            catch(RequestFailedException requestError)
            {
                // No network
                AuthState = AuthState.Error;
                Debug.LogError(requestError);
            }

            tries++;
            // Need a delay because we'll hit a rate limit
            await Task.Delay(1500);
        }

        // We went through all our tries and still didn't auth.
        if(AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning("Player did not sign in.");
            AuthState = AuthState.TimeOut;
        }
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
