
// In-memory db of user state. 
global.userDb = {};

module.exports = {

    signIn: function (uid, access_token) {
        global.userDb.uid = access_token;
    },

    getToken: function (uid) {
        return global.userDb.uid;
    },
    
    signOut: function (uid) {
        delete global.userDb.uid;
    }
}