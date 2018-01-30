import { createUserManager } from 'redux-oidc';
import config from '../configuration/config'
import { UserManager, Log } from 'oidc-client';

const { openIdConnectSettings } = config.apiSettings;

class UserManagerSingleton {
    private static instanceInternal: UserManager;

    static get instance() {
        if (UserManagerSingleton.instanceInternal) {
            return UserManagerSingleton.instanceInternal;
        }

        const userManagerConfig = {
            client_id: openIdConnectSettings.clientId,
            redirect_uri: `${window.location.origin}/callback`,
            post_logout_redirect_uri: `${window.location.origin}`,
            response_type: 'id_token token',
            scope: openIdConnectSettings.scope,
            authority: openIdConnectSettings.authority,
            silent_redirect_uri: `${window.location.origin}${openIdConnectSettings.silentRenewUrl}`,
            automaticSilentRenew: true,
            filterProtocolClaims: true,
            loadUserInfo: false,
            //accessTokenExpiringNotificationTime: 3580,
            monitorSession: false,
            revokeAccessTokenOnSignout: true
        };

        // TODO: turn off for production
        Log.logger = console;
        Log.level = Log.DEBUG;

        UserManagerSingleton.instanceInternal = createUserManager(userManagerConfig);
        return UserManagerSingleton.instanceInternal;
    }
}

const userManagerInstance = UserManagerSingleton.instance;
export default userManagerInstance;