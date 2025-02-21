using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public class FirebaseAuthHandler : MonoBehaviour
{
    public TMP_InputField emailInputField;   // Email input field
    public TMP_InputField passwordInputField; // Password input field
    public Button registerButton;           // Register button
    public TMP_Text feedbackText;               // Feedback text area

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Add listener for the Register button
        registerButton.onClick.AddListener(() => StartCoroutine(RegisterUser()));
    }

    IEnumerator RegisterUser()
    {
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        // Validate input fields
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Please enter both email and password.";
            yield break;
        }

        // Firebase Auth Registration
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            feedbackText.text = "Registration failed: " + registerTask.Exception.GetBaseException().Message;
            yield break;
        }

        // Get the registered user
        FirebaseUser newUser = registerTask.Result.User;

        var userData = new
        {
            email = newUser.Email,
            status = "pending",
            createdAt = Timestamp.GetCurrentTimestamp()
        };

        // Reference Firestore and add user document
        DocumentReference userRef = firestore.Collection("users").Document(newUser.UserId);
        var firestoreTask = userRef.SetAsync(userData);
        yield return new WaitUntil(() => firestoreTask.IsCompleted);

        if (firestoreTask.Exception != null)
        {
            feedbackText.text = "Firestore write failed: " + firestoreTask.Exception.GetBaseException().Message;
        }
        else
        {
            feedbackText.text = "Registration successful!\nAwait admin approval.";
        }
    }
}
