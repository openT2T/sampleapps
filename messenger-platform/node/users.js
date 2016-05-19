'use strict';

// In-memory db of user data
global.userTokens = {};
global.userSignInNotifications = {};
global.userLampIds = {};

module.exports = {

    signIn: function (uid, access_token) {
        global.userTokens[uid] = access_token;
    },
    
    signOut: function (uid) {
        delete global.userTokens[uid];
        delete global.userSignInNotifications[uid]
    },
   
    getToken: function (uid) {
        return global.userTokens[uid];
    },
    
    setSignInNotified: function (uid, state) {
        global.userSignInNotifications[uid] = state;
    },
    
    getSignInNotified: function (uid) {
        return !!global.userSignInNotifications[uid];
    },
    
    setLampIds: function (uid, lampIds) {
        global.userLampIds[uid] = lampIds;
    },
    
    getLampIds: function (uid) {
        return global.userLampIds[uid];
    }
}