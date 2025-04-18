/* eslint-disable max-len */
const express = require("express");
const cors = require("cors");
const {Readable} = require("stream");
const admin = require("firebase-admin");
const {v4: uuidv4} = require("uuid");
const fileParser = require("express-multipart-file-parser");

const app = express();

app.use(cors({origin: true}));
app.use(fileParser);

app.post("/registerAdmin", async (req, res) => {
  try {
    const bucket = admin.storage().bucket();
    const now = admin.firestore.Timestamp.now();

    // Extract form fields
    const {
      email,
      password,
      adminName,
      role,
      campusName,
      city,
      country,
      description,
    } = req.body;

    if (!email || !password || !adminName || !role || !campusName || !city || !country) {
      return res.status(400).json({error: "Missing required form fields."});
    }

    const logo = req.files.find((f) => f.fieldname === "logo");
    const map = req.files.find((f) => f.fieldname === "map");
    const approval = req.files.find((f) => f.fieldname === "approval");
    const profilePicture = req.files.find((f) => f.fieldname === "profilePicture");


    if (!logo || !map || !approval || !profilePicture) {
      return res.status(400).json({error: "Missing required file uploads."});
    }

    // Check for duplicate campus name
    const campusQuery = await admin.firestore()
        .collection("Campuses")
        .where("name", "==", campusName)
        .limit(1)
        .get();

    if (!campusQuery.empty) {
      return res.status(400).json({error: "Campus name already exists."});
    }

    // Check for duplicate email in Firebase Auth
    let userRecord;
    try {
      userRecord = await admin.auth().getUserByEmail(email);
      if (userRecord) {
        return res.status(400).json({error: "Email is already registered."});
      }
    } catch (err) {
      if (err.code !== "auth/user-not-found") {
        console.error("Firebase Auth lookup error:", err);
        return res.status(500).json({error: "Error checking email."});
      }
    }

    // Create Firebase Auth user
    userRecord = await admin.auth().createUser({email, password});
    const uid = userRecord.uid;

    const sanitize = (str) =>
      str.trim().toLowerCase().replace(/[^a-z0-9]+/g, "_").replace(/^_|_$/g, "");

    const campusId = uuidv4();
    const campusFolder = sanitize(campusName);
    const campusPath = `campuses/${campusFolder}`;

    // Function to uplaod file URL + token to Firestore
    const uploadFile = async (file, destination) => {
      const token = uuidv4();

      const metadata = {
        metadata: {
          firebaseStorageDownloadTokens: token, // token for access
        },
        contentType: file.mimetype,
      };

      const fileRef = bucket.file(destination);
      const stream = fileRef.createWriteStream({metadata});

      Readable.from(file.buffer).pipe(stream);

      await new Promise((resolve, reject) => {
        stream.on("finish", resolve);
        stream.on("error", reject);
      });

      // Return tokenized download URL
      return `https://firebasestorage.googleapis.com/v0/b/${bucket.name}/o/${encodeURIComponent(destination)}?alt=media&token=${token}`;
    };

    const uploadedUrls = {
      logoURL: await uploadFile(logo, `${campusPath}/Meta/logo`),
      mapURL: await uploadFile(map, `${campusPath}/Meta/map`),
      approvalURL: await uploadFile(approval, `${campusPath}/Meta/${uid}/approval/${approval.originalname}`),
      profilePictureURL: await uploadFile(profilePicture, `${campusPath}/Meta/${uid}/profile/${profilePicture.originalname}`),
    };

    // Create new campus document
    await admin.firestore().collection("Campuses").doc(campusId).set({
      name: campusName,
      city,
      country,
      description: description || "",
      logoURL: uploadedUrls.logoURL,
      mapImageURL: uploadedUrls.mapURL,
      storageFolder: campusFolder,
      createdAt: now,
      adminId: [uid],
    });

    // Create empty Buildings subcollection with a placeholder document
    await admin.firestore()
        .collection("Campuses")
        .doc(campusId)
        .collection("Buildings")
        .doc("_placeholder")
        .set({createdAt: now});

    // Create admin profile document
    await admin.firestore().collection("Admin_Profiles").doc(uid).set({
      adminName,
      email,
      role,
      campusId,
      status: "pending",
      createdAt: now,
      employeeApprovalFileURL: uploadedUrls.approvalURL,
      adminPhotoURL: uploadedUrls.profilePictureURL || null,
    });

    return res.status(200).json({
      success: true,
      message: "Admin registered successfully.",
      adminUID: uid,
      campusId,
      uploadedUrls,
    });
  } catch (error) {
    console.error("Registration error:", error);
    return res.status(500).json({error: "Internal Server Error"});
  }
});

module.exports = {app};
