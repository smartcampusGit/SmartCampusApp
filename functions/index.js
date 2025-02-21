const functions = require("firebase-functions/v1");
const admin = require("firebase-admin");

// Initialize Firebase Admin
admin.initializeApp();

// Cloud Function: Trigger when a new user is created
exports.createUserInFirestore = functions.auth.user().onCreate(async (user) => {
    const db = admin.firestore();
    const userRef = db.collection("users").doc(user.uid);

    try {
        await userRef.set({
            email: user.email,
            status: "pending",
            createdAt: admin.firestore.FieldValue.serverTimestamp()
        });
        console.log(`User ${user.email} added to Firestore with 'pending' status.`);
    } catch (error) {
        console.error("Error adding user to Firestore:", error);
    }
});

