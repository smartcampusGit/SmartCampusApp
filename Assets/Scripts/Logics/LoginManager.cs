using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public LoginUI loginUI;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void OnLoginClicked()
    {
        loginUI.HideMessage();

        string email = emailField.text.Trim();
        string password = passwordField.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {

            if (task.IsFaulted || task.Exception != null)
            {
                string error = task.Exception?.InnerExceptions[0].Message.ToLower() ?? "unknown";

                if (error.Contains("no user record"))
                    loginUI.ShowUserNotFound();
                else if (error.Contains("password is invalid"))
                    loginUI.ShowWrongPassword();
                else
                    loginUI.ShowMessage("Login failed", "error");

                return;
            }

            FirebaseUser user = task.Result.User;
            string uid = user.UserId;

            // Step 1: Get token with claims and check role
            user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
            {
                if (tokenTask.IsFaulted)
                {
                    loginUI.ShowMessage("Token error", "error");
                    return;
                }

                string jwt = tokenTask.Result;
                Dictionary<string, object> claims = DecodeJwt(jwt);

                if (!claims.ContainsKey("role"))
                {
                    loginUI.ShowMessage("No role assigned", "error");
                    return;
                }

                string role = claims["role"].ToString();
                Debug.Log("Role: " + role);

                if (role == "sysadmin")
                {
                    Debug.Log("Logged in as sysadmin");
                    loginUI.ShowLoginSuccess("Sysadmin");
                    // TODO: Load sysadmin scene here
                    // SceneManager.LoadScene("SysAdminScene");
                    return;
                }

                if (role == "admin")
                {
                    string campusId = claims.ContainsKey("campusId") ? claims["campusId"].ToString() : null;
                    if (campusId == null)
                    {
                        loginUI.ShowMessage("Login error: missing campus assignment", "error");
                        return;
                    }

                    Debug.Log("Campus ID: " + campusId);
                    SessionData.Instance.CampusId = campusId;

                    db.Collection("Admin_Profiles").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(snapshotTask =>
                    {
                        if (snapshotTask.IsFaulted || !snapshotTask.Result.Exists)
                        {
                            loginUI.ShowUserNotFound();
                            return;
                        }

                        var doc = snapshotTask.Result;
                        string status = doc.GetValue<string>("status");
                        string adminName = doc.GetValue<string>("adminName");

                        if (status == "pending")
                        {
                            loginUI.ShowWaitingApproval();
                            return;
                        }
                        if (status == "rejected")
                        {
                            loginUI.ShowRejected();
                            return;
                        }
                        if (status != "active")
                        {
                            loginUI.ShowMessage("Unknown account status", "error");
                            return;
                        }

                        db.Collection("Campuses").Document(campusId).GetSnapshotAsync().ContinueWithOnMainThread(campusTask =>
                        {
                            if (!campusTask.IsFaulted && campusTask.Result.Exists)
                            {
                                string campusName = campusTask.Result.GetValue<string>("name");
                                Debug.Log("Campus Name: " + campusName);
                            }

                            loginUI.ShowLoginSuccess(adminName);
                            // TODO: Load admin scene here
                            // SceneManager.LoadScene("MyBuildingsScene");
                        });
                    });
                    return;
                }

                loginUI.ShowMessage("Invalid role", "error");
            });
        });
    }

    private Dictionary<string, object> DecodeJwt(string jwt)
    {
        string payload = jwt.Split('.')[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        return MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
    }
}