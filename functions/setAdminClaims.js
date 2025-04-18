// functions/setAdminClaims.js
const functions = require("firebase-functions");
const admin = require("firebase-admin");

/**
 * Callable CF – executed by a signed‑in **sysadmin** after
 *               approving a new admin.
 * Expects:  data = { adminId: "<UID‑of‑approved‑admin>" }
 * Action :  ‑ looks up campusId in Admin_Profiles/<adminId>
 *           ‑ writes { role:"admin", campusId } to the user’s custom claims
 */
exports.setAdminClaims = functions.https.onCall(async (data, context) => {
  /* ── Security ─────────────────────────────────────────────────────────── */
  if (!context.auth) {
    throw new functions.https.HttpsError(
        "unauthenticated",
        "You must be signed‑in to invoke this function.",
    );
  }
  if (context.auth.token.role !== "sysadmin") {
    throw new functions.https.HttpsError(
        "permission-denied",
        "Only sysadmins can assign admin claims.",
    );
  }

  /* ── Validate & extract input (no optional‑chaining) ──────────────────── */
  const safeGet = (obj, key) =>
    (obj && Object.prototype.hasOwnProperty.call(obj, key) ?
      obj[key] : undefined);
  const adminId = safeGet(data, "adminId");

  if (!adminId) {
    throw new functions.https.HttpsError(
        "invalid-argument", "Missing field: adminId");
  }

  /* ── Look up the admin profile to get campusId ───────────────────────── */
  const profileSnap = await admin
      .firestore()
      .collection("Admin_Profiles")
      .doc(adminId)
      .get();

  if (!profileSnap.exists) {
    throw new functions.https.HttpsError(
        "not-found",
        `Admin_Profiles/${adminId} does not exist`,
    );
  }

  const campusId = profileSnap.data().campusId;
  if (!campusId) {
    throw new functions.https.HttpsError(
        "failed-precondition",
        "CampusId missing in admin profile",
    );
  }

  /* ── Write the custom claims ──────────────────────────────────────────── */
  await admin.auth().setCustomUserClaims(adminId, {
    role: "admin",
    campusId,
  });

  return {message: "Custom claims updated successfully."};
});
