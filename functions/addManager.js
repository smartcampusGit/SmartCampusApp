/* eslint-disable max-len */
const express = require("express");
const cors = require("cors");
const admin = require("firebase-admin");
const {Readable} = require("stream");
const {v4: uuidv4} = require("uuid");
const fileParser = require("express-multipart-file-parser");

const app = express();
app.use(cors({origin: true}));
app.use(fileParser);

// helper – "H.I.T Campus!" → "hit_campus"
const sanitize = (str) =>
  str.trim().toLowerCase().replace(/[^a-z0-9]+/g, "_").replace(/^_|_$/g, "");

app.post("/addManager", async (req, res) => {
  try {
    /* ------------------------------------------------------------------
           1. Verify caller’s ID‑token (avoid optional‑chaining here)      */
    const authHeader = req.headers.authorization || "";
    const idToken = authHeader.startsWith("Bearer ") ?
            authHeader.split("Bearer ")[1] :
            "";

    let caller = null;
    if (idToken) {
      try {
        caller = await admin.auth().verifyIdToken(idToken);
      } catch (e) {/* ignore – caller stays null */}
    }
    if (!caller || !(caller.role || caller.isSysAdmin)) {
      return res.status(403).json({error: "Not authorized."});
    }

    /* ------------------------------------------------------------------
           2. Validate form fields                                           */
    const {email, password, adminName, role, campusId} = req.body;
    if (!email || !password || !adminName || !role || !campusId) {
      return res.status(400).json({error: "Missing required fields."});
    }

    /* ------------------------------------------------------------------
           3. Storage helper                                                 */
    const bucket = admin.storage().bucket();
    const uploadFile = async (file, destination) => {
      const token = uuidv4();
      const metadata = {
        contentType: file.mimetype,
        metadata: {firebaseStorageDownloadTokens: token},
      };
      const fileRef = bucket.file(destination);
      const stream = fileRef.createWriteStream({metadata});
      Readable.from(file.buffer).pipe(stream);
      await new Promise((ok, err) => {
        stream.on("finish", ok); stream.on("error", err);
      });
      return `https://firebasestorage.googleapis.com/v0/b/${bucket.name}/o/${encodeURIComponent(destination)}?alt=media&token=${token}`;
    };

    /* ------------------------------------------------------------------
           4. Create Auth user                                               */
    const {uid} = await admin.auth().createUser({email, password});

    /* ------------------------------------------------------------------
           5. Figure out campus folder name                                  */
    const campusSnap = await admin.firestore().collection("Campuses").doc(campusId).get();
    if (!campusSnap.exists) return res.status(404).json({error: "Campus not found"});

    const campusFolder = campusSnap.get("storageFolder") || sanitize(campusSnap.get("name"));
    const adminFolder = `campuses/${campusFolder}/Meta/${uid}`;

    /* ------------------------------------------------------------------
           6. Optional uploads                                               */
    const approval = req.files.find((f) => f.fieldname === "approval");
    const profilePhoto = req.files.find((f) => f.fieldname === "profilePhoto");
    const uploads = {};

    if (approval) {
      uploads.employeeApprovalFileURL =
                await uploadFile(approval, `${adminFolder}/approval/${approval.originalname}`);
    }
    if (profilePhoto) {
      uploads.adminPhotoURL =
                await uploadFile(profilePhoto, `${adminFolder}/profile/${profilePhoto.originalname}`);
    }

    /* ------------------------------------------------------------------
           7. Atomic Firestore transaction                                   */
    const now = admin.firestore.FieldValue.serverTimestamp();
    await admin.firestore().runTransaction(async (tx) => {
      const campusRef = admin.firestore().collection("Campuses").doc(campusId);

      /* add new UID to campus.adminId */
      tx.update(campusRef, {
        adminId: admin.firestore.FieldValue.arrayUnion(uid),
        updatedAt: now,
      });

      /* create Admin_Profiles doc */
      tx.set(admin.firestore().collection("Admin_Profiles").doc(uid), {
        adminName,
        email,
        role,
        campusId,
        status: "active",
        createdAt: now,
        ...uploads,
      });
    });

    /* ------------------------------------------------------------------
           8. Custom claims + response                                       */
    await admin.auth().setCustomUserClaims(uid, {role: "admin", campusId});
    return res.status(200).json({success: true, newAdminUid: uid});
  } catch (err) {
    console.error("addManager error:", err);
    return res.status(500).json({error: err.message});
  }
});

module.exports = {app};
