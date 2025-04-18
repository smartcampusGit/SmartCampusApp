/* eslint-disable no-unused-vars */
// functions/index.js  –for "firebase-functions" v4+
const {initializeApp} = require("firebase-admin/app");
initializeApp();

/* ── v4 style imports ─────────────────────────── */
const {onRequest} = require("firebase-functions/v2/https");
const {onCall} = require("firebase-functions/v2/https"); // same module
const {app: registerAdminApp} = require("./registerAdmin");
const {app: addManagerApp} = require("./addManager");
const {setAdminClaims} = require("./setAdminClaims"); // already onCall

/* ── HTTP endpoints ───────────────────────────── */
exports.registerAdmin = onRequest(
    {region: "us-central1"}, // first argument is options
    registerAdminApp,
);

exports.addManager = onRequest(
    {region: "us-central1"},
    addManagerApp,
);

/* ── Callable (onCall) endpoint ───────────────── */
exports.setAdminClaims = setAdminClaims;
