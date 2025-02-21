using UnityEngine;
using TMPro; // For TextMeshPro
using Firebase.Auth; // Firebase Authentication
using UnityEngine.UI; // For Button interaction

public class CreateAccountHandler : MonoBehaviour
{
    public TMP_InputField emailInputField; // Email input
    public TMP_InputField passwordInputField; // Password input
    public Button createAccountButton; // Button for account creation
    public TMP_Text feedbackText; // Text field to display feedback


    private FirebaseAuth auth;

    void Start()
    {

        // Initialize Firebase Authentication
        auth = FirebaseAuth.DefaultInstance;

        // Add listener to the create account button
        createAccountButton.onClick.AddListener(() => CreateAccount(emailInputField.text, passwordInputField.text));
    }

    void CreateAccount(string email, string password)
    {
        // Check if inputs are valid
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email or password cannot be empty.");
            return;
        }

        // Attempt to create user in Firebase Authentication
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                feedbackText.text = "User registered successfully!";
               // Debug.Log("User registered successfully!");
            }
            else
            {
                feedbackText.text = "Error registering user: " + task.Exception?.Message;
               // Debug.LogError("Error registering user: " + task.Exception);
            }
        });
    }
}
