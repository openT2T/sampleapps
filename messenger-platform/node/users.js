'use strict';

// In-memory db of user data
global.userTokens = {};
global.userSignInNotifications = {};

module.exports = {

    signIn: function (uid, access_token) {
        global.userTokens.uid = access_token;
    },

    getToken: function (uid) {
        return global.userTokens.uid;
    },
    
    signOut: function (uid) {
        delete global.userTokens.uid;
        delete global.userSignInNotifications.uid;
    },
    
    setSignInNotified: function (uid, state) {
        global.userSignInNotifications.uid = state;
    },
    
    signInNotified: function () {
        return !!global.userSignInNotifications.uid;
    }
}